using System.IO;
using UnityEngine;
using TMPro;

public class SongIconLoader : MonoBehaviour
{
    public Transform ScrollViewContent; 
    public GameObject SongIconPrefab; 

    void Start()
    {
        LoadSongs();
    }

    void LoadSongs()
    {
        string path =Path.Combine(Application.persistentDataPath, "Songs");

        if (!Directory.Exists(path))
        {
            Debug.Log("Persistent data path not found.");
            return;
        }

        string[] folders = Directory.GetDirectories(path);

        foreach (string folder in folders)
        {
            Debug.Log("Found folder: " + folder);
            string folderName = Path.GetFileName(folder);

            GameObject icon = Instantiate(SongIconPrefab, ScrollViewContent);

            TextMeshProUGUI text = icon.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
                text.text = folderName;
        }
    }
}