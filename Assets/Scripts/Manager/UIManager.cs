using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Text DebugLabel;

    bool _debugVisible = false;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        GameStateManager.OnStateChanged += HandleStateChanged;

        if (DebugLabel != null)
            DebugLabel.gameObject.SetActive(false);
    }

    void OnDestroy()
    {
        GameStateManager.OnStateChanged -= HandleStateChanged;
    }

    void HandleStateChanged(GameState from, GameState to)
    {
        RefreshDebugVisibility(to);
    }

    public void SetDebugVisible(bool visible)
    {
        _debugVisible = visible;
        RefreshDebugVisibility(GameStateManager.Instance.CurrentState);
    }

    void RefreshDebugVisibility(GameState state)
    {
        if (DebugLabel == null) return;
        DebugLabel.gameObject.SetActive(state == GameState.MainMenu && _debugVisible);
    }

    void Update()
    {
        if (DebugLabel == null || !DebugLabel.gameObject.activeSelf) return;

        float dis = (GameManager.Instance.leftHand - GameManager.Instance.rightHand).magnitude;
        DebugLabel.text = $"Using Device: {(InputManager.useKinect ? "Kinect" : "Mouse")}\n" +
                          $"Left Hand: {GameManager.Instance.leftHand}\n" +
                          $"Right Hand: {GameManager.Instance.rightHand}\n" +
                          $"Distance: {dis}";
    }
}
