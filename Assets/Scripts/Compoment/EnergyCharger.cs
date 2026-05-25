using UnityEngine;

// 每次拍手為 EnergyManager 充能，掛在任意 GameObject 上即生效
public class EnergyCharger : MonoBehaviour
{
    [Tooltip("每次拍手獲得的 EP")]
    public int EnergyPerClap = 10;

    void OnEnable()  => GameManager.OnHandClap += OnHandClap;
    void OnDisable() => GameManager.OnHandClap -= OnHandClap;

    void OnHandClap() => EnergyManager.Instance?.AddEnergy(EnergyPerClap);
}
