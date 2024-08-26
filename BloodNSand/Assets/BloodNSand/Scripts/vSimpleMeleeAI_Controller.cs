using Invector.vEventSystems;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Invector.vCharacterController.AI
{
    [vClassHeader("Simple Melee AI", "This is a Simple Melee AI that comes with the MeleeCombat package as a bonus, if you want a more advanced AI check our AI Template")]
    public class vSimpleMeleeAI_Controller : vSimpleMeleeAI_Animator, vIMeleeFighter
    {
        [vEditorToolbar("Iterations")]
        public float stateRoutineIteration = 0.15f;
        public float destinationRoutineIteration = 0.25f;
        public float findTargetIteration = 0.25f;
        public float smoothSpeed = 5f;

        [vEditorToolbar("Events")]
        [Header("--- On Change State Events ---")]
        public UnityEngine.Events.UnityEvent onIdle;
        public UnityEngine.Events.UnityEvent onChase;
        public UnityEngine.Events.UnityEvent onPatrol;

        protected AIStates oldState;
        protected float ignorePatrolTimer;
        protected float moveToSpeed;
        protected Vector3 moveToDestination;
        private Dictionary<Transform, float> aggroTable = new Dictionary<Transform, float>(); // Tracks aggro for each target
        public int clusterID; // New field to store the AI's cluster ID

        protected override void Start()
        {
            base.Start();
            ignorePatrolTimer = -1f;
            moveToDestination = transform.position;
            Init();

            // Start routines immediately
            StartCoroutine(StateRoutine());
            StartCoroutine(FindTarget());
            StartCoroutine(DestinationBehaviour());

            // Add OnDead event listener
            vOnDeadTrigger onDeadTrigger = GetComponent<vOnDeadTrigger>();
            if (onDeadTrigger != null)
            {
                onDeadTrigger.OnDead.AddListener(OnDeath);
            }

            // Start searching for a target immediately
            SetTarget();
        }

        protected void FixedUpdate()
        {
            ControlLocomotion();
        }

        #region AI Target

        public virtual void SetCurrentTarget(Transform target)
        {
            if (target != currentTarget.transform)
            {
                currentTarget.transform = target;
                currentTarget.colliderTarget = target.GetComponent<Collider>();
                currentTarget.character = target.GetComponent<vIHealthController>();
            }
            sphereSensor.AddTarget(target);
        }

        public virtual void RemoveCurrentTarget()
        {
            if (currentTarget.transform)
            {
                currentTarget.transform = null;
                currentTarget.colliderTarget = null;
                currentTarget.character = null;
            }
        }

        // Override SetTarget to prioritize targets within the same cluster
        protected void SetTarget()
        {
            if (currentHealth > 0 && sphereSensor != null)
            {
                sphereSensor.CheckTargetsAround(fieldOfView, minDetectDistance, maxDetectDistance, layersToDetect, sortTargetFromDistance);
                var targets = sphereSensor.GetTargets();

                if (targets == null || targets.Count == 0)
                {
                    // No targets found, move to the arena center
                    MoveToArenaCenter();
                    return;
                }

                // Prioritize targets within the same cluster
                var sameClusterTargets = targets.Where(t => t != null && t.GetComponent<vSimpleMeleeAI_Controller>()?.clusterID == clusterID).ToList();

                Transform targetTransform = null;

                if (sameClusterTargets.Count > 0)
                {
                    targetTransform = sameClusterTargets[Random.Range(0, sameClusterTargets.Count)].transform;
                }
                else if (targets.Count > 0) // If no targets in the same cluster, choose any target
                {
                    targetTransform = targets[Random.Range(0, targets.Count)].transform;
                }

                if (targetTransform != null)
                {
                    SetCurrentTarget(targetTransform);
                }
                else
                {
                    MoveToArenaCenter();
                }
            }
            else if (currentHealth <= 0f)
            {
                destination = transform.position;
                RemoveCurrentTarget();
            }
        }

        protected IEnumerator FindTarget()
        {
            while (true)
            {
                yield return new WaitForSeconds(findTargetIteration);
                if (currentHealth > 0)
                {
                    SetTarget();
                    if (currentTarget.transform == null)
                    {
                        // If no target found, move to the closest cluster or center
                        MoveToClosestClusterOrCenter();
                    }
                    else
                    {
                        CheckTarget();
                    }
                }
            }
        }

        // New method to move to the arena center
        void MoveToArenaCenter()
        {
            Vector3 centerPosition = BattleManager.Instance.arenaCenter.position;
            MoveTo(centerPosition, chaseSpeed);
            StartCoroutine(CheckArrivalAtArenaCenter(centerPosition));  // Start checking if the AI has arrived at the arena center
            Debug.Log($"{gameObject.name} is moving to the arena center.");
        }

        protected IEnumerator CheckArrivalAtArenaCenter(Vector3 centerPosition)
        {
            // Keep checking if the AI has reached the arena center
            while (Vector3.Distance(transform.position, centerPosition) > agent.stoppingDistance)
            {
                yield return null;
            }

            // Once the AI reaches the center, start circling in place
            StartCoroutine(CircleInPlace());
        }

        protected IEnumerator CircleInPlace()
        {
            float rotationSpeed = 1f; // Slow down the rotation speed to 60 degrees per second
            float rotationDuration = 5f; // Duration to circle in place
            float rotationStartTime = Time.time;

            while (Time.time < rotationStartTime + rotationDuration)
            {
                // Rotate the AI slowly in place
                transform.Rotate(0, rotationSpeed * Time.deltaTime, 0);

                // Check if any new targets are found during circling
                if (currentTarget.transform != null && canSeeTarget)
                {
                    Debug.Log($"{gameObject.name} has spotted a target while circling and is resuming chase.");
                    currentState = AIStates.Chase;
                    yield break; // Exit the circling and resume chasing
                }

                yield return null;
            }

            // After circling, attempt to find a new target
            SetTarget();
            currentState = AIStates.Chase; // Resume chasing state
        }


        // New method to move to the closest cluster or center
        void MoveToClosestClusterOrCenter()
        {
            Vector3 closestClusterPosition = GetClosestClusterPosition();
            if (closestClusterPosition != Vector3.zero)
            {
                MoveTo(closestClusterPosition, chaseSpeed);
                Debug.Log($"{gameObject.name} is moving to the closest cluster.");
            }
            else
            {
                MoveToArenaCenter();
            }
        }

        // Logic to calculate closest cluster position
        Vector3 GetClosestClusterPosition()
        {
            var allAI = FindObjectsOfType<vSimpleMeleeAI_Controller>();
            Vector3 closestPosition = Vector3.zero;
            float closestDistance = float.MaxValue;

            foreach (var ai in allAI)
            {
                if (ai != this && ai.clusterID != clusterID) // Only consider other clusters
                {
                    float distance = Vector3.Distance(transform.position, ai.transform.position);
                    if (distance < closestDistance)
                    {
                        closestDistance = distance;
                        closestPosition = ai.transform.position;
                    }
                }
            }

            return closestPosition == Vector3.zero ? Vector3.zero : closestPosition;
        }

        bool CheckTargetIsAlive()
        {
            if (currentTarget.transform == null || currentTarget.character == null)
            {
                return false;
            }

            return currentTarget.character.currentHealth > 0;
        }

        public void AddAggro(Transform target, float amount)
        {
            if (aggroTable.ContainsKey(target))
            {
                aggroTable[target] += amount;
            }
            else
            {
                aggroTable[target] = amount;
            }

            // Switch to the highest aggro target
            UpdateTarget();
        }

        // Override UpdateTarget to recheck clusters when switching targets
        private void UpdateTarget()
        {
            Transform highestAggroTarget = null;
            float highestAggro = 0;

            foreach (var entry in aggroTable)
            {
                if (entry.Value > highestAggro)
                {
                    highestAggro = entry.Value;
                    highestAggroTarget = entry.Key;
                }
            }

            if (highestAggroTarget != null)
            {
                // If the highest aggro target is not in the same cluster, check for same cluster targets
                if (highestAggroTarget.GetComponent<vSimpleMeleeAI_Controller>()?.clusterID != clusterID)
                {
                    SetTarget();
                }
                else
                {
                    SetCurrentTarget(highestAggroTarget);
                }
            }
        }

        public void RemoveAggro(Transform target)
        {
            if (aggroTable.ContainsKey(target))
            {
                aggroTable.Remove(target);
            }
        }

        #endregion

        #region AI Locomotion

        void ControlLocomotion()
        {
            if (AgentDone() && agent.updatePosition || lockMovement)
            {
                agent.speed = 0f;
                combatMovement = Vector3.zero;
            }
            if (agent.isOnOffMeshLink)
            {
                float speed = agent.desiredVelocity.magnitude;
                UpdateAnimator(AgentDone() ? 0f : speed, direction);
            }
            else
            {
                var desiredVelocity = agent.enabled ? agent.updatePosition ? agent.desiredVelocity : (agent.nextPosition - transform.position) : (destination - transform.position);
                if (OnStrafeArea)
                {
                    var destin = transform.InverseTransformDirection(desiredVelocity).normalized;
                    combatMovement = Vector3.Lerp(combatMovement, destin, 2f * Time.deltaTime);
                    UpdateAnimator(AgentDone() ? 0f : combatMovement.z, combatMovement.x);
                }
                else
                {
                    float speed = desiredVelocity.magnitude;
                    combatMovement = Vector3.zero;
                    UpdateAnimator(AgentDone() ? 0f : speed, 0f);
                }
            }
        }

        Vector3 AgentDirection()
        {
            var forward = AgentDone() ? (currentTarget.transform != null && OnStrafeArea && canSeeTarget ?
                         (new Vector3(destination.x, transform.position.y, destination.z) - transform.position) :
                         transform.forward) : agent.desiredVelocity;

            fwd = Vector3.Lerp(fwd, forward, 20 * Time.deltaTime);
            return fwd;
        }

        protected virtual IEnumerator DestinationBehaviour()
        {
            while (true)
            {
                yield return new WaitForSeconds(destinationRoutineIteration);
                CheckGroundDistance();
                if (agent.updatePosition)
                {
                    UpdateDestination(destination);
                }
            }
        }

        protected virtual void UpdateDestination(Vector3 position)
        {
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(position);
            }

            #region debug Path
            if (agent.enabled && agent.hasPath)
            {
                if (drawAgentPath)
                {
                    Debug.DrawLine(transform.position, position, Color.red, 0.5f);
                    var oldPos = transform.position;
                    for (int i = 0; i < agent.path.corners.Length; i++)
                    {
                        var pos = agent.path.corners[i];
                        Debug.DrawLine(oldPos, pos, Color.green, 0.5f);
                        oldPos = pos;
                    }
                }
            }
            #endregion
        }

        protected void CheckIsOnNavMesh()
        {
            // check if the AI is on a valid Navmesh, if not he dies
            if (!agent.isOnNavMesh && agent.enabled && !ragdolled)
            {
                Debug.LogWarning("Missing NavMesh Bake, character will die - Please Bake your navmesh again!");
                currentHealth = 0;
            }
        }

        #endregion

        #region AI States

        protected IEnumerator StateRoutine()
        {
            while (this.enabled)
            {
                CheckIsOnNavMesh();
                CheckAutoCrouch();
                yield return new WaitForSeconds(stateRoutineIteration);
                if (!lockMovement)
                {
                    // Start with Chase state to seek out enemies immediately
                    if (currentState == AIStates.Idle)
                    {
                        currentState = AIStates.Chase;
                    }

                    switch (currentState)
                    {
                        case AIStates.Idle:
                            if (currentState != oldState) { onIdle.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(Idle());
                            break;
                        case AIStates.Chase:
                            if (currentState != oldState) { onChase.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(Chase());
                            break;
                        case AIStates.PatrolSubPoints:
                            if (currentState != oldState) { onPatrol.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(PatrolSubPoints());
                            break;
                        case AIStates.PatrolWaypoints:
                            if (currentState != oldState) { onPatrol.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(PatrolWaypoints());
                            break;
                        case AIStates.Wander:
                            if (currentState != oldState) { onPatrol.Invoke(); oldState = currentState; }
                            yield return StartCoroutine(Wander());
                            break;
                    }
                }
            }
        }

        protected IEnumerator Idle()
        {
            while (currentHealth <= 0)
            {
                yield return null;
            }

            if (canSeeTarget)
            {
                currentState = AIStates.Chase;
            }
            else
            {
                agent.speed = Mathf.Lerp(agent.speed, 0f, smoothSpeed * Time.deltaTime);
            }
        }

        protected IEnumerator Chase()
        {
            while (currentHealth <= 0)
            {
                yield return null;
            }

            // Ensure that the AI agent's speed is set correctly
            agent.speed = Mathf.Lerp(agent.speed, chaseSpeed, smoothSpeed * Time.deltaTime);
            agent.stoppingDistance = chaseStopDistance;

            // Block if appropriate
            if (!isBlocking && !tryingBlock)
            {
                StartCoroutine(CheckChanceToBlock(chanceToBlockInStrafe, lowerShield));
            }

            // If there is no target or the AI is not aggressive at first sight, start patrolling
            if (currentTarget.transform == null || !agressiveAtFirstSight)
            {
                currentState = AIStates.PatrolWaypoints;
                yield break;
            }

            // Ensure target is still valid before proceeding
            if (currentTarget.transform == null || currentTarget.character == null || currentTarget.character.isDead)
            {
                RemoveCurrentTarget();
                SetTarget();  // Find a new target if available
                yield break;
            }

            // Debug log for tracking
            Debug.Log($"{gameObject.name} is chasing {currentTarget.transform.name}, Target Distance: {TargetDistance}, Can Attack: {canAttack}");

            // Begin the Attack Routine when close to the Target
            if (currentTarget.transform != null && TargetDistance <= distanceToAttack && meleeManager != null && canAttack && !actions)
            {
                Debug.Log($"{gameObject.name} is attacking {currentTarget.transform.name}");
                canAttack = false;
                yield return StartCoroutine(MeleeAttackRoutine());
            }

            if (attackCount <= 0 && !inResetAttack && !isAttacking)
            {
                StartCoroutine(ResetAttackCount());
                yield return null;
            }

            // Strafing while close to the Target
            if (OnStrafeArea && strafeSideways)
            {
                if (strafeSwapeFrequency <= 0)
                {
                    sideMovement = GetRandonSide();
                    strafeSwapeFrequency = UnityEngine.Random.Range(minStrafeSwape, maxStrafeSwape);
                }
                else
                {
                    strafeSwapeFrequency -= Time.deltaTime;
                }

                fwdMovement = (TargetDistance < distanceToAttack) ? (strafeBackward ? -1 : 0) : TargetDistance > distanceToAttack ? 1 : 0;
                var dir = ((transform.right * sideMovement) + (transform.forward * fwdMovement));
                Ray ray = new Ray(new Vector3(transform.position.x, currentTarget.transform != null ? currentTarget.transform.position.y : transform.position.y, transform.position.z), dir);

                if (currentTarget.transform != null && TargetDistance < strafeDistance - 0.5f)
                {
                    destination = OnStrafeArea ? ray.GetPoint(agent.stoppingDistance + 0.5f) : currentTarget.transform.position;
                }
                else if (currentTarget.transform != null)
                {
                    destination = currentTarget.transform.position;
                }

                // Start strafing and then attack after the timer ends
                StartCoroutine(StrafeAndAttack());
                yield break;  // Exit the Chase coroutine to let StrafeAndAttack handle the next actions
            }
            // Chase Target
            else
            {
                if (currentTarget.transform != null)
                {
                    destination = currentTarget.transform.position;
                }
                else
                {
                    fwdMovement = (TargetDistance < distanceToAttack) ? (strafeBackward ? -1 : 0) : TargetDistance > distanceToAttack ? 1 : 0;
                    Ray ray = new Ray(transform.position, transform.forward * fwdMovement);

                    if (TargetDistance < strafeDistance - 0.5f)
                    {
                        destination = (fwdMovement != 0) ? ray.GetPoint(agent.stoppingDistance + ((fwdMovement > 0) ? TargetDistance : 1f)) : transform.position;
                    }
                    else if (currentTarget.transform != null)
                    {
                        destination = currentTarget.transform.position;
                    }
                }
            }

            // Check if the agent is on the NavMesh before setting the destination
            if (agent.isOnNavMesh)
            {
                agent.SetDestination(destination);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name} is not on the NavMesh and cannot move to the destination.");
            }
        }


        protected IEnumerator StrafeAndAttack()
        {
            float strafeDuration = UnityEngine.Random.Range(3f, 7f);
            float strafeStartTime = Time.time;

            while (Time.time < strafeStartTime + strafeDuration && currentTarget.transform != null && TargetDistance > distanceToAttack)
            {
                yield return null;
            }

            // After strafing, try to attack
            if (currentTarget.transform != null && TargetDistance <= distanceToAttack && meleeManager != null && !actions)
            {
                Debug.Log($"{gameObject.name} is attacking {currentTarget.transform.name} after strafing.");
                canAttack = true;
                yield return StartCoroutine(MeleeAttackRoutine());
            }
        }

        protected IEnumerator PatrolSubPoints()
        {
            while (!agent.enabled)
            {
                yield return null;
            }

            if (targetWaypoint)
            {
                if (targetPatrolPoint == null || !targetPatrolPoint.isValid)
                {
                    targetPatrolPoint = GetPatrolPoint(targetWaypoint);
                }
                else
                {
                    agent.speed = Mathf.Lerp(agent.speed, (agent.hasPath && targetPatrolPoint.isValid) ? patrolSpeed : 0, smoothSpeed * Time.deltaTime);
                    agent.stoppingDistance = patrollingStopDistance;
                    destination = targetPatrolPoint.isValid ? targetPatrolPoint.position : transform.position;
                    if (Vector3.Distance(transform.position, destination) < targetPatrolPoint.areaRadius && targetPatrolPoint.CanEnter(transform) && !targetPatrolPoint.IsOnWay(transform))
                    {
                        targetPatrolPoint.Enter(transform);
                        wait = Time.time + targetPatrolPoint.timeToStay;
                        visitedPatrolPoint.Add(targetPatrolPoint);
                    }
                    else if (Vector3.Distance(transform.position, destination) < targetPatrolPoint.areaRadius && (!targetPatrolPoint.CanEnter(transform) || !targetPatrolPoint.isValid))
                    {
                        targetPatrolPoint = GetPatrolPoint(targetWaypoint);
                    }

                    if (targetPatrolPoint != null && (targetPatrolPoint.IsOnWay(transform) && Vector3.Distance(transform.position, destination) < distanceToChangeWaypoint))
                    {
                        if (wait < Time.time || !targetPatrolPoint.isValid)
                        {
                            wait = 0;
                            if (visitedPatrolPoint.Count == pathArea.GetValidSubPoints(targetWaypoint).Count)
                            {
                                currentState = AIStates.PatrolWaypoints;
                                targetWaypoint.Exit(transform);
                                targetPatrolPoint.Exit(transform);
                                targetWaypoint = null;
                                targetPatrolPoint = null;
                                visitedPatrolPoint.Clear();
                            }
                            else
                            {
                                targetPatrolPoint.Exit(transform);
                                targetPatrolPoint = GetPatrolPoint(targetWaypoint);
                            }
                        }
                    }
                }
            }
            if (canSeeTarget)
            {
                currentState = AIStates.Chase;
            }
        }

        protected IEnumerator PatrolWaypoints()
        {
            while (!agent.enabled)
            {
                yield return null;
            }

            if (pathArea != null && pathArea.waypoints.Count > 0)
            {
                if (targetWaypoint == null || !targetWaypoint.isValid)
                {
                    targetWaypoint = GetWaypoint();
                }
                else
                {
                    agent.speed = Mathf.Lerp(agent.speed, (agent.hasPath && targetWaypoint.isValid) ? patrolSpeed : 0, smoothSpeed * Time.deltaTime);

                    agent.stoppingDistance = patrollingStopDistance;

                    destination = targetWaypoint.position;
                    if (Vector3.Distance(transform.position, destination) < targetWaypoint.areaRadius && targetWaypoint.CanEnter(transform) && !targetWaypoint.IsOnWay(transform))
                    {
                        targetWaypoint.Enter(transform);
                        wait = Time.time + targetWaypoint.timeToStay;
                    }
                    else if (Vector3.Distance(transform.position, destination) < targetWaypoint.areaRadius && (!targetWaypoint.CanEnter(transform) || !targetWaypoint.isValid))
                    {
                        targetWaypoint = GetWaypoint();
                    }

                    if (targetWaypoint != null && targetWaypoint.IsOnWay(transform) && Vector3.Distance(transform.position, destination) < distanceToChangeWaypoint)
                    {
                        if (wait < Time.time || !targetWaypoint.isValid)
                        {
                            wait = 0;
                            if (targetWaypoint.subPoints.Count > 0)
                            {
                                currentState = AIStates.PatrolSubPoints;
                            }
                            else
                            {
                                targetWaypoint.Exit(transform);
                                visitedPatrolPoint.Clear();
                                targetWaypoint = GetWaypoint();
                            }
                        }
                    }
                }
            }
            else if (ignorePatrolTimer < Time.time)
            {
                switch (patrolWithoutAreaStyle)
                {
                    case AIPatrolWithOutAreaStyle.GoToStartPoint:
                        yield return StartCoroutine(GoToStartingPoint());
                        break;
                    case AIPatrolWithOutAreaStyle.Idle:
                        currentState = AIStates.Idle;
                        break;
                    case AIPatrolWithOutAreaStyle.Wander:
                        currentState = AIStates.Wander;
                        break;
                }
            }
            else if (ignorePatrolTimer > Time.time)
            {
                yield return StartCoroutine(GoToDestionation());
            }

            if (canSeeTarget)
            {
                currentState = AIStates.Chase;
            }
        }

        protected virtual IEnumerator GoToDestionation()
        {
            yield return null;
            agent.speed = Mathf.Lerp(agent.speed, moveToSpeed, smoothSpeed * Time.deltaTime);
            agent.stoppingDistance = patrollingStopDistance;
            destination = moveToDestination;
        }

        protected virtual IEnumerator GoToStartingPoint()
        {
            yield return null;
            agent.speed = Mathf.Lerp(agent.speed, patrolSpeed, smoothSpeed * Time.deltaTime);
            agent.stoppingDistance = patrollingStopDistance;
            destination = startPosition;
        }

        protected virtual IEnumerator Wander()
        {
            agent.speed = Mathf.Lerp(agent.speed, wanderSpeed, smoothSpeed * Time.deltaTime);
            do
            {
                yield return null;
                destination = transform.position + (Quaternion.AngleAxis(UnityEngine.Random.Range(-120, 120), transform.up) * transform.forward) * (patrollingStopDistance + 4);
            } while (agent.enabled && agent.isOnNavMesh && agent.remainingDistance <= patrollingStopDistance);
            if (canSeeTarget)
            {
                currentState = AIStates.Chase;
            }
        }

        #endregion

        #region AI Waypoint & PatrolPoint

        vWaypoint GetWaypoint()
        {
            var waypoints = pathArea.GetValidPoints();

            if (randomWaypoints)
            {
                currentWaypoint = randomWaypoint.Next(waypoints.Count);
            }
            else
            {
                currentWaypoint++;
            }

            if (currentWaypoint >= waypoints.Count)
            {
                currentWaypoint = 0;
            }

            if (waypoints.Count == 0)
            {
                agent.isStopped = true;
                return null;
            }
            if (visitedWaypoint.Count == waypoints.Count)
            {
                visitedWaypoint.Clear();
            }

            if (visitedWaypoint.Contains(waypoints[currentWaypoint]))
            {
                return null;
            }

            agent.isStopped = false;
            return waypoints[currentWaypoint];
        }

        vPoint GetPatrolPoint(vWaypoint waypoint)
        {
            var subPoints = pathArea.GetValidSubPoints(waypoint);
            if (waypoint.randomPatrolPoint)
            {
                currentPatrolPoint = randomPatrolPoint.Next(subPoints.Count);
            }
            else
            {
                currentPatrolPoint++;
            }

            if (currentPatrolPoint >= subPoints.Count)
            {
                currentPatrolPoint = 0;
            }

            if (subPoints.Count == 0)
            {
                agent.isStopped = true;
                return null;
            }
            if (visitedPatrolPoint.Contains(subPoints[currentPatrolPoint]))
            {
                return null;
            }

            agent.isStopped = false;
            return subPoints[currentPatrolPoint];
        }

        #endregion

        #region AI Melee Combat

        protected IEnumerator MeleeAttackRoutine()
        {
            if (!isAttacking && !actions && attackCount > 0 && !lockMovement && !isRolling)
            {
                sideMovement = GetRandonSide();
                agent.stoppingDistance = distanceToAttack;
                attackCount--;
                MeleeAttack();
                yield return null;
            }
        }

        public void FinishAttack()
        {
            isAttacking = false;  // Mark as not attacking
            canAttack = true;     // Allow the AI to attack again
            Debug.Log($"{gameObject.name} has finished an attack and can attack again.");
        }

        IEnumerator ResetAttackCount()
        {
            inResetAttack = true;
            canAttack = false;
            Debug.Log($"{gameObject.name} is resetting attack count.");

            float value = firstAttack ? firstAttackDelay : UnityEngine.Random.Range(minTimeToAttack, maxTimeToAttack);
            firstAttack = false;

            yield return new WaitForSeconds(value);

            attackCount = randomAttackCount ? UnityEngine.Random.Range(1, maxAttackCount + 1) : maxAttackCount;
            canAttack = true;
            Debug.Log($"{gameObject.name} is ready to attack again with {attackCount} attacks available.");

            inResetAttack = false;
        }

        public void OnEnableAttack()
        {
            isAttacking = true;
        }

        public void OnDisableAttack()
        {
            isAttacking = false;
            canAttack = true;
        }

        public void ResetAttackTriggers()
        {
            animator.ResetTrigger("WeakAttack");
        }

        public void BreakAttack(int breakAtkID)
        {
            ResetAttackCount();
            ResetAttackTriggers();
            OnRecoil(breakAtkID);
        }

        public void OnRecoil(int recoilID)
        {
            TriggerRecoil(recoilID);
        }

        public void OnReceiveAttack(vDamage damage, vIMeleeFighter attacker)
        {
            StartCoroutine(CheckChanceToBlock(chanceToBlockAttack, 0));

            var attackPos = (attacker != null && attacker.character != null) ? attacker.character.transform.position : damage.hitPosition;
            if (!damage.ignoreDefense && isBlocking && meleeManager != null && meleeManager.CanBlockAttack(attackPos))
            {
                var damageReduction = meleeManager != null ? meleeManager.GetDefenseRate() : 0;
                if (damageReduction > 0)
                {
                    damage.ReduceDamage(damageReduction);
                }

                if (attacker != null && meleeManager != null && meleeManager.CanBreakAttack())
                {
                    attacker.OnRecoil(meleeManager.GetDefenseRecoilID());
                }

                meleeManager.OnDefense();
            }
            // Apply tag from the character that hit you and start chase
            if (!passiveToDamage && damage.sender != null)
            {
                SetCurrentTarget(damage.sender);
                AddAggro(damage.sender.transform, damage.damageValue);
                currentState = AIStates.Chase;
            }
            damage.hitReaction = !isBlocking;
            if (!passiveToDamage)
            {
                SetAgressive(true);
            }

            TakeDamage(damage);
        }

        public virtual void MoveTo(Vector3 position, float moveToSpeed = 1f, float ignorePatrolTimer = 2f)
        {
            moveToDestination = position;
            currentState = AIStates.PatrolWaypoints;
            this.moveToSpeed = moveToSpeed;
            this.ignorePatrolTimer = Time.time + ignorePatrolTimer;
        }

        public vICharacter character
        {
            get { return this; }
        }

        #endregion

        #region Death Handling

        protected void OnDeath()
        {
            // Clear the current target as this AI has died
            RemoveCurrentTarget();

            // Start a coroutine to find a new target after death
            StartCoroutine(FindTarget());
        }

        #endregion
    }
}
