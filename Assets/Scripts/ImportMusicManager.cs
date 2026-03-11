using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SFB;

public class ImportMusicManager : MonoBehaviour
{
    public AudioSource musicFile;

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ImportSong);
    }

    void ImportSong()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Import Music",
            "",
            new[] { new ExtensionFilter("Audio Files", "mp3", "wav") },
            false
        );
        if (paths.Length > 0)
            StoreSong(paths[0]);
    }

    void StoreSong(string sourcePath)
    {
        //make folder for song
        string songsRoot = Path.Combine(Application.persistentDataPath, "Songs");
        Directory.CreateDirectory(songsRoot);

        //create file path for song and add song to folder
        string fileName = Path.GetFileNameWithoutExtension(sourcePath);
        string songFolder = Path.Combine(songsRoot, fileName);
        Directory.CreateDirectory(songFolder);

        //copy audio file to song folder
        string ext = Path.GetExtension(sourcePath);
        string destPath = Path.Combine(songFolder, fileName + ext);

        if (!File.Exists(destPath))
            File.Copy(sourcePath, destPath);
            //TODO: Retrieve audio file and ran through an audio analyzer to get BPM and other info, then save that info to a file in the song folder
            AudioConverter converter = gameObject.AddComponent<AudioConverter>();
            converter.audioSource = musicFile;
            converter.ConvertAudio();

        StartCoroutine(LoadAudio(destPath));
    }

    IEnumerator LoadAudio(string path)
    {
        using var req = UnityWebRequestMultimedia.GetAudioClip(
            "file://" + path,
            GetAudioType(path)
        );

        yield return req.SendWebRequest();

        if (req.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(req.error);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(req);
        musicFile.clip = clip;
        musicFile.Play();
    }

    AudioType GetAudioType(string path)
    {
        if (path.EndsWith(".mp3")) return AudioType.MPEG;
        if (path.EndsWith(".wav")) return AudioType.WAV;
        return AudioType.UNKNOWN;
    }
}