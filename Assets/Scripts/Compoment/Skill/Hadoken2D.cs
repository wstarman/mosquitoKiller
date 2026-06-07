using UnityEngine;

public class Hadoken2DScript : MonoBehaviour
{
    [Header("基礎設定")]
    public float speed = 10f;
    public float lifeTime = 3f;
    public int damage = 1;

    [Header("蓄力設定")]
    public float chargeDuration = 0.4f;
    public float minScale = 0.2f;
    public float maxScale = 1.0f;

    [Header("波浪設定")]
    public float waveAmplitude = 0.5f;
    public float waveFrequency = 8.0f;

    [Header("爆炸與檢測設定")]
    public GameObject explosionEffectPrefab; // 拖入爆炸特效
    public float explosionRadius = 2.0f;     // 爆炸範圍
    public LayerMask enemyLayer;             // 在 Inspector 勾選 Enemy 層級

    private float _timer = 0f;
    private bool _isCharging = true;
    private Vector3 _startPos;
    private int _handType;
    private Vector3 _lastHandPos;
    private Vector3 _launchDirection = Vector3.right;

    public void Initialize(int sId)
    {
        // 如果是左手，設為 0
        if (sId == (int)Skill.HadokenLeft)
        {
            _handType = 0;
        }
        // 如果是右手，設為 1
        else if (sId == (int)Skill.HadokenRight)
        {
            _handType = 1;
        }
        _lastHandPos = GetHandPosition();
        transform.localScale = Vector3.one * minScale;
    }

    Vector3 GetHandPosition()
    {
        return (_handType == 0) ? GameManager.Instance.leftHand : GameManager.Instance.rightHand;
    }

    void Start()
    {
        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        if (_isCharging)
            UpdateCharging();
        else
            UpdateFlying();
    }

    void UpdateCharging()
    {
        _timer += Time.deltaTime;
        Vector3 currentHandPos = GetHandPosition();

        // 方向計算 (保底向右)
        Vector3 delta = currentHandPos - _lastHandPos;
        _launchDirection = delta.magnitude > 0.01f ? delta.normalized : Vector3.right;
        _lastHandPos = currentHandPos;

        transform.position = currentHandPos;
        transform.localScale = Vector3.one * Mathf.Lerp(minScale, maxScale, _timer / chargeDuration);

        if (_timer >= chargeDuration)
        {
            _isCharging = false;
            _startPos = transform.position;
            float angle = Mathf.Atan2(_launchDirection.y, _launchDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void UpdateFlying()
    {
        // 直線位移
        transform.position += _launchDirection * speed * Time.deltaTime;

        // 波浪效果
        Vector3 right = _launchDirection;
        Vector3 up = new Vector3(-right.y, right.x, 0);
        float yOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;

        // 修正：在 Update 中直接設定位置會覆蓋掉直線位移，改為累加偏移量
        transform.position += up * (yOffset * Time.deltaTime * waveFrequency);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isCharging) return;

        // 觸發爆炸與傷害邏輯
        Explode();
    }

    void Explode()
    {
        // 1. 生成特效
        if (explosionEffectPrefab != null)
            Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);

        // 2. 範圍檢測 (檢測 enemyLayer)
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayer);
        foreach (Collider2D hit in hits)
        {
            MosquitoBase mosquito = hit.GetComponent<MosquitoBase>();
            if (mosquito != null)
            {
                mosquito.TakeDamage(damage, DamageSource.Explosion);
            }
        }

        // 3. 銷毀本體
        Destroy(gameObject);
    }

    // 在 Unity 中方便除錯看到範圍
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}