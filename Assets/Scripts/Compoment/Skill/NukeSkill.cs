using UnityEngine;

public class NukeSkill : MonoBehaviour
{
    [Header("核爆設定")]
    public GameObject nukeEffectPrefab;
    public LayerMask enemyLayer;
    public int nukeSkillId = 3; 

    void OnEnable()
    {
        // 訂閱事件：當技能被觸發時執行
        EnergyManager.OnSkillActivated += HandleSkillActivated;
    }

    void OnDisable()
    {
        // 取消訂閱：防止物件銷毀後還試圖接收事件，導致報錯
        EnergyManager.OnSkillActivated -= HandleSkillActivated;
    }

    void HandleSkillActivated(int sId)
    {
        // 檢查是否為核爆的 ID
        if (sId == nukeSkillId)
        {
            ActivateNuke();
        }
    }

    public void ActivateNuke()
    {
        if (nukeEffectPrefab != null)
            Instantiate(nukeEffectPrefab, Vector3.zero, Quaternion.identity);

        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);

        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & enemyLayer) != 0)
            {
                var damageable = obj.GetComponent<MosquitoBase>();
                if (damageable != null)
                {
                    damageable.TakeDamage(9999, DamageSource.Explosion);
                }
            }
        }

    }
}