using UnityEngine;

public abstract class BasePanel : MonoBehaviour
{
    protected abstract GameState TargetState { get; }

    protected virtual void Awake()
    {
        GameStateManager.OnStateChanged += HandleStateChanged;
        gameObject.SetActive(false);
    }

    protected virtual void OnDestroy()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState from, GameState to)
    {
        if (to == TargetState) OnShow();
        else OnHide();
    }

    protected virtual void OnShow() => gameObject.SetActive(true);
    protected virtual void OnHide() => gameObject.SetActive(false);
}
