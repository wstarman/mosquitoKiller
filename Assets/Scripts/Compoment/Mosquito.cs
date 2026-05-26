using UnityEngine;

/// <summary>
/// 最普通的蚊子，在畫面中隨機遊蕩，被 ClapEffect 碰到一次即死。
/// 繼承自 BaseEnemy，只需實作移動邏輯。
///
/// SpawnMode 行為：
///   InsideScreen → 直接在畫面內遊蕩
///   FromEdge     → 先飛入畫面中心區域，再切換成遊蕩模式
/// </summary>
public class Mosquito : BaseEnemy
{
    [Header("Movement")]
    public float MoveSpeed = 2f;
    [Tooltip("距畫面邊緣的安全距離，遊蕩目標點不會太靠近邊界")]
    public float EdgeMargin = 0.5f;

    Vector3 _waypoint;
    bool _isEntering; // 從畫面外飛入中，尚未抵達畫面內

    public override void OnSpawn()
    {
        base.OnSpawn();
        _isEntering = !IsInsideCamera();
        PickNewWaypoint();
    }

    protected override void UpdateMovement()
    {
        transform.position = Vector3.MoveTowards(transform.position, _waypoint, MoveSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _waypoint) < 0.2f)
        {
            // 抵達目標點後，若仍在畫面外繼續飛往下一點，直到進入畫面
            _isEntering = !IsInsideCamera();
            PickNewWaypoint();
        }
    }

    // 遊蕩目標點限制在相機可視範圍內
    void PickNewWaypoint()
    {
        Camera cam = Camera.main;
        float halfH = cam.orthographicSize - EdgeMargin;
        float halfW = halfH * cam.aspect - EdgeMargin;

        _waypoint = new Vector3(
            Random.Range(-halfW, halfW),
            Random.Range(-halfH, halfH),
            0f
        );
    }

    bool IsInsideCamera()
    {
        if (Camera.main == null) return true;
        Vector3 vp = Camera.main.WorldToViewportPoint(transform.position);
        return vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;
    }
}
