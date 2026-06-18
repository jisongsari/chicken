using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// 게임 시작 화면 클래스: 현재 점수와 최고 점수를 표시하고 게임 시작 버튼을 처리
public class GameStart : MonoBehaviour
{
    // 현재 점수, 최고 점수 텍스트 UI 참조
    public Text currentScoreText;
    public Text bestScoreText;
// 게임 시작 시 호출 - 현재 점수와 최고 점수 표시
    void Start()
    {
        int currentScore = PlayerPrefs.GetInt("CurrentScore", 0);
        int bestScore = PlayerPrefs.GetInt("BestScore", 0);

        currentScoreText.text = currentScore.ToString();
        bestScoreText.text = bestScore.ToString();
    }
// 게임 시작 버튼 클릭 시 호출 - 메인 씬으로 전환
    public void GoToMainScene()
    {
        SceneManager.LoadScene("Main");
    }
}
