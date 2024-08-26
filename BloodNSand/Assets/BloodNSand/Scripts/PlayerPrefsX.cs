using UnityEngine;

public static class PlayerPrefsX
{
    public static void SetStringArray(string key, string[] stringArray)
    {
        PlayerPrefs.SetInt(key + "_Count", stringArray.Length);
        for (int i = 0; i < stringArray.Length; i++)
        {
            PlayerPrefs.SetString(key + "_" + i, stringArray[i]);
        }
    }

    public static string[] GetStringArray(string key)
    {
        int count = PlayerPrefs.GetInt(key + "_Count");
        string[] stringArray = new string[count];
        for (int i = 0; i < count; i++)
        {
            stringArray[i] = PlayerPrefs.GetString(key + "_" + i);
        }
        return stringArray;
    }
}
