using UnityEngine;
using TMPro;
using System.IO;
using System.Linq;
using UnityEngine.Networking;
using System.Collections;
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

    AudioType GetAudioType(string path)
    {
        path = path.ToLower();

        if (path.EndsWith(".mp3")) return AudioType.MPEG;
        if (path.EndsWith(".wav")) return AudioType.WAV;
        if (path.EndsWith(".ogg")) return AudioType.OGGVORBIS;

        return AudioType.UNKNOWN;
    }
    public IEnumerator DisplayInfo(string folderName, string folderPath)
    {
        Debug.Log("Displaying info for folder: " + folderPath);

        // Find all JSON files in the folder
        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");
        string audioPath = Directory.GetFiles(folderPath)
        .First(f => f.EndsWith(".mp3") || f.EndsWith(".wav") || f.EndsWith(".ogg"));

        using var req = UnityWebRequestMultimedia.GetAudioClip(
            "file://" + audioPath,
            GetAudioType(audioPath)
        );

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(req);

        playButtonBehaviour.setMapFolderPath(jsonFiles); // Pass the JSON files to the PlayButtonBehaviour
        playButtonBehaviour.setSelectedMusicClip(clip); // Set the selected music clip

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