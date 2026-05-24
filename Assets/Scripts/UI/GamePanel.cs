using UnityEngine;
using TMPro;

public class GamePanel : BasePanel
{
    public TMP_Text PhaseLabel;
    public TMP_Text ScoreLabel;

    protected override GameState TargetState => GameState.Playing;

    protected override void Awake()
    {
        base.Awake();
        GamePhaseManager.OnPhaseChanged += HandlePhaseChanged;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GamePhaseManager.OnPhaseChanged -= HandlePhaseChanged;
    }

    void Update()
    {
        if (ScoreLabel != null && ScoreManager.Instance != null)
            ScoreLabel.text = $"Score: {ScoreManager.Instance.Score}";
    }

    void HandlePhaseChanged(GamePhase from, GamePhase to)
    {
        if (PhaseLabel != null)
            PhaseLabel.text = $"Phase: {to}";
    }
}
