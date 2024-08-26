using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class APIManager : MonoBehaviour
{
    public static APIManager instance;

    private const string FileName = "all_character_accessories.json";

    public AllCharactersAccessoryData allData;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Start()
    {
        allData = LoadAllCharacterData();
    }

    private void SaveAllCharacterData()
    {
        Debug.Log("Saved to file");
        string json = JsonUtility.ToJson(allData, true);
        string filePath = GetFilePath();
        File.WriteAllText(filePath, json);
    }

    public void SaveCharacterAccessoryData(int characterIndex, AccessoryValues accessoryValues)
    {
        var characterData = allData.characterAccessoryDataList.Find(c => c.characterIndex == characterIndex);

        if (characterData == null)
        {
            characterData = new CharacterAccessoryData { characterIndex = characterIndex };
            allData.characterAccessoryDataList.Add(characterData);
        }

        characterData.accessoryValues = accessoryValues;

        SaveAllCharacterData(); // Save the entire dataset only once
    }

    public CharacterAccessoryData LoadCharacterData(int characterIndex)
    {
        var characterData = allData.characterAccessoryDataList.Find(c => c.characterIndex == characterIndex);

        if (characterData == null)
        {
            // If the character data does not exist, create a new character data with default values
            characterData = new CharacterAccessoryData
            {
                characterIndex = characterIndex,
                accessoryValues = new AccessoryValues
                {
                    helmetValue = -1,
                    gauntletValue = -1,
                    thighpadValue = -1,
                    shoeValue = -1
                }
            };
            allData.characterAccessoryDataList.Add(characterData);
            SaveAllCharacterData();
        }

        return characterData;
    }

    private AllCharactersAccessoryData LoadAllCharacterData()
    {
        string filePath = GetFilePath();
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            return JsonUtility.FromJson<AllCharactersAccessoryData>(json);
        }
        else
        {
            return new AllCharactersAccessoryData();
        }
    }

    private string GetFilePath()
    {
        return Path.Combine(Application.dataPath, FileName);
    }
}

[System.Serializable]
public class CharacterAccessoryData
{
    public int characterIndex;
    public AccessoryValues accessoryValues;
}

[System.Serializable]
public class AllCharactersAccessoryData
{
    public List<CharacterAccessoryData> characterAccessoryDataList = new List<CharacterAccessoryData>();
}

[System.Serializable]
public class AccessoryValues
{
    public int helmetValue;
    public int gauntletValue;
    public int thighpadValue;
    public int shoeValue;
}
