using UnityEngine;
using UnityEditor;
using UnityEngine.UI;

/// <summary>
/// 编辑器工具：在游戏HUD中添加声音控制按钮（一直显示）
/// </summary>
public class AddSoundButton
{
    [MenuItem("Tools/Add Sound Button to Game HUD")]
    static void AddSoundButtonToGameHUD()
    {
        // 查找GameHUDCanvas（游戏界面Canvas）
        GameObject gameHUDCanvas = GameObject.Find("GameHUDCanvas");
        if (gameHUDCanvas == null)
        {
            // 尝试查找所有Canvas
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
        
        // 检查是否已存在声音按钮（在场景中任何地方）
        GameObject existingButton = GameObject.Find("SoundButton");
        if (existingButton != null)
        {
            if (EditorUtility.DisplayDialog("声音按钮已存在", "场景中已存在SoundButton，是否要删除并重新创建？", "是", "否"))
            {
                Object.DestroyImmediate(existingButton);
            }
            else
            {
                return;
            }
        }
        
        // 查找PauseButton来作为位置和样式参考
        Transform pauseButton = gameHUDCanvas.transform.Find("PauseButton");
        Vector2 buttonPosition = new Vector2(600, 380); // 默认位置（右上角，在PauseButton附近）
        
        // 查找PauseButton的样式作为参考
        RectTransform pauseRectRef = null;
        Image pauseImageRef = null;
        if (pauseButton != null)
        {
            pauseRectRef = pauseButton.GetComponent<RectTransform>();
            pauseImageRef = pauseButton.GetComponent<Image>();
            if (pauseRectRef != null)
            {
                // 放在PauseButton下方
                buttonPosition = new Vector2(pauseRectRef.anchoredPosition.x, pauseRectRef.anchoredPosition.y - 50);
            }
        }
        
        // 创建声音按钮
        GameObject soundButtonObj = new GameObject("SoundButton");
        soundButtonObj.transform.SetParent(gameHUDCanvas.transform, false);
        
        RectTransform rect = soundButtonObj.AddComponent<RectTransform>();
        // 使用与PauseButton相同的尺寸，或默认值
        rect.sizeDelta = pauseRectRef != null ? pauseRectRef.sizeDelta : new Vector2(200, 30);
        rect.anchoredPosition = buttonPosition;
        // 锚点在中心（和PauseButton一样）
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        
        // 添加Image组件（按钮背景）
        Image buttonImage = soundButtonObj.AddComponent<Image>();
        if (pauseImageRef != null)
        {
            // 使用与PauseButton相同的样式
            buttonImage.sprite = pauseImageRef.sprite;
            buttonImage.type = pauseImageRef.type;
            buttonImage.color = pauseImageRef.color; // 使用相同的颜色
        }
        else
        {
            buttonImage.color = new Color(0.2f, 0.4f, 0.6f, 0.8f); // 半透明背景
        }
        buttonImage.raycastTarget = true;
        
        // 添加Button组件
        Button button = soundButtonObj.AddComponent<Button>();
        
        // 创建按钮文本
        GameObject textObj = new GameObject("Text (TMP)");
        textObj.transform.SetParent(soundButtonObj.transform, false);
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        
        // 先获取TextMeshPro类型（检查是否可用）
        System.Type tmpType = System.Type.GetType("TMPro.TextMeshProUGUI");
        System.Type tmpFontAssetType = System.Type.GetType("TMPro.TMP_FontAsset");
        
        // 查找现有按钮的TextMeshPro组件作为参考（获取字体资源）
        object referenceFontAsset = null;
        object referenceSharedMaterial = null;
        Color referenceColor = Color.white; // 默认白色
        
        // 先尝试从PauseButton获取
        if (pauseButton != null && tmpType != null)
        {
            var pauseTMP = pauseButton.GetComponentInChildren(tmpType);
            if (pauseTMP != null)
            {
                var fontAssetProp = pauseTMP.GetType().GetProperty("fontAsset");
                var sharedMaterialProp = pauseTMP.GetType().GetProperty("sharedMaterial");
                var colorProp = pauseTMP.GetType().GetProperty("color");
                
                if (fontAssetProp != null) referenceFontAsset = fontAssetProp.GetValue(pauseTMP);
                if (sharedMaterialProp != null) referenceSharedMaterial = sharedMaterialProp.GetValue(pauseTMP);
                if (colorProp != null)
                {
                    var colorValue = colorProp.GetValue(pauseTMP);
                    if (colorValue is Color) referenceColor = (Color)colorValue;
                }
            }
        }
        
        // 如果没找到，尝试从主菜单按钮获取
        if (referenceFontAsset == null && tmpType != null)
        {
            GameObject mainMenuPanel = GameObject.Find("MainMenuPanel");
            if (mainMenuPanel != null)
            {
                Transform startButton = mainMenuPanel.transform.Find("StartButton");
                if (startButton != null)
                {
                    var startTMP = startButton.GetComponentInChildren(tmpType);
                    if (startTMP != null)
                    {
                        var fontAssetProp = startTMP.GetType().GetProperty("fontAsset");
                        var sharedMaterialProp = startTMP.GetType().GetProperty("sharedMaterial");
                        var colorProp = startTMP.GetType().GetProperty("color");
                        
                        if (fontAssetProp != null) referenceFontAsset = fontAssetProp.GetValue(startTMP);
                        if (sharedMaterialProp != null) referenceSharedMaterial = sharedMaterialProp.GetValue(startTMP);
                        if (colorProp != null)
                        {
                            var colorValue = colorProp.GetValue(startTMP);
                            if (colorValue is Color) referenceColor = (Color)colorValue;
                        }
                    }
                }
            }
        }
        
        // 如果还是没找到，尝试通过GUID加载默认字体
        if (referenceFontAsset == null && tmpFontAssetType != null)
        {
            string fontGuid = "8f586378b4e144a9851e7b34d9b748ee";
            string fontPath = AssetDatabase.GUIDToAssetPath(fontGuid);
            if (!string.IsNullOrEmpty(fontPath))
            {
                referenceFontAsset = AssetDatabase.LoadAssetAtPath(fontPath, tmpFontAssetType);
                Debug.Log($"✅ 通过GUID加载字体: {fontPath}");
            }
            else
            {
                Debug.LogWarning("⚠️ 无法通过GUID找到字体路径，尝试其他方法...");
            }
        }
        
        // 如果还是没找到，尝试从TMP Settings获取默认字体
        if (referenceFontAsset == null && tmpFontAssetType != null)
        {
            try
            {
                var tmpSettingsType = System.Type.GetType("TMPro.TMP_Settings");
                if (tmpSettingsType != null)
                {
                    var instanceProp = tmpSettingsType.GetProperty("instance", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
                    if (instanceProp != null)
                    {
                        var settings = instanceProp.GetValue(null);
                        if (settings != null)
                        {
                            var defaultFontProp = settings.GetType().GetProperty("defaultFontAsset");
                            if (defaultFontProp != null)
                            {
                                referenceFontAsset = defaultFontProp.GetValue(settings);
                                if (referenceFontAsset != null)
                                {
                                    Debug.Log("✅ 从TMP Settings获取默认字体");
                                }
                            }
                        }
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ 获取TMP Settings失败: {e.Message}");
            }
        }
        
        // 使用TextMeshPro（如果可用）
        if (tmpType != null)
        {
            var tmpComponent = textObj.AddComponent(tmpType);
            if (referenceFontAsset == null)
            {
                Debug.LogError("❌ 无法找到TextMeshPro字体资源！文本可能不会显示。");
            }
            SetTextMeshProProperties(tmpComponent, "Music", 30, referenceColor, referenceFontAsset, referenceSharedMaterial);
            
            // 强制刷新TextMeshPro（确保文字显示）
            try
            {
                var forceMeshUpdateMethod = tmpComponent.GetType().GetMethod("ForceMeshUpdate", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (forceMeshUpdateMethod != null)
                {
                    forceMeshUpdateMethod.Invoke(tmpComponent, null);
                    Debug.Log("✅ 强制刷新TextMeshPro网格");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"⚠️ 强制刷新TextMeshPro失败: {e.Message}");
            }
        }
        else
        {
            // 使用传统Text（降级方案）
            Debug.Log("⚠️ TextMeshPro不可用，使用传统Text组件");
            Text textComponent = textObj.AddComponent<Text>();
            textComponent.text = "Music";
            textComponent.fontSize = 30;
            textComponent.color = Color.white;
            textComponent.alignment = TextAnchor.MiddleCenter;
            
            // 尝试多个字体选项
            Font arialFont = Resources.GetBuiltinResource<Font>("Arial.ttf");
            if (arialFont != null)
            {
                textComponent.font = arialFont;
                Debug.Log("✅ 使用Arial字体");
            }
            else
            {
                // 尝试从Resources加载
                Font legacyFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                if (legacyFont != null)
                {
                    textComponent.font = legacyFont;
                    Debug.Log("✅ 使用LegacyRuntime字体");
                }
                else
                {
                    Debug.LogError("❌ 无法找到任何可用字体！文本可能不会显示。");
                }
            }
        }
        
        // 尝试找到UIManager并连接按钮
        UIManager uiManager = Object.FindObjectOfType<UIManager>();
        if (uiManager != null)
        {
            // 使用反射来设置soundButton字段（因为它是public的）
            var soundButtonField = typeof(UIManager).GetField("soundButton", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (soundButtonField != null)
            {
                soundButtonField.SetValue(uiManager, soundButtonObj);
                // 标记场景为已修改
                UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(UnityEditor.SceneManagement.EditorSceneManager.GetActiveScene());
                Debug.Log("✅ UIManager的soundButton字段已更新！");
            }
        }
        
        Debug.Log("✅ 声音按钮已添加到游戏HUD！");
        EditorUtility.DisplayDialog("成功", "声音按钮已添加到游戏HUD！\n\n提示：\n- 按钮会一直显示在游戏界面\n- 按钮已自动连接到UIManager\n- 点击按钮可以切换声音播放/暂停\n- 可以在Inspector中调整按钮位置和样式\n\n注意：如果UIManager未找到，运行时也会自动查找并连接", "确定");
        
        // 选中新创建的按钮
        Selection.activeGameObject = soundButtonObj;
    }
    
    static void SetTextMeshProProperties(object tmpComponent, string text, int fontSize, Color color, object fontAsset, object sharedMaterial)
    {
        var textProp = tmpComponent.GetType().GetProperty("text");
        var fontSizeProp = tmpComponent.GetType().GetProperty("fontSize");
        var fontSizeBaseProp = tmpComponent.GetType().GetProperty("fontSizeBase");
        var colorProp = tmpComponent.GetType().GetProperty("color");
        var alignmentProp = tmpComponent.GetType().GetProperty("alignment");
        var fontAssetProp = tmpComponent.GetType().GetProperty("fontAsset");
        var sharedMaterialProp = tmpComponent.GetType().GetProperty("sharedMaterial");
        var horizontalAlignmentProp = tmpComponent.GetType().GetProperty("horizontalAlignment");
        var verticalAlignmentProp = tmpComponent.GetType().GetProperty("verticalAlignment");
        var enableWordWrappingProp = tmpComponent.GetType().GetProperty("enableWordWrapping");
        
        if (textProp != null) textProp.SetValue(tmpComponent, text);
        if (fontSizeProp != null) fontSizeProp.SetValue(tmpComponent, (float)fontSize);
        if (fontSizeBaseProp != null) fontSizeBaseProp.SetValue(tmpComponent, (float)fontSize);
        if (colorProp != null) colorProp.SetValue(tmpComponent, color);
        
        // 设置字体资源（非常重要！）
        if (fontAssetProp != null)
        {
            if (fontAsset != null)
            {
                fontAssetProp.SetValue(tmpComponent, fontAsset);
                Debug.Log($"✅ 设置TextMeshPro字体资源: {fontAsset.GetType().Name}");
            }
            else
            {
                Debug.LogError("❌ fontAsset为null，TextMeshPro可能无法显示文本！");
            }
        }
        
        // 设置共享材质
        if (sharedMaterialProp != null && sharedMaterial != null)
        {
            sharedMaterialProp.SetValue(tmpComponent, sharedMaterial);
        }
        
        // 设置对齐方式
        if (horizontalAlignmentProp != null)
        {
            var horizontalEnum = System.Enum.Parse(System.Type.GetType("TMPro.HorizontalAlignmentOptions"), "Center");
            horizontalAlignmentProp.SetValue(tmpComponent, horizontalEnum);
        }
        
        if (verticalAlignmentProp != null)
        {
            var verticalEnum = System.Enum.Parse(System.Type.GetType("TMPro.VerticalAlignmentOptions"), "Middle");
            verticalAlignmentProp.SetValue(tmpComponent, verticalEnum);
        }
        
        if (alignmentProp != null)
        {
            var alignmentEnum = System.Enum.Parse(System.Type.GetType("TMPro.TextAlignmentOptions"), "Center");
            alignmentProp.SetValue(tmpComponent, alignmentEnum);
        }
        
        // 启用自动换行
        if (enableWordWrappingProp != null)
        {
            enableWordWrappingProp.SetValue(tmpComponent, true);
        }
    }
}

