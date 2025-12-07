using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 编辑器工具：在GameHUDCanvas中添加游戏教程面板
/// </summary>
public class AddTutorialPanel
{
    [MenuItem("Tools/Add Tutorial Panel")]
    static void AddTutorialPanelToGameHUD()
    {
        // 查找GameHUDCanvas
        GameObject gameHUDCanvas = GameObject.Find("GameHUDCanvas");
        if (gameHUDCanvas == null)
        {
            Canvas[] canvases = Object.FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name == "GameHUDCanvas")
                {
                    gameHUDCanvas = canvas.gameObject;
                    break;
                }
            }
        }
        
        if (gameHUDCanvas == null)
        {
            EditorUtility.DisplayDialog("错误", "找不到GameHUDCanvas！\n\n请确保场景中有名为'GameHUDCanvas'的对象。", "确定");
            return;
        }

        // 检查是否已存在教程面板
        Transform existingPanel = gameHUDCanvas.transform.Find("TutorialPanel");
        if (existingPanel != null)
        {
            if (EditorUtility.DisplayDialog("教程面板已存在", "场景中已存在TutorialPanel，是否要删除并重新创建？", "是", "否"))
            {
                Object.DestroyImmediate(existingPanel.gameObject);
            }
            else
            {
                return;
            }
        }

        // 获取TextMeshPro字体资源（用于文本显示）
        TMP_FontAsset referenceFontAsset = null;
        Material referenceSharedMaterial = null;
        Color referenceColor = Color.white;

        // 尝试从现有按钮获取字体
        GameObject pauseButton = GameObject.Find("PauseButton");
        if (pauseButton != null)
        {
            var pauseTMP = pauseButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (pauseTMP != null)
            {
                referenceFontAsset = pauseTMP.font;
                referenceSharedMaterial = pauseTMP.fontSharedMaterial;
                referenceColor = pauseTMP.color;
            }
        }

        // 如果没找到，尝试从主菜单按钮获取
        if (referenceFontAsset == null)
        {
            GameObject mainMenuPanel = GameObject.Find("MainMenuPanel");
            if (mainMenuPanel != null)
            {
                Transform startButton = mainMenuPanel.transform.Find("StartButton");
                if (startButton != null)
                {
                    var startTMP = startButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
                    if (startTMP != null)
                    {
                        referenceFontAsset = startTMP.font;
                        referenceSharedMaterial = startTMP.fontSharedMaterial;
                        referenceColor = startTMP.color;
                    }
                }
            }
        }

        // 如果还是没找到，尝试通过GUID加载默认字体
        if (referenceFontAsset == null)
        {
            string fontGuid = "8f586378b4e144a9851e7b34d9b748ee"; // LiberationSans SDF GUID
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGuid);
            if (!string.IsNullOrEmpty(fontPath))
            {
                referenceFontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);
            }
        }

        // 创建教程面板（父对象）
        GameObject tutorialPanel = new GameObject("TutorialPanel");
        tutorialPanel.transform.SetParent(gameHUDCanvas.transform, false);
        
        RectTransform panelRect = tutorialPanel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(600, 450); // 增加高度以容纳更多内容
        panelRect.anchoredPosition = Vector2.zero; // 居中

        // 添加背景Image
        Image panelBackground = tutorialPanel.AddComponent<Image>();
        panelBackground.color = new Color(0.1f, 0.1f, 0.15f, 0.95f); // 深色半透明背景
        panelBackground.raycastTarget = true;

        // 创建标题文本
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(tutorialPanel.transform, false);
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 1f);
        titleRect.anchorMax = new Vector2(0.5f, 1f);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.sizeDelta = new Vector2(580, 60);
        titleRect.anchoredPosition = new Vector2(0, -20); // 标题位置：相对于顶部向下20像素

        TMPro.TextMeshProUGUI titleText = titleObj.AddComponent<TMPro.TextMeshProUGUI>();
        titleText.text = "Controls";
        titleText.fontSize = 36;
        titleText.color = Color.white;
        titleText.alignment = TMPro.TextAlignmentOptions.Center;
        titleText.fontStyle = TMPro.FontStyles.Bold;
        if (referenceFontAsset != null)
        {
            titleText.font = referenceFontAsset;
            if (referenceSharedMaterial != null)
            {
                titleText.fontSharedMaterial = referenceSharedMaterial;
            }
        }

        // 创建内容文本
        GameObject contentObj = new GameObject("ContentText");
        contentObj.transform.SetParent(tutorialPanel.transform, false);
        RectTransform contentRect = contentObj.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(580, 300); // 增加内容区域高度
        contentRect.anchoredPosition = new Vector2(0, -50); // 内容位置：相对于中心向下移动，数值越小（越负）距离标题越远

        TMPro.TextMeshProUGUI contentText = contentObj.AddComponent<TMPro.TextMeshProUGUI>();
        contentText.text = "Press <b>SPACE</b> to Jump\n\n" +
                          "Use <b>WASD</b> or <b>Arrow Keys</b> to Move\n\n" +
                          "Collect 9 puzzle pieces to complete the level";
        contentText.fontSize = 28;
        contentText.color = new Color(0.9f, 0.9f, 0.9f, 1f);
        contentText.alignment = TMPro.TextAlignmentOptions.Center;
        contentText.enableWordWrapping = true;
        if (referenceFontAsset != null)
        {
            contentText.font = referenceFontAsset;
            if (referenceSharedMaterial != null)
            {
                contentText.fontSharedMaterial = referenceSharedMaterial;
            }
        }

        // 创建关闭按钮（X按钮）
        GameObject closeButtonObj = new GameObject("CloseButton");
        closeButtonObj.transform.SetParent(tutorialPanel.transform, false);
        RectTransform closeRect = closeButtonObj.AddComponent<RectTransform>();
        closeRect.anchorMin = new Vector2(1f, 1f);
        closeRect.anchorMax = new Vector2(1f, 1f);
        closeRect.pivot = new Vector2(1f, 1f);
        closeRect.sizeDelta = new Vector2(40, 40);
        closeRect.anchoredPosition = new Vector2(-10, -10);

        Image closeButtonImage = closeButtonObj.AddComponent<Image>();
        closeButtonImage.color = new Color(0.8f, 0.2f, 0.2f, 1f); // 红色背景
        closeButtonImage.raycastTarget = true;

        Button closeButton = closeButtonObj.AddComponent<Button>();

        // 创建关闭按钮文本（X符号）
        GameObject closeTextObj = new GameObject("CloseText");
        closeTextObj.transform.SetParent(closeButtonObj.transform, false);
        RectTransform closeTextRect = closeTextObj.AddComponent<RectTransform>();
        closeTextRect.anchorMin = Vector2.zero;
        closeTextRect.anchorMax = Vector2.one;
        closeTextRect.offsetMin = Vector2.zero;
        closeTextRect.offsetMax = Vector2.zero;
        closeTextRect.pivot = new Vector2(0.5f, 0.5f);

        TMPro.TextMeshProUGUI closeText = closeTextObj.AddComponent<TMPro.TextMeshProUGUI>();
        closeText.text = "×";
        closeText.fontSize = 36;
        closeText.color = Color.white;
        closeText.alignment = TMPro.TextAlignmentOptions.Center;
        if (referenceFontAsset != null)
        {
            closeText.font = referenceFontAsset;
            if (referenceSharedMaterial != null)
            {
                closeText.fontSharedMaterial = referenceSharedMaterial;
            }
        }

        // 初始状态设置为隐藏（在StartGame中显示）
        tutorialPanel.SetActive(false);

        // 尝试找到UIManager并连接教程面板
        UIManager uiManager = Object.FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            var tutorialPanelField = typeof(UIManager).GetField("tutorialPanel", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (tutorialPanelField != null)
            {
                tutorialPanelField.SetValue(uiManager, tutorialPanel);
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log("✅ UIManager的tutorialPanel字段已更新！");
            }
        }

        Debug.Log("✅ 教程面板已创建！");
        EditorUtility.DisplayDialog("成功", 
            "教程面板已添加到游戏HUD中！\n\n" +
            "功能说明：\n" +
            "- 点击开始游戏后会自动显示\n" +
            "- 显示操作提示（空格跳跃，WASD/方向键移动）\n" +
            "- 点击右上角的×按钮可以关闭\n\n" +
            "提示：\n" +
            "- 面板默认是隐藏状态\n" +
            "- 需要在UIManager的StartGame()方法中调用显示\n" +
            "- 如果UIManager未找到，运行时也会自动连接", 
            "确定");

        // 选中新创建的面板
        Selection.activeGameObject = tutorialPanel;
    }
}

