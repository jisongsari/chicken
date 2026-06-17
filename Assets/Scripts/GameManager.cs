using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
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

    private float remainingTime;
    private int currentHealth;
    private int currentChickenCount;
    private int currentChickenLegSkillCount;
    private bool gameEnded;

    public int MaxChickenLegSkillCount => maxChickenLegSkillCount;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
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

        remainingTime -= Time.deltaTime;

        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            UpdateTimeUI();
            EndGame(false);
            return;
        }

        UpdateTimeUI();
    }

    public bool TryShoot()
    {
        if (currentChickenCount <= 0) return false;
        currentChickenCount--;
        UpdateChickenCountUI();
        return true;
    }

    // Returns the chicken leg array index used, or -1 if none available
    public int TryUseChickenLegSkill()
    {
        if (currentChickenLegSkillCount <= 0) return -1;
        int index = maxChickenLegSkillCount - currentChickenLegSkillCount;
        currentChickenLegSkillCount--;
        return index;
    }

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

    public void OnGoalReached()
    {
        EndGame(true);
    }

    public void GameOver()
    {
        EndGame(false);
    }

    private void EndGame(bool success)
    {
        if (gameEnded) return;
        gameEnded = true;

        int score = success ? CalculateScore() : 0;
        PlayerPrefs.SetInt("CurrentScore", score);

        if (success)
        {
            int bestScore = PlayerPrefs.GetInt("BestScore", 0);
            if (score > bestScore)
            {
                PlayerPrefs.SetInt("BestScore", score);
            }
        }

        PlayerPrefs.Save();
        SceneManager.LoadScene("GameStart");
    }

    private int CalculateScore()
    {
        // 남은 시간 + 남은 치킨 수 * 2 + 남은 하트 수 * 3 + 남은 닭다리 수 * 5
        return (int)remainingTime
            + currentChickenCount * 2
            + currentHealth * 3
            + currentChickenLegSkillCount * 5;
    }

    private void UpdateHealthUI()
    {
        string hearts = "";
        for (int i = 0; i < currentHealth; i++) hearts += "♥";
        healthText.text = hearts;
    }

    private void UpdateChickenCountUI()
    {
        chickenCountText.text = "남은 치킨 : " + currentChickenCount;
    }

    private void UpdateTimeUI()
    {
        timeText.text = "남은 시간 : " + remainingTime.ToString("F1") + "s";
    }
}
