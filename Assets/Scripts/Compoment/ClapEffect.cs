using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClapEffect : MonoBehaviour
{
    [Tooltip("碰撞箱開啟持續時間（秒）")]
    public float ColliderDuration = 0.1f;

    int _visualCounter = 0;
    bool _show = false;
    float _colliderTimer = 0f;

    Collider2D _collider;
    Vector3 _playingScale;
    Vector3 _cursorScale;

    // Start is called before the first frame update
    void Start()
    {
        _collider = GetComponent<Collider2D>();
        GetComponent<Renderer>().enabled = false;
        _collider.enabled = false;

        _playingScale = transform.localScale;
        _cursorScale = _playingScale;
    }

    // Update is called once per frame
    void Update()
    {
        // 視覺倒數（與碰撞箱無關）
        if (_visualCounter > 0) _visualCounter--;
        else if (_show)
        {
            GetComponent<Renderer>().enabled = false;
            _show = false;
        }

        // 碰撞箱獨立 timer，到期自動關閉
        if (_colliderTimer > 0f)
        {
            _colliderTimer -= Time.deltaTime;
            if (_colliderTimer <= 0f)
                _collider.enabled = false;
        }
    }

    void OnEnable()
    {
        GameManager.OnHandClap += OnHandClap;
        GamePhaseManager.OnPhaseChanged += OnPhaseChanged;
    }

    void OnDisable()
    {
        GameManager.OnHandClap -= OnHandClap;
        GamePhaseManager.OnPhaseChanged -= OnPhaseChanged;
    }

    void OnPhaseChanged(GamePhase from, GamePhase to)
    {
        if (to != GamePhase.Transition) return;

        // 轉場開始：強制關閉視覺與碰撞箱
        GetComponent<Renderer>().enabled = false;
        _show = false;
        _visualCounter = 0;
        _collider.enabled = false;
        _colliderTimer = 0f;
    }

    void OnHandClap()
    {
        Vector3 pos = (GameManager.Instance.leftHand + GameManager.Instance.rightHand) / 2;
        pos.z = 0;
        transform.position = pos;

        bool isPlaying = GameStateManager.Instance != null &&
                         GameStateManager.Instance.CurrentState == GameState.Playing;

        transform.localScale = isPlaying ? _playingScale : _cursorScale;
        _visualCounter = isPlaying ? 60 : 18;   // playing: 1 sec, cursor: 0.3 sec

        // 視覺顯示
        GetComponent<Renderer>().enabled = true;
        _show = true;

        // 碰撞箱只短暫開啟
        _collider.enabled = true;
        _colliderTimer = ColliderDuration;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 觸發後立刻停用，確保一次拍手只按到一個按鈕
        _collider.enabled = false;
        _colliderTimer = 0f;
    }
}
