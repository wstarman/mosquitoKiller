using UnityEngine;
using TMPro;
using System.Threading;

public class ResultPanel : BasePanel
{
    public TMP_Text FinalScoreLabel;
    public WorldSpaceButton RetryButton;
    public WorldSpaceButton MainMenuButton;
    public float buttonShowDelay = 1.0f;
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
        RetryButton.gameObject.SetActive(false);
        MainMenuButton.gameObject.SetActive(false);
        Invoke(nameof(enableButton), buttonShowDelay);

        if (FinalScoreLabel != null && ScoreManager.Instance != null)
            FinalScoreLabel.text = $"Final Score: {ScoreManager.Instance.Score}";
    }

    void enableButton()
    {
        RetryButton.gameObject.SetActive(true);
        MainMenuButton.gameObject.SetActive(true);
    }
}
