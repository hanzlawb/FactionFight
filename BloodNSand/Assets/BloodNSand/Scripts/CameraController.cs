using System.Collections.Generic;
using UnityEngine;
using Invector.vCharacterController;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public List<Transform> targets = new List<Transform>();
    public float distance = 10.0f;
    public float xSpeed = 75.0f; // Reduced sensitivity for mouse
    public float ySpeed = 37.5f;  // Reduced sensitivity for mouse
    public float touchSensitivityMultiplier = 0.3f; // Further reduced sensitivity for touch
    public float yMinLimit = 5f; // Set a small positive value to ensure it stays above the ground
    public float yMaxLimit = 80f;
    public float distanceMin = 3f;
    public float distanceMax = 15f;

    private int currentTargetIndex = 0;
    private float x = 0.0f;
    private float y = 0.0f;
    private float pinchZoomSpeed = 0.1f;

    private Vector2 touchStartPos;
    private float minSwipeDistance = 50f; // Minimum swipe distance to detect a swipe

    void Start()
    {
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        if (targets.Count > 0)
        {
            target = targets[currentTargetIndex];
        }
    }

    void Update()
    {
        if (targets.Count == 0) return;

        // Cycle through targets with arrow keys
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CycleTargets(-1);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CycleTargets(1);
        }

        // Handle touch input for cycling targets
        HandleTouchInputForCyclingTargets();

        // Check if the target is dead and switch to the closest alive AI
        if (target == null || (target.GetComponent<vCharacter>() != null && target.GetComponent<vCharacter>().isDead))
        {
            targets.Remove(target);
            if (targets.Count > 0)
            {
                target = FindClosestTarget();
                currentTargetIndex = targets.IndexOf(target);
            }
            else
            {
                target = null;
            }
        }

        // Rotate around the target with mouse or touch
        if (target && (Input.GetMouseButton(1) || Input.touchCount == 1))
        {
            if (Input.GetMouseButton(1))
            {
                x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
                y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
            }
            else if (Input.touchCount == 1)
            {
                Touch touch = Input.GetTouch(0);
                x += touch.deltaPosition.x * xSpeed * distance * 0.02f * touchSensitivityMultiplier;
                y -= touch.deltaPosition.y * ySpeed * 0.02f * touchSensitivityMultiplier;
            }

            y = ClampAngle(y, yMinLimit, yMaxLimit);
        }

        // Zoom in and out with the mouse wheel or pinch zoom
        if (Input.mouseScrollDelta.y != 0)
        {
            distance = Mathf.Clamp(distance - Input.mouseScrollDelta.y * 5, distanceMin, distanceMax);
        }
        else
        {
            HandlePinchZoom();
        }
    }

    void LateUpdate()
    {
        if (target)
        {
            Quaternion rotation = Quaternion.Euler(y, x, 0);
            Vector3 position = rotation * new Vector3(0.0f, 0.0f, -distance) + target.position;

            // Ensure camera stays above the target's height
            if (position.y < target.position.y + 1f)
            {
                position.y = target.position.y + 1f;
            }

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    Transform FindClosestTarget()
    {
        Transform closest = null;
        float closestDistance = Mathf.Infinity;
        Vector3 currentPosition = transform.position;
        foreach (Transform potentialTarget in targets)
        {
            if (potentialTarget == null || (potentialTarget.GetComponent<vCharacter>() != null && potentialTarget.GetComponent<vCharacter>().isDead)) continue;

            float distanceToTarget = Vector3.Distance(potentialTarget.position, currentPosition);
            if (distanceToTarget < closestDistance)
            {
                closest = potentialTarget;
                closestDistance = distanceToTarget;
            }
        }
        return closest;
    }

    static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    public void AddTarget(Transform newTarget)
    {
        if (newTarget != null && !targets.Contains(newTarget))
        {
            targets.Add(newTarget);
            Debug.Log("Target added: " + newTarget.name);
            if (target == null)
            {
                target = newTarget;
                currentTargetIndex = targets.IndexOf(target);
            }
        }
    }

    public void ClearTargets()
    {
        target = null;
        currentTargetIndex = 0;
        Debug.Log("Targets cleared.");
    }

    void HandlePinchZoom()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            distance = Mathf.Clamp(distance + deltaMagnitudeDiff * pinchZoomSpeed, distanceMin, distanceMax);
        }
    }

    void HandleTouchInputForCyclingTargets()
    {
        if (Input.touchCount == 2)
        {
            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            // Calculate the swipe direction and magnitude for both fingers
            Vector2 touchZeroStartPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOneStartPos = touchOne.position - touchOne.deltaPosition;

            Vector2 touchZeroEndPos = touchZero.position;
            Vector2 touchOneEndPos = touchOne.position;

            Vector2 swipeDirectionZero = touchZeroEndPos - touchZeroStartPos;
            Vector2 swipeDirectionOne = touchOneEndPos - touchOneStartPos;

            // Check if both fingers are swiping in the same direction
            if (Vector2.Dot(swipeDirectionZero.normalized, swipeDirectionOne.normalized) > 0.8f)
            {
                if (Mathf.Abs(swipeDirectionZero.x) > Mathf.Abs(swipeDirectionZero.y) && swipeDirectionZero.magnitude > minSwipeDistance)
                {
                    if (swipeDirectionZero.x > 0)
                    {
                        CycleTargets(1);  // Swipe right: next target
                    }
                    else
                    {
                        CycleTargets(-1);  // Swipe left: previous target
                    }
                }
            }
        }
    }

    void CycleTargets(int direction)
    {
        currentTargetIndex = (currentTargetIndex + direction + targets.Count) % targets.Count;
        target = targets[currentTargetIndex];
    }
}
