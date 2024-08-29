using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;
using UnityEngine.SceneManagement;
using System.Drawing;

public class BattleManager : MonoBehaviour
{
    public List<Faction> factions;
    public List<Transform> enemiesTransforms;

    public Transform arenaCenter;
    public float arenaSize;
    public GameObject victoryScreen;
    public Text victoryText;
    public Button returnToMenuButton,backBtn;
    public Button replayButton;
    //public CameraController cameraController;
    public NameTagManager nameTagManager;

    public Image countdownImage;
    public Image fightImage;
    public Sprite[] countdownSprites;
    //public Sprite fightSprite;

    public float initialHealth = 100f;
    private List<GameObject> bots = new List<GameObject>();
    private bool battleOngoing = false;
    private string[] factionNames;
    private string randomName;
    [System.Serializable]
    public class Faction
    {
        public GameObject[] enemyPrefabs;
    }
    public static BattleManager Instance { get; private set; }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        factionNames = PlayerPrefsX.GetStringArray("Faction_Names");
        randomName = PlayerPrefs.GetString("Random_Name", "");

        returnToMenuButton.onClick.AddListener(ReturnToMenu);
        backBtn.onClick.AddListener(ReturnToMenu);
        replayButton.onClick.AddListener(ReplayBattle);

        StartBattle();
    }

    void Update()
    {
        if (battleOngoing && bots.Count > 1)
        {
            bots.RemoveAll(bot => bot == null || bot.GetComponent<vCharacter>().isDead);

            if (bots.Count == 1 && finished==false)
            {
                EndBattle(false); // Single winner
            }
            else if (bots.Count == 0 && finished == false)
            {
                EndBattle(true); // Draw scenario
            }
        }
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

    int totalBotCount;
    void StartBattle()
    {
        botCounter = 0;
        ShuffleList(enemiesTransforms);
        totalBotCount = DataContainer.Instance.randomEntries.Count +
           DataContainer.Instance.divineEntries.Count +
           DataContainer.Instance.rootEntries.Count +
           DataContainer.Instance.paragonEntries.Count +
           DataContainer.Instance.ordinamEntries.Count;
        //SpawnBots(factionNames, randomName);
        InstantiateEnemies(DataContainer.Instance.randomEntries, -1);
        InstantiateEnemies(DataContainer.Instance.divineEntries, 0);
        InstantiateEnemies(DataContainer.Instance.rootEntries, 1);
        InstantiateEnemies(DataContainer.Instance.paragonEntries, 2);
        InstantiateEnemies(DataContainer.Instance.ordinamEntries, 3);
        battleOngoing = false;
       
        victoryScreen.SetActive(false);
        StartCoroutine(CountdownAndStartBattle());
    }

    int botCounter;
    private void InstantiateEnemies(List<string> entries, int factionNo)
    {
        int count = entries.Count;
        int randFaction = 0;
        int selectedCharacter = 0;
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
                    selectedCharacter = i % 2;
                }
                else
                {
                    randFaction = factionNo;
                    selectedCharacter = Random.Range(0, factions[randFaction].enemyPrefabs.Length);
                }
            }
            GameObject _enemy = Instantiate(factions[randFaction].enemyPrefabs[selectedCharacter]);
            
            _enemy.GetComponent<BotStats>().name = entries[i];
            switch (randFaction)
            {
                case 0:
                    _enemy.GetComponent<BotStats>().factionName = "Divine Wind";
                    break;
                case 1:
                    _enemy.GetComponent<BotStats>().factionName = "Root Prime";
                    break;
                case 2:
                    _enemy.GetComponent<BotStats>().factionName = "Project Paragon";
                    break;
                case 3:
                    _enemy.GetComponent<BotStats>().factionName = "Ordinem";
                    break;
            }

            Debug.Log(totalBotCount);
            int numClusters = Mathf.CeilToInt(totalBotCount / 10f);
            float clusterSize = arenaSize / Mathf.Sqrt(numClusters);
            int clusterIndex = botCounter % numClusters;
            Vector3 spawnPosition = GetRandomPositionInCluster(clusterIndex, clusterSize, numClusters);
            _enemy.transform.position = spawnPosition;
            _enemy.transform.rotation = enemiesTransforms[botCounter].transform.rotation;
            //instantiatedEnemies.Add(_enemy);
            bots.Add(_enemy);
            InitializeBot(_enemy, entries[i], clusterIndex);
            _enemy.SetActive(true);
            botCounter++;
        }
    }

    IEnumerator CountdownAndStartBattle()
    {
        if (countdownImage != null) countdownImage.gameObject.SetActive(false);
        if (fightImage != null) fightImage.gameObject.SetActive(false);

        SetBotsActive(false);

        for (int i = 0; i < countdownSprites.Length; i++)
        {
            countdownImage.sprite = countdownSprites[i];
            countdownImage.gameObject.SetActive(true);
            yield return new WaitForSeconds(1f);
        }

        countdownImage.gameObject.SetActive(false);
        //fightImage.sprite = fightSprite;
        fightImage.gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);

        fightImage.gameObject.SetActive(false);
        battleOngoing = true;

        SetBotsActive(true);
    }

    void SetBotsActive(bool active)
    {
        foreach (var bot in bots)
        {
            var aiController = bot.GetComponent<vSimpleMeleeAI_Controller>();
            if (aiController != null)
            {
                aiController.enabled = active;
            }
        }
    }

    public List<CharactersData> allPrefabs;
    [System.Serializable]
    public class CharactersData
    {
        public GameObject characterPrefab;
        public string factionName;

        public CharactersData(GameObject allCharacters,string factionName)
        {
            this.characterPrefab = allCharacters;
            this.factionName = factionName;
        }
    }
    void InitializeBot(GameObject bot, string name,int clusterIndex)
    {
        vSimpleMeleeAI_Controller aiController = bot.GetComponent<vSimpleMeleeAI_Controller>();
        if (aiController != null)
        {
            //aiController.clusterID = Random.Range(0,6);
            aiController.clusterID = clusterIndex;
            aiController.enabled = false;
        }

        //cameraController.AddTarget(bot.transform);

        BotStats botStats = bot.GetComponent<BotStats>();
        if (botStats == null)
        {
            botStats = bot.AddComponent<BotStats>();
        }
        botStats.botName = name;
        botStats.nameTagObject = nameTagManager.CreateNameTag(bot, name);
    }
    bool finished;
    void EndBattle(bool isDraw)
    {
        finished = true;
        StartCoroutine(VictoryCo(isDraw));
    }
    IEnumerator VictoryCo(bool isDraw)
    {
        yield return new WaitForSeconds(4.0f);
        battleOngoing = false;
        victoryScreen.SetActive(true);

        if (isDraw)
        {
            victoryText.text = "Draw! Both bots eliminated each other.";
        }
        else if (bots.Count > 0 && bots[0] != null)
        {
            GameObject winner = bots[0];
            BotStats winnerStats = winner.GetComponent<BotStats>();
            if (winnerStats != null)
            {
                string winnerName = winner.name;
                string winnerFaction = winnerStats.factionName;
                int winnerKills = winnerStats.kills;

                VictoryScreen victoryScreenComponent = victoryScreen.GetComponent<VictoryScreen>();
                if (victoryScreenComponent != null)
                {
                    victoryScreenComponent.ShowVictory(winnerName, winnerFaction, winnerKills);
                }
            }
            else
            {
                //Debug.LogError("Winner does not have BotStats component.");
                victoryText.text = "Victory! Last One standing.";
            }
        }
        else
        {
            //Debug.LogError("No valid winner found.");
            victoryText.text = "Victory! No bots remaining.";
        }
    }

    void ReturnToMenu()
    {
        foreach (GameObject bot in bots)
        {
            Destroy(bot);
        }
        bots.Clear();
        victoryScreen.SetActive(false);
        //cameraController.targets.Clear();
        SceneManager.LoadScene("MainMenu");
    }

    void ReplayBattle()
    {
        foreach (GameObject bot in bots)
        {
            Destroy(bot);
        }
        bots.Clear();
        victoryScreen.SetActive(false);
        StartBattle();
    }

    Vector3 GetRandomPositionInCluster(int clusterIndex, float clusterSize, int numClusters)
    {
        float halfSize = clusterSize / 2.5f;
        float clustersPerRow = Mathf.Ceil(Mathf.Sqrt(numClusters));

        float clusterSpacing = clusterSize * 2.5f;

        Vector3 clusterCenter = arenaCenter.position + new Vector3(
            (clusterIndex % clustersPerRow) * clusterSpacing - (clustersPerRow * clusterSpacing / 2.5f),
            0,
            (clusterIndex / clustersPerRow) * clusterSpacing - (clustersPerRow * clusterSpacing / 2.5f)
        );

        Vector3 randomPoint = new Vector3(
            Random.Range(-halfSize, halfSize),
            0,
            Random.Range(-halfSize, halfSize)
        );

        randomPoint += clusterCenter;

        UnityEngine.AI.NavMeshHit hit;
        if (UnityEngine.AI.NavMesh.SamplePosition(randomPoint, out hit, clusterSize, UnityEngine.AI.NavMesh.AllAreas))
        {
            randomPoint = hit.position;
        }
        else
        {
            Debug.LogWarning("Failed to find a valid position on the NavMesh, using cluster center as fallback.");
            randomPoint = clusterCenter;
        }

        return randomPoint;
    }
}
