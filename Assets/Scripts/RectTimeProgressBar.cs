using UnityEngine;
using UnityEngine.UI;

public class RectTimeProgressBar : MonoBehaviour
{
    [Header("UI组件")]
    [SerializeField] private RectTransform fillRect;    // 填充部分的RectTransform
    [SerializeField] private Image fillImage;           // 填充部分的Image
    [SerializeField] private Text timeText;             // 时间文本
    
    [Header("时间设置")]
    [SerializeField] [Range(1, 600)] private float totalTime = 60f;
    [SerializeField] private bool autoStart = true;
    [SerializeField] private bool countdown = true;     // true = 倒计时（从满到空）
    
    [Header("颜色设置")]
    [SerializeField] private Color startColor = Color.green;
    [SerializeField] private Color middleColor = Color.yellow;
    [SerializeField] private Color endColor = Color.red;
    [SerializeField] private bool useColorChange = true;
    
    private float currentTime;
    private bool isRunning = false;
    private float originalWidth;
    private float originalHeight;
    
    /// <summary>
    /// 倒计时：progress = 1→0  （满→空）
    /// 正计时：progress = 0→1
    /// </summary>
    public float Progress => countdown
        ? currentTime / totalTime
        : currentTime / totalTime;

    void Start()
    {
        Initialize();
        if (autoStart) StartTimer();
    }

    void Update()
    {
        if (!isRunning) return;

        if (countdown)
        {
            currentTime -= Time.deltaTime;
            if (currentTime <= 0)
            {
                currentTime = 0;
                isRunning = false;
                OnTimerComplete();
            }
        }
        else
        {
            currentTime += Time.deltaTime;
            if (currentTime >= totalTime)
            {
                currentTime = totalTime;
                isRunning = false;
                OnTimerComplete();
            }
        }

        UpdateProgress();
        UpdateTimeText();
    }

    private void Initialize()
    {
        if (fillRect == null)
        {
            Debug.LogError("请分配 fillRect！");
            enabled = false;
            return;
        }

        if (fillImage == null)
            fillImage = fillRect.GetComponent<Image>();

        originalWidth = fillRect.rect.width;
        originalHeight = fillRect.rect.height;

        currentTime = countdown ? totalTime : 0f;
        isRunning = false;

        UpdateProgress();
        UpdateTimeText();
    }

    /// <summary>
    /// 右向左减少（重点修改）
    /// </summary>
    private void UpdateProgress()
    {
        float progress = Progress;

        // 设置宽度，右侧缩小——只改宽度即可
        float newWidth = originalWidth * progress;
        fillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);

        // 注意：为了“右向左”效果，pivot 必须设为 (0, 0.5)
        // 你可以在 Editor 中设定，也可以自动设定：
        fillRect.pivot = new Vector2(0f, 0.5f);

        // 颜色变化（可选）
        if (useColorChange && fillImage != null)
        {
            if (progress > 0.5f)
            {
                fillImage.color = Color.Lerp(middleColor, startColor, (progress - 0.5f) * 2f);
            }
            else
            {
                fillImage.color = Color.Lerp(endColor, middleColor, progress * 2f);
            }
        }
    }

    private void UpdateTimeText()
    {
        if (timeText == null) return;

        float displayTime = countdown ? currentTime : totalTime - currentTime;
        int seconds = Mathf.CeilToInt(displayTime);

        if (seconds >= 60)
        {
            int min = seconds / 60;
            int sec = seconds % 60;
            timeText.text = $"{min}:{sec:D2}";
        }
        else
        {
            timeText.text = $"{seconds}s";
        }

        // 低于10秒提醒
        if (countdown && seconds <= 10)
        {
            timeText.color = Color.red;

            if (seconds <= 5)
            {
                float pulse = 1.2f + Mathf.Sin(Time.time * 10f) * 0.2f;
                timeText.transform.localScale = new Vector3(pulse, pulse, 1);
            }
        }
        else
        {
            timeText.color = Color.white;
            timeText.transform.localScale = Vector3.one;
        }
    }

    private void OnTimerComplete()
    {
        Debug.Log("计时完成！");
        StartCoroutine(FlashEffect());
    }

    private System.Collections.IEnumerator FlashEffect()
    {
        if (fillImage == null) yield break;

        bool original = fillImage.enabled;

        for (int i = 0; i < 6; i++)
        {
            fillImage.enabled = !fillImage.enabled;
            yield return new WaitForSeconds(0.1f);
        }

        fillImage.enabled = original;
    }

    // —— 公开方法 ——

    public void StartTimer() => isRunning = true;
    public void PauseTimer() => isRunning = false;

    public void ResetTimer()
    {
        currentTime = countdown ? totalTime : 0f;
        isRunning = false;

        UpdateProgress();
        UpdateTimeText();
    }

    public void SetTotalTime(float t)
    {
        totalTime = Mathf.Max(1f, t);
        currentTime = Mathf.Clamp(currentTime, 0, totalTime);
        UpdateProgress();
        UpdateTimeText();
    }

    public void SkipTime(float sec)
    {
        currentTime = countdown ? currentTime - sec : currentTime + sec;
        currentTime = Mathf.Clamp(currentTime, 0, totalTime);

        UpdateProgress();
        UpdateTimeText();
    }

    public void SetProgress(float p)
    {
        p = Mathf.Clamp01(p);
        currentTime = totalTime * p;
        UpdateProgress();
        UpdateTimeText();
    }

    public float GetProgress() => Progress;
}
