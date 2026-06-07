using UnityEngine;

/// <summary>
/// 控制閃電手技能的腳本，支援雙手獨立特效設定。
/// 包含主特效(具備殺傷力)與副特效(純視覺)的生成、位置同步與範圍秒殺判定。
/// </summary>
public class LightningHandSkill : MonoBehaviour
{
    [Header("技能設定")]
    [Tooltip("對應 EnergyManager 觸發此技能的 ID")]
    public int lightningSkillId = 4;

    [Header("主特效 (有傷害判定)")]
    [Tooltip("拖入具有殺傷力的閃電特效預製件")]
    public GameObject mainLightningPrefab;
    [Tooltip("主特效綁定的手部：0 = 左手，1 = 右手")]
    public int mainHandType = 1;
    [Tooltip("接觸到主特效的致死判定半徑")]
    public float killRadius = 1.5f;
    [Tooltip("受閃電影響的敵軍層級 (需勾選 Enemy)")]
    public LayerMask enemyLayer;

    [Header("副特效 (純視覺，無傷害)")]
    [Tooltip("拖入純裝飾、無殺傷力的特效預製件")]
    public GameObject visualOnlyPrefab;
    [Tooltip("副特效綁定的手部：0 = 左手，1 = 右手 (若與主特效相同則會重疊)")]
    public int visualHandType = 0;

    [Header("通用設定")]
    [Tooltip("技能持續放電的時間 (秒)")]
    public float duration = 5f;

    // --- 內部狀態變數 ---
    private bool _isActive = false;      // 技能是否正在施放中
    private float _timer = 0f;           // 技能持續時間計時器
    private GameObject _mainInstance;    // 主特效的實例
    private GameObject _visualInstance;  // 副特效的實例

    private void OnEnable()
    {
        EnergyManager.OnSkillActivated += HandleSkillActivated;
    }

    private void OnDisable()
    {
        EnergyManager.OnSkillActivated -= HandleSkillActivated;
    }

    /// <summary>
    /// 事件監聽：當管理器廣播技能啟動時觸發
    /// </summary>
    private void HandleSkillActivated(int sId)
    {
        if (sId == lightningSkillId)
        {
            ActivateLightning();
        }
    }

    /// <summary>
    /// 啟動閃電技能：鎖定玩家狀態、計時歸零，並生成對應手部的特效
    /// </summary>
    public void ActivateLightning()
    {
        // 避免在技能執行期間被重複觸發
        if (_isActive) return;

        // 1. 鎖定玩家施法狀態 (禁止放下一招)
        GameManager.Instance.isSkillReleasing = true;
        _isActive = true;
        _timer = 0f;

        // 2. 生成主特效 (有傷害)
        if (mainLightningPrefab != null)
        {
            _mainInstance = Instantiate(mainLightningPrefab, GetHandPosition(mainHandType), Quaternion.identity);
        }

        // 3. 生成副特效 (無傷害)
        if (visualOnlyPrefab != null)
        {
            _visualInstance = Instantiate(visualOnlyPrefab, GetHandPosition(visualHandType), Quaternion.identity);
        }
    }

    private void Update()
    {
        // 只有在技能啟動時才執行邏輯
        if (!_isActive) return;

        _timer += Time.deltaTime;

        // 檢查技能是否結束
        if (_timer >= duration)
        {
            DeactivateLightning();
            return;
        }

        // 取得當下雙手的即時位置
        Vector3 mainPos = GetHandPosition(mainHandType);
        Vector3 visualPos = GetHandPosition(visualHandType);

        // 1. 同步更新兩個特效的位置，讓閃電緊緊黏在手上
        if (_mainInstance != null) _mainInstance.transform.position = mainPos;
        if (_visualInstance != null) _visualInstance.transform.position = visualPos;

        // 2. 範圍檢測：只針對「主特效」的位置進行 Hitbox 掃描
        Collider2D[] hits = Physics2D.OverlapCircleAll(mainPos, killRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            MosquitoBase mosquito = hit.GetComponent<MosquitoBase>();
            if (mosquito != null)
            {
                // 給予極大傷害達成秒殺效果
                mosquito.TakeDamage(9999, DamageSource.Explosion);
            }
        }
    }

    /// <summary>
    /// 結束閃電技能：銷毀特效實例，並解除玩家的施法鎖定狀態
    /// </summary>
    private void DeactivateLightning()
    {
        _isActive = false;

        // 清理場上的特效物件
        if (_mainInstance != null) Destroy(_mainInstance);
        if (_visualInstance != null) Destroy(_visualInstance);

        // 技能釋放完畢，玩家可以施放下一招
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isSkillReleasing = false;
        }
    }

    /// <summary>
    /// 根據傳入的參數 (0 或 1) 取得對應左手或右手在世界空間的座標
    /// </summary>
    private Vector3 GetHandPosition(int hType)
    {
        return (hType == 0) ? GameManager.Instance.leftHand : GameManager.Instance.rightHand;
    }

    /// <summary>
    /// 在 Unity 編輯器 Scene 視窗中繪製除錯輔助線，方便調整判定半徑
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        if (_isActive)
        {
            // 只會畫出主特效的黃色判定圈，直觀顯示殺傷範圍
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetHandPosition(mainHandType), killRadius);
        }
    }
}