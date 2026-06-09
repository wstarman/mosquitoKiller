using UnityEngine;

/// <summary>
/// 掛在 healthBar GameObject 上，將 GameManager.OnHealthChanged 轉發給 SliderBar。
/// 仿 EnergyBar 結構。
///
/// 場景結構：
///   healthBar (此腳本)
///   └── SliderBar (Slider 包裝元件)
/// </summary>
public class HealthBar : MonoBehaviour
{
    public SliderBar bar;

    void OnEnable()
    {
        GameManager.OnHealthChanged += Refresh;
        Refresh(GameManager.Instance != null
            ? (float)GameManager.Instance.CurrentHP / GameManager.Instance.MaxHP
            : 1f);
    }

    void OnDisable() => GameManager.OnHealthChanged -= Refresh;

    void Refresh(float normalized)
    {
        bar.SetHealth(normalized * 100);
    }
}
