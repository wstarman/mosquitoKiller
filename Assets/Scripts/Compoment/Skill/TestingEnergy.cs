using UnityEngine;

public class CheatEnergy : MonoBehaviour
{
    void Update()
    {
        // 偵測按下空白鍵
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (EnergyManager.Instance != null)
            {
                // 取得最大能量值
                int maxEp = EnergyManager.Instance.MaxEP;

                EnergyManager.Instance.AddEnergy(maxEp); 

            }
        }
    }
}