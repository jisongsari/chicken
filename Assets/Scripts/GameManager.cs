using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject gameStart;
    public GameObject gameUI;
    public Move player;
    public EnemySpawner enemySpawner;

    void Awake()
    {
        if (gameStart != null)
        {
            gameStart.SetActive(true);
        }

        if (gameUI != null)
        {
            gameUI.SetActive(false);
        }

        if (player != null)
        {
            player.SetGameActive(false);
        }

        if (enemySpawner != null)
        {
            enemySpawner.enabled = false;
        }
    }

    public void StartGame()
    {
        if (gameStart != null)
        {
            gameStart.SetActive(false);
        }

        if (gameUI != null)
        {
            gameUI.SetActive(true);
        }

        if (player != null)
        {
            player.SetGameActive(true);
        }

        if (enemySpawner != null)
        {
            enemySpawner.enabled = true;
        }
    }
}
