using UnityEngine;

public enum SpawnMode
{
    InsideScreen,   // 直接在畫面內隨機位置出現
    FromEdge        // 從畫面外邊緣飛入
}

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject Prefab;

    [Tooltip("從此 Phase 開始生成")]
    public GamePhase UnlockAtPhase = GamePhase.Phase1;

    [Tooltip("各 Phase 每次 burst 生成的數量，index 對應 GamePhase enum。\n若當前 Phase 超出陣列長度，使用最後一個值。")]
    public int[] BurstCountPerPhase = { 3 };

    [Tooltip("各 Phase 的 burst 間隔秒數，index 對應 GamePhase enum。\n若當前 Phase 超出陣列長度，使用最後一個值。")]
    public float[] BurstIntervalPerPhase = { 8f, 6f, 4f, 3f, 3f, 5f };

    [Tooltip("burst 內每隻生成的間隔秒數（避免同時湧出）")]
    public float SpawnStagger = 0.2f;

    [Tooltip("各 Phase 的生成模式，index 對應 GamePhase enum。\n若當前 Phase 超出陣列長度，使用最後一個值。")]
    public SpawnMode[] SpawnModePerPhase = { SpawnMode.FromEdge };

    [Tooltip("HP 覆寫；0 = 使用 Prefab 上 BaseEnemy 的預設值")]
    public int HPOverride = 0;
}
