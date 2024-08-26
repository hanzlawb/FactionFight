using UnityEngine;
using System.Collections.Generic;

public enum ObjectiveType
{
    Dialogue,
    Fetch,
    Combat,
    ReachPoint,
    Tutorial,
    SpawnMobs,
    Crafting,
    BossBattle
}

[CreateAssetMenu(fileName = "Objective", menuName = "Game/Objective")]
public class ObjectiveSO : ScriptableObject
{
    public ObjectiveType type;
    public string npcName;
    public int quantity;
    public GameObject mobPrefab;
    public Vector3 targetPoint;
    public List<string> tutorialSteps;
    public List<Vector3> checkpoints;
    public GameObject bossPrefab;
}
