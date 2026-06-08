using UnityEngine;

public class Butterfly : MosquitoBase
{
    [Header("Butterfly")]
    [Tooltip("漫遊存活秒數，到時飛離畫面")]
    public float LifeTime = 6f;
    [Tooltip("離場飛行速度")]
    public float LeaveSpeed = 4f;

    enum ButterflyState { Wandering, Leaving }
    ButterflyState _bState;
    float _lifeTimer;
    Vector2 _leaveDir;

    public override void OnSpawn()
    {
        base.OnSpawn();
        _bState = ButterflyState.Wandering;
        _lifeTimer = LifeTime;
        HealOnKill = 0;   // 蝴蝶為陷阱，打死不回血
    }

    protected override void Start()
    {
        base.Start();
        _bState = ButterflyState.Wandering;
        _lifeTimer = LifeTime;
        HealOnKill = 0;
    }

    protected override void Update()
    {
        if (GameStateManager.Instance?.CurrentState != GameState.Playing) return;

        if (_bState == ButterflyState.Leaving)
        {
            // 離場：直線飛出，不受遊玩範圍限制（不呼叫 base，故豁免 ClampToPlayArea）
            transform.Translate(_leaveDir * LeaveSpeed * Time.deltaTime);
            if (IsOffScreen())
                ReturnToPool();
            return;
        }

        base.Update();   // Wandering 時沿用 base（含 ClampToPlayArea 遊玩範圍）
    }

    protected override void UpdateWandering()
    {
        transform.Translate(_moveDir * MoveSpeed * Time.deltaTime);

        _wanderTimer -= Time.deltaTime;
        if (_wanderTimer <= 0f)
        {
            _wanderTimer = WanderChangeInterval + Random.Range(-0.3f, 0.3f);
            _moveDir = Random.insideUnitCircle.normalized;
        }

        _lifeTimer -= Time.deltaTime;
        if (_lifeTimer <= 0f)
            EnterLeaving();
        // 蝴蝶不進入攻擊狀態，只漫遊或離場
    }

    void EnterLeaving()
    {
        _bState = ButterflyState.Leaving;
        _leaveDir = RandomLeaveDirection();
    }

    // 隨機離場方向，略微偏向遠離遊玩區中心，確保是「飛出去」而非繞回場內
    Vector2 RandomLeaveDirection()
    {
        Vector2 dir = Random.insideUnitCircle.normalized;
        if (dir == Vector2.zero) dir = Vector2.up;

        if (GetPlayBounds(out Vector2 c, out _))
        {
            Vector2 outward = (Vector2)transform.position - c;
            if (outward.sqrMagnitude > 0.0001f)
                dir = (dir + outward.normalized).normalized;
        }
        return dir;
    }

    bool IsOffScreen()
    {
        Camera cam = Camera.main;
        if (cam == null) return true;
        float h = cam.orthographicSize + 1f;   // +margin 確保完全離開畫面才回收
        float w = h * cam.aspect + 1f;
        Vector3 p = transform.position;
        return Mathf.Abs(p.x) > w || Mathf.Abs(p.y) > h;
    }

    // 打到蝴蝶扣分（陷阱），不加分、不回血
    protected override void OnDeath(DamageSource source)
    {
        ScoreManager.Instance?.Add(-ScoreValue);
        if (bloodEffectPrefab != null)
            Instantiate(bloodEffectPrefab, transform.position, Quaternion.identity);
        ReturnToPool();
    }
}
