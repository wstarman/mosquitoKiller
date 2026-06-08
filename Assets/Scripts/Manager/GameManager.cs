using System;
using System.Collections;
using System.Collections.Generic;
// using UnityEditor.Experimental.GraphView; // Editor-only namespace，不可進 build，暫時保留以追溯來源
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public Vector3 leftHand = new(-200, 0, 0);
    public Vector3 rightHand = new(200, 0, 0);
    public float distance;
    public bool isHandContact = false;  // 是否兩手碰在一起

    [Header("Playing Mode")]
    public float PlayingClapDistance = 1.5f;
    public float PlayingResetDistance = 2.0f;

    [Header("Cursor Mode")]
    public float CursorClapDistance = 0.3f;
    public float CursorResetDistance = 0.6f;

    [Header("Clap Cooldown")]
    [Tooltip("拍手事件發出後的冷卻時間（秒），防止切換場景時誤觸新按鈕")]
    public float ClapCooldown = 0.125f;

    public static event Action OnHandClap;
    public bool isSkillReleasing = false;

    [Header("Player Health")]
    public int MaxHP = 100;
    public int CurrentHP { get; private set; }
    /// <summary>HP 變動時發出，傳入 0~1 normalized 值，供血條 UI 訂閱（與 EnergyManager 一致）。</summary>
    public static event Action<float> OnHealthChanged;

    float _clapCooldownTimer = 0f;
    System.Random r = new System.Random();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        Init();
    }

    void Init()
    {
        leftHand = new(-2, 0, 0);
        rightHand = new(2, 0, 0);
        InputManager.DetectedSkill += OnSkillDetected;
    }

    void Start()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    private void OnDestroy()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    void Update()
    {
        
    }

    public void UpdateHand()
    {
        distance = (leftHand - rightHand).magnitude;

        if (_clapCooldownTimer > 0f) _clapCooldownTimer -= Time.deltaTime;

        // 轉場期間禁止拍手事件（僅限 Playing 狀態；CurrentPhase 離開 Playing 後不會重置）
        if (GameStateManager.Instance?.CurrentState == GameState.Playing &&
            GamePhaseManager.Instance?.CurrentPhase == GamePhase.Transition) return;

        bool isCursor = GameStateManager.Instance == null ||
                        GameStateManager.Instance.CurrentState != GameState.Playing;
        float clapDist = isCursor ? CursorClapDistance : PlayingClapDistance;
        float resetDist = isCursor ? CursorResetDistance : PlayingResetDistance;

        if (distance < clapDist)
        {
            if (!isHandContact && _clapCooldownTimer <= 0f)
            {
                OnHandClap?.Invoke();
                AudioManager.Instance.PlaySFX($"clap{r.Next(1,6)}");
                _clapCooldownTimer = ClapCooldown;
            }
            isHandContact = true;
        }
        else if (distance > resetDist)
        {
            isHandContact = false;
        }
    }

    void OnSkillDetected(int sId)
    {
        EnergyManager.Instance?.TryUseSkill(sId);
    }
    void HandleStateChanged(GameState from, GameState to)
    {
        if (to == GameState.Playing)
        {
            isSkillReleasing = false;
            CurrentHP = MaxHP;
            OnHealthChanged?.Invoke((float)CurrentHP / MaxHP);
        }
    }

    /// <summary>被叮等來源呼叫，只在 Playing 狀態下有效。歸零時提前進入 Result。</summary>
    public void TakeDamage(int amount)
    {
        if (GameStateManager.Instance?.CurrentState != GameState.Playing) return;
        if (amount <= 0) return;

        CurrentHP = Mathf.Clamp(CurrentHP - amount, 0, MaxHP);
        OnHealthChanged?.Invoke((float)CurrentHP / MaxHP);

        if (CurrentHP <= 0)
            GameStateManager.Instance.Transition(GameState.Result);
    }

    /// <summary>打死敵人等來源呼叫，只在 Playing 狀態下有效。</summary>
    public void Heal(int amount)
    {
        if (GameStateManager.Instance?.CurrentState != GameState.Playing) return;
        if (amount <= 0) return;

        CurrentHP = Mathf.Clamp(CurrentHP + amount, 0, MaxHP);
        OnHealthChanged?.Invoke((float)CurrentHP / MaxHP);
    }
}
