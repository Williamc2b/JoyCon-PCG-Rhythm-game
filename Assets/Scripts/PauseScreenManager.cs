using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class PauseScreenManager : MonoBehaviour
{
    public Button ResumeButton;
    public Button RestartButton;
    public Button QuitButton;
    public GameObject PauseScreenUI;
    private bool isPaused = false;
    void Start()
    {
        ResumeButton.onClick.AddListener(ResumeGame);
        RestartButton.onClick.AddListener(RestartGame);
        QuitButton.onClick.AddListener(QuitGame);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (PauseScreenUI.activeSelf)
            {
                if(isPaused)
                {
                    ResumeGame();
                }
                else
                {
                    PauseGame();
                }
            }
        }
    }
    void ResumeGame()
    {
        Time.timeScale = 1f;
        PauseScreenUI.SetActive(false);
        isPaused = false;
    }
    void PauseGame()
    {
        Time.timeScale = 0f;
        PauseScreenUI.SetActive(true);
        isPaused = true;
    }
    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    void QuitGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("SongSelect");
    }
}
