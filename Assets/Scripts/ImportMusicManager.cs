using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using SFB;
//https://github.com/quangdungtr/UnityStandaloneFileBrowser
//This script handles importing music files, storing them in a specific folder, and loading them into an AudioSource for playback and conversion. It uses the StandaloneFileBrowser library to allow users to select audio files from their system. Once a file is selected, it is copied to a designated "Songs" folder within the application's persistent data path. The audio file is then loaded into an AudioSource component, and the AudioConverter script is called to process the audio for beatmap generation.
public class ImportMusicManager : MonoBehaviour
{
    public AudioSource musicFile;
    public AudioConverter audioConverter;
    void Start()
    {
        GetComponent<Button>().onClick.AddListener(ImportSong);
    }

    void ImportSong()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel(
            "Import Music",
            "",
            new[] { new ExtensionFilter("Audio Files", "mp3", "wav","ogg") },
            false
        );
        if (paths.Length > 0)
        {
            Debug.Log("Selected file: " + paths[0]);
            StoreSong(paths[0]);
        }

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
        Debug.Log("Copying file from: " + sourcePath + " to: " + destPath);
        if (!File.Exists(destPath))
            File.Copy(sourcePath, destPath);

        StartCoroutine(LoadAudio(destPath,songFolder));
    }

    IEnumerator LoadAudio(string path,string songfolder)
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
        clip.name = Path.GetFileNameWithoutExtension(path);
        audioConverter.audioSource = musicFile;
        audioConverter.songFolder = songfolder;
        audioConverter.ConvertAudio();
        musicFile.Play();
    }

    AudioType GetAudioType(string path)
    {
        if (path.EndsWith(".mp3")) return AudioType.MPEG;
        if (path.EndsWith(".wav")) return AudioType.WAV;
        if (path.EndsWith(".ogg")) return AudioType.OGGVORBIS;
        return AudioType.UNKNOWN;
    }
}  