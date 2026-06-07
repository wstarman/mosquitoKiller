using UnityEngine;

/// <summary>
/// 控制波動拳技能的腳本，包含蓄力、隨手部移動、波浪軌跡飛行與碰撞爆炸邏輯。
/// </summary>
public class Hadoken2DScript : MonoBehaviour
{
    [Header("基礎設定")]
    [Tooltip("波動拳飛行的直線速度")]
    public float speed = 10f;
    [Tooltip("波動拳最多存在幾秒 (避免飛出場外永遠不消失)")]
    public float lifeTime = 3f;
    [Tooltip("爆炸時造成的傷害值")]
    public int damage = 1;

    [Header("蓄力設定")]
    [Tooltip("基礎蓄力時間")]
    public float chargeDuration = 0.4f;
    [Tooltip("最大蓄力時間保底 (就算手沒動，時間到也會強制發射)")]
    public float maxChargeDuration = 0.6f;
    [Tooltip("剛產生時的最小縮放值")]
    public float minScale = 0.2f;
    [Tooltip("蓄力完成後的最大縮放值")]
    public float maxScale = 1.0f;

    [Header("波浪設定")]
    [Tooltip("波浪軌跡的上下起伏幅度")]
    public float waveAmplitude = 0.5f;
    [Tooltip("波浪軌跡的震動頻率")]
    public float waveFrequency = 8.0f;

    [Header("爆炸與檢測設定")]
    [Tooltip("拖入爆炸時要產生的特效預製件")]
    public GameObject explosionEffectPrefab;
    [Tooltip("爆炸的傷害檢測半徑")]
    public float explosionRadius = 2.0f;
    [Tooltip("要檢測的敵方圖層 (Inspector 中勾選 Enemy)")]
    public LayerMask enemyLayer;

    // --- 內部狀態變數 ---
    private float _timer = 0f;
    private bool _isCharging = true;
    private Vector3 _startPos;
    private int _handType; // 0=左手, 1=右手
    private Vector3 _lastHandPos;
    private Vector3 _launchDirection = Vector3.right;

    // --- 拖尾特效參考 ---
    private TrailRenderer _trail;
    private float _initialTrailWidth;

    /// <summary>
    /// 初始化波動拳，決定是由哪隻手發射
    /// </summary>
    public void Initialize(int sId)
    {
        // 判斷發射手
        if (sId == (int)Skill.HadokenLeft)
        {
            _handType = 0;
        }
        else if (sId == (int)Skill.HadokenRight)
        {
            _handType = 1;
        }

        _lastHandPos = GetHandPosition();

        // 初始化大小與翻轉方向
        transform.localScale = Vector3.one * minScale;
        GetComponent<SpriteRenderer>().flipX = _handType == 1 ? false : true;

        // 鎖定玩家施法狀態，避免連續放招
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isSkillReleasing = true;
        }

        // 同步初始化拖尾寬度
        UpdateTrailWidth(minScale);
    }

    /// <summary>
    /// 取得對應手部的當前座標
    /// </summary>
    private Vector3 GetHandPosition()
    {
        return (_handType == 0) ? GameManager.Instance.leftHand : GameManager.Instance.rightHand;
    }

    private void Start()
    {
        // 取得子物件的拖尾特效，並記錄初始寬度以利後續等比例放大
        _trail = GetComponentInChildren<TrailRenderer>();
        if (_trail != null)
        {
            _initialTrailWidth = _trail.widthMultiplier;
        }

        // 設定壽命，時間到自動銷毀 (觸發 OnDestroy)
        Destroy(gameObject, lifeTime);
    }

    private void Update()
    {
        if (_isCharging)
            UpdateCharging();
        else
            UpdateFlying();
    }

    /// <summary>
    /// 蓄力階段邏輯：跟隨手部、變大、計算發射方向
    /// </summary>
    private void UpdateCharging()
    {
        _timer += Time.deltaTime;
        Vector3 currentHandPos = GetHandPosition();

        // 方向計算：如果手部移動距離足夠，就用移動方向；否則根據左右手給予預設方向
        Vector3 delta = currentHandPos - _lastHandPos;
        _launchDirection = delta.magnitude > 0.1f ? delta.normalized : (_handType == 1 ? Vector3.right : Vector3.left);
        _lastHandPos = currentHandPos;

        // 緊跟手部位置
        transform.position = currentHandPos;

        // 根據時間比例放大，並同步放大拖尾
        float currentScale = Mathf.Lerp(minScale, maxScale, _timer / chargeDuration);
        transform.localScale = Vector3.one * currentScale;
        UpdateTrailWidth(currentScale);

        // 判斷是否滿足發射條件：達到最大保底時間，或是手有動且達到基本蓄力時間
        if (_timer >= maxChargeDuration || (delta.magnitude > 0.1f && _timer >= chargeDuration))
        {
            _isCharging = false;
            _startPos = transform.position;

            // 將物件旋轉朝向發射方向
            float angle = Mathf.Atan2(_launchDirection.y, _launchDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    /// <summary>
    /// 動態更新拖尾特效的寬度
    /// </summary>
    private void UpdateTrailWidth(float scale)
    {
        if (_trail != null)
        {
            _trail.widthMultiplier = _initialTrailWidth * scale;
        }
    }

    /// <summary>
    /// 飛行階段邏輯：直線前進並疊加波浪偏移量
    /// </summary>
    private void UpdateFlying()
    {
        // 發射後重置翻轉狀態，確保圖片跟隨旋轉角度正確顯示
        GetComponent<SpriteRenderer>().flipX = false;
        if (_handType == 0)
        {
            GetComponent<SpriteRenderer>().flipY = true;
        }

        // 1. 直線位移
        transform.position += _launchDirection * speed * Time.deltaTime;

        // 2. 波浪效果計算 (利用外積概念求出垂直向量 up)
        Vector3 right = _launchDirection;
        Vector3 up = new Vector3(-right.y, right.x, 0);
        float yOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;

        // 累加偏移量產生波浪軌跡
        transform.position += up * (yOffset * Time.deltaTime * waveFrequency);

        // 3. 檢查是否飛出螢幕邊緣
        CheckScreenBounds();
    }

    /// <summary>
    /// 檢查是否超出攝影機畫面邊緣，若超出則引爆
    /// </summary>
    private void CheckScreenBounds()
    {
        if (Camera.main == null) return;

        // 將世界座標轉為畫面比例 (0~1)
        Vector3 viewportPos = Camera.main.WorldToViewportPoint(transform.position);

        if (viewportPos.x < 0 || viewportPos.x > 1 || viewportPos.y < 0 || viewportPos.y > 1)
        {
            Explode();
        }
    }

    /// <summary>
    /// 碰到任何碰撞體 (Enemy) 時觸發
    /// </summary>
    private void OnTriggerEnter2D(Collider2D other)
    {
        // 蓄力階段不產生傷害
        if (_isCharging) return;

        Explode();
    }

    /// <summary>
    /// 核心爆炸邏輯：產生特效、範圍結算傷害並銷毀自身
    /// </summary>
    private void Explode()
    {
        // 1. 生成視覺爆炸特效
        if (explosionEffectPrefab != null)
        {
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
        }

        // 2. 圓形範圍傷害檢測
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            MosquitoBase mosquito = hit.GetComponent<MosquitoBase>();
            if (mosquito != null)
            {
                mosquito.TakeDamage(damage, DamageSource.Explosion);
            }
        }

        // 3. 銷毀自身 (會自動觸發 OnDestroy)
        Destroy(gameObject);
    }

    /// <summary>
    /// 終極安全網：無論是被打掉、撞牆還是超時銷毀，必定解除玩家施法狀態
    /// </summary>
    private void OnDestroy()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.isSkillReleasing = false;
        }
    }

    /// <summary>
    /// 在編輯器中畫出紅色線框，方便調整爆炸半徑
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}