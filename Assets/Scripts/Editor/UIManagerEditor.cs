using UnityEngine;
using UnityEditor;
using UnityEngine.UI;
using TMPro;

[CustomEditor(typeof(UIManager))]
public class UIManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        // 绘制默认的Inspector
        DrawDefaultInspector();
        
        // 添加一个分隔线
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        EditorGUILayout.Space();
        
        // 获取UIManager实例
        UIManager uiManager = (UIManager)target;
        
        // 添加测试按钮
        EditorGUILayout.LabelField("测试工具", EditorStyles.boldLabel);
        
        GUI.backgroundColor = Color.green;
        if (GUILayout.Button("🧪 测试：显示庆祝界面", GUILayout.Height(30)))
        {
            if (uiManager != null)
            {
                uiManager.TestShowCelebration();
                Debug.Log("✅ 已触发测试：显示庆祝界面");
            }
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.Space();
        
        // 添加胜利面板样式设置按钮
        EditorGUILayout.LabelField("胜利面板样式设置", EditorStyles.boldLabel);
        
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("🎨 应用胜利面板样式（编辑器模式）", GUILayout.Height(30)))
        {
            ApplyVictoryPanelStyles(uiManager);
        }
        GUI.backgroundColor = Color.white;
        
        EditorGUILayout.HelpBox("点击上面的按钮可以在编辑器中直接应用胜利面板的样式设置，这样就能在Inspector中看到效果了。", MessageType.Info);
    }
    
    // 在编辑器中应用胜利面板样式
    void ApplyVictoryPanelStyles(UIManager uiManager)
    {
        if (uiManager == null) return;
        
        // 使用反射获取私有字段
        var victoryPanelField = typeof(UIManager).GetField("victoryPanel", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var victoryTextField = typeof(UIManager).GetField("victoryText", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var victoryRestartButtonField = typeof(UIManager).GetField("victoryRestartButton", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var victoryQuitButtonField = typeof(UIManager).GetField("victoryQuitButton", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var startButtonField = typeof(UIManager).GetField("startButton", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        var quitButtonField = typeof(UIManager).GetField("quitButton", 
            System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
        
        GameObject victoryPanel = victoryPanelField?.GetValue(uiManager) as GameObject;
        GameObject victoryText = victoryTextField?.GetValue(uiManager) as GameObject;
        GameObject victoryRestartButton = victoryRestartButtonField?.GetValue(uiManager) as GameObject;
        GameObject victoryQuitButton = victoryQuitButtonField?.GetValue(uiManager) as GameObject;
        GameObject startButton = startButtonField?.GetValue(uiManager) as GameObject;
        GameObject quitButton = quitButtonField?.GetValue(uiManager) as GameObject;
        
        if (victoryPanel == null)
        {
            Debug.LogWarning("⚠️ VictoryPanel为空，请先在Inspector中设置！");
            return;
        }
        
        // 设置胜利文本
        if (victoryText != null)
        {
            RectTransform textRect = victoryText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                textRect.anchoredPosition = new Vector2(0, 200f);
                
                TMPro.TextMeshProUGUI tmpText = victoryText.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.fontSize = 96f;
                    tmpText.alignment = TMPro.TextAlignmentOptions.Center;
                }
                else
                {
                    Text textLegacy = victoryText.GetComponent<Text>();
                    if (textLegacy != null)
                    {
                        textLegacy.fontSize = 96;
                        textLegacy.alignment = TextAnchor.MiddleCenter;
                    }
                }
                
                EditorUtility.SetDirty(victoryText);
                Debug.Log("✅ 胜利文本样式已应用");
            }
        }
        
        // 设置重新开始按钮
        if (victoryRestartButton != null && startButton != null)
        {
            RectTransform startRect = startButton.GetComponent<RectTransform>();
            RectTransform restartRect = victoryRestartButton.GetComponent<RectTransform>();
            
            if (startRect != null && restartRect != null)
            {
                restartRect.sizeDelta = startRect.sizeDelta;
                restartRect.localScale = Vector3.one;
                restartRect.anchorMin = new Vector2(0.5f, 0.5f);
                restartRect.anchorMax = new Vector2(0.5f, 0.5f);
                restartRect.pivot = new Vector2(0.5f, 0.5f);
                restartRect.anchoredPosition = new Vector2(0, startRect.anchoredPosition.y - 30f); // 往下移动30像素
                
                Image startImage = startButton.GetComponent<Image>();
                Image restartImage = victoryRestartButton.GetComponent<Image>();
                if (startImage != null && restartImage != null)
                {
                    restartImage.color = startImage.color;
                }
                
                CopyButtonTextStyle(startButton, victoryRestartButton);
                EnsureButtonFitsText(victoryRestartButton);
                
                EditorUtility.SetDirty(victoryRestartButton);
                Debug.Log("✅ 重新开始按钮样式已应用");
            }
        }
        
        // 设置退出按钮
        if (victoryQuitButton != null && quitButton != null)
        {
            RectTransform quitRect = quitButton.GetComponent<RectTransform>();
            RectTransform victoryQuitRect = victoryQuitButton.GetComponent<RectTransform>();
            
            if (quitRect != null && victoryQuitRect != null)
            {
                // 设置按钮大小：与 restartButton 一样大小
                if (victoryRestartButton != null)
                {
                    RectTransform restartRect = victoryRestartButton.GetComponent<RectTransform>();
                    if (restartRect != null)
                    {
                        victoryQuitRect.sizeDelta = restartRect.sizeDelta; // 使用 restartButton 的大小
                    }
                }
                else
                {
                    victoryQuitRect.sizeDelta = quitRect.sizeDelta; // 如果没有 restartButton，使用 quitButton 的大小
                }
                victoryQuitRect.localScale = Vector3.one;
                victoryQuitRect.anchorMin = new Vector2(0.5f, 0.5f);
                victoryQuitRect.anchorMax = new Vector2(0.5f, 0.5f);
                victoryQuitRect.pivot = new Vector2(0.5f, 0.5f);
                
                float buttonY = 0f;
                if (victoryRestartButton != null)
                {
                    RectTransform restartRect = victoryRestartButton.GetComponent<RectTransform>();
                    if (restartRect != null)
                    {
                        // 计算 congratulations 文本底部到 restartButton 顶部的距离
                        float textToRestartDistance = 150f; // 默认间距
                        
                        if (victoryText != null)
                        {
                            RectTransform textRect = victoryText.GetComponent<RectTransform>();
                            if (textRect != null)
                            {
                                // 文本中心Y坐标
                                float textCenterY = textRect.anchoredPosition.y;
                                
                                // 计算文本高度
                                float textHeight = 0f;
                                TMPro.TextMeshProUGUI tmpText = victoryText.GetComponent<TMPro.TextMeshProUGUI>();
                                if (tmpText != null)
                                {
                                    textHeight = tmpText.preferredHeight;
                                }
                                else
                                {
                                    Text textLegacy = victoryText.GetComponent<Text>();
                                    if (textLegacy != null)
                                    {
                                        textHeight = textLegacy.preferredHeight;
                                    }
                                }
                                
                                // 文本底部 = 文本中心 - 文本高度/2
                                float textBottom = textCenterY - textHeight / 2f;
                                
                                // restartButton 顶部 = restartButton 中心 + 按钮高度/2
                                float restartTop = restartRect.anchoredPosition.y + restartRect.sizeDelta.y / 2f;
                                
                                // 计算距离
                                textToRestartDistance = textBottom - restartTop;
                            }
                        }
                        
                        // quitButton 顶部应该在 restartButton 下方，距离相同
                        // restartButton 底部 = restartButton 中心 - 按钮高度/2
                        float restartBottom = restartRect.anchoredPosition.y - restartRect.sizeDelta.y / 2f;
                        
                        // quitButton 中心 = restartButton 底部 - 间距 - quitButton高度/2
                        buttonY = restartBottom - textToRestartDistance - victoryQuitRect.sizeDelta.y / 2f;
                    }
                }
                else
                {
                    // 如果没有 restartButton，使用 quitButton 的位置往下移动
                    buttonY = quitRect.anchoredPosition.y - 30f;
                }
                victoryQuitRect.anchoredPosition = new Vector2(0, buttonY);
                
                Image quitImage = quitButton.GetComponent<Image>();
                Image victoryQuitImage = victoryQuitButton.GetComponent<Image>();
                if (quitImage != null && victoryQuitImage != null)
                {
                    victoryQuitImage.color = quitImage.color;
                }
                
                CopyButtonTextStyle(quitButton, victoryQuitButton);
                EnsureButtonFitsText(victoryQuitButton);
                
                EditorUtility.SetDirty(victoryQuitButton);
                Debug.Log("✅ 退出按钮样式已应用");
            }
        }
        
        Debug.Log("🎨 胜利面板样式已全部应用！");
        Debug.Log("📋 查看效果的方法：");
        Debug.Log("   1. 在Hierarchy中找到 VictoryPanel（在 GameHUDCanvas 下）");
        Debug.Log("   2. 展开 VictoryPanel，选中 VictoryText、VictoryRestartButton、VictoryQuitButton");
        Debug.Log("   3. 在Inspector中查看它们的 RectTransform、字体大小、颜色等属性");
        Debug.Log("   4. 或者在Scene视图中直接查看UI元素的位置和大小");
        
        // 尝试选中 VictoryPanel 以便用户查看
        if (victoryPanel != null)
        {
            Selection.activeGameObject = victoryPanel;
            EditorGUIUtility.PingObject(victoryPanel);
        }
    }
    
    void CopyButtonTextStyle(GameObject sourceButton, GameObject targetButton)
    {
        if (sourceButton == null || targetButton == null) return;
        
        TMPro.TextMeshProUGUI sourceText = sourceButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI targetText = targetButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (sourceText != null && targetText != null)
        {
            targetText.color = sourceText.color;
            targetText.fontSize = sourceText.fontSize;
            targetText.fontStyle = sourceText.fontStyle;
        }
        else
        {
            Text sourceTextLegacy = sourceButton.GetComponentInChildren<Text>();
            Text targetTextLegacy = targetButton.GetComponentInChildren<Text>();
            if (sourceTextLegacy != null && targetTextLegacy != null)
            {
                targetTextLegacy.color = sourceTextLegacy.color;
                targetTextLegacy.fontSize = sourceTextLegacy.fontSize;
                targetTextLegacy.fontStyle = sourceTextLegacy.fontStyle;
            }
        }
    }
    
    void EnsureButtonFitsText(GameObject button)
    {
        if (button == null) return;
        
        TMPro.TextMeshProUGUI text = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            text.enableAutoSizing = false;
            float textWidth = text.preferredWidth;
            float textHeight = text.preferredHeight;
            
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                float minWidth = Mathf.Max(buttonRect.sizeDelta.x, textWidth * 1.2f);
                float minHeight = Mathf.Max(buttonRect.sizeDelta.y, textHeight * 1.3f);
                buttonRect.sizeDelta = new Vector2(minWidth, minHeight);
            }
        }
        else
        {
            Text textLegacy = button.GetComponentInChildren<Text>();
            if (textLegacy != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    buttonRect.sizeDelta = new Vector2(
                        Mathf.Max(buttonRect.sizeDelta.x, textLegacy.preferredWidth + 40),
                        Mathf.Max(buttonRect.sizeDelta.y, textLegacy.preferredHeight + 20)
                    );
                }
            }
        }
    }
}

