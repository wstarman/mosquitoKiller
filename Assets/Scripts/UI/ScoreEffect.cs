using UnityEngine;
using TMPro; using System.Collections;

public class ScoreEffect : MonoBehaviour
{
    private TextMeshProUGUI scoreText;
    private Vector3 originalScale;
    private Color originalColor;

    [Header("特效設定")]
    public float popSize = 1.5f;     
    public float duration = 0.2f;    
    public Color highlightColor = Color.yellow; 

    void Start()
    {
        scoreText = GetComponent<TextMeshProUGUI>();
        originalScale = transform.localScale;
        originalColor = scoreText.color;
        ScoreManager.scoreAdded += PlayPopEffect;
    }

    private void OnDestroy()
    {
        ScoreManager.scoreAdded -= PlayPopEffect;
    }


    public void PlayPopEffect()
    {
        
        StopAllCoroutines(); 
        
        StartCoroutine(PopRoutine());
    }

    
    IEnumerator PopRoutine()
    {
        float timer = 0f;

        
        transform.localScale = originalScale * popSize;
        scoreText.color = highlightColor;

        
        while (timer < duration)
        {
            timer += Time.deltaTime;
            float progress = timer / duration; // 計算進度 (0 到 1)

            
            transform.localScale = Vector3.Lerp(originalScale * popSize, originalScale, progress);
            scoreText.color = Color.Lerp(highlightColor, originalColor, progress);

            yield return null; 
        }

        
        transform.localScale = originalScale;
        scoreText.color = originalColor;
    }
}