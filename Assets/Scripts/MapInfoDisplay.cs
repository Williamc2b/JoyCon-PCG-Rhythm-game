using UnityEngine;
using TMPro;
using System.IO;

[System.Serializable]
public class BeatEvent
{
    public float timestamp;
    public BeatType type;
    public float duration;
}

[System.Serializable]
public class BeatType
{
    public string name;
}

[System.Serializable]
public class BeatmapData
{
    public float bpm;
    public string mapName;
    public double mapDuration;
    public BeatEvent[] beatEvents;
}

public class MapInfoDisplay : MonoBehaviour
{
    public TextMeshProUGUI MapStats;

    public double bpm;
    public float duration;
    public PlayButtonBehaviour playButtonBehaviour;

    public void DisplayInfo(string folderName, string folderPath)
    {
        Debug.Log("Displaying info for folder: " + folderPath);

        // Find all JSON files in the folder
        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");
        playButtonBehaviour.setMapFolderPath(jsonFiles); // Pass the JSON files to the PlayButtonBehaviour
        if (jsonFiles.Length == 0)
        {
            Debug.LogError("No map JSON found in folder.");
            return;
        }
        string json = File.ReadAllText(jsonFiles[0]);

        BeatmapData beatmapData = JsonUtility.FromJson<BeatmapData>(json);
        
        beatmapData.mapName = folderName; // Set map name to folder name for display
        bpm = beatmapData.bpm;
        duration = (float)beatmapData.mapDuration;

        int minutes = Mathf.FloorToInt(duration / 60);
        int seconds = Mathf.FloorToInt(duration % 60);

        MapStats.text = $"Map: {beatmapData.mapName}\nBPM: {bpm}\nDuration: {minutes}:{seconds:00}";

        Debug.Log($"Loaded map: {beatmapData.mapName}, BPM: {bpm}, Duration: {duration}");
    }
}