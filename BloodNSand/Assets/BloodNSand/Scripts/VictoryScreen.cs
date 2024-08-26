using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class VictoryScreen : MonoBehaviour
{
    public GameObject victoryPanel;
    public Text victoryText;

    public void ShowVictory(string winnerName, string factionName, int kills)
    {
        victoryPanel.SetActive(true);
        victoryText.text = $"Victory! Last AI standing: {winnerName}\nFaction: {factionName}\nKills: {kills}";
    }


    public void ShowDraw(string bot1Name, string bot2Name)
    {
        victoryPanel.SetActive(true);
        victoryText.text = $"Draw! Both {bot1Name} and {bot2Name} were eliminated.";
    }

    public void Restart()
    {
        SceneManager.LoadScene("BattleScene");
    }

    public void ReturnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
