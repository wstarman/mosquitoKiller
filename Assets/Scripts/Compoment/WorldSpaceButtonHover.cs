using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class WorldSpaceButtonHover : MonoBehaviour
{
    public Color NormalColor = new Color(0, 0, 0, 0);
    public Color HoverColor = new Color(0, 0.5f, 1f, 0.5f);
    public float HoverRadius = 1f;
    public float LerpSpeed = 8f;

    SpriteRenderer _renderer;
    bool _isHovered = false;

    // Start is called before the first frame update
    void Start()
    {
        _renderer = GetComponent<SpriteRenderer>();
        _renderer.color = NormalColor;
    }

    // Update is called once per frame
    void Update()
    {
        bool hovered = IsWithinRange();

        if (hovered)
            ButtonHoverManager.Instance?.RequestHover(this);
        else
            ButtonHoverManager.Instance?.ReleaseHover(this);

        Color target = _isHovered ? HoverColor : NormalColor;
        _renderer.color = Color.Lerp(_renderer.color, target, Time.deltaTime * LerpSpeed);
    }

    bool IsWithinRange()
    {
        if (GameManager.Instance == null) return false;
        float distL = Vector2.Distance(transform.position, GameManager.Instance.leftHand);
        float distR = Vector2.Distance(transform.position, GameManager.Instance.rightHand);
        return distL < HoverRadius || distR < HoverRadius;
    }

    public void SetHovered(bool value) => _isHovered = value;

    public void ForceNormal() => _isHovered = false;
}
