using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PlayButtonBehaviour : MonoBehaviour
{
    public Button EasyButton;
    public Button NormalButton;
    public Button HardButton;
    public Button PlayButton;
    public string[] JSONmapFiles;
    private string mapFolderPath;
    public BeatManager beatManager;
    public AudioClip selectedMusicClip;
    public void setMapFolderPath(string[] path)
    {
        JSONmapFiles = path;
        Debug.Log("Map folder path set to: " + string.Join(", ", JSONmapFiles));
    }
    public void setSelectedMusicClip(AudioClip clip)
    {
        selectedMusicClip = clip;
        Debug.Log("Selected music clip set to: " + selectedMusicClip.name);
    }
    void Start()
    {
        EasyButton.onClick.AddListener(() => SelectDifficulty("Easy"));
        NormalButton.onClick.AddListener(() => SelectDifficulty("Normal"));
        HardButton.onClick.AddListener(() => SelectDifficulty("Hard"));
        PlayButton.onClick.AddListener(PlayGame);
    }
    public void SelectDifficulty(string difficulty)
    {
        switch (difficulty)
        {
            case "Easy":
                mapFolderPath = JSONmapFiles[0];
                break;
            case "Normal":
                mapFolderPath = JSONmapFiles[2];
                break;
            case "Hard":
                mapFolderPath = JSONmapFiles[1];
                break;
        }
        Debug.Log("Map folder path set to: " + mapFolderPath);
    }
    public void PlayGame()
    {
        if (string.IsNullOrEmpty(mapFolderPath))
        {
            Debug.LogWarning("Please select a difficulty before playing.");
            return;
        }
        Debug.Log("Starting game with map folder path: " + mapFolderPath);
        GameManager.Instance.selectedMapPath = mapFolderPath;
        GameManager.Instance.selectedMusicClip = selectedMusicClip;
        SceneManager.LoadScene("MainGameplay");

    }
}
