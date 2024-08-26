using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    public int totalEnemiesToInstantiate, totalAliveCount;
    public List<Faction> factions;
    public List<Transform> enemiesTransforms;
    public List<GameObject> instantiatedEnemies;
    public Material[] allEnemiesMat;

    [Header("UI")]
    public GameObject fightScreen; 
    public GameObject winScreen;
    public TextMeshProUGUI totalAliveText;
    public TextMeshProUGUI winnerNameText;

    public List<GameObject> allWeapons;

    [System.Serializable]
    public class Faction
    {
        public GameObject[] enemyPrefabs;
    }

    private void Awake()
    {
        instance = this;
    }
    int posNo;
    private void Start()
    {
        ShuffleList(enemiesTransforms);
        posNo = 0;
        fightScreen.SetActive(true);
        Invoke(nameof(DisappearFight), 2f);

        InstantiateEnemies(DataContainer.Instance.randomEntries,-1);
        InstantiateEnemies(DataContainer.Instance.divineEntries ,0);
        InstantiateEnemies(DataContainer.Instance.rootEntries,1);
        InstantiateEnemies(DataContainer.Instance.paragonEntries, 2);
        InstantiateEnemies(DataContainer.Instance.ordinamEntries, 3);

        totalEnemiesToInstantiate = instantiatedEnemies.Count;
        totalAliveCount = instantiatedEnemies.Count;
        totalAliveText.text = "Total Alive Count: " + totalAliveCount + " / " + totalEnemiesToInstantiate;
    }

    void ShuffleList(List<Transform> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            Transform value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    private void InstantiateEnemies(List<string> entries,int factionNo)
    {
        int count = entries.Count;
        int randFaction = 0;
        int selectedCharacter=0;
        for (int i = 0; i < count; i++)
        {
            if (factionNo == -1)
            {
                randFaction = Random.Range(0, factions.Count);
                int enemyNo = Random.Range(0, factions[randFaction].enemyPrefabs.Length);
                selectedCharacter = enemyNo;
            }
            else
            {
                if (factionNo == 0)
                {
                    selectedCharacter = i%2;
                }
                else{
                    randFaction = factionNo;
                    selectedCharacter = Random.Range(0, factions[randFaction].enemyPrefabs.Length);
                }
            }
            GameObject _enemy = Instantiate(factions[randFaction].enemyPrefabs[selectedCharacter]);
            _enemy.GetComponent<EnemyCustomizations>().enemyName = entries[i];
            switch (randFaction)
            {
                case 0:
                _enemy.GetComponent<EnemyCustomizations>().factionName = "Divine Wind";
                    break;
                case 1:
                    _enemy.GetComponent<EnemyCustomizations>().factionName = "Root Prime";
                    break;
                case 2:
                    _enemy.GetComponent<EnemyCustomizations>().factionName = "Project Paragon";
                    break;
                case 3:
                    _enemy.GetComponent<EnemyCustomizations>().factionName = "Ordinem";
                    break;
            }

            _enemy.transform.position = enemiesTransforms[posNo].transform.position;
            _enemy.transform.rotation = enemiesTransforms[posNo].transform.rotation;
            instantiatedEnemies.Add(_enemy);
            _enemy.SetActive(true);
            posNo++;
        }
    }

    void DisappearFight()
    {
        fightScreen.SetActive(false);
    }
    public void MainMenu()
    {
        SceneManager.LoadScene(0);
    }
    public void Restart()
    {
        SceneManager.LoadScene(1);
    }

    //GameObject foundObj;
    public void CheckWinner()
    {
        totalAliveCount = instantiatedEnemies.Count;
        totalAliveText.text = "Total Alive Count: " + totalAliveCount + " / " + totalEnemiesToInstantiate;

        if (totalAliveCount == 1)
        {
            //winnerNameText.text=""
            winScreen.SetActive(true);

            //foreach(GameObject obj in instantiatedEnemies)
            //{
            //    if (obj.activeInHierarchy)
            //    {
            //        foundObj = obj;
            //        break;
            //    }
            //}
            winnerNameText.text = instantiatedEnemies[0].GetComponent<EnemyCustomizations>().enemyName + 
                " from " + instantiatedEnemies[0].GetComponent<EnemyCustomizations>().factionName;
        }
    }
}
