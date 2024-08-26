using EmeraldAI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EnemyCustomizations : MonoBehaviour
{
    public EmeraldAISystem enemyAI;
    public string enemyName,factionName;
    public Image imageHealth;
    public TextMeshProUGUI enemyNameText;
    public float currentHealth, totalHealth;
    public List<GameObject> weapons;
    private void Start()
    {
        //enemyAI=this.GetComponent<EmeraldAISystem>();
        enemyNameText.text = enemyName;
        foreach(GameObject weapon in weapons)
        {
            weapon.SetActive(false);
        }
        weapons[Random.Range(0, weapons.Count)].SetActive(true);
    }

    float n;
    private void Update()
    {
        currentHealth = enemyAI.CurrentHealth;
        totalHealth = enemyAI.StartingHealth;

        n = (float)((float)enemyAI.CurrentHealth / (float)enemyAI.StartingHealth);
        

        Color newColor = Color.Lerp(Color.red,Color.white,n );

        // Apply the new color to the material of the target renderer
        enemyNameText.color = newColor;
        //imageHealth.fillAmount = enemyAI.CurrentHealth / enemyAI.StartingHealth;
    }
    public void DisappearBody()
    {
        Invoke(nameof(Disappear), 6f);
    }

    void Disappear()
    {
        GameManager.instance.instantiatedEnemies.Remove(this.gameObject);
        GameManager.instance.CheckWinner();
        this.gameObject.SetActive(false);

        //GameManager.instance.CheckWinner();
    }
}
