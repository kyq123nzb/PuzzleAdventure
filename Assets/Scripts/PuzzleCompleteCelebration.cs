using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 拼图收集完成庆祝界面 - 显示撒花效果和祝贺文本
/// </summary>
public class PuzzleCompleteCelebration : MonoBehaviour
{
    [Header("UI组件")]
    public GameObject celebrationPanel;
    public GameObject celebrationText; // 支持Text和TextMeshPro
    public ParticleSystem confettiParticleSystem; // 撒花粒子效果
    
    [Header("粒子效果设置")]
    public bool autoCreateConfetti = true;
    public int confettiCount = 200; // 粒子数量
    public float confettiLifetime = 5f; // 粒子存活时间
    public Color[] confettiColors = new Color[] { 
        Color.red, Color.blue, Color.green, Color.yellow, 
        Color.magenta, Color.cyan, Color.white 
    };
    
    [Header("文本设置")]
    public string celebrationMessage = "恭喜你已经收集9块拼图！";
    public float textAnimationDuration = 1f;
    
    [Header("动画设置")]
    public bool useFadeAnimation = true;
    public bool useScaleAnimation = true;
    public float fadeSpeed = 2f;
    
    private CanvasGroup canvasGroup;
    private RectTransform panelRect;
    private bool isShowing = false;
    
    void Start()
    {
        // 初始化组件
        if (celebrationPanel != null)
        {
            canvasGroup = celebrationPanel.GetComponent<CanvasGroup>();
            if (canvasGroup == null && useFadeAnimation)
            {
                canvasGroup = celebrationPanel.AddComponent<CanvasGroup>();
            }
            
            panelRect = celebrationPanel.GetComponent<RectTransform>();
            
            // 初始隐藏
            celebrationPanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
        
        // 自动创建粒子效果
        if (autoCreateConfetti && confettiParticleSystem == null)
        {
            CreateConfettiSystem();
        }
    }
    
    void CreateConfettiSystem()
    {
        // 创建粒子系统对象
        GameObject particleObj = new GameObject("ConfettiParticles");
        
        // 优先添加到Canvas（UI Canvas）
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            particleObj.transform.SetParent(canvas.transform, false);
            Debug.Log($"粒子系统添加到Canvas: {canvas.name}");
        }
        else
        {
            // 如果没有Canvas，添加到当前对象下
            particleObj.transform.SetParent(transform);
            Debug.LogWarning("未找到Canvas，粒子系统添加到当前对象下");
        }
        
        confettiParticleSystem = particleObj.AddComponent<ParticleSystem>();
        
        // 配置粒子系统
        var main = confettiParticleSystem.main;
        main.startLifetime = confettiLifetime;
        main.startSpeed = 8f;
        main.startSize = 0.4f;
        // 创建初始颜色渐变（从颜色数组中创建渐变）
        Gradient startColorGradient = new Gradient();
        if (confettiColors != null && confettiColors.Length > 0)
        {
            // 使用颜色数组创建渐变，分布多个颜色点
            GradientColorKey[] colorKeys = new GradientColorKey[confettiColors.Length];
            for (int i = 0; i < confettiColors.Length; i++)
            {
                float position = confettiColors.Length > 1 ? (float)i / (confettiColors.Length - 1) : 0f;
                colorKeys[i] = new GradientColorKey(confettiColors[i], position);
            }
            
            startColorGradient.SetKeys(
                colorKeys,
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
        }
        else
        {
            // 如果没有设置颜色，使用默认的彩虹色渐变
            startColorGradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(Color.red, 0f),
                    new GradientColorKey(Color.yellow, 0.25f),
                    new GradientColorKey(Color.green, 0.5f),
                    new GradientColorKey(Color.cyan, 0.75f),
                    new GradientColorKey(Color.blue, 1f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(1f, 1f)
                }
            );
        }
        main.startColor = new ParticleSystem.MinMaxGradient(startColorGradient);
        main.maxParticles = confettiCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = false;
        main.scalingMode = ParticleSystemScalingMode.Shape; // 缩放模式
        
        // 发射设置
        var emission = confettiParticleSystem.emission;
        emission.rateOverTime = 0; // 不使用持续发射
        // 限制数量在short范围内，并转换为short
        short burstCount = (short)Mathf.Clamp(confettiCount, 1, short.MaxValue);
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, burstCount, burstCount, (short)1, 0.01f)
        });
        
        // 形状设置（从屏幕上方发射）
        var shape = confettiParticleSystem.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(25f, 0.1f, 1f); // 加宽发射区域
        
        // 速度设置（向下飘落）
        var velocity = confettiParticleSystem.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        velocity.x = new ParticleSystem.MinMaxCurve(-8f, 8f); // 横向速度
        velocity.y = new ParticleSystem.MinMaxCurve(-12f, -6f); // 向下速度
        velocity.z = new ParticleSystem.MinMaxCurve(-3f, 3f);
        
        // 旋转设置（旋转飘落更自然）
        var rotation = confettiParticleSystem.rotationOverLifetime;
        rotation.enabled = true;
        rotation.z = new ParticleSystem.MinMaxCurve(-720f, 720f); // 旋转更明显
        
        // 重力设置
        var forceOverLifetime = confettiParticleSystem.forceOverLifetime;
        forceOverLifetime.enabled = true;
        forceOverLifetime.space = ParticleSystemSimulationSpace.World;
        forceOverLifetime.y = -9.81f; // 重力
        
        // 颜色渐变（保持颜色直到消失）
        var colorOverLifetime = confettiParticleSystem.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient colorGradient = new Gradient();
        colorGradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.white, 0f),
                new GradientColorKey(Color.white, 0.9f),
                new GradientColorKey(Color.white, 1f)
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0f, 1f) // 最后淡出
            }
        );
        colorOverLifetime.color = colorGradient;
        
        // 设置粒子渲染器（使用简单的材质）
        var renderer = confettiParticleSystem.GetComponent<ParticleSystemRenderer>();
        if (renderer != null)
        {
            renderer.renderMode = ParticleSystemRenderMode.Billboard;
            renderer.sortingOrder = 999; // 确保在最上层显示
        }
        
        Debug.Log("PuzzleCompleteCelebration: 自动创建了撒花粒子系统");
    }
    
    public void ShowCelebration()
    {
        if (isShowing)
        {
            Debug.LogWarning("PuzzleCompleteCelebration: 界面已经在显示中，忽略重复调用");
            return;
        }
        
        isShowing = true;
        Debug.LogWarning("🎉🎉🎉 PuzzleCompleteCelebration: ShowCelebration() 被调用！显示庆祝界面！");
        
        // 更新文本
        UpdateText();
        
        // 显示面板
        if (celebrationPanel != null)
        {
            Debug.LogWarning($"准备激活面板: {celebrationPanel.name}");
            
            // 确保所有父对象都激活（关键修复！）
            Transform parent = celebrationPanel.transform.parent;
            while (parent != null)
            {
                parent.gameObject.SetActive(true);
                Debug.LogWarning($"激活父对象: {parent.name}");
                parent = parent.parent;
            }
            
            // 确保面板在Canvas最上层显示
            Canvas canvas = celebrationPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // 确保Canvas启用
                canvas.enabled = true;
                canvas.gameObject.SetActive(true);
                
                // 设置最高排序顺序，确保在最上层
                canvas.sortingOrder = 999;
                
                Debug.LogWarning($"设置Canvas排序顺序: {canvas.sortingOrder}");
            }
            
            // 确保面板在Canvas的最后一个子对象（最上层）
            celebrationPanel.transform.SetAsLastSibling();
            
            // 强制激活面板
            celebrationPanel.SetActive(true);
            Debug.LogWarning($"面板已激活: {celebrationPanel.activeSelf}");
            
            // 确保面板在最上层Canvas
            if (canvas != null)
            {
                // 将面板移到Canvas的直接子对象（最上层）
                celebrationPanel.transform.SetParent(canvas.transform, false);
                celebrationPanel.transform.SetAsLastSibling();
                Debug.LogWarning($"面板已移动到Canvas最上层");
            }
            
            if (useFadeAnimation || useScaleAnimation)
            {
                StartCoroutine(ShowAnimation());
            }
            else
            {
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                    Debug.LogWarning($"CanvasGroup alpha: {canvasGroup.alpha}");
                }
                if (panelRect != null)
                {
                    panelRect.localScale = Vector3.one;
                    Debug.LogWarning($"面板缩放: {panelRect.localScale}");
                }
            }
            
            Debug.LogWarning($"✅ 庆祝面板应该已经显示了！面板路径: {GetGameObjectPath(celebrationPanel)}");
        }
        else
        {
            Debug.LogError("❌ celebrationPanel为空！无法显示界面！");
        }
        
        // 播放粒子效果
        PlayConfetti();
    }
    
    private string GetGameObjectPath(GameObject obj)
    {
        if (obj == null) return "null";
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
    
    void UpdateText()
    {
        if (celebrationText != null)
        {
            // 尝试使用TextMeshPro
            TMPro.TextMeshProUGUI tmpText = celebrationText.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = celebrationMessage;
            }
            else
            {
                // 使用传统Text
                Text textComponent = celebrationText.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = celebrationMessage;
                }
            }
        }
    }
    
    void PlayConfetti()
    {
        if (confettiParticleSystem != null)
        {
            // 设置发射位置（屏幕上方，玩家前方）
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // 计算屏幕上方在世界空间的位置
                Vector3 screenCenterTop = new Vector3(Screen.width / 2f, Screen.height + 50f, 0f);
                Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                    new Vector3(screenCenterTop.x, screenCenterTop.y, mainCamera.nearClipPlane + 15f)
                );
                confettiParticleSystem.transform.position = worldPos;
                Debug.Log($"粒子系统位置: {worldPos}");
            }
            else
            {
                // 如果没有相机，使用默认位置（玩家前方上方）
                Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
                if (playerTransform != null)
                {
                    confettiParticleSystem.transform.position = playerTransform.position + 
                        Vector3.up * 10f + playerTransform.forward * 5f;
                }
            }
            
            confettiParticleSystem.Play();
            Debug.Log("PuzzleCompleteCelebration: 播放撒花粒子效果");
            
            // 自动停止（可选，让粒子自然消失）
            StartCoroutine(StopConfettiAfterDelay());
        }
    }
    
    IEnumerator StopConfettiAfterDelay()
    {
        yield return new WaitForSeconds(confettiLifetime);
        if (confettiParticleSystem != null && confettiParticleSystem.isPlaying)
        {
            confettiParticleSystem.Stop();
        }
    }
    
    IEnumerator ShowAnimation()
    {
        if (canvasGroup != null)
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
        
        float elapsedTime = 0f;
        float duration = 1f / fadeSpeed;
        
        while (elapsedTime < duration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float progress = elapsedTime / duration;
            
            // 淡入
            if (useFadeAnimation && canvasGroup != null)
            {
                canvasGroup.alpha = Mathf.Lerp(0f, 1f, progress);
            }
            
            // 缩放
            if (useScaleAnimation && panelRect != null)
            {
                float scale = Mathf.SmoothStep(0f, 1f, progress);
                panelRect.localScale = new Vector3(scale, scale, 1f);
            }
            
            yield return null;
        }
        
        // 确保最终状态
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        if (panelRect != null)
        {
            panelRect.localScale = Vector3.one;
        }
    }
    
    public void HideCelebration()
    {
        isShowing = false;
        
        if (celebrationPanel != null)
        {
            celebrationPanel.SetActive(false);
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
            }
            if (panelRect != null)
            {
                panelRect.localScale = Vector3.zero;
            }
        }
        
        if (confettiParticleSystem != null)
        {
            confettiParticleSystem.Stop();
        }
    }
}


