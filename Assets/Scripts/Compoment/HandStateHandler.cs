using UnityEngine;

// 掛在 leftHand 和 rightHand 上，根據遊戲狀態切換 Cursor / Player 模式
public class HandStateHandler : MonoBehaviour
{
    public Vector3 CursorScale = new Vector3(0.1f, 0.1f, 1f);
    public Vector3 PlayerScale = new Vector3(0.25f, 0.25f, 1f);

    void Awake()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
    }

    void OnDestroy()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState from, GameState to)
    {
        transform.localScale = to == GameState.Playing ? PlayerScale : CursorScale;
    }
}
