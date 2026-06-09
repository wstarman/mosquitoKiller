using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BloodSplatter : MonoBehaviour
{
    public float Duration = 5f;
    [Tooltip("血跡渲染層級；需高於敵人（預設 0）才能蓋住視線造成干擾")]
    public int SortingOrder = 100;

    SpriteRenderer _sr;
    float _timer;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _sr.sortingOrder = SortingOrder;   // 程式強制，避免 prefab 設定漂移
        _timer = Duration;
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        Color c = _sr.color;
        c.a = Mathf.Clamp01(_timer*2f / Duration);
        _sr.color = c;

        if (_timer <= 0f)
            Destroy(gameObject);
    }
}
