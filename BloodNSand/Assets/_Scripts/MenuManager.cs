using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    private void Awake()
    {
        Instance = this;
    }
    public void StartGame()
    {
        DataContainer.Instance.randomEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[0].entries);
        DataContainer.Instance.divineEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[1].entries);
        DataContainer.Instance.rootEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[2].entries);
        DataContainer.Instance.paragonEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[3].entries);
        DataContainer.Instance.ordinamEntries = new List<string>(NamesRelated.Instance.allLinesHandlers[4].entries);

        SceneManager.LoadScene(1);
    }
}
