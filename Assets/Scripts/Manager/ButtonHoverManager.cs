using UnityEngine;

// 統一管理 hover 狀態，確保同一時間只有最近的按鈕顯示 hover
public class ButtonHoverManager : MonoBehaviour
{
    public static ButtonHoverManager Instance { get; private set; }

    WorldSpaceButtonHover _currentHovered;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void RequestHover(WorldSpaceButtonHover requester)
    {
        if (_currentHovered == requester) return;
        _currentHovered?.ForceNormal();
        _currentHovered = requester;
        _currentHovered.SetHovered(true);
    }

    public void ReleaseHover(WorldSpaceButtonHover requester)
    {
        if (_currentHovered == requester)
        {
            _currentHovered.ForceNormal();
            _currentHovered = null;
        }
    }
}
