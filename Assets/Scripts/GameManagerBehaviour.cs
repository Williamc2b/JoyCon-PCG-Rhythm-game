using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public string selectedMapPath;
    public AudioClip selectedMusicClip;

    void Awake()
    {
        if (Instance == null)//singleton pattern to pass data between scenes
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}