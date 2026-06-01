using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class BloodSplatter : MonoBehaviour
{
    public float Duration = 5f;

    SpriteRenderer _sr;
    float _timer;

    void Start()
    {
        _sr = GetComponent<SpriteRenderer>();
        _timer = Duration;
    }

    void Update()
    {
        _timer -= Time.deltaTime;
        Color c = _sr.color;
        c.a = Mathf.Clamp01(_timer / Duration);
        _sr.color = c;

        if (_timer <= 0f)
            Destroy(gameObject);
    }
}
