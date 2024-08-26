using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataContainer : MonoBehaviour
{
    public static DataContainer Instance;
    private void Awake()
    {
        DataContainer[] obj = FindObjectsOfType<DataContainer>();

        if (obj.Length > 1)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    public List<string> randomEntries,divineEntries,rootEntries,paragonEntries,ordinamEntries;
}
