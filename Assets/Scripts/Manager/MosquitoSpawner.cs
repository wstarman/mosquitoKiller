using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 統一管理敵人生成與回收。
/// Inspector 設定 EnemyConfigs 陣列，每筆各自的 Phase 解鎖時機與 burst 節奏。
/// Boss 在進入 Boss Phase 時獨立生成。
/// </summary>
public class MosquitoSpawner : MonoBehaviour
{
    public static MosquitoSpawner Instance { get; private set; }

    [Header("一般敵人")]
    public EnemySpawnConfig[] EnemyConfigs = {};

    [Header("Boss Phase")]
    public GameObject BossPrefab;
    public Vector3 BossSpawnPosition = new(0, 6f, 0);

    [Header("生成位置")]
    [Tooltip("FromEdge 模式：超出相機邊界外的距離")]
    public float EdgeSpawnMargin = 1f;
    [Tooltip("InsideScreen 模式：距相機邊界的安全內縮距離")]
    public float InsideSpawnMargin = 1f;

    // ── 內部狀態 ──────────────────────────────────────────────────────────

    bool _isPlaying;
    float[] _burstTimers;   // 每個 config 距下次 burst 的剩餘秒數

    // per-prefab object pool：prefab → 閒置 instance queue
    readonly Dictionary<GameObject, Queue<GameObject>> _pools = new();
    // instance → 來源 prefab，供 ReturnToPool 使用
    readonly Dictionary<GameObject, GameObject> _instanceToPrefab = new();

    // ── Unity 生命週期 ────────────────────────────────────────────────────

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnEnable()
    {
        GameStateManager.OnStateChanged += OnStateChanged;
        GamePhaseManager.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable()
    {
        GameStateManager.OnStateChanged -= OnStateChanged;
        GamePhaseManager.OnPhaseChanged -= OnPhaseChanged;
    }

    void Update()
    {
        if (!_isPlaying) return;

        var currentPhase = GamePhaseManager.Instance?.CurrentPhase ?? GamePhase.Phase1;

        for (int i = 0; i < EnemyConfigs.Length; i++)
        {
            var cfg = EnemyConfigs[i];
            if (currentPhase < cfg.UnlockAtPhase) continue;

            _burstTimers[i] -= Time.deltaTime;
            if (_burstTimers[i] <= 0f)
            {
                _burstTimers[i] = GetInterval(cfg, currentPhase);
                StartCoroutine(SpawnBurst(cfg, currentPhase));
            }
        }
    }

    // ── 事件處理 ──────────────────────────────────────────────────────────

    void OnStateChanged(GameState from, GameState to)
    {
        _isPlaying = to == GameState.Playing;
        if (to == GameState.Playing)
            _burstTimers = new float[EnemyConfigs.Length];
        else
            DespawnAll();
    }

    void OnPhaseChanged(GamePhase from, GamePhase to)
    {
        if (to == GamePhase.Boss && BossPrefab != null)
            SpawnSingle(BossPrefab, BossSpawnPosition);

        if (to == GamePhase.Transition)
        {
            // 轉場開始：停止生成並清除所有敵人
            _isPlaying = false;
            DespawnAll();
        }
    }

    // ── 生成邏輯 ──────────────────────────────────────────────────────────

    IEnumerator SpawnBurst(EnemySpawnConfig cfg, GamePhase phase)
    {
        int count = GetBurstCount(cfg, phase);
        SpawnMode mode = GetSpawnMode(cfg, phase);

        for (int i = 0; i < count; i++)
        {
            Vector3 pos = mode == SpawnMode.InsideScreen
                ? RandomInsidePosition()
                : RandomEdgePosition();
            SpawnSingle(cfg.Prefab, pos, cfg);
            if (cfg.SpawnStagger > 0f)
                yield return new WaitForSeconds(cfg.SpawnStagger);
        }
    }

    void SpawnSingle(GameObject prefab, Vector3 position, EnemySpawnConfig cfg = null)
    {
        if (prefab == null) return;

        var go = GetFromPool(prefab);
        go.transform.position = position;
        go.SetActive(true);

        var enemy = go.GetComponent<BaseEnemy>();
        if (enemy != null)
        {
            if (cfg != null && cfg.HPOverride > 0) enemy.MaxHP = cfg.HPOverride;
            enemy.OnSpawn();
        }
    }

    // 從畫面外四邊隨機一邊生成
    Vector3 RandomEdgePosition()
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize + EdgeSpawnMargin;
        float halfW = halfH * cam.aspect + EdgeSpawnMargin;

        return Random.Range(0, 4) switch
        {
            0 => new Vector3(Random.Range(-halfW, halfW),  halfH, 0f), // 上
            1 => new Vector3(Random.Range(-halfW, halfW), -halfH, 0f), // 下
            2 => new Vector3(-halfW, Random.Range(-halfH, halfH), 0f), // 左
            _ => new Vector3( halfW, Random.Range(-halfH, halfH), 0f), // 右
        };
    }

    // 在畫面內隨機位置生成
    Vector3 RandomInsidePosition()
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize - InsideSpawnMargin;
        float halfW = halfH * cam.aspect - InsideSpawnMargin;

        return new Vector3(
            Random.Range(-halfW, halfW),
            Random.Range(-halfH, halfH),
            0f
        );
    }

    float GetInterval(EnemySpawnConfig cfg, GamePhase phase)
    {
        int idx = (int)phase;
        var arr = cfg.BurstIntervalPerPhase;
        if (arr == null || arr.Length == 0) return 5f;
        return arr[Mathf.Min(idx, arr.Length - 1)];
    }

    int GetBurstCount(EnemySpawnConfig cfg, GamePhase phase)
    {
        int idx = (int)phase;
        var arr = cfg.BurstCountPerPhase;
        if (arr == null || arr.Length == 0) return 1;
        return arr[Mathf.Min(idx, arr.Length - 1)];
    }

    SpawnMode GetSpawnMode(EnemySpawnConfig cfg, GamePhase phase)
    {
        int idx = (int)phase;
        var arr = cfg.SpawnModePerPhase;
        if (arr == null || arr.Length == 0) return SpawnMode.FromEdge;
        return arr[Mathf.Min(idx, arr.Length - 1)];
    }

    // ── Object Pool ───────────────────────────────────────────────────────

    GameObject GetFromPool(GameObject prefab)
    {
        if (!_pools.ContainsKey(prefab))
            _pools[prefab] = new Queue<GameObject>();

        var pool = _pools[prefab];
        if (pool.Count > 0) return pool.Dequeue();

        var go = Instantiate(prefab);
        go.SetActive(false);
        _instanceToPrefab[go] = prefab;
        return go;
    }

    public void ReturnToPool(GameObject go)
    {
        go.SetActive(false);
        if (_instanceToPrefab.TryGetValue(go, out var prefab) && _pools.ContainsKey(prefab))
            _pools[prefab].Enqueue(go);
        else
            Destroy(go);
    }

    void DespawnAll()
    {
        StopAllCoroutines();
        foreach (var enemy in FindObjectsByType<BaseEnemy>(FindObjectsSortMode.None))
        {
            enemy.OnDespawn();
            ReturnToPool(enemy.gameObject);
        }
    }
}
