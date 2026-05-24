using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance { get; private set; }

    public Text DebugLabel;
    public Text DebugLabel2;

    bool _debugVisible = false;
    int currentSId = 0;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        GameStateManager.OnStateChanged += HandleStateChanged;

        DebugLabel.gameObject.SetActive(true);
        DebugLabel2.gameObject.SetActive(true);
        GameManager.SkillReleased += OnSkillReleased;
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
        DebugLabel.gameObject.SetActive(_debugVisible);
        DebugLabel2.gameObject.SetActive(_debugVisible);
    }

    void Update()
    {
        if (DebugLabel == null || !DebugLabel.gameObject.activeSelf) return;

        float dis = (GameManager.Instance.leftHand - GameManager.Instance.rightHand).magnitude;
        DebugLabel.text = $"Using Device: {(InputManager.useKinect ? "Kinect" : "Mouse")}\n" +
                          $"Left Hand: {GameManager.Instance.leftHand}\n" +
                          $"Right Hand: {GameManager.Instance.rightHand}\n" +
                          $"Distance: {dis}\n" +
                          $"Skill: {currentSId}\n";
    }

    void OnSkillReleased(int sId)
    {
        Skill skill = (Skill)sId;
        this.currentSId = sId;
    }
}
