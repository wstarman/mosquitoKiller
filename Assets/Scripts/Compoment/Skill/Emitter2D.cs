using UnityEngine;

public class Emitter2D : MonoBehaviour
{
    public GameObject hadokenPrefab;

    void OnEnable()
    {
        // 因為是靜態事件，直接用「類別名稱.事件」來訂閱
        EnergyManager.OnSkillActivated += HandleSkillActivated;
        Debug.Log("波動拳發射器已成功訂閱靜態事件！");
    }

    void OnDisable()
    {
        // 關閉或銷毀時，同樣直接取消訂閱即可
        EnergyManager.OnSkillActivated -= HandleSkillActivated;
    }

    void HandleSkillActivated(int sId)
    {
        Debug.Log($"收到技能發動訊號！技能 ID: {sId}");

        if (hadokenPrefab == null)
        {
            Debug.LogError("你沒有把波動拳的 Prefab 拖給發射器！");
            return;
        }

        if (sId == (int)Skill.HadokenLeft)
        {
            Vector3 spawnPos = GameManager.Instance != null ? GameManager.Instance.leftHand : transform.position;
            Instantiate(hadokenPrefab, spawnPos, Quaternion.identity);
            Debug.Log("發射左手波動拳！座標：" + spawnPos);
        }
        else if (sId == (int)Skill.HadokenRight)
        {
            Vector3 spawnPos = GameManager.Instance != null ? GameManager.Instance.rightHand : transform.position;
            Instantiate(hadokenPrefab, spawnPos, Quaternion.identity);
            Debug.Log("發射右手波動拳！座標：" + spawnPos);
        }
    }
}