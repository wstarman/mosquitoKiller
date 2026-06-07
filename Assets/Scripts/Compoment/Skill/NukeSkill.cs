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
    public string boomClipName = "VineBoom";
    [Tooltip("用於獲取音效長度的參考檔案")]
    public AudioClip alertClipReference;

    [Header("擊殺特效")]
    [Tooltip("蚊子死後原地留下的圖片預製件")]
    public GameObject ashPrefab;

    [Header("彩蛋特效")]
    [Tooltip("閃白時顯示的彩蛋圖片 Sprite。請在 Inspector 中指派圖片。")]
    public Sprite easterEggSprite;
    [Tooltip("彩蛋圖片在白屏幕上的初始透明度 (0 ~ 1)")]
    [Range(0f, 1f)]
    public float easterEggAlpha = 0.5f;
    [Tooltip("剛出現時的變形比例 (X為寬, Y為高)。預設 0.2, 3 代表極度瘦長")]
    public Vector2 easterEggStartScale = new Vector2(0.2f, 3f);
    [Tooltip("消失前的變形比例。預設 4, 0.5 代表極度扁平且寬大")]
    public Vector2 easterEggEndScale = new Vector2(4f, 0.5f);

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

            GameManager.Instance.isSkillReleasing = true;

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
                    damageable.TakeDamage(9999, DamageSource.SkillExplotion);
                }
            }
        }
        AudioManager.Instance.PlaySFX(boomClipName, 0.15f);
        GameManager.Instance.isSkillReleasing = false;
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

        RectTransform whiteRect = flashImg.GetComponent<RectTransform>();
        whiteRect.anchorMin = Vector2.zero;
        whiteRect.anchorMax = Vector2.one;
        whiteRect.offsetMin = Vector2.zero;
        whiteRect.offsetMax = Vector2.zero;

        // --- 彩蛋圖片設定 ---
        GameObject easterEggObj = null;
        Image easterEggImg = null;
        Color easterEggColor = Color.white;

        if (easterEggSprite != null)
        {
            easterEggObj = new GameObject("EasterEggImage");
            easterEggObj.transform.SetParent(flashCanvasObj.transform, false);

            easterEggImg = easterEggObj.AddComponent<Image>();
            easterEggImg.sprite = easterEggSprite;
            // 關鍵修改：關閉保持比例，讓圖片可以被暴力拉伸
            easterEggImg.preserveAspect = false;

            // 讓圖片預設就填滿整個螢幕，之後再用 Scale 放大扭曲
            RectTransform eggRect = easterEggImg.GetComponent<RectTransform>();
            eggRect.anchorMin = Vector2.zero;
            eggRect.anchorMax = Vector2.one;
            eggRect.offsetMin = Vector2.zero;
            eggRect.offsetMax = Vector2.zero;

            easterEggColor = new Color(1f, 1f, 1f, easterEggAlpha);
            easterEggImg.color = easterEggColor;

            // 套用初始超醜比例
            eggRect.localScale = new Vector3(easterEggStartScale.x, easterEggStartScale.y, 1f);
        }

        float flashDuration = 1.5f;
        float elapsed = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float fadeRatio = elapsed / flashDuration;

            float whiteAlpha = Mathf.Lerp(1f, 0f, fadeRatio);
            flashImg.color = new Color(1f, 1f, 1f, whiteAlpha);

            if (easterEggImg != null)
            {
                // 透明度漸隱
                float currentEggAlpha = Mathf.Lerp(easterEggAlpha, 0f, fadeRatio);
                easterEggImg.color = new Color(easterEggColor.r, easterEggColor.g, easterEggColor.b, currentEggAlpha);

                // 關鍵修改：動態暴力變形 (從瘦長變成寬扁)
                float currentScaleX = Mathf.Lerp(easterEggStartScale.x, easterEggEndScale.x, fadeRatio);
                float currentScaleY = Mathf.Lerp(easterEggStartScale.y, easterEggEndScale.y, fadeRatio);
                easterEggImg.transform.localScale = new Vector3(currentScaleX, currentScaleY, 1f);
            }

            yield return null;
        }

        Destroy(flashCanvasObj);
    }
}