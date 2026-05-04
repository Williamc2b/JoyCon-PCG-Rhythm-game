using System.IO;
using UnityEngine;
using TMPro;

public class SongIconLoader : MonoBehaviour
{
    public Transform ScrollViewContent; 
    public GameObject SongIconPrefab; 
    public MapInfoDisplay SongInfoDisplay;

    void Start()
    {
        LoadSongs();
    }

    public void LoadSongs()
    {
        foreach (Transform child in ScrollViewContent) // Clear existing icons
        {
            Destroy(child.gameObject);
        }
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
            SongSelectBehaviour selectBehaviour = icon.GetComponent<SongSelectBehaviour>();
            if (selectBehaviour != null)
            {
                selectBehaviour.Initialise(folder, SongInfoDisplay);
                Debug.Log("Initialized SongSelectBehaviour for folder: " + folderName);
            }

            TextMeshProUGUI text = icon.GetComponentInChildren<TextMeshProUGUI>();

            if (text != null)
                text.text = folderName;
        }
    }
}