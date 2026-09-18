using UnityEngine;
using UnityEngine.SceneManagement;
using System;

/// <summary>
/// Oyunun genel akışını yönetir: skor, hız, oyun durumu (state), coin sayısı.
/// Singleton pattern kullanılarak her yerden erişilebilir hale getirildi.
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public enum GameState { Menu, Playing, Paused, GameOver }
    public GameState CurrentState { get; private set; } = GameState.Menu;

    [Header("Skor Ayarları")]
    public int score = 0;
    public int coins = 0;
    public float distance = 0f;
    public float scorePerMeter = 1f;

    [Header("Hız Ayarları")]
    public float baseSpeed = 8f;
    public float currentSpeed;
    public float speedIncreaseRate = 0.15f; // saniyede hız artışı
    public float maxSpeed = 25f;

    // UI ve diğer sistemlere haber vermek için event'ler
    public event Action<int> OnScoreChanged;
    public event Action<int> OnCoinsChanged;
    public event Action OnGameOver;
    public event Action OnGameStart;

    private void Awake()
    {
        // Singleton kurulumu
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Update()
    {
        if (CurrentState != GameState.Playing) return;

        // Zamanla hız artışı (Subway Surfers tarzı zorluk eğrisi)
        currentSpeed = Mathf.Min(baseSpeed + (distance * speedIncreaseRate * 0.01f), maxSpeed);

        // Mesafeye göre otomatik skor artışı
        distance += currentSpeed * Time.deltaTime;
        int newScore = Mathf.FloorToInt(distance * scorePerMeter);
        if (newScore != score)
        {
            score = newScore;
            OnScoreChanged?.Invoke(score);
        }
    }

    public void StartGame()
    {
        score = 0;
        coins = 0;
        distance = 0f;
        currentSpeed = baseSpeed;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
        OnGameStart?.Invoke();
    }

    public void AddCoin(int amount = 1)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public void PauseGame()
    {
        if (CurrentState != GameState.Playing) return;
        CurrentState = GameState.Paused;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        if (CurrentState != GameState.Paused) return;
        CurrentState = GameState.Playing;
        Time.timeScale = 1f;
    }

    public void GameOver()
    {
        if (CurrentState == GameState.GameOver) return;
        CurrentState = GameState.GameOver;
        Time.timeScale = 0f;

        // Yüksek skor kaydı
        int highScore = PlayerPrefs.GetInt("HighScore", 0);
        if (score > highScore)
        {
            PlayerPrefs.SetInt("HighScore", score);
            PlayerPrefs.Save();
        }

        OnGameOver?.Invoke();
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
