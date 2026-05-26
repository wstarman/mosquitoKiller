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
    [Tooltip("依序拖入 Block_00 ~ Block_19")]
    public Image[] Blocks = new Image[20];

    [Tooltip("疊在 Bar 中央的數值文字，格式：60 / 100")]
    public TMP_Text ValueLabel;

    public Color FilledColor = new Color(0.2f, 0.55f, 1f);
    public Color EmptyColor  = new Color(0.08f, 0.08f, 0.15f);

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
        int filled = Mathf.RoundToInt(normalized * Blocks.Length);
        for (int i = 0; i < Blocks.Length; i++)
        {
            if (Blocks[i] != null)
                Blocks[i].color = i < filled ? FilledColor : EmptyColor;
        }

        if (ValueLabel != null && EnergyManager.Instance != null)
            ValueLabel.text = $"{EnergyManager.Instance.CurrentEP} / {EnergyManager.Instance.MaxEP}";
    }
}
