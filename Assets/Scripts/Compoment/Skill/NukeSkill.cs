using UnityEngine;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// 控制核爆技能的邏輯，包含UI警報、音效播放、傷害結算及全螢幕閃光特效。
/// </summary>
public class NukeSkill : MonoBehaviour
{
    [Header("核爆設定")]
    [Tooltip("爆炸時生成的視覺特效預製件")]
    public GameObject nukeEffectPrefab;
    [Tooltip("受核爆影響的敵軍層級")]
    public LayerMask enemyLayer;
    [Tooltip("對應 EnergyManager 的技能 ID")]
    public int nukeSkillId = 3;

    [Header("UI 與音效設定")]
    [Tooltip("場景中隱藏的警報 UI 物件")]
    public GameObject radiationIcon;
    [Tooltip("AudioManager 字典中的音效鍵值")]
    public string alertClipName = "NuclearWarningSFX";
    [Tooltip("用於獲取音效長度的參考檔案")]
    public AudioClip alertClipReference;

    private void OnEnable()
    {
        EnergyManager.OnSkillActivated += HandleSkillActivated;
    }

    private void OnDisable()
    {
        EnergyManager.OnSkillActivated -= HandleSkillActivated;
    }

    /// <summary>
    /// 事件監聽器：當管理器通知技能觸發時執行
    /// </summary>
    private void HandleSkillActivated(int sId)
    {
        if (sId == nukeSkillId)
        {
            ActivateNuke();
        }
    }

    /// <summary>
    /// 啟動核爆程序：顯示警報、播放音效並排程爆炸
    /// </summary>
    public void ActivateNuke()
    {
        if (radiationIcon != null)
        {
            // 顯示警報圖標
            radiationIcon.SetActive(true);

            // 播放警報音效
            AudioManager.Instance.PlaySFX(alertClipName, 0.05f);

            // 根據音效時長決定爆炸延遲，若無參考則預設 3 秒
            float duration = alertClipReference != null ? alertClipReference.length : 3.0f;

            // 確保不會重複排程，並於音效結束時執行爆炸
            CancelInvoke(nameof(ExecuteNukeLogic));
            Invoke(nameof(ExecuteNukeLogic), duration);
        }
    }

    /// <summary>
    /// 執行核爆核心邏輯：隱藏UI、生成特效、結算傷害並觸發閃光
    /// </summary>
    private void ExecuteNukeLogic()
    {
        // 隱藏警報 UI
        if (radiationIcon != null) radiationIcon.SetActive(false);

        // 生成視覺特效 (於原點)
        if (nukeEffectPrefab != null)
            Instantiate(nukeEffectPrefab, Vector3.zero, Quaternion.identity);

        // 啟動全螢幕閃白效果
        StartCoroutine(ScreenFlashEffect());

        // 傷害結算：掃描場景內所有目標
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            // 檢查目標是否屬於敵軍層級
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

    /// <summary>
    /// 協程：動態產生全螢幕白色影像並漸隱，模擬核爆強光
    /// </summary>
    private IEnumerator ScreenFlashEffect()
    {
        // 1. 動態建立 Canvas 作為 UI 容器
        GameObject flashCanvasObj = new GameObject("FlashCanvas");
        Canvas canvas = flashCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999; // 強制設定於最上層

        // 2. 動態建立白光 Image 物件
        GameObject flashImgObj = new GameObject("FlashImage");
        flashImgObj.transform.SetParent(flashCanvasObj.transform, false);
        Image flashImg = flashImgObj.AddComponent<Image>();
        flashImg.color = Color.white;

        // 3. 設定 RectTransform 填滿螢幕
        RectTransform rect = flashImg.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        // 4. 動畫漸隱循環
        float flashDuration = 1.5f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            // 隨時間線性插值計算透明度
            float alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            flashImg.color = new Color(1f, 1f, 1f, alpha);

            yield return null; // 等待下一幀
        }

        // 5. 特效結束，銷毀動態物件以節省資源
        Destroy(flashCanvasObj);
    }
}