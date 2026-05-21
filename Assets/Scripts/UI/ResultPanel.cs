using UnityEngine;
using TMPro;

public class ResultPanel : BasePanel
{
    public TMP_Text FinalScoreLabel;
    public WorldSpaceButton RetryButton;
    public WorldSpaceButton MainMenuButton;

    protected override GameState TargetState => GameState.Result;

    protected override void Awake()
    {
        base.Awake();
        RetryButton.OnPressed.AddListener(() =>
            GameStateManager.Instance.Transition(GameState.Playing));
        MainMenuButton.OnPressed.AddListener(() =>
            GameStateManager.Instance.Transition(GameState.MainMenu));
    }

    protected override void OnShow()
    {
        base.OnShow();
        if (FinalScoreLabel != null && ScoreManager.Instance != null)
            FinalScoreLabel.text = $"Final Score: {ScoreManager.Instance.Score}";
    }
}
