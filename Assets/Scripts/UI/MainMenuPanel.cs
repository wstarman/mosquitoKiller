using UnityEngine;

public class MainMenuPanel : BasePanel
{
    public WorldSpaceButton StartButton;
    public WorldSpaceToggleButton DebugButton;

    protected override GameState TargetState => GameState.MainMenu;

    protected override void Awake()
    {
        base.Awake();
        StartButton.OnPressed.AddListener(() =>
            GameStateManager.Instance.Transition(GameState.Playing));
    }

    protected override void OnShow()
    {
        base.OnShow();
        if (DebugButton != null) DebugButton.gameObject.SetActive(true);
    }

    protected override void OnHide()
    {
        base.OnHide();
        if (DebugButton != null) DebugButton.gameObject.SetActive(false);
    }
}
