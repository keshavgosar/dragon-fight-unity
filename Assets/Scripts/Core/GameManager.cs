using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>Watches both dragons, shows the Winner Screen and restarts the scene.</summary>
public class GameManager : MonoBehaviour
{
    public static bool IsOver { get; private set; }

    [SerializeField] Health player;
    [SerializeField] Health enemy;
    [SerializeField] string playerName = "Player Dragon";
    [SerializeField] string enemyName = "Enemy Dragon";
    [Header("Winner screen")]
    [SerializeField] GameObject winnerPanel;
    [SerializeField] TMP_Text winnerText;
    [SerializeField] Button restartButton;
    [SerializeField] float slowMoTime = 1.2f;

    void Awake()
    {
        IsOver = false;
        Time.timeScale = 1f;
        winnerPanel.SetActive(false);
        player.Died += OnDied;
        enemy.Died += OnDied;
        restartButton.onClick.AddListener(Restart);
    }

    void OnDestroy()
    {
        if (player) player.Died -= OnDied;
        if (enemy) enemy.Died -= OnDied;
    }

    void Update()
    {
        if (IsOver && Input.GetKeyDown(KeyCode.R)) Restart();
    }

    void OnDied(Health loser)
    {
        if (IsOver) return;
        IsOver = true;
        winnerText.text = (loser == player ? enemyName : playerName) + " WINS!";
        StartCoroutine(ShowWinner());
    }

    IEnumerator ShowWinner()
    {
        Time.timeScale = 0.3f; // brief slow-mo on the killing blow
        yield return new WaitForSecondsRealtime(slowMoTime);
        Time.timeScale = 1f;
        winnerPanel.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
