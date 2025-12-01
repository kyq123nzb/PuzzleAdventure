using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PuzzleProgressUI : MonoBehaviour
{
    [Header("UI引用")]
    public Text progressText;           // 进度文本（如：3/9）
    public Image progressBar;           // 进度条
    public Text puzzleHintText;         // 提示文本
    public GameObject puzzleIconPrefab; // 拼图图标预制体
    public Transform iconsContainer;    // 图标容器
    
    [Header("视觉设置")]
    public Color collectedColor = Color.green;
    public Color missingColor = Color.gray;
    public float iconSpacing = 50f;
    
    private List<Image> puzzleIcons = new List<Image>();
    
    void Start()
    {
        InitializeUI();
        UpdateUI();
        
        // 订阅事件
        GameManager.OnPuzzleCollected += OnPuzzleCollected;
        GameManager.OnGameStateChanged += OnGameStateChanged;
    }
    
    void OnDestroy()
    {
        // 取消订阅
        GameManager.OnPuzzleCollected -= OnPuzzleCollected;
        GameManager.OnGameStateChanged -= OnGameStateChanged;
    }
    
    void InitializeUI()
    {
        if (iconsContainer != null && puzzleIconPrefab != null)
        {
            // 清除现有图标
            foreach (Transform child in iconsContainer)
            {
                Destroy(child.gameObject);
            }
            puzzleIcons.Clear();
            
            // 创建新的图标
            for (int i = 0; i < GameManager.Instance.TotalPuzzles; i++)
            {
                GameObject icon = Instantiate(puzzleIconPrefab, iconsContainer);
                
                // 计算位置
                RectTransform rect = icon.GetComponent<RectTransform>();
                rect.anchoredPosition = new Vector2(i * iconSpacing, 0);
                
                // 存储引用
                Image iconImage = icon.GetComponent<Image>();
                if (iconImage != null)
                {
                    puzzleIcons.Add(iconImage);
                }
            }
        }
    }
    
    void UpdateUI()
    {
        if (GameManager.Instance == null) return;
        
        int collected = GameManager.Instance.GetCollectedPuzzlesCount();
        int total = GameManager.Instance.TotalPuzzles;
        float progress = GameManager.Instance.GetProgressPercentage();
        
        // 更新文本
        if (progressText != null)
        {
            progressText.text = $"拼图: {collected}/{total}";
        }
        
        // 更新进度条
        if (progressBar != null)
        {
            progressBar.fillAmount = progress;
            progressBar.color = Color.Lerp(Color.red, Color.green, progress);
        }
        
        // 更新图标
        UpdatePuzzleIcons();
        
        // 更新提示
        UpdateHintText(collected, total);
    }
    
    void UpdatePuzzleIcons()
    {
        for (int i = 0; i < puzzleIcons.Count; i++)
        {
            if (i < GameManager.Instance.TotalPuzzles)
            {
                bool isCollected = GameManager.Instance.IsPuzzleCollected(i + 1);
                puzzleIcons[i].color = isCollected ? collectedColor : missingColor;
            }
        }
    }
    
    void UpdateHintText(int collected, int total)
    {
        if (puzzleHintText != null)
        {
            if (collected == 0)
            {
                puzzleHintText.text = "寻找散落的拼图碎片";
            }
            else if (collected < total / 2)
            {
                puzzleHintText.text = $"继续寻找，还差{total - collected}个拼图";
            }
            else if (collected < total)
            {
                puzzleHintText.text = $"快完成了，只差{total - collected}个拼图！";
            }
            else
            {
                puzzleHintText.text = "已收集所有拼图！";
            }
        }
    }
    
    // 事件处理
    void OnPuzzleCollected(int puzzleId)
    {
        UpdateUI();
        
        // 显示收集动画
        ShowCollectAnimation(puzzleId);
    }
    
    void OnGameStateChanged(GameManager.GameState newState)
    {
        if (newState == GameManager.GameState.Playing)
        {
            UpdateUI();
        }
    }
    
    void ShowCollectAnimation(int puzzleId)
    {
        // 这里可以添加收集时的UI动画
        // 比如让对应的拼图图标闪烁
        StartCoroutine(FlashIcon(puzzleId - 1));
    }
    
    IEnumerator FlashIcon(int iconIndex)
    {
        if (iconIndex >= 0 && iconIndex < puzzleIcons.Count)
        {
            Image icon = puzzleIcons[iconIndex];
            Color originalColor = icon.color;
            
            // 闪烁效果
            for (int i = 0; i < 3; i++)
            {
                icon.color = Color.yellow;
                yield return new WaitForSeconds(0.1f);
                icon.color = collectedColor;
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}