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

    private float _timer = 0f;
    private bool _isCharging = true;
    private Vector3 _startPos;
    private int _handType;
    private Vector3 _lastHandPos;
    private Vector3 _launchDirection = Vector3.right;

    public void Initialize(int sId)
    {
        _handType = (sId == (int)Skill.HadokenLeft) ? 0 : 1;
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
        {
            UpdateCharging();
        }
        else
        {
            UpdateFlying();
        }
    }

    void UpdateCharging()
    {
        _timer += Time.deltaTime;

        Vector3 currentHandPos = GetHandPosition();

        // 計算發射方向 (動量)
        Vector3 delta = currentHandPos - _lastHandPos;
        if (delta.magnitude > 0.01f)
        {
            _launchDirection = delta.normalized;
        }
        else
        {
            // 位移太小或沒動時，預設往右
            _launchDirection = Vector3.right;
        }
        _lastHandPos = currentHandPos;

        // 跟隨手部
        transform.position = currentHandPos;

        // 縮放動畫
        float progress = _timer / chargeDuration;
        float currentScale = Mathf.Lerp(minScale, maxScale, progress);
        transform.localScale = Vector3.one * currentScale;

        if (_timer >= chargeDuration)
        {
            _isCharging = false;
            _startPos = transform.position;

            // 設定飛行方向的旋轉
            float angle = Mathf.Atan2(_launchDirection.y, _launchDirection.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0, 0, angle);
        }
    }

    void UpdateFlying()
    {
        // 1. 累加移動距離 (使用 Time.deltaTime)
        // 讓物件依照發射方向持續移動
        transform.position += (Vector3)_launchDirection * speed * Time.deltaTime;

        // 2. 波浪效果計算
        // 我們計算「從發射點開始」過了多久
        float timeSinceLaunch = Time.time - (_timer + (_timer - _timer)); // 修正計時基準

        // 使用正弦波做位移，將波動拳「推」離中心軸
        Vector3 right = _launchDirection;
        Vector3 up = new Vector3(-right.y, right.x, 0);

        float yOffset = Mathf.Sin(Time.time * waveFrequency) * waveAmplitude;

        // 將位置修正為：原始中心路徑 + 波浪位移
        // 注意：我們直接在 UpdateFlying 裡持續累加位置，不應該依賴 _startPos 減法
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (_isCharging) return;

        MosquitoBase mosquito = other.GetComponent<MosquitoBase>();
        if (mosquito != null)
        {
            mosquito.TakeDamage(damage, DamageSource.Explosion);
            Destroy(gameObject);
        }
    }
}