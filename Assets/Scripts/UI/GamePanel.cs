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
        InputManager.DetectedPose += HandlePose;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        GamePhaseManager.OnPhaseChanged -= HandlePhaseChanged;
        InputManager.DetectedPose -= HandlePose;
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

    void HandlePose(int sid)
    {
        Skill skill = (Skill)sid;
        switch (skill)
        {
            // TODO: Highlight the skill
            case Skill.HadokenLeft:
            case Skill.HadokenRight:
                break;
            case Skill.Explosion:
                break;
            case Skill.Swatter:
                break;
        }
    }
}
