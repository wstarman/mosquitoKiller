using UnityEngine;

public class LightningHandSkill : MonoBehaviour
{
    [Header("技能設定")]
    public int lightningSkillId = 4; // 觸發此技能的 ID

    [Header("主特效 (有傷害判定)")]
    public GameObject mainLightningPrefab; // 拖入會殺敵的特效
    public int mainHandType = 1; // 0 = 左手，1 = 右手
    public float killRadius = 1.5f; // 致死判定範圍
    public LayerMask enemyLayer;

    [Header("副特效 (純視覺，無傷害)")]
    public GameObject visualOnlyPrefab; // 拖入純裝飾的特效
    public int visualHandType = 0; // 0 = 左手，1 = 右手 (設為跟主特效一樣就會疊在一起)

    [Header("通用設定")]
    public float duration = 5f; // 技能持續時間

    private bool _isActive = false;
    private float _timer = 0f;
    private GameObject _mainInstance;
    private GameObject _visualInstance;

    private void OnEnable()
    {
        EnergyManager.OnSkillActivated += HandleSkillActivated;
    }

    private void OnDisable()
    {
        EnergyManager.OnSkillActivated -= HandleSkillActivated;
    }

    private void HandleSkillActivated(int sId)
    {
        if (sId == lightningSkillId)
        {
            ActivateLightning();
        }
    }

    public void ActivateLightning()
    {
        if (_isActive) return;

        GameManager.Instance.isSkillReleasing = true;
        _isActive = true;
        _timer = 0f;

        // 生成主特效
        if (mainLightningPrefab != null)
        {
            _mainInstance = Instantiate(mainLightningPrefab, GetHandPosition(mainHandType), Quaternion.identity);
        }

        // 生成副特效
        if (visualOnlyPrefab != null)
        {
            _visualInstance = Instantiate(visualOnlyPrefab, GetHandPosition(visualHandType), Quaternion.identity);
        }
    }

    private void Update()
    {
        if (!_isActive) return;

        _timer += Time.deltaTime;

        if (_timer >= duration)
        {
            DeactivateLightning();
            return;
        }

        Vector3 mainPos = GetHandPosition(mainHandType);
        Vector3 visualPos = GetHandPosition(visualHandType);

        // 1. 同步更新兩個特效的位置
        if (_mainInstance != null) _mainInstance.transform.position = mainPos;
        if (_visualInstance != null) _visualInstance.transform.position = visualPos;

        // 2. 「只有」主特效的位置會執行範圍檢測 (Hitbox 生效)
        Collider2D[] hits = Physics2D.OverlapCircleAll(mainPos, killRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            MosquitoBase mosquito = hit.GetComponent<MosquitoBase>();
            if (mosquito != null)
            {
                mosquito.TakeDamage(9999, DamageSource.Explosion);
            }
        }
    }

    private void DeactivateLightning()
    {
        _isActive = false;
        if (_mainInstance != null) Destroy(_mainInstance);
        if (_visualInstance != null) Destroy(_visualInstance);
        GameManager.Instance.isSkillReleasing = false;
    }

    private Vector3 GetHandPosition(int hType)
    {
        return (hType == 0) ? GameManager.Instance.leftHand : GameManager.Instance.rightHand;
    }

    private void OnDrawGizmosSelected()
    {
        if (_isActive)
        {
            // 在 Scene 視窗只會畫出主特效的黃色判定圈
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(GetHandPosition(mainHandType), killRadius);
        }
    }
}