using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using Invector.vCharacterController.AI;  // Reference to the correct namespace

namespace Invector.vCharacterController
{
    public class vOnDeadTrigger : MonoBehaviour
    {
        public UnityEvent OnDead;
        private bool isDead = false;  // Flag to ensure death logic is only handled once

        void Start()
        {
            vCharacter character = GetComponent<vCharacter>();
            if (character)
            {
                
                character.onDead.AddListener(OnDeadHandle);
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: vCharacter not found.");
            }
        }

        public void OnDeadHandle(GameObject target)
        {
            if (isDead)
            {
                
                return;  // If this bot is already dead, don't process again
            }

            
            isDead = true;  // Mark this bot as dead

            OnDead.Invoke();

            // Determine the killer and increment their kill count
            vDamage lastDamage = target.GetComponent<vCharacter>().lastDamage;
            if (lastDamage != null && lastDamage.sender != null)
            {
                BotStats killerStats = lastDamage.sender.GetComponent<BotStats>();
                if (killerStats != null)
                {
                    
                    killerStats.AddKill();  // Add kill without additional checks
                }
                else
                {
                    Debug.LogWarning($"{lastDamage.sender.name}: BotStats not found on killer.");
                }
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Last damage or sender is null.");
            }

            // Remove the dead AI from all aggro tables
            foreach (vSimpleMeleeAI_Controller ai in FindObjectsOfType<vSimpleMeleeAI_Controller>())
            {
                ai.RemoveAggro(target.transform);
            }

            StartCoroutine(DespawnBot(target));
        }

        IEnumerator DespawnBot(GameObject bot)
        {
            yield return new WaitForSeconds(2f);
            Destroy(bot);
        }
    }


}