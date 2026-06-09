using System;
using UnityEngine;

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager Instance { get; private set; }
    public GamePhase CurrentPhase { get; private set; }

    [Tooltip("每個 Phase 的持續秒數，順序對應 GamePhase enum")]
    public float[] PhaseDurations = { 5f, 5f, 5f };

    [Tooltip("最後一個可玩階段；此階段結束後直接進 Result（目前無 Boss/Transition，預設 Phase4）")]
    public GamePhase LastPhase = GamePhase.Phase4;

    public static event Action<GamePhase, GamePhase> OnPhaseChanged;

    bool _isPlaying = false;
    float _timer = 0f;

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

    void Update()
    {
        if (!_isPlaying) return;

        _timer -= Time.deltaTime;
        if (_timer <= 0f)
            NextPhase();
    }

    void HandleStateChanged(GameState from, GameState to)
    {
        if (to == GameState.Playing)
        {
            _isPlaying = true;
            EnterPhase(GamePhase.Phase1);
        }
        else
        {
            _isPlaying = false;
        }
    }

    public void EnterPhase(GamePhase phase)
    {
        var prev = CurrentPhase;
        CurrentPhase = phase;
        int i = (int)phase;
        _timer = i < PhaseDurations.Length ? PhaseDurations[i] : 5f;
        OnPhaseChanged?.Invoke(prev, phase);
    }

    public void NextPhase()
    {
        // 到達最後可玩階段就直接結束（跳過 Boss / Transition）
        if (CurrentPhase >= LastPhase)
        {
            GameStateManager.Instance.Transition(GameState.Result);
            return;
        }
        EnterPhase((GamePhase)((int)CurrentPhase + 1));
    }
}
