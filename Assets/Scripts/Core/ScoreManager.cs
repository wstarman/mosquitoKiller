using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }
    public int Score { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState from, GameState to)
    {
        if (to == GameState.Playing) Score = 0;
    }

    public void Add(int points) => Score += points;
}
