using UnityEngine;

public class ClapScorer : MonoBehaviour
{
    [Tooltip("每次拍手獲得的分數")]
    public int PointsPerClap = 67;

    void OnEnable()
    {
        GameManager.OnHandClap += OnHandClap;
    }

    void OnDisable()
    {
        GameManager.OnHandClap -= OnHandClap;
    }

    void OnHandClap()
    {
        if (GameStateManager.Instance.CurrentState != GameState.Playing) return;
        ScoreManager.Instance.Add(PointsPerClap);
    }
}
