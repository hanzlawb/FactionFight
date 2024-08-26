using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "Chapter", menuName = "Game/Chapter")]
public class ChapterSO : ScriptableObject
{
    public string chapterName;
    public List<MissionSO> missions;
}
