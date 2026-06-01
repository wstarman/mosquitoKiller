using UnityEngine;

/// <summary>
/// 所有敵人的抽象基底類別。
/// 組員繼承此類並覆寫 UpdateMovement()（或各 UpdateXxx 狀態方法），
/// 以及視需要覆寫 OnHit / OnDeath / OnSting / OnClapHit / OnSpawn / OnDespawn。
///
/// 場景設定需求：
///   - 此 GameObject 需掛 Collider2D（Is Trigger）才能被 Explosion 擊中
///   - 需掛 Rigidbody2D（Kinematic）才能觸發 OnTriggerEnter2D
/// </summary>
[RequireComponent(typeof(Collider2D))]
[RequireComponent(typeof(Rigidbody2D))]
public abstract class MosquitoBase : MonoBehaviour
{
    protected enum MosquitoState { Wandering, PrepareAttack, Attacking }

    // ── 基礎數值 ───────────────────────────────────────────────────────────

    [Header("Base Stats")]
    public int MaxHP = 1;
    [Tooltip("死亡得分，負值代表扣分（如吃瓜蝴蝶）")]
    public int PointsOnDeath = 100;
    [Tooltip("死亡時給予的充能量（EP）")]
    public int EnergyOnDeath = 10;

    [Header("Score")]
    public int ScoreValue = 100;
    public int StingScorePenalty = 20;

    [Header("Movement")]
    public float MoveSpeed = 2f;
    public float WanderChangeInterval = 1.5f;
    public float WanderDuration = 4f;
    public float PrepareAttackSpeed = 0.2f;

    [Header("Attack")]
    public float AttackWindupDuration = 2f;
    public float AttackScaleMultiplier = 3f;
    public float AttackZoomInDuration = 0.5f;
    public float AttackZoomOutDuration = 0.8f;

    // ── 內部狀態 ───────────────────────────────────────────────────────────

    protected MosquitoState _state = MosquitoState.Wandering;
    protected Vector2 _moveDir;          // 當前移動方向（單位向量）
    protected float _stateTimer;         // 當前狀態的倒數計時器（用於 PrepareAttack windup）
    protected float _wanderTimer;        // 距離下次隨機換向的剩餘時間
    protected float _wanderDuration;     // 本次 Wandering 已持續的累計時間

    Vector3 _baseScale;        // 進入 Attacking 前記錄的原始縮放，用於還原
    float _attackPhaseTimer;   // 攻擊動畫（放大 / 縮小）的計時器
    bool _attackZoomingOut;    // true = 正在執行縮回階段；false = 正在執行放大階段

    // ── HP 屬性 ────────────────────────────────────────────────────────────

    protected int CurrentHP { get; private set; }
    protected bool IsAlive => CurrentHP > 0;

    // ── 生命週期 hook（由 MosquitoSpawner 呼叫）──────────────────────────

    /// <summary>從 Pool 取出時呼叫，負責重置狀態。覆寫時記得呼叫 base.OnSpawn()。</summary>
    public virtual void OnSpawn()
    {
        CurrentHP = MaxHP;
        _state = MosquitoState.Wandering;
        _moveDir = Random.insideUnitCircle.normalized;
        _wanderTimer = WanderChangeInterval;
        _wanderDuration = 0f;
    }

    /// <summary>回到 Pool 前呼叫，負責清除暫存狀態（如特效、協程）。</summary>
    public virtual void OnDespawn() { }

    // ── Unity 訊息 ─────────────────────────────────────────────────────────

    /// <summary>初始化移動方向、HP，並訂閱遊戲狀態事件。</summary>
    protected virtual void Start()
    {
        _moveDir = Random.insideUnitCircle.normalized;
        _wanderTimer = WanderChangeInterval;
        _wanderDuration = 0f;
        CurrentHP = MaxHP;
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    /// <summary>取消訂閱遊戲狀態事件，避免物件銷毀後仍收到回調。</summary>
    protected virtual void OnDestroy()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    // 遊戲離開 Playing 狀態（暫停、結算等）時，強制回收此敵人。
    void HandleStateChanged(GameState from, GameState to)
    {
        if (to != GameState.Playing)
            ReturnToPool();
    }

    /// <summary>每幀驅動狀態機；非 Playing 狀態或已死亡時提早返回。攻擊中不做邊界夾住，讓縮放動畫可超出畫面。</summary>
    protected virtual void Update()
    {
        if (GameStateManager.Instance?.CurrentState != GameState.Playing) return;
        if (!IsAlive) return;
        UpdateStateMachine();
        if (_state != MosquitoState.Attacking)
            ClampToScreen();
    }

    // ── 狀態機 ─────────────────────────────────────────────────────────────

    /// <summary>根據當前狀態派送到對應的 Update 方法。子類可覆寫以插入額外邏輯。</summary>
    protected virtual void UpdateStateMachine()
    {
        switch (_state)
        {
            case MosquitoState.Wandering:     UpdateWandering();     break;
            case MosquitoState.PrepareAttack: UpdatePrepareAttack(); break;
            case MosquitoState.Attacking:     UpdateAttacking();     break;
        }
    }

    /// <summary>
    /// 隨機遊走：每隔 WanderChangeInterval 隨機換向；
    /// 持續 WanderDuration 後進入 PrepareAttack。
    /// </summary>
    protected virtual void UpdateWandering()
    {
        transform.Translate(_moveDir * MoveSpeed * Time.deltaTime);

        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            // 加入 ±0.3 秒的隨機偏移，使換向時機不過於規律
            _wanderTimer = WanderChangeInterval + Random.Range(-0.3f, 0.3f);
            _moveDir = Random.insideUnitCircle.normalized;
        }

        _wanderDuration += Time.deltaTime;
        if (_wanderDuration >= WanderDuration)
            EnterState(MosquitoState.PrepareAttack);
    }

    /// <summary>攻擊前搖：以較慢速度繼續移動，倒數 AttackWindupDuration 後進入 Attacking。</summary>
    protected virtual void UpdatePrepareAttack()
    {
        transform.Translate(_moveDir * (PrepareAttackSpeed * Time.deltaTime));

        _stateTimer -= Time.deltaTime;
        if (_stateTimer <= 0f)
            EnterState(MosquitoState.Attacking);
    }

    /// <summary>
    /// 攻擊動畫：先在 AttackZoomInDuration 內放大到 AttackScaleMultiplier 倍，
    /// 放大完成時觸發 OnSting()，再於 AttackZoomOutDuration 內縮回原尺寸，
    /// 完成後回到 Wandering。
    /// </summary>
    protected virtual void UpdateAttacking()
    {
        _attackPhaseTimer += Time.deltaTime;

        if (!_attackZoomingOut)
        {
            float t = Mathf.Clamp01(_attackPhaseTimer / AttackZoomInDuration);
            transform.localScale = Vector3.Lerp(_baseScale, _baseScale * AttackScaleMultiplier, t);

            if (t >= 1f)
            {
                OnSting();
                _attackZoomingOut = true;
                _attackPhaseTimer = 0f;
            }
        }
        else
        {
            float t = Mathf.Clamp01(_attackPhaseTimer / AttackZoomOutDuration);
            transform.localScale = Vector3.Lerp(_baseScale * AttackScaleMultiplier, _baseScale, t);

            if (t >= 1f)
            {
                transform.localScale = _baseScale; // 確保縮放完全還原，避免浮點誤差殘留
                EnterState(MosquitoState.Wandering);
            }
        }
    }

    /// <summary>切換狀態並重置該狀態所需的計時器與變數。</summary>
    protected virtual void EnterState(MosquitoState newState)
    {
        _state = newState;
        switch (newState)
        {
            case MosquitoState.PrepareAttack:
                _stateTimer = AttackWindupDuration;
                break;
            case MosquitoState.Wandering:
                _wanderTimer = WanderChangeInterval;
                _wanderDuration = 0f;
                _moveDir = Random.insideUnitCircle.normalized;
                break;
            case MosquitoState.Attacking:
                _baseScale = transform.localScale;  // 記錄進入攻擊前的尺寸
                _attackPhaseTimer = 0f;
                _attackZoomingOut = false;
                break;
        }
    }

    // ── 傷害 ───────────────────────────────────────────────────────────────

    /// <summary>框架統一入口，由技能或 Explosion 碰撞呼叫。</summary>
    public void TakeDamage(int dmg, DamageSource source)
    {
        if (!IsAlive) return;
        CurrentHP -= dmg;
        OnHit(source);
        if (CurrentHP <= 0) HandleDeath(source);
    }

    /// <summary>每次受擊時呼叫，HP 已扣除但尚未判斷死亡。可用於特殊受擊反應。</summary>
    protected virtual void OnHit(DamageSource source) { }

    /// <summary>HP 歸零時呼叫。預設行為：加分、充能、回收到 Pool。覆寫時如需保留預設行為請呼叫 base.OnDeath()。</summary>
    protected virtual void OnDeath(DamageSource source)
    {
        ScoreManager.Instance?.Add(
            _state == MosquitoState.Attacking
                ? (int)(ScoreValue * 1.5f)   // Parry bonus (x1.5)
                : ScoreValue
        );
        EnergyManager.Instance?.AddEnergy(EnergyOnDeath);
        ReturnToPool();
    }

    /// <summary>技能清場用，不觸發加分與充能，直接回收。</summary>
    public void KillSilent()
    {
        if (!IsAlive) return;
        CurrentHP = 0;
        ReturnToPool();
    }

    void HandleDeath(DamageSource source) => OnDeath(source);

    // ── 互動事件 ───────────────────────────────────────────────────────────

    /// <summary>被拍擊（Explosion / Clap）擊中時呼叫。預設行為：造成 1 點傷害。</summary>
    protected virtual void OnClapHit() =>
        TakeDamage(1, DamageSource.Explosion);

    /// <summary>螢幕上刺擊玩家時呼叫（攻擊動畫放大到最大時觸發）。</summary>
    protected virtual void OnSting()
    {
        ScoreManager.Instance?.Add(-StingScorePenalty);
    }

    /// <summary>與 Hand 碰撞時呼叫，預設不做任何事。</summary>
    protected virtual void OnHandTouch() { }

    // ── 碰撞 ───────────────────────────────────────────────────────────────

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Explosion"))
            OnClapHit();
        else if (other.CompareTag("Hand"))
            OnHandTouch();

        // 技能碰撞由各技能腳本直接呼叫 TakeDamage(dmg, DamageSource.XXX)
        OnTriggerEntered(other);
    }

    /// <summary>額外的碰撞 hook，供組員在不覆寫 OnTriggerEnter2D 的情況下處理自訂碰撞邏輯。</summary>
    protected virtual void OnTriggerEntered(Collider2D other) { }

    // ── 工具方法 ───────────────────────────────────────────────────────────

    /// <summary>回傳左右手中點，常被子類用來計算追蹤方向。GameManager 不存在時回傳零點。</summary>
    protected Vector2 GetPlayerCenter()
    {
        if (GameManager.Instance == null) return Vector2.zero;
        return ((Vector2)GameManager.Instance.leftHand + (Vector2)GameManager.Instance.rightHand) * 0.5f;
    }

    // 以正交攝影機的半寬高計算邊界；碰壁時反彈並夾住位置，防止敵人卡出畫面外。
    void ClampToScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return;
        float h = cam.orthographicSize;
        float w = h * cam.aspect;
        Vector3 pos = transform.position;
        if (Mathf.Abs(pos.x) >= w) _moveDir.x = -_moveDir.x;
        if (Mathf.Abs(pos.y) >= h) _moveDir.y = -_moveDir.y;
        pos.x = Mathf.Clamp(pos.x, -w, w);
        pos.y = Mathf.Clamp(pos.y, -h, h);
        transform.position = pos;
    }

    // 統一回收入口：先通知子類清理（OnDespawn），再交還給 Pool。
    protected void ReturnToPool()
    {
        OnDespawn();
        MosquitoSpawner.Instance?.ReturnToPool(gameObject);
    }
}