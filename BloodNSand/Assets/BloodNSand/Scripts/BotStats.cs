using UnityEngine;

public class BotStats : MonoBehaviour
{
    public int kills = 0;
    public string botName;    // Store the bot's name
    public string factionName; // Store the faction name

    public void AddKill()
    {
        kills++;
        Debug.Log($"{botName} has {kills} kills."); // Log using the bot's name
    }
}
