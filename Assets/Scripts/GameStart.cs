using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameStart : MonoBehaviour
{
    public Text currentScoreText;
    public Text bestScoreText;

    void Start()
    {
        int currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        currentScoreText.text = currentScore.ToString();
        bestScoreText.text = bestScore.ToString();
    }

    public void GoToMainScene()
    {
        SceneManager.LoadScene("Main");
    }
}
