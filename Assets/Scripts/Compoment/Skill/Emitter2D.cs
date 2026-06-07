using UnityEngine;

public class Emitter2D : MonoBehaviour
{
    public GameObject hadokenPrefab;

    void OnEnable()
    {
        // 因為是靜態事件，直接用「類別名稱.事件」來訂閱
        EnergyManager.OnSkillActivated += HandleSkillActivated;
    }

    void OnDisable()
    {
        // 關閉或銷毀時，同樣直接取消訂閱即可
        EnergyManager.OnSkillActivated -= HandleSkillActivated;
    }

    void HandleSkillActivated(int sId)
    {

        if (hadokenPrefab == null) return;

        // 判斷是否為波動拳相關技能
        if (sId == (int)Skill.HadokenLeft || sId == (int)Skill.HadokenRight)
        {
            // 1. 決定座標
            Vector3 spawnPos = (sId == (int)Skill.HadokenLeft) 
                ? GameManager.Instance.leftHand 
                : GameManager.Instance.rightHand;

            // 2. 生成物件
            GameObject go = Instantiate(hadokenPrefab, spawnPos, Quaternion.identity);
            
            // 3. --- 關鍵修正：呼叫 Initialize ---
            Hadoken2DScript script = go.GetComponent<Hadoken2DScript>();
            if (script != null)
            {
                script.Initialize(sId); // 把正確的 ID 傳進去，它才會正確設定 _handType
            }
        }
    }
}