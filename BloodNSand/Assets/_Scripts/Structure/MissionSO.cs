using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Mission", menuName = "Game/Mission")]
public class MissionSO : ScriptableObject
{
    public string missionName;
    public string premise;
    public List<ObjectiveSO> objectives;
    public AreaSO area;
    public bool isCompleted;
}
