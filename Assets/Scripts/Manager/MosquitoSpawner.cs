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
    [Tooltip("敵人從此半徑的邊緣隨機角度出現")]
    public float SpawnRadius = 9f;

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
                float interval = GetInterval(cfg, currentPhase);
                _burstTimers[i] = interval;
                StartCoroutine(SpawnBurst(cfg));
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
    }

    // ── 生成邏輯 ──────────────────────────────────────────────────────────

    IEnumerator SpawnBurst(EnemySpawnConfig cfg)
    {
        for (int i = 0; i < cfg.BurstCount; i++)
        {
            SpawnSingle(cfg.Prefab, RandomEdgePosition(), cfg);
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

    Vector3 RandomEdgePosition()
    {
        float angle = Random.Range(0f, Mathf.PI * 2f);
        return new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0f) * SpawnRadius;
    }

    float GetInterval(EnemySpawnConfig cfg, GamePhase phase)
    {
        int idx = (int)phase;
        var arr = cfg.BurstIntervalPerPhase;
        if (arr == null || arr.Length == 0) return 5f;
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
