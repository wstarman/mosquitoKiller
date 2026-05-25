using UnityEngine;

[System.Serializable]
public class EnemySpawnConfig
{
    public GameObject Prefab;

    [Tooltip("遊戲開始後幾秒才開始生成此敵人（0 = 立即）")]
    public float UnlockAfterSeconds = 0f;

    [Tooltip("每秒生成幾隻（0 = 停止生成）")]
    public float SpawnRate = 1f;

    [Tooltip("HP 覆寫；0 = 使用 Prefab 上 BaseEnemy 的預設值")]
    public int HPOverride = 0;
}
