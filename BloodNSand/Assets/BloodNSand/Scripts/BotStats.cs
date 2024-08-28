using Invector.vCharacterController.AI;
using UnityEngine;

public class BotStats : MonoBehaviour
{
    public int kills = 0;
    public string botName;    // Store the bot's name
    public string factionName; // Store the faction name
    public TextMesh nameTagObject;

    vSimpleMeleeAI_Controller thisAIController;
    float totalHealth,currentHealth;
    private void Start()
    {
        thisAIController=this.GetComponent<vSimpleMeleeAI_Controller>();
        totalHealth = thisAIController.currentHealth;
    }
    public void AddKill()
    {
        kills++;
        Debug.Log($"{botName} has {kills} kills."); // Log using the bot's name
    }

    private void Update()
    {
        currentHealth = thisAIController.currentHealth;

        float n = (float)((float)thisAIController.currentHealth / (float)totalHealth);


        Color newColor = Color.Lerp(Color.red, Color.white, n);

        // Apply the new color to the material of the target renderer
        nameTagObject.color = newColor;
        //imageHealth.fillAmount = enemyAI.CurrentHealth / enemyAI.StartingHealth;
    }
}
