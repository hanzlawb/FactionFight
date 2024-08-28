using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class MainMenu : MonoBehaviour
{
    public Canvas mainMenuCanvas;
    public Canvas nameInputCanvas;
    public InputField[] factionInputFields; // Array of input fields for each faction
    public InputField randomInputField; // Input field for random bot
    public Button startGameButton;
    public Button addNamesButton;
    public Button saveNamesButton;
    public Button backButton;
    public Button clearNamesButton;

    void Start()
    {
        mainMenuCanvas.gameObject.SetActive(true);
        nameInputCanvas.gameObject.SetActive(false);

        startGameButton.onClick.AddListener(StartGame);
        addNamesButton.onClick.AddListener(OpenNameInput);
        saveNamesButton.onClick.AddListener(NamesRelated.Instance.CallSave);
        backButton.onClick.AddListener(BackToMainMenu);
        clearNamesButton.onClick.AddListener(NamesRelated.Instance.CallReset);
    }

    void StartGame()
    {
        //List<string> factionNames = new List<string>();
        //foreach (var inputField in factionInputFields)
        //{
        //    if (!string.IsNullOrEmpty(inputField.text))
        //    {
        //        factionNames.Add(inputField.text);
        //    }
        //}

        //string randomName = randomInputField.text;
        //PlayerPrefsX.SetStringArray("Faction_Names", factionNames.ToArray());
        //PlayerPrefs.SetString("Random_Name", randomName);
        DataContainer.Instance.randomEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[0].entries);
        DataContainer.Instance.divineEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[1].entries);
        DataContainer.Instance.rootEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[2].entries);
        DataContainer.Instance.paragonEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[3].entries);
        DataContainer.Instance.ordinamEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[4].entries);

        UnityEngine.SceneManagement.SceneManager.LoadScene("BattleScene");
    }

    void OpenNameInput()
    {
        mainMenuCanvas.gameObject.SetActive(false);
        nameInputCanvas.gameObject.SetActive(true);
    }

    //void SaveNames()
    //{
    //    List<string> factionNames = new List<string>();
    //    foreach (var inputField in factionInputFields)
    //    {
    //        if (!string.IsNullOrEmpty(inputField.text))
    //        {
    //            factionNames.Add(inputField.text);
    //        }
    //    }

    //    string randomName = randomInputField.text;
    //    PlayerPrefsX.SetStringArray("Faction_Names", factionNames.ToArray());
    //    PlayerPrefs.SetString("Random_Name", randomName);

    //    BackToMainMenu();
    //}

    void BackToMainMenu()
    {
        mainMenuCanvas.gameObject.SetActive(true);
        nameInputCanvas.gameObject.SetActive(false);
    }

    //void ClearNames()
    //{
    //    foreach (var inputField in factionInputFields)
    //    {
    //        inputField.text = "";
    //    }
    //    randomInputField.text = "";
    //}
}
