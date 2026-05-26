using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject Prefab;

    [Tooltip("從此 Phase 開始生成")]
    public GamePhase UnlockAtPhase = GamePhase.Phase1;

    [Tooltip("每次 burst 固定生成幾隻")]
    public int BurstCount = 3;

    [Tooltip("各 Phase 的 burst 間隔秒數，index 對應 GamePhase enum。\n若當前 Phase 超出陣列長度，使用最後一個值。")]
    public float[] BurstIntervalPerPhase = { 8f, 6f, 4f, 3f, 5f };

    [Tooltip("burst 內每隻生成的間隔秒數（避免同時湧出）")]
    public float SpawnStagger = 0.2f;

    [Tooltip("HP 覆寫；0 = 使用 Prefab 上 BaseEnemy 的預設值")]
    public int HPOverride = 0;
}
