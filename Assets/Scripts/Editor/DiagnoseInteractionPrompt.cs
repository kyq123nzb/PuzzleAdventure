using UnityEngine;
using UnityEditor;

/// <summary>
/// 诊断工具：检查交互提示系统配置
/// </summary>
public class DiagnoseInteractionPrompt
{
    [MenuItem("Tools/诊断交互提示系统")]
    static void DiagnoseInteractionPromptSystem()
    {
        string report = "=== 交互提示系统诊断报告 ===\n\n";
        bool hasErrors = false;
        
        // 1. 检查 InteractionPrompt 对象
        GameObject interactionPrompt = GameObject.Find("InteractionPrompt");
        if (interactionPrompt == null)
        {
            report += "❌ 错误：场景中没有 InteractionPrompt 对象\n";
            report += "   解决方法：Tools > Add Interaction Prompt to Scene\n\n";
            hasErrors = true;
        }
        else
        {
            report += "✅ InteractionPrompt 对象存在\n";
            InteractionPrompt promptScript = interactionPrompt.GetComponent<InteractionPrompt>();
            if (promptScript == null)
            {
                report += "❌ 错误：InteractionPrompt 对象没有 InteractionPrompt 脚本\n\n";
                hasErrors = true;
            }
            else
            {
                report += $"✅ InteractionPrompt 脚本存在\n";
                report += $"   检测距离: {promptScript.interactionDistance}m\n";
                report += $"   检测角度: {promptScript.detectionAngle}°\n";
                report += $"   检测层: {LayerMask.LayerToName(promptScript.interactableLayerMask.value)}\n\n";
            }
        }
        
        // 2. 检查 InteractionPromptPanel UI
        GameObject panel = GameObject.Find("InteractionPromptPanel");
        if (panel == null)
        {
            report += "❌ 错误：场景中没有 InteractionPromptPanel UI 对象\n";
            report += "   解决方法：检查 GameHUDCanvas 下是否有 InteractionPromptPanel\n\n";
            hasErrors = true;
        }
        else
        {
            report += "✅ InteractionPromptPanel UI 存在\n";
            if (!panel.activeInHierarchy)
            {
                report += "⚠️ 警告：InteractionPromptPanel 当前未激活（这是正常的，只有检测到可交互物体时才会激活）\n\n";
            }
        }
        
        // 3. 检查 InteractionPromptText
        GameObject textObj = GameObject.Find("InteractionPromptText");
        if (textObj == null)
        {
            report += "❌ 错误：场景中没有 InteractionPromptText 对象\n\n";
            hasErrors = true;
        }
        else
        {
            report += "✅ InteractionPromptText 存在\n";
            if (textObj.GetComponent<TMPro.TextMeshProUGUI>() == null && 
                textObj.GetComponent<UnityEngine.UI.Text>() == null)
            {
                report += "❌ 错误：InteractionPromptText 没有 Text 或 TextMeshPro 组件\n\n";
                hasErrors = true;
            }
        }
        
        // 4. 检查 UIManager
        UIManager uiManager = Object.FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            report += "❌ 错误：场景中没有 UIManager\n\n";
            hasErrors = true;
        }
        else
        {
            report += "✅ UIManager 存在\n";
            var panelField = typeof(UIManager).GetField("interactionPromptPanel", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (panelField != null)
            {
                var panelValue = panelField.GetValue(uiManager);
                if (panelValue == null)
                {
                    report += "⚠️ 警告：UIManager 的 interactionPromptPanel 字段未设置\n";
                    report += "   解决方法：在 UIManager 的 Inspector 中拖拽 InteractionPromptPanel 到字段\n\n";
                }
                else
                {
                    report += "✅ UIManager.interactionPromptPanel 已设置\n\n";
                }
            }
        }
        
        // 5. 检查拼图物体
        PuzzleItem[] puzzleItems = Object.FindObjectsOfType<PuzzleItem>();
        report += $"✅ 找到 {puzzleItems.Length} 个拼图物体\n";
        
        if (puzzleItems.Length == 0)
        {
            report += "⚠️ 警告：场景中没有拼图物体（PuzzleItem）\n";
        }
        else
        {
            int validPuzzles = 0;
            foreach (PuzzleItem puzzle in puzzleItems)
            {
                if (puzzle.GetComponent<Collider>() != null)
                {
                    validPuzzles++;
                }
                else
                {
                    report += $"⚠️ 警告：拼图 {puzzle.gameObject.name} 没有 Collider 组件\n";
                }
                
                if (!puzzle.canInteract)
                {
                    report += $"⚠️ 警告：拼图 {puzzle.gameObject.name} 的 canInteract 为 false\n";
                }
            }
            report += $"   其中 {validPuzzles}/{puzzleItems.Length} 个有 Collider\n\n";
        }
        
        // 6. 检查 Camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Camera[] cameras = Object.FindObjectsOfType<Camera>();
            if (cameras.Length == 0)
            {
                report += "❌ 错误：场景中没有 Camera\n\n";
                hasErrors = true;
            }
            else
            {
                report += $"⚠️ 警告：没有标记为 Main Camera 的相机，找到 {cameras.Length} 个相机\n\n";
            }
        }
        else
        {
            report += "✅ Main Camera 存在\n\n";
        }
        
        // 总结
        if (hasErrors)
        {
            report += "=== 总结 ===\n";
            report += "❌ 发现错误，请先解决上述问题\n";
            EditorUtility.DisplayDialog("诊断完成", report, "确定");
        }
        else
        {
            report += "=== 总结 ===\n";
            report += "✅ 基本配置正常\n";
            report += "\n如果仍然无法显示提示，请：\n";
            report += "1. 在 InteractionPrompt 对象的 Inspector 中启用 'Enable Debug Log'\n";
            report += "2. 运行游戏，查看 Console 窗口的日志\n";
            report += "3. 确保拼图物体在 InteractionPrompt 检测的图层上\n";
            report += "4. 尝试靠近拼图（5米内，60度角度范围内）\n";
            EditorUtility.DisplayDialog("诊断完成", report, "确定");
        }
        
        Debug.Log(report);
    }
}

