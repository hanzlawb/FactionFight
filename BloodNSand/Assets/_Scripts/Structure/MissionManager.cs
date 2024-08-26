using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Linq;

public class MissionManager : MonoBehaviour
{
    public List<ChapterSO> chapters;

    private int currentChapterIndex;
    private int currentMissionIndex;
    private int currentCheckpointIndex;

    private void Start()
    {
        LoadProgress();
        LoadCurrentMission();
    }

    public void LoadChapter(int chapterIndex)
    {
        if (chapterIndex < 0 || chapterIndex >= chapters.Count)
        {
            Debug.LogError("Invalid chapter index");
            return;
        }

        ChapterSO chapter = chapters[chapterIndex];
        foreach (MissionSO mission in chapter.missions)
        {
            Debug.Log("Loading Mission: " + mission.missionName);
            Debug.Log("Premise: " + mission.premise);
            HandleArea(mission.area);
            foreach (ObjectiveSO objective in mission.objectives)
            {
                HandleObjective(objective);
            }
            
        }
    }

    private void HandleArea(AreaSO area)
    {
        if (!area.isUnlocked)
        {
            area.isUnlocked = true;
            Debug.Log("Unlocking Area: " + area.areaName);
        }
        Debug.Log("Setting Spawn Point: " + area.spawnPoint);
        // Set player spawn point logic here
    }

    private void HandleObjective(ObjectiveSO objective)
    {
        switch (objective.type)
        {
            case ObjectiveType.Dialogue:
                HandleDialogueObjective(objective);
                break;
            case ObjectiveType.Fetch:
                HandleFetchObjective(objective);
                break;
            case ObjectiveType.Combat:
                HandleCombatObjective(objective);
                break;
            case ObjectiveType.ReachPoint:
                HandleReachPointObjective(objective);
                break;
            case ObjectiveType.Tutorial:
                HandleTutorialObjective(objective);
                break;
            case ObjectiveType.SpawnMobs:
                HandleSpawnMobsObjective(objective);
                break;
            case ObjectiveType.Crafting:
                HandleCraftingObjective(objective);
                break;
            case ObjectiveType.BossBattle:
                HandleBossBattleObjective(objective);
                break;
            default:
                Debug.LogWarning("Unknown objective type: " + objective.type);
                break;
        }
    }

    private void HandleDialogueObjective(ObjectiveSO objective)
    {
        Debug.Log("Starting dialogue with " + objective.npcName);
        // Dialogue logic here
    }

    private void HandleFetchObjective(ObjectiveSO objective)
    {
        Debug.Log("Fetching " + objective.quantity + " items for " + objective.npcName);
        // Fetch logic here
    }

    private void HandleCombatObjective(ObjectiveSO objective)
    {
        Debug.Log("Starting combat against mobs");
        // Combat logic here
    }

    private void HandleReachPointObjective(ObjectiveSO objective)
    {
        Debug.Log("Reaching point at " + objective.targetPoint);
        // Reach point logic here
    }

    private void HandleTutorialObjective(ObjectiveSO objective)
    {
        Debug.Log("Starting tutorial with steps: " + string.Join(", ", objective.tutorialSteps));
        currentCheckpointIndex = 0;
        ShowTutorialStep(objective);
    }

    private void ShowTutorialStep(ObjectiveSO objective)
    {
        if (currentCheckpointIndex < objective.checkpoints.Count)
        {
            Debug.Log("Showing tutorial step: " + objective.tutorialSteps[currentCheckpointIndex]);
            // Display the current tutorial step
        }
        else
        {
            Debug.Log("Tutorial completed.");
            // Tutorial is completed
        }
    }

    private void HandleSpawnMobsObjective(ObjectiveSO objective)
    {
        Debug.Log("Spawning mobs");
        // Spawn mobs logic here
        for (int i = 0; i < objective.quantity; i++)
        {
            Instantiate(objective.mobPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    private void HandleCraftingObjective(ObjectiveSO objective)
    {
        Debug.Log("Crafting item");
        // Crafting logic here
    }

    private void HandleBossBattleObjective(ObjectiveSO objective)
    {
        Debug.Log("Starting boss battle with " + objective.bossPrefab.name);
        // Boss battle logic here
        Instantiate(objective.bossPrefab, Vector3.zero, Quaternion.identity);
    }

    public void CompleteMission()
    {
        var currentMission = GetCurrentMission();
        if (currentMission != null)
        {
            currentMission.isCompleted = true;
            Debug.Log("Mission completed: " + currentMission.missionName);
            UnlockNextArea(currentMission);
            SaveProgress();
        }
    }

    private void UnlockNextArea(MissionSO mission)
    {
        var nextMission = GetNextMission();
        if (nextMission != null)
        {
            nextMission.area.isUnlocked = true;
            Debug.Log("Unlocking next area: " + nextMission.area.areaName);
        }
    }

    private void SaveProgress()
    {
        var progress = new ProgressData
        {
            currentChapterIndex = currentChapterIndex,
            currentMissionIndex = currentMissionIndex
        };

        string json = JsonUtility.ToJson(progress);
        File.WriteAllText(Application.persistentDataPath + "/progress.json", json);
    }

    private void LoadProgress()
    {
        string path = Application.persistentDataPath + "/progress.json";
        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            var progress = JsonUtility.FromJson<ProgressData>(json);
            currentChapterIndex = progress.currentChapterIndex;
            currentMissionIndex = progress.currentMissionIndex;
        }
        else
        {
            currentChapterIndex = 0;
            currentMissionIndex = 0;
        }
    }

    private void LoadCurrentMission()
    {
        var mission = GetCurrentMission();
        if (mission != null)
        {
            LoadChapter(currentChapterIndex);
            Debug.Log("Loaded Mission: " + mission.missionName);
        }
    }

    private MissionSO GetCurrentMission()
    {
        if (currentChapterIndex >= 0 && currentChapterIndex < chapters.Count)
        {
            var chapter = chapters[currentChapterIndex];
            if (currentMissionIndex >= 0 && currentMissionIndex < chapter.missions.Count)
            {
                return chapter.missions[currentMissionIndex];
            }
        }
        return null;
    }

    private MissionSO GetNextMission()
    {
        if (currentChapterIndex >= 0 && currentChapterIndex < chapters.Count)
        {
            var chapter = chapters[currentChapterIndex];
            if (currentMissionIndex + 1 < chapter.missions.Count)
            {
                return chapter.missions[currentMissionIndex + 1];
            }
            else if (currentChapterIndex + 1 < chapters.Count)
            {
                currentChapterIndex++;
                currentMissionIndex = 0;
                return chapters[currentChapterIndex].missions[currentMissionIndex];
            }
        }
        return null;
    }

    public void ReachCheckpoint(Vector3 checkpoint)
    {
        var currentMission = GetCurrentMission();
        if (currentMission != null)
        {
            var currentObjective = currentMission.objectives.FirstOrDefault(o => o.type == ObjectiveType.Tutorial);
            if (currentObjective != null && currentCheckpointIndex < currentObjective.checkpoints.Count)
            {
                if (currentObjective.checkpoints[currentCheckpointIndex] == checkpoint)
                {
                    currentCheckpointIndex++;
                    ShowTutorialStep(currentObjective);
                    if (currentCheckpointIndex >= currentObjective.checkpoints.Count)
                    {
                        Debug.Log("Tutorial completed.");
                        // Mark the objective as completed or proceed to next mission
                    }
                }
            }
        }
    }

    [System.Serializable]
    private class ProgressData
    {
        public int currentChapterIndex;
        public int currentMissionIndex;
    }
}
