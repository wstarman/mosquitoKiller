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

    [Header("擊殺特效")]
    [Tooltip("蚊子死後原地留下的圖片預製件")]
    public GameObject ashPrefab;

    private void OnEnable()
    {
        EnergyManager.OnSkillActivated += HandleSkillActivated;
    }

    private void OnDisable()
    {
        EnergyManager.OnSkillActivated -= HandleSkillActivated;
    }

    private void HandleSkillActivated(int sId)
    {
        if (sId == nukeSkillId)
        {
            ActivateNuke();
        }
    }

    public void ActivateNuke()
    {
        if (radiationIcon != null)
        {
            radiationIcon.SetActive(true);
            AudioManager.Instance.PlaySFX(alertClipName, 0.05f);
            float duration = alertClipReference != null ? alertClipReference.length : 3.0f;

            CancelInvoke(nameof(ExecuteNukeLogic));
            Invoke(nameof(ExecuteNukeLogic), duration);
        }
    }

    private void ExecuteNukeLogic()
    {
        if (radiationIcon != null) radiationIcon.SetActive(false);
        if (nukeEffectPrefab != null) Instantiate(nukeEffectPrefab, Vector3.zero, Quaternion.identity);

        StartCoroutine(ScreenFlashEffect());

        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & enemyLayer) != 0)
            {
                var damageable = obj.GetComponent<MosquitoBase>();
                if (damageable != null)
                {
                    // 1. 在蚊子死亡前，於原地生成一張圖片
                    if (ashPrefab != null)
                    {
                        GameObject ash = Instantiate(ashPrefab, obj.transform.position, Quaternion.identity);

                        // 嘗試把這張圖片強制染成黑色
                        SpriteRenderer sr = ash.GetComponent<SpriteRenderer>();
                        if (sr != null) sr.color = Color.black;

                        // 設定 3 秒後自動刪除該圖片
                        Destroy(ash, 3.0f);
                    }

                    // 2. 造成核爆傷害
                    damageable.TakeDamage(9999, DamageSource.Explosion);
                }
            }
        }
    }

    private IEnumerator ScreenFlashEffect()
    {
        GameObject flashCanvasObj = new GameObject("FlashCanvas");
        Canvas canvas = flashCanvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 999;

        GameObject flashImgObj = new GameObject("FlashImage");
        flashImgObj.transform.SetParent(flashCanvasObj.transform, false);
        Image flashImg = flashImgObj.AddComponent<Image>();
        flashImg.color = Color.white;

        RectTransform rect = flashImg.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;

        float flashDuration = 1.5f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / flashDuration);
            flashImg.color = new Color(1f, 1f, 1f, alpha);
            yield return null;
        }

        Destroy(flashCanvasObj);
    }
}