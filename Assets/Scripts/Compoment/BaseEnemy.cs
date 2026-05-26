using UnityEngine;

/// <summary>
/// 所有敵人的抽象基底類別。
/// 組員繼承此類並覆寫 UpdateMovement()，以及視需要覆寫 OnHit / OnDeath / OnSpawn / OnDespawn。
///
/// 場景設定需求：
///   - 此 GameObject 需掛 Collider2D（Is Trigger）才能被 Explotion 擊中
///   - 需掛 Rigidbody2D（Kinematic）才能觸發 OnTriggerEnter2D
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Base Stats")]
    public int MaxHP = 1;
    [Tooltip("死亡得分，負值代表扣分（如吃瓜蝴蝶）")]
    public int PointsOnDeath = 100;
    [Tooltip("死亡時給予的充能量（EP）")]
    public int EnergyOnDeath = 10;

    protected int CurrentHP { get; private set; }
    protected bool IsAlive => CurrentHP > 0;

    // ── 生命週期 hook（由 MosquitoSpawner 呼叫）──────────────────────────

    /// <summary>從 Pool 取出時呼叫，負責重置狀態。覆寫時記得呼叫 base.OnSpawn()。</summary>
    public virtual void OnSpawn()
    {
        CurrentHP = MaxHP;
    }

    /// <summary>回到 Pool 前呼叫，負責清除暫存狀態（如特效、協程）。</summary>
    public virtual void OnDespawn() { }

    // ── 傷害 ───────────────────────────────────────────────────────────────

    /// <summary>框架統一入口，由技能或 Explotion 碰撞呼叫。</summary>
    public void TakeDamage(int dmg, DamageSource source)
    {
        if (!IsAlive) return;
        CurrentHP -= dmg;
        OnHit(source);
        if (CurrentHP <= 0) HandleDeath(source);
    }

    /// <summary>每次受擊時呼叫，HP 已扣除但尚未判斷死亡。可用於特殊受擊反應（如小強蚊第一次被打下墜）。</summary>
    protected virtual void OnHit(DamageSource source) { }

    /// <summary>HP 歸零時呼叫。預設行為：加分、充能、回收到 Pool。覆寫時如需保留預設行為請呼叫 base.OnDeath()。</summary>
    protected virtual void OnDeath(DamageSource source)
    {
        ScoreManager.Instance?.Add(PointsOnDeath);
        EnergyManager.Instance?.AddEnergy(EnergyOnDeath);
        MosquitoSpawner.Instance?.ReturnToPool(gameObject);
    }

    /// <summary>技能清場用，不觸發加分與充能，直接回收。</summary>
    public void KillSilent()
    {
        if (!IsAlive) return;
        CurrentHP = 0;
        OnDespawn();
        MosquitoSpawner.Instance?.ReturnToPool(gameObject);
    }

    // ── 移動（必須實作）────────────────────────────────────────────────────

    /// <summary>每幀呼叫，組員在此實作移動邏輯。只在 Playing 狀態下呼叫。</summary>
    protected abstract void UpdateMovement();

    // ── Unity 訊息 ─────────────────────────────────────────────────────────

    void Update()
    {
        if (GameStateManager.Instance?.CurrentState != GameState.Playing) return;
        if (!IsAlive) return;
        UpdateMovement();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explosion"))
            TakeDamage(1, DamageSource.Explosion);

        // 技能碰撞由各技能腳本直接呼叫 TakeDamage(dmg, DamageSource.XXX)
        OnTriggerEntered(other);
    }

    /// <summary>額外的碰撞 hook，供組員在不覆寫 OnTriggerEnter2D 的情況下處理自訂碰撞邏輯。</summary>
    protected virtual void OnTriggerEntered(Collider2D other) { }

    // ────────────────────────────────────────────────────────────────────────

    void HandleDeath(DamageSource source) => OnDeath(source);
}
