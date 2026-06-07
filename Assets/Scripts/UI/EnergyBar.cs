using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 掛在 energyBar GameObject 上，依 EnergyManager.OnEnergyChanged 填色 20 個 block 並更新數值 label。
///
/// 場景結構：
///   energyBar (此腳本)
///   └── Frame (Image — 邊框色)
///       └── Background (Image — panel 底色，四邊內縮製造邊框寬度)
///           ├── BlocksContainer (HorizontalLayoutGroup)
///           │   └── Block_00 ~ Block_19 (Image × 20)
///           └── ValueLabel (TMP_Text — 疊在 Bar 中央，Raycast Target 關閉)
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
