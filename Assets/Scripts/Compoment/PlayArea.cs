using UnityEngine;

/// <summary>
/// 定義蚊子的有效活動範圍（世界座標）。掛在一個空物件上即可：
///   Center  = 此物件的 transform.position
///   大小    = Size（世界長寬，中心對齊 position）
///
/// 與顯示完全無關——UI 邊框只是視覺裝飾，需手動對齊到這個範圍。
/// Scene 視窗會以 Gizmo 畫出範圍方框，方便對齊。
/// 場景中沒有 PlayArea 時，MosquitoBase 退回使用相機可視範圍。
/// </summary>
public class PlayArea : MonoBehaviour
{
    public static PlayArea Instance { get; private set; }

    [Tooltip("遊玩範圍的世界長寬（中心為此物件的位置）")]
    public Vector2 Size = new(10f, 6f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    /// <summary>邊界中心（世界座標）。</summary>
    public Vector2 Center => transform.position;

    /// <summary>邊界半寬高（世界座標）。</summary>
    public Vector2 HalfExtents => Size * 0.5f;

    // 在 Scene 視窗畫出範圍方框，方便把 UI 邊框對齊到此世界範圍
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(Size.x, Size.y, 0f));
    }
}
