using System;
using UnityEngine;

public class EnergyManager : MonoBehaviour
{
    public static EnergyManager Instance { get; private set; }

    [Header("Settings")]
    public int MaxEP = 100;
    public int InitialEP = 0;
    [Tooltip("各技能 EP 消耗，index 對應 Skill enum（0=None, 1=HadokenLeft, ...）")]
    public int[] SkillCosts = { 0, 30, 30, 50, 100 };

    public int CurrentEP { get; private set; }

    /// <summary>EP 變動時發出，傳入 0~1 normalized 值，供充能條 UI 訂閱。</summary>
    public static event Action<float> OnEnergyChanged;

    /// <summary>技能確認發動（EP 已扣除）時發出，傳入 Skill id，供各技能腳本訂閱。</summary>
    public static event Action<int> OnSkillActivated;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()  => GameStateManager.OnStateChanged += OnStateChanged;
    void OnDisable() => GameStateManager.OnStateChanged -= OnStateChanged;

    void OnStateChanged(GameState from, GameState to)
    {
        if (to == GameState.Playing) ResetEP();
    }

    /// <summary>殺敵、拍手等來源呼叫，只在 Playing 狀態下有效。</summary>
    public void AddEnergy(int amount)
    {
        if (GameStateManager.Instance?.CurrentState != GameState.Playing) return;
        CurrentEP = Mathf.Clamp(CurrentEP + amount, 0, MaxEP);
        OnEnergyChanged?.Invoke((float)CurrentEP / MaxEP);
    }

    /// <summary>由 GameManager 在偵測到技能姿勢時呼叫，EP 足夠才發動並扣除。</summary>
    public bool TryUseSkill(int sId)
    {
        if (sId <= 0 || sId >= SkillCosts.Length) return false;
        if (CurrentEP < SkillCosts[sId]) return false;

        CurrentEP -= SkillCosts[sId];
        OnEnergyChanged?.Invoke((float)CurrentEP / MaxEP);
        OnSkillActivated?.Invoke(sId);
        return true;
    }

    void ResetEP()
    {
        CurrentEP = InitialEP;
        OnEnergyChanged?.Invoke((float)CurrentEP / MaxEP);
    }
}
