using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

/// <summary>
/// 拼图进度综合展示 - 负责图标和文本，进度条控制交给ProgressBarFill
/// </summary>
public class PuzzleProgressDisplay : MonoBehaviour
{
    [Header("进度条组件")]
    public ProgressBarFill progressBarFill;     // 进度条控制器
    public TextMeshProUGUI progressText;        // 进度文本
    
    [Header("拼图图标设置")]
    public Transform iconsContainer;            // 图标容器
    public GameObject iconPrefab;               // 图标预制体
    public List<Sprite> collectedIconSprites = new List<Sprite>(); // 每个拼图的收集图标（按ID顺序）
    public Sprite defaultUncollectedIcon;       // 默认未收集图标
    
    [Header("图标颜色设置")]
    public Color collectedColor = Color.white;           // 已收集颜色
    public Color uncollectedColor = new Color(1f, 1f, 1f, 0.3f); // 未收集颜色
    
    [Header("布局设置")]
    public int maxIconsPerRow = 9;              // 每行最多图标数
    public float iconSpacing = 60f;             // 图标间距
    public Vector2 iconSize = new Vector2(60, 60); // 图标大小
    
    [Header("文本显示")]
    public string progressFormat = "收集进度: {0}/{1}"; // 进度文本格式
    
    [Header("动画设置")]
    public bool animateCollect = true;          // 是否播放收集动画
    public float iconRevealDelay = 0.1f;        // 图标显示延迟
    
    [Header("文本效果设置")]
    public bool addTextOutline = true;          // 是否为收集的文本添加轮廓
    public Color outlineColor = Color.yellow;   // 轮廓颜色
    public Vector2 outlineDistance = new Vector2(2, 2); // 轮廓距离
    
    // 存储图标组件
    private List<Image> iconImages = new List<Image>();
    private List<TextMeshProUGUI> iconTexts = new List<TextMeshProUGUI>();
    private List<RectTransform> iconTransforms = new List<RectTransform>();
    private List<int> iconPuzzleIds = new List<int>(); // 存储每个图标对应的拼图ID
    
    void Start()
    {
        // 先尝试自动查找组件
        AutoFindComponents();
        
        // 检查必要的组件
        if (progressBarFill == null)
        {
            Debug.LogError("PuzzleProgressDisplay: 未找到 ProgressBarFill 组件！");
        }
        
        if (progressText == null)
        {
            Debug.LogError("PuzzleProgressDisplay: 未找到 progressText 组件！");
        }
        
        InitializeDisplay();
        
        // 订阅事件
        if (GameManager.Instance != null)
        {
            GameManager.OnPuzzleCollected += OnPuzzleCollected;
            GameManager.OnGameStateChanged += OnGameStateChanged;
        }
    }
    
    /// <summary>
    /// 自动查找缺失的组件
    /// </summary>
    void AutoFindComponents()
    {
        if (progressBarFill == null)
            progressBarFill = GetComponentInChildren<ProgressBarFill>();
        
        if (progressText == null)
            progressText = GetComponentInChildren<TextMeshProUGUI>();
        
        if (iconsContainer == null)
        {
            // 尝试查找名为"IconsContainer"的子对象
            Transform found = transform.Find("IconsContainer");
            if (found == null)
            {
                // 如果没有找到，创建一个
                GameObject container = new GameObject("IconsContainer");
                container.transform.SetParent(transform);
                iconsContainer = container.transform;
                
                // 添加RectTransform并设置为网格布局
                RectTransform rt = container.AddComponent<RectTransform>();
                
                // 设置为拉伸到父对象
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.offsetMin = new Vector2(20, 20);
                rt.offsetMax = new Vector2(-20, -20);
                
                // 添加网格布局组件
                GridLayoutGroup grid = container.AddComponent<GridLayoutGroup>();
                grid.cellSize = iconSize;
                grid.spacing = new Vector2(iconSpacing, iconSpacing);
                grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
                grid.startAxis = GridLayoutGroup.Axis.Horizontal;
                grid.childAlignment = TextAnchor.MiddleCenter;
                grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                grid.constraintCount = maxIconsPerRow;
            }
            else
            {
                iconsContainer = found;
            }
        }
    }
    
    void OnDestroy()
    {
        // 取消订阅
        if (GameManager.Instance != null)
        {
            GameManager.OnPuzzleCollected -= OnPuzzleCollected;
            GameManager.OnGameStateChanged -= OnGameStateChanged;
        }
    }
    
    void InitializeDisplay()
    {
        // 获取总拼图数
        int totalPuzzles = GameManager.Instance != null ? GameManager.Instance.TotalPuzzles : 9;
        
        Debug.Log($"初始化拼图显示，总共 {totalPuzzles} 个拼图");
        
        // 创建图标
        CreatePuzzleIcons(totalPuzzles);
        
        // 初始更新
        UpdateProgressDisplay();
        UpdateAllIcons();
    }
    
    void CreatePuzzleIcons(int totalPuzzles)
    {
        if (iconsContainer == null)
        {
            Debug.LogWarning("图标容器未设置");
            return;
        }
        
        // 清除现有图标
        foreach (Transform child in iconsContainer)
        {
            Destroy(child.gameObject);
        }
        
        iconImages.Clear();
        iconTexts.Clear();
        iconTransforms.Clear();
        iconPuzzleIds.Clear();
        
        // 创建图标
        for (int i = 1; i <= totalPuzzles; i++)
        {
            // 实例化图标（如果有预制体使用预制体，否则动态创建）
            GameObject iconObj = CreateIconObject(i);
            
            // 获取或添加组件
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage == null)
            {
                iconImage = iconObj.AddComponent<Image>();
            }
            
            // 添加编号文本
            GameObject textObj = new GameObject("PuzzleNumber");
            textObj.transform.SetParent(iconObj.transform);
            TextMeshProUGUI iconText = textObj.AddComponent<TextMeshProUGUI>();
            
            // 设置TextMeshPro组件
            iconText.alignment = TextAlignmentOptions.Center;
            iconText.fontSize = 16;
            iconText.color = Color.white;
            iconText.text = i.ToString();
            
            // 设置文本的RectTransform - 覆盖整个图标
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(2, 2);
            textRect.offsetMax = new Vector2(-2, -2);
            textRect.pivot = new Vector2(0.5f, 0.5f);
            
            RectTransform rectTransform = iconObj.GetComponent<RectTransform>();
            
            // 存储组件
            iconImages.Add(iconImage);
            iconTexts.Add(iconText);
            iconTransforms.Add(rectTransform);
            iconPuzzleIds.Add(i); // 记录这个图标对应的拼图ID
            
            // 添加事件监听（可选）
            AddIconEventListeners(iconObj, i);
            
            Debug.Log($"创建拼图图标: ID {i}");
        }
        
        // 确保容器布局更新
        LayoutRebuilder.ForceRebuildLayoutImmediate(iconsContainer.GetComponent<RectTransform>());
    }
    
    /// <summary>
    /// 创建图标对象
    /// </summary>
    GameObject CreateIconObject(int puzzleId)
    {
        GameObject iconObj;
        
        if (iconPrefab != null)
        {
            // 使用预制体
            iconObj = Instantiate(iconPrefab, iconsContainer);
        }
        else
        {
            // 动态创建
            iconObj = new GameObject($"PuzzleIcon_{puzzleId}");
            iconObj.transform.SetParent(iconsContainer);
            
            // 添加RectTransform并设置大小
            RectTransform rt = iconObj.AddComponent<RectTransform>();
            rt.sizeDelta = iconSize;
        }
        
        iconObj.name = $"PuzzleIcon_{puzzleId}";
        return iconObj;
    }
    
    void AddIconEventListeners(GameObject iconObj, int puzzleId)
    {
        // 添加EventTrigger组件用于交互
        EventTrigger trigger = iconObj.GetComponent<EventTrigger>();
        if (trigger == null)
        {
            trigger = iconObj.AddComponent<EventTrigger>();
        }
        
        // 点击事件
        EventTrigger.Entry clickEntry = new EventTrigger.Entry();
        clickEntry.eventID = EventTriggerType.PointerClick;
        clickEntry.callback.AddListener((data) => OnIconClicked(puzzleId));
        trigger.triggers.Add(clickEntry);
        
        // 悬停事件
        EventTrigger.Entry enterEntry = new EventTrigger.Entry();
        enterEntry.eventID = EventTriggerType.PointerEnter;
        enterEntry.callback.AddListener((data) => OnIconEnter(puzzleId));
        trigger.triggers.Add(enterEntry);
        
        // 离开事件
        EventTrigger.Entry exitEntry = new EventTrigger.Entry();
        exitEntry.eventID = EventTriggerType.PointerExit;
        exitEntry.callback.AddListener((data) => OnIconExit(puzzleId));
        trigger.triggers.Add(exitEntry);
    }
    
    void OnPuzzleCollected(int puzzleId)
    {
        Debug.Log($"收到拼图收集事件: ID {puzzleId}");
        
        // 更新进度显示
        UpdateProgressDisplay();
        
        // 更新对应图标
        UpdatePuzzleIcon(puzzleId);
        
        // 播放收集效果
        PlayCollectEffect(puzzleId);
    }
    
    void OnGameStateChanged(GameManager.GameState newState)
    {
        Debug.Log($"游戏状态改变: {newState}");
        
        // 只有在游戏进行中才显示进度
        bool shouldShow = newState == GameManager.GameState.Playing || 
                         newState == GameManager.GameState.PuzzleSolving ||
                         newState == GameManager.GameState.MainMenu;
        
        gameObject.SetActive(shouldShow);
        
        if (shouldShow)
        {
            UpdateProgressDisplay();
            UpdateAllIcons();
        }
    }
    
    void UpdateProgressDisplay()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager.Instance 为空");
            return;
        }
        
        int collected = GameManager.Instance.GetCollectedPuzzlesCount();
        int total = GameManager.Instance.TotalPuzzles;
        
        Debug.Log($"更新进度显示: {collected}/{total}");
        
        // 更新进度条
        if (progressBarFill != null)
        {
            progressBarFill.SetProgress(collected, total);
        }
        else
        {
            Debug.LogWarning("ProgressBarFill 组件为空");
        }
        
        // 更新进度文本
        if (progressText != null)
        {
            progressText.text = string.Format(progressFormat, collected, total);
            
            // 如果全部收集完成，改变颜色
            if (collected >= total)
            {
                progressText.color = Color.yellow;
                progressText.fontStyle = FontStyles.Bold;
            }
            else
            {
                progressText.color = Color.white;
                progressText.fontStyle = FontStyles.Normal;
            }
            
            Debug.Log($"设置进度文本: {progressText.text}");
        }
        else
        {
            Debug.LogWarning("progressText 组件为空");
        }
    }
    
    void UpdateAllIcons()
    {
        if (GameManager.Instance == null) return;
        
        for (int i = 0; i < iconPuzzleIds.Count; i++)
        {
            UpdatePuzzleIcon(iconPuzzleIds[i]);
        }
    }
    
    void UpdatePuzzleIcon(int puzzleId)
    {
        // 找到对应的图标索引
        int index = iconPuzzleIds.IndexOf(puzzleId);
        if (index == -1 || index >= iconImages.Count) 
        {
            Debug.LogWarning($"找不到拼图ID {puzzleId} 对应的图标");
            return;
        }
        
        bool isCollected = GameManager.Instance != null && 
                          GameManager.Instance.IsPuzzleCollected(puzzleId);
        
        Debug.Log($"更新拼图图标 {puzzleId}: 已收集 = {isCollected}");
        
        // 更新图标
        if (iconImages[index] != null)
        {
            // 根据是否收集来设置图标
            if (isCollected)
            {
                // 已收集：使用对应的收集图标
                if (puzzleId - 1 < collectedIconSprites.Count && collectedIconSprites[puzzleId - 1] != null)
                {
                    iconImages[index].sprite = collectedIconSprites[puzzleId - 1];
                    iconImages[index].color = collectedColor;
                }
                else if (defaultUncollectedIcon != null)
                {
                    // 如果没有指定特定图标，使用默认图标但调整颜色
                    iconImages[index].sprite = defaultUncollectedIcon;
                    iconImages[index].color = collectedColor;
                }
            }
            else
            {
                // 未收集：使用未收集图标
                if (defaultUncollectedIcon != null)
                {
                    iconImages[index].sprite = defaultUncollectedIcon;
                }
                iconImages[index].color = uncollectedColor;
            }
        }
        
        // 更新文本
        if (iconTexts[index] != null)
        {
            iconTexts[index].color = isCollected ? collectedColor : uncollectedColor;
            iconTexts[index].text = puzzleId.ToString(); // 显示拼图编号
            
            // 如果收集了，可以添加一些特效
            if (isCollected && addTextOutline)
            {
                AddTextOutlineEffect(iconTexts[index]);
            }
            else
            {
                RemoveTextOutlineEffect(iconTexts[index]);
            }
        }
        
        // 如果刚收集，播放动画
        if (isCollected && animateCollect)
        {
            StartCoroutine(PlayIconCollectAnimation(index, puzzleId * iconRevealDelay));
        }
    }
    
    /// <summary>
    /// 为TextMeshPro文本添加轮廓效果
    /// </summary>
    void AddTextOutlineEffect(TextMeshProUGUI text)
    {
        if (text == null) return;
        
        var outline = text.GetComponent<Outline>();
        if (outline == null)
        {
            outline = text.gameObject.AddComponent<Outline>();
        }
        
        outline.effectColor = outlineColor;
        outline.effectDistance = outlineDistance;
    }
    
    /// <summary>
    /// 移除TextMeshPro文本的轮廓效果
    /// </summary>
    void RemoveTextOutlineEffect(TextMeshProUGUI text)
    {
        if (text == null) return;
        
        var outline = text.GetComponent<Outline>();
        if (outline != null)
        {
            Destroy(outline);
        }
    }
    
    IEnumerator PlayIconCollectAnimation(int iconIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (iconIndex >= iconTransforms.Count || iconTransforms[iconIndex] == null)
            yield break;
        
        RectTransform rect = iconTransforms[iconIndex];
        Vector3 originalScale = rect.localScale;
        
        // 收集动画
        float duration = 0.3f;
        float elapsedTime = 0f;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            
            // 使用弹性动画
            float scale = 1f + Mathf.Sin(t * Mathf.PI * 2) * 0.3f * (1f - t);
            rect.localScale = originalScale * scale;
            
            yield return null;
        }
        
        rect.localScale = originalScale;
    }
    
    void PlayCollectEffect(int puzzleId)
    {
        // 进度条脉冲效果
        if (progressBarFill != null)
        {
            StartCoroutine(PulseProgressBar());
        }
    }
    
    IEnumerator PulseProgressBar()
    {
        // 获取进度条填充的RectTransform
        RectTransform rectTransform = progressBarFill.GetComponent<RectTransform>();
        if (rectTransform == null) yield break;
        
        Vector3 originalScale = rectTransform.localScale;
        float pulseTime = 0.2f;
        float elapsedTime = 0f;
        
        while (elapsedTime < pulseTime)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / pulseTime;
            float scale = Mathf.Lerp(1f, 1.1f, Mathf.Sin(t * Mathf.PI));
            rectTransform.localScale = originalScale * scale;
            yield return null;
        }
        
        rectTransform.localScale = originalScale;
    }
    
    // 图标交互事件
    void OnIconClicked(int puzzleId)
    {
        bool isCollected = GameManager.Instance != null && 
                          GameManager.Instance.IsPuzzleCollected(puzzleId);
        
        if (isCollected)
        {
            Debug.Log($"拼图 {puzzleId} 已收集");
            // 这里可以打开拼图详情面板
            // 播放收集的拼图图案
            ShowPuzzleDetails(puzzleId);
        }
        else
        {
            Debug.Log($"拼图 {puzzleId} 尚未收集");
            // 提示玩家去收集
        }
        
        // 点击动画
        int index = iconPuzzleIds.IndexOf(puzzleId);
        if (index != -1)
        {
            StartCoroutine(PlayIconClickAnimation(index));
        }
    }
    
    /// <summary>
    /// 显示拼图详情（可以在这里显示完整的拼图图案）
    /// </summary>
    void ShowPuzzleDetails(int puzzleId)
    {
        // 这里可以实现显示拼图大图的功能
        Debug.Log($"显示拼图 {puzzleId} 的详细信息");
        // 例如：打开一个UI面板，显示完整的拼图图案
    }
    
    void OnIconEnter(int puzzleId)
    {
        int index = iconPuzzleIds.IndexOf(puzzleId);
        if (index < 0 || index >= iconTransforms.Count || iconTransforms[index] == null)
            return;
        
        // 悬停效果：轻微放大
        StartCoroutine(ScaleIcon(iconTransforms[index], 1.2f, 0.1f));
    }
    
    void OnIconExit(int puzzleId)
    {
        int index = iconPuzzleIds.IndexOf(puzzleId);
        if (index < 0 || index >= iconTransforms.Count || iconTransforms[index] == null)
            return;
        
        // 恢复原大小
        StartCoroutine(ScaleIcon(iconTransforms[index], 1f, 0.1f));
    }
    
    IEnumerator PlayIconClickAnimation(int iconIndex)
    {
        if (iconIndex < 0 || iconIndex >= iconTransforms.Count || iconTransforms[iconIndex] == null)
            yield break;
        
        RectTransform rect = iconTransforms[iconIndex];
        Vector3 originalScale = rect.localScale;
        
        // 点击缩小再恢复
        float duration = 0.1f;
        
        // 缩小
        yield return StartCoroutine(ScaleIcon(rect, 0.8f, duration));
        
        // 恢复
        yield return StartCoroutine(ScaleIcon(rect, 1f, duration));
    }
    
    IEnumerator ScaleIcon(RectTransform rect, float targetScale, float duration)
    {
        Vector3 startScale = rect.localScale;
        Vector3 endScale = startScale * targetScale;
        
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / duration;
            rect.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }
        
        rect.localScale = endScale;
    }
    
    /// <summary>
    /// 手动刷新显示
    /// </summary>
    public void RefreshDisplay()
    {
        UpdateProgressDisplay();
        UpdateAllIcons();
    }
    
    /// <summary>
    /// 重置显示（用于游戏重开）
    /// </summary>
    public void ResetDisplay()
    {
        int totalPuzzles = GameManager.Instance != null ? GameManager.Instance.TotalPuzzles : 9;
        CreatePuzzleIcons(totalPuzzles);
        UpdateProgressDisplay();
        UpdateAllIcons();
        
        // 重置进度条
        if (progressBarFill != null)
        {
            progressBarFill.ResetProgress();
        }
    }
    
    /// <summary>
    /// 设置拼图图标（为每个拼图设置特定的收集图标）
    /// </summary>
    public void SetPuzzleIcons(List<Sprite> icons)
    {
        if (icons != null)
        {
            collectedIconSprites = icons;
            UpdateAllIcons();
        }
    }
    
    /// <summary>
    /// 设置默认未收集图标
    /// </summary>
    public void SetDefaultIcon(Sprite icon)
    {
        defaultUncollectedIcon = icon;
        UpdateAllIcons();
    }
}