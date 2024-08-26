using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TestColorTransit : MonoBehaviour
{
    [Range(0f, 1f)]
    public float range; 
    void Start()
    {
        
    }

    private void Update()
    {
        Color newColor = Color.Lerp(Color.red, Color.white, range);

        // Apply the new color to the material of the target renderer
        this.GetComponent<Image>().color = newColor;
        //imageHealth.fillAmount = enemyAI.CurrentHealth / enemyAI.StartingHealth;
    }
}
