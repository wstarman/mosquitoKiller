using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ClapEffect : MonoBehaviour
{
    int counter = 0;
    bool show = false;

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
        if (counter > 0) counter--;
        else if (show)
        {
            GetComponent<Renderer>().enabled = false;
            _collider.enabled = show = false;
        }
    }

    void OnEnable()
    {
        GameManager.OnHandClap += OnHandClap;
        GameStateManager.OnStateChanged += OnStateChanged;
    }

    void OnDisable()
    {
        GameManager.OnHandClap -= OnHandClap;
        GameStateManager.OnStateChanged -= OnStateChanged;
    }

    void OnStateChanged(GameState from, GameState to)
    {
        // 切換狀態時立刻隱藏，避免殘留 collider 觸發新場景的按鈕
        counter = 0;
        GetComponent<Renderer>().enabled = false;
        _collider.enabled = show = false;
    }

    void OnHandClap()
    {
        Vector3 pos = (GameManager.Instance.leftHand + GameManager.Instance.rightHand) / 2;
        pos.z = 0;
        transform.position = pos;

        bool isPlaying = GameStateManager.Instance != null &&
                         GameStateManager.Instance.CurrentState == GameState.Playing;

        transform.localScale = isPlaying ? _playingScale : _cursorScale;
        counter = isPlaying ? 60 : 18;   // playing: 1 sec, cursor: 0.3 sec

        GetComponent<Renderer>().enabled = true;
        _collider.enabled = show = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // 觸發後立刻停用，確保一次拍手只按到一個按鈕
        _collider.enabled = false;
    }
}