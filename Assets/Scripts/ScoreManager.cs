using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI comboText;
    private int score;
    private int combo;
    public void notehit(int points)
    {
        score += points;
        combo++;
        updateText();
    }
    public void notemiss()
    {
        combo = 0;
        updateText();
    }
    void updateText()
    {
        scoreText.text = "Score: " + score;
        comboText.text = "Combo: " + combo;
    }
    
}
