using UnityEngine;

[CreateAssetMenu(fileName = "Area", menuName = "Game/Area")]
public class AreaSO : ScriptableObject
{
    public string areaName;
    public Vector3 spawnPoint;
    public bool isUnlocked;
}
