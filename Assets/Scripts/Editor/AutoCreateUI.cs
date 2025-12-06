using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

/// <summary>
/// 自动创建UI系统的编辑器脚本
/// 使用：Unity菜单栏 > Tools > Auto Create UI Systems
/// </summary>
public class AutoCreateUI : EditorWindow
{
    [MenuItem("Tools/Auto Create UI Systems")]
    public static void ShowWindow()
    {
        GetWindow<AutoCreateUI>("Auto Create UI");
    }

    void OnGUI()
    {
        GUILayout.Label("自动创建UI系统", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("创建主菜单UI", GUILayout.Height(30)))
        {
            CreateMainMenuUI();
        }

        if (GUILayout.Button("创建进度UI", GUILayout.Height(30)))
        {
            CreateProgressUI();
        }

        if (GUILayout.Button("创建交互提示UI", GUILayout.Height(30)))
        {
            CreateInteractionPromptUI();
        }

        GUILayout.Space(10);
        
        if (GUILayout.Button("创建所有UI系统", GUILayout.Height(40)))
        {
            CreateMainMenuUI();
            CreateProgressUI();
            CreateInteractionPromptUI();
            CreateUIManager();
            CreateInteractionPromptScript();
            
            Debug.Log("所有UI系统创建完成！");
            EditorUtility.DisplayDialog("完成", "所有UI系统已创建！\n请检查Hierarchy并配置脚本引用。", "确定");
        }

        GUILayout.Space(10);
        GUILayout.Label("提示：创建后需要手动配置脚本引用", EditorStyles.helpBox);
    }

    static void CreateMainMenuUI()
    {
        // 创建主菜单Canvas
        GameObject mainMenuCanvas = CreateCanvas("MainMenuCanvas");
        
        // 创建主菜单容器
        GameObject mainMenuPanel = new GameObject("MainMenuPanel");
        mainMenuPanel.transform.SetParent(mainMenuCanvas.transform, false);
        RectTransform panelRect = mainMenuPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        mainMenuPanel.AddComponent<CanvasGroup>();

        // 创建标题文本
        CreateText(mainMenuPanel.transform, "TitleText", "拼图冒险 Puzzle Adventure", 
            new Vector2(0, 200), 72, new Color(1f, 0.84f, 0f));

        // 创建开始按钮
        GameObject startButton = CreateButton(mainMenuCanvas.transform, "StartButton", "开始游戏",
            new Vector2(0, 50), new Vector2(300, 60));
        startButton.transform.SetParent(mainMenuPanel.transform, false);

        // 创建退出按钮
        GameObject quitButton = CreateButton(mainMenuCanvas.transform, "QuitButton", "退出游戏",
            new Vector2(0, -50), new Vector2(300, 60));
        quitButton.transform.SetParent(mainMenuPanel.transform, false);
        
        // 创建声音控制按钮
        GameObject soundButton = CreateButton(mainMenuCanvas.transform, "SoundButton", "🔊 声音: 开",
            new Vector2(0, -130), new Vector2(300, 60));
        soundButton.transform.SetParent(mainMenuPanel.transform, false);

        Debug.Log("主菜单UI创建完成！");
    }

    static void CreateProgressUI()
    {
        // 创建游戏内Canvas
        GameObject gameHUDCanvas = CreateCanvas("GameHUDCanvas");
        
        // 创建GameHUD容器
        GameObject gameHUD = new GameObject("GameHUD");
        gameHUD.transform.SetParent(gameHUDCanvas.transform, false);
        gameHUD.AddComponent<RectTransform>();

        // 创建进度面板
        GameObject progressPanel = CreateImage(gameHUD.transform, "ProgressPanel",
            new Vector2(-150, -50), new Vector2(300, 80), new Color(0, 0, 0, 0.6f));
        
        // 设置锚点到右上角
        RectTransform panelRect = progressPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(1, 1);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.pivot = new Vector2(1, 1);
        panelRect.anchoredPosition = new Vector2(-150, -50);

        // 创建进度文本
        CreateText(progressPanel.transform, "ProgressText", "拼图收集: 0/9",
            new Vector2(0, 20), 24, Color.white);

        // 创建进度条背景
        GameObject progressBarBg = CreateImage(progressPanel.transform, "ProgressBarBackground",
            new Vector2(0, 10), new Vector2(280, 15), new Color(0.2f, 0.2f, 0.2f));
        RectTransform bgRect = progressBarBg.GetComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0);
        bgRect.anchorMax = new Vector2(1, 0);
        bgRect.pivot = new Vector2(0.5f, 0);
        bgRect.offsetMin = new Vector2(10, 10);
        bgRect.offsetMax = new Vector2(-10, 25);

        // 创建进度条填充
        GameObject progressBarFill = CreateImage(progressBarBg.transform, "ProgressBarFill",
            Vector2.zero, Vector2.one, new Color(0.2f, 0.8f, 0.2f));
        RectTransform fillRect = progressBarFill.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;
        
        // 添加ProgressBarFill脚本
        if (System.Type.GetType("ProgressBarFill") != null)
        {
            progressBarFill.AddComponent(System.Type.GetType("ProgressBarFill"));
        }

        Debug.Log("进度UI创建完成！");
    }

    static void CreateInteractionPromptUI()
    {
        // 查找GameHUDCanvas
        GameObject gameHUDCanvas = GameObject.Find("GameHUDCanvas");
        if (gameHUDCanvas == null)
        {
            gameHUDCanvas = CreateCanvas("GameHUDCanvas");
        }

        // 查找GameHUD容器
        GameObject gameHUD = GameObject.Find("GameHUD");
        if (gameHUD == null)
        {
            gameHUD = new GameObject("GameHUD");
            gameHUD.transform.SetParent(gameHUDCanvas.transform, false);
            gameHUD.AddComponent<RectTransform>();
        }

        // 创建交互提示面板
        GameObject promptPanel = CreateImage(gameHUD.transform, "InteractionPromptPanel",
            new Vector2(0, 100), new Vector2(300, 60), new Color(0, 0, 0, 0.7f));
        
        // 设置锚点到底部居中
        RectTransform panelRect = promptPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0);
        panelRect.anchorMax = new Vector2(0.5f, 0);
        panelRect.pivot = new Vector2(0.5f, 0);
        panelRect.anchoredPosition = new Vector2(0, 100);

        // 初始隐藏
        promptPanel.SetActive(false);

        // 创建提示文本
        CreateText(promptPanel.transform, "InteractionPromptText", "Press E to Interact",
            Vector2.zero, 28, new Color(1f, 1f, 0.4f));

        Debug.Log("交互提示UI创建完成！");
    }

    static void CreateUIManager()
    {
        // 查找或创建UIManager
        GameObject uiManager = GameObject.Find("UIManager");
        if (uiManager == null)
        {
            uiManager = new GameObject("UIManager");
        }

        // 添加UIManager脚本
        if (uiManager.GetComponent("UIManager") == null)
        {
            System.Type uiManagerType = System.Type.GetType("UIManager");
            if (uiManagerType != null)
            {
                uiManager.AddComponent(uiManagerType);
            }
        }

        Debug.Log("UIManager对象创建完成！请手动配置Inspector引用。");
    }

    static void CreateInteractionPromptScript()
    {
        // 查找或创建InteractionPrompt对象
        GameObject interactionPrompt = GameObject.Find("InteractionPrompt");
        if (interactionPrompt == null)
        {
            interactionPrompt = new GameObject("InteractionPrompt");
        }

        // 添加InteractionPrompt脚本
        if (interactionPrompt.GetComponent("InteractionPrompt") == null)
        {
            System.Type promptType = System.Type.GetType("InteractionPrompt");
            if (promptType != null)
            {
                interactionPrompt.AddComponent(promptType);
            }
        }

        Debug.Log("InteractionPrompt对象创建完成！");
    }

    // 辅助方法：创建Canvas
    static GameObject CreateCanvas(string name)
    {
        GameObject canvasObj = new GameObject(name);
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        
        canvasObj.AddComponent<GraphicRaycaster>();
        
        // 创建EventSystem（如果不存在）
        if (GameObject.FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        return canvasObj;
    }

    // 辅助方法：创建文本
    static GameObject CreateText(Transform parent, string name, string text, Vector2 position, int fontSize, Color color)
    {
        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent, false);
        
        RectTransform rect = textObj.AddComponent<RectTransform>();
        rect.sizeDelta = new Vector2(400, 100);
        rect.anchoredPosition = position;

        // 尝试使用TextMeshPro
        if (System.Type.GetType("TMPro.TextMeshProUGUI") != null)
        {
            var tmpComponent = textObj.AddComponent(System.Type.GetType("TMPro.TextMeshProUGUI"));
            SetTextProperties(tmpComponent, text, fontSize, color);
        }
        else
        {
            // 使用传统Text
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = text;
            textComponent.fontSize = fontSize;
            textComponent.color = color;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return textObj;
    }

    static void SetTextProperties(UnityEngine.Component textComponent, string text, int fontSize, Color color)
    {
        var textProp = textComponent.GetType().GetProperty("text");
        var fontSizeProp = textComponent.GetType().GetProperty("fontSize");
        var colorProp = textComponent.GetType().GetProperty("color");
        var alignmentProp = textComponent.GetType().GetProperty("alignment");

        if (textProp != null) textProp.SetValue(textComponent, text);
        if (fontSizeProp != null) fontSizeProp.SetValue(textComponent, (float)fontSize);
        if (colorProp != null) colorProp.SetValue(textComponent, color);
        if (alignmentProp != null)
        {
            var alignmentEnum = System.Enum.Parse(System.Type.GetType("TMPro.TextAlignmentOptions"), "Center");
            alignmentProp.SetValue(textComponent, alignmentEnum);
        }
    }

    // 辅助方法：创建按钮
    static GameObject CreateButton(Transform parent, string name, string buttonText, Vector2 position, Vector2 size)
    {
        GameObject buttonObj = new GameObject(name);
        buttonObj.transform.SetParent(parent, false);

        RectTransform rect = buttonObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = buttonObj.AddComponent<Image>();
        image.color = new Color(0.2f, 0.4f, 0.6f, 1f);

        Button button = buttonObj.AddComponent<Button>();

        // 创建按钮文本
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(buttonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        if (System.Type.GetType("TMPro.TextMeshProUGUI") != null)
        {
            var tmpComponent = textObj.AddComponent(System.Type.GetType("TMPro.TextMeshProUGUI"));
            SetTextProperties(tmpComponent, buttonText, 36, Color.white);
        }
        else
        {
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = buttonText;
            textComponent.fontSize = 36;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            textComponent.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }

        return buttonObj;
    }

    // 辅助方法：创建Image
    static GameObject CreateImage(Transform parent, string name, Vector2 position, Vector2 size, Color color)
    {
        GameObject imageObj = new GameObject(name);
        imageObj.transform.SetParent(parent, false);

        RectTransform rect = imageObj.AddComponent<RectTransform>();
        rect.sizeDelta = size;
        rect.anchoredPosition = position;

        Image image = imageObj.AddComponent<Image>();
        image.color = color;

        return imageObj;
    }
}

