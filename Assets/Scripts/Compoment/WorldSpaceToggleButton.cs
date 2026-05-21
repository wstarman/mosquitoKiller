using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(SpriteRenderer), typeof(Collider2D))]
public class WorldSpaceToggleButton : MonoBehaviour
{
    public Sprite OffSprite;
    public Sprite OnSprite;

    public float HoverRadius = 1f;
    public Vector3 HoverScale = new Vector3(1.1f, 1.1f, 1f);
    public float LerpSpeed = 8f;

    public UnityEvent OnToggleOn;
    public UnityEvent OnToggleOff;

    public bool IsOn { get; private set; } = false;

    SpriteRenderer _renderer;
    Vector3 _baseScale;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _baseScale = transform.localScale;
        _renderer.sprite = OffSprite;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 targetScale = IsHovered() ? HoverScale : _baseScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * LerpSpeed);
    }

    bool IsHovered()
    {
        if (GameManager.Instance == null) return false;
        float distL = Vector2.Distance(transform.position, GameManager.Instance.leftHand);
        float distR = Vector2.Distance(transform.position, GameManager.Instance.rightHand);
        return distL < HoverRadius || distR < HoverRadius;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Explosion")) return;
        IsOn = !IsOn;
        _renderer.sprite = IsOn ? OnSprite : OffSprite;
        if (IsOn) OnToggleOn.Invoke();
        else OnToggleOff.Invoke();
    }
}
