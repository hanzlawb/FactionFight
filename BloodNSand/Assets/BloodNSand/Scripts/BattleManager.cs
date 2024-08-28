using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Invector.vCharacterController;
using Invector.vCharacterController.AI;
using UnityEngine.SceneManagement;

public class BattleManager : MonoBehaviour
{
    public List<GameObject> faction1Prefabs; // Male and female prefabs for faction 1
    public List<GameObject> faction2Prefabs; // Male and female prefabs for faction 2
    public List<GameObject> faction3Prefabs; // Male and female prefabs for faction 3
    public List<GameObject> faction4Prefabs; // Male and female prefabs for faction 4
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

    private List<GameObject> bots = new List<GameObject>();
    private bool battleOngoing = false;
    private string[] factionNames;
    private string randomName;

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

            if (bots.Count == 1)
            {
                EndBattle(false); // Single winner
            }
            else if (bots.Count == 0)
            {
                EndBattle(true); // Draw scenario
            }
        }
    }

    void StartBattle()
    {
        SpawnBots(factionNames, randomName);
        battleOngoing = false;

        victoryScreen.SetActive(false);
        StartCoroutine(CountdownAndStartBattle());
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
    void SpawnBots(string[] factionNames, string randomNames)
    {
        if (faction1Prefabs == null || faction1Prefabs.Count == 0 ||
            faction2Prefabs == null || faction2Prefabs.Count == 0 ||
            faction3Prefabs == null || faction3Prefabs.Count == 0 ||
            faction4Prefabs == null || faction4Prefabs.Count == 0)
        {
            Debug.LogError("Faction Prefabs are not assigned.");
            return;
        }

        allPrefabs = new List<CharactersData>();
        
        foreach(GameObject obj in faction1Prefabs)
        {
            allPrefabs.Add(new CharactersData(obj, "Divine Wind"));
        }
        foreach (GameObject obj in faction1Prefabs)
        {
            allPrefabs.Add(new CharactersData(obj, "Root Prime"));
        }
        foreach (GameObject obj in faction1Prefabs)
        {
            allPrefabs.Add(new CharactersData(obj, "Project Paragon"));
        }
        foreach (GameObject obj in faction1Prefabs)
        {
            allPrefabs.Add(new CharactersData(obj, "Ordinem"));
        }

        int botCounter = 0;
        int totalBotCount = 0;

        // Define the faction names
        string[] predefinedFactionNames = { "Divine Wind", "Root Prime", "Project Paragon", "Ordinem" };

        // Count total number of bots to be spawned for each faction
        foreach (string factionName in factionNames)
        {
            string[] names = factionName.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            totalBotCount += names.Length;
        }

        // Split and count total bots for random field
        string[] randomNamesArray = randomNames.Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
        totalBotCount += randomNamesArray.Length;

        // If no bots are specified, we need to set totalBotCount to 2 manually
        if (totalBotCount == 0)
        {
            totalBotCount = 2;
        }

        int numClusters = Mathf.CeilToInt(totalBotCount / 4f);
        float clusterSize = arenaSize / Mathf.Sqrt(numClusters);

        // Spawn bots for each faction input
        for (int i = 0; i < factionNames.Length; i++)
        {
            string[] names = factionNames[i].Split(new char[] { '\n', '\r' }, System.StringSplitOptions.RemoveEmptyEntries);
            foreach (string name in names)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    GameObject prefab = GetFactionPrefab(i);
                    if (prefab != null)
                    {
                        int clusterIndex = botCounter % numClusters;
                        Vector3 spawnPosition = GetRandomPositionInCluster(clusterIndex, clusterSize, numClusters);
                        GameObject bot = Instantiate(prefab, spawnPosition, Quaternion.identity);
                        bot.name = name.Trim();
                        bots.Add(bot);

                        InitializeBot(bot, name.Trim(), clusterIndex, predefinedFactionNames[i]);
                        botCounter++;
                    }
                }
            }
        }

        // Spawn bots for each name in the random input field
        foreach (string randomName in randomNamesArray)
        {
            if (!string.IsNullOrEmpty(randomName))
            {
                CharactersData randomPrefab = allPrefabs[Random.Range(0, allPrefabs.Count)];
                int clusterIndex = botCounter % numClusters;
                Vector3 spawnPosition = GetRandomPositionInCluster(clusterIndex, clusterSize, numClusters);
                GameObject bot = Instantiate(randomPrefab.characterPrefab, spawnPosition, Quaternion.identity);
                bot.name = randomName.Trim();
                bots.Add(bot);

                InitializeBot(bot, randomName.Trim(), clusterIndex, randomPrefab.factionName); // Placeholder faction name for random bots
                botCounter++;
            }
        }

        //// If no names were provided, spawn two default bots
        //if (bots.Count == 0)
        //{
        //    for (int i = 0; i < 2; i++)
        //    {
        //        CharactersData randomPrefab = allPrefabs[Random.Range(0, allPrefabs.Count)];
        //        int clusterIndex = i % numClusters;
        //        Vector3 spawnPosition = GetRandomPositionInCluster(clusterIndex, clusterSize, numClusters);
        //        GameObject bot = Instantiate(randomPrefab.characterPrefab, spawnPosition, Quaternion.identity);
        //        bot.name = "Bot " + (i + 1);
        //        bots.Add(bot);

        //        InitializeBot(bot, bot.name, clusterIndex, randomPrefab.factionName); // Placeholder faction name for default bots
        //    }
        //}
    }

    GameObject GetFactionPrefab(int factionIndex)
    {
        switch (factionIndex)
        {
            case 0:
                return faction1Prefabs[Random.Range(0, faction1Prefabs.Count)];
            case 1:
                return faction2Prefabs[Random.Range(0, faction2Prefabs.Count)];
            case 2:
                return faction3Prefabs[Random.Range(0, faction3Prefabs.Count)];
            case 3:
                return faction4Prefabs[Random.Range(0, faction4Prefabs.Count)];
            default:
                return null;
        }
    }

    void InitializeBot(GameObject bot, string name, int clusterIndex, string factionName)
    {
        vSimpleMeleeAI_Controller aiController = bot.GetComponent<vSimpleMeleeAI_Controller>();
        if (aiController != null)
        {
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
        botStats.factionName = factionName;

        botStats.nameTagObject= nameTagManager.CreateNameTag(bot, name);
    }

    Vector3 GetRandomPositionInCluster(int clusterIndex, float clusterSize, int numClusters)
    {
        float halfSize = clusterSize / 2f;
        float clustersPerRow = Mathf.Ceil(Mathf.Sqrt(numClusters));

        float clusterSpacing = clusterSize * 2f;

        Vector3 clusterCenter = arenaCenter.position + new Vector3(
            (clusterIndex % clustersPerRow) * clusterSpacing - (clustersPerRow * clusterSpacing / 2f),
            0,
            (clusterIndex / clustersPerRow) * clusterSpacing - (clustersPerRow * clusterSpacing / 2f)
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

    void EndBattle(bool isDraw)
    {
        battleOngoing = false;
        victoryScreen.SetActive(true);

        if (victoryText == null)
        {
            return;
        }

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
                Debug.LogError("Winner does not have BotStats component.");
                victoryText.text = "Victory! Last One standing.";
            }
        }
        else
        {
            Debug.LogError("No valid winner found.");
            victoryText.text = "Victory! No bots remaining.";
        }

        Debug.Log($"UI Text should be: {victoryText.text}");
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
}
