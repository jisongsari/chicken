using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// 게임 매니저 클래스: 게임의 상태를 관리하고 UI를 업데이트
public class GameManager : MonoBehaviour
{
    // 싱글톤으로 GameManager를 어디서든 접근 가능하게 설정
    public static GameManager Instance { get; private set; }

    [Header("Game Settings")]
    public float timeLimit = 90f;
    public int maxHealth = 5;
    public int maxChickenCount = 20;
    public int maxChickenLegSkillCount = 2;

    [Header("UI")]
    public Text healthText;
    public Text chickenCountText;
    public Text timeText;

    // 현재 게임 상태
    private float remainingTime;
    private int currentHealth;
    private int currentChickenCount;
    private int currentChickenLegSkillCount;
    private bool gameEnded;

    public int MaxChickenLegSkillCount => maxChickenLegSkillCount;

    void Awake()
    {
        // GameManager가 하나만 존재하도록 설정
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        // 게임 시작 시 초기값 설정
        currentHealth = maxHealth;
        currentChickenCount = maxChickenCount;
        currentChickenLegSkillCount = maxChickenLegSkillCount;
        remainingTime = timeLimit;

        UpdateHealthUI();
        UpdateChickenCountUI();
        UpdateTimeUI();
    }

    void Update()
    {
        if (gameEnded) return;

        // 남은 시간 감소
        remainingTime -= Time.deltaTime;

        // 시간이 모두 지나면 게임 종료
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimeUI();
            EndGame(false);
            return;
        }

        UpdateTimeUI();
    }

    // 치킨 발사 가능 여부 확인
    public bool TryShoot()
    {
        if (currentChickenCount <= 0) return false;

        currentChickenCount--;
        UpdateChickenCountUI();
        return true;
    }

    // 닭다리 스킬 사용 가능 여부 확인
    // 사용한 스킬 슬롯 번호를 반환, 없으면 -1 반환
    public int TryUseChickenLegSkill()
    {
        if (currentChickenLegSkillCount <= 0) return -1;

        int index = maxChickenLegSkillCount - currentChickenLegSkillCount;
        currentChickenLegSkillCount--;

        return index;
    }

    // 플레이어 체력 감소
    public void TakeDamage(int damage)
    {
        if (gameEnded) return;

        currentHealth = Mathf.Max(currentHealth - damage, 0);
        UpdateHealthUI();

        if (currentHealth == 0)
        {
            EndGame(false);
        }
    }

    // 목표 지점 도착 시 성공 처리
    public void OnGoalReached()
    {
        EndGame(true);
    }

    // 외부에서 게임 오버 호출
    public void GameOver()
    {
        EndGame(false);
    }

    // 게임 종료 및 결과 저장
    private void EndGame(bool success)
    {
        if (gameEnded) return;
        gameEnded = true;

        int score = success ? CalculateScore() : 0;

        // 현재 점수 저장
        PlayerPrefs.SetInt("CurrentScore", score);

        // 최고 점수 갱신
        if (success)
        {
            int bestScore = PlayerPrefs.GetInt("BestScore", 0);

            if (score > bestScore)
            {
                PlayerPrefs.SetInt("BestScore", score);
            }
        }

        PlayerPrefs.Save();

        // 시작 화면으로 이동
        SceneManager.LoadScene("GameStart");
    }

    // 남은 자원을 이용해 최종 점수 계산
    private int CalculateScore()
    {
        return (int)remainingTime
            + currentChickenCount * 2
            + currentHealth * 3
            + currentChickenLegSkillCount * 5;
    }

    // 체력 UI 갱신
    private void UpdateHealthUI()
    {
        string hearts = "";

        for (int i = 0; i < currentHealth; i++)
            hearts += "♥";

        healthText.text = hearts;
    }

    // 치킨 개수 UI 갱신
    private void UpdateChickenCountUI()
    {
        chickenCountText.text = "남은 치킨 : " + currentChickenCount;
    }

    // 남은 시간 UI 갱신
    private void UpdateTimeUI()
    {
        timeText.text = "남은 시간 : " + remainingTime.ToString("F1") + "s";
    }
}