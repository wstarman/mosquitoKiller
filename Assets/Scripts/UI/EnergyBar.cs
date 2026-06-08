using UnityEngine;

/// <summary>
/// 掛在 energyBar GameObject 上，將 EnergyManager.OnEnergyChanged 轉發給 SliderBar。
///
/// 場景結構：
///   energyBar (此腳本)
///   └── SliderBar (Slider 包裝元件)
/// </summary>
public class EnergyBar : MonoBehaviour
{
    public SliderBar bar;

    void OnEnable()
    {
        EnergyManager.OnEnergyChanged += Refresh;
        Refresh(EnergyManager.Instance != null
            ? (float)EnergyManager.Instance.CurrentEP / EnergyManager.Instance.MaxEP
            : 0f);
    }

    void OnDisable() => EnergyManager.OnEnergyChanged -= Refresh;

    void Refresh(float normalized)
    {
        bar.SetHealth(normalized*100);
    }
}
