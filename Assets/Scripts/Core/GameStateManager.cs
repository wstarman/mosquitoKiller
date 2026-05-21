using System;
using UnityEngine;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }
    public GameState CurrentState { get; private set; } = GameState.MainMenu;

    public static event Action<GameState, GameState> OnStateChanged;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        // 通知所有 Panel 初始狀態
        OnStateChanged?.Invoke(GameState.MainMenu, GameState.MainMenu);
    }

    public void Transition(GameState next)
    {
        if (next == CurrentState) return;
        var prev = CurrentState;
        CurrentState = next;
        OnStateChanged?.Invoke(prev, next);
    }
}
