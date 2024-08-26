using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class MultiLineInputHandler : MonoBehaviour
{
    public TMP_InputField inputField;
    public int maxCharactersPerLine = 15;

    public List<string> entries = new List<string>();

    public string playerPrefString;

    private void Start()
    {
        inputField.onValueChanged.AddListener(OnValueChanged);

        inputField.lineType = TMP_InputField.LineType.MultiLineNewline;

        RestoreEntries();
    }

    private void OnValueChanged(string text)
    {
        Debug.Log("Called " + this.gameObject.name);
        string[] thisFieldLines = text.Split('\n');
        NamesRelated.Instance.totalLines = 0;

        foreach (TMP_InputField input in NamesRelated.Instance.allInputs)
        {
            string[] lines = input.text.Split('\n');
            int nonEmptyLinesCount = lines.Count(line => !string.IsNullOrWhiteSpace(line));
            NamesRelated.Instance.totalLines += nonEmptyLinesCount;
            Debug.Log("Called1");
        }

        if (NamesRelated.Instance.totalLines > NamesRelated.Instance.maxEntries)
        {
            int excessLines = NamesRelated.Instance.totalLines - NamesRelated.Instance.maxEntries;
            Debug.Log("Excess lines: " + excessLines);

            thisFieldLines = thisFieldLines
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .Take(thisFieldLines.Length - excessLines)
                .ToArray();

            Debug.Log("Called2");
        }

        for (int i = 0; i < thisFieldLines.Length; i++)
        {
            string line = thisFieldLines[i].Trim();

            if (line.Length > maxCharactersPerLine)
            {
                line = line.Substring(0, maxCharactersPerLine);
            }

            if (line.Contains(" "))
            {
                line = line.Replace(" ", ""); // Remove spaces
            }

            thisFieldLines[i] = line;
        }

        string processedText = string.Join("\n", thisFieldLines);
        if (processedText != inputField.text)
        {
            UpdateInputFieldText(processedText);
        }

        NamesRelated.Instance.saveButton.SetActive(true);
    }

    private void UpdateInputFieldText(string newText)
    {
        Debug.Log("Updating Input Field Text: " + newText);
        inputField.text = newText;
    }

    int totalCount = 0;
    public void SaveEntries()
    {
        
        string entriesText = inputField.text;
        PlayerPrefs.SetString(playerPrefString, entriesText);
        PlayerPrefs.Save();
        Debug.Log("Entries Saved: " + entriesText);
        RestoreEntries();
    }
    public void RestoreEntries()
    {
        totalCount = 0;
        if (PlayerPrefs.HasKey(playerPrefString))
        {
            string savedEntries = PlayerPrefs.GetString(playerPrefString);
            Debug.Log("Restored Entries: " + savedEntries);
            entries = new List<string>(savedEntries.Split('\n'));
            entries.RemoveAll(entry => string.IsNullOrEmpty(entry));
            UpdateInputFieldText(savedEntries);
        }

        foreach (MultiLineInputHandler mli in NamesRelated.Instance.allLinesHandlers)
        {
            totalCount += mli.entries.Count;
        }
        NamesRelated.Instance.totalEntries.text = $"Players Count: {totalCount} / {NamesRelated.Instance.maxEntries}";
    }
}
