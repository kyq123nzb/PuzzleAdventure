using UnityEngine;
using UnityEditor;

/// <summary>
/// 交互提示排查工具
/// </summary>
public class TroubleshootInteractionPrompt
{
    [MenuItem("Tools/排查交互提示不显示问题")]
    static void Troubleshoot()
    {
        string report = "=== 交互提示排查报告 ===\n\n";
        bool hasProblems = false;
        
        // 1. 检查InteractionPrompt对象
        GameObject interactionPrompt = GameObject.Find("InteractionPrompt");
        if (interactionPrompt == null)
        {
            report += "❌ 问题1: 场景中没有 InteractionPrompt 对象\n";
            report += "   解决方法: Tools > Add Interaction Prompt to Scene\n\n";
            hasProblems = true;
        }
        else
        {
            report += "✅ InteractionPrompt 对象存在\n";
            InteractionPrompt script = interactionPrompt.GetComponent<InteractionPrompt>();
            if (script != null)
            {
                // 自动启用调试日志
                if (!script.enableDebugLog)
                {
                    script.enableDebugLog = true;
                    EditorUtility.SetDirty(interactionPrompt);
                    report += "✅ 已自动启用调试日志（请在运行时查看Console）\n";
                }
                else
                {
                    report += "✅ 调试日志已启用\n";
                }
                
                report += $"   检测距离: {script.interactionDistance}m\n";
                report += $"   检测角度: {script.detectionAngle}°\n\n";
            }
        }
        
        // 2. 检查拼图物体
        PuzzleItem[] puzzleItems = Object.FindObjectsOfType<PuzzleItem>();
        if (puzzleItems.Length == 0)
        {
            report += "❌ 问题2: 场景中没有找到拼图物体（PuzzleItem）\n\n";
            hasProblems = true;
        }
        else
        {
            report += $"✅ 找到 {puzzleItems.Length} 个拼图物体\n";
            int validCount = 0;
            foreach (PuzzleItem puzzle in puzzleItems)
            {
                bool valid = true;
                string issues = "";
                
                // 检查Collider
                Collider col = puzzle.GetComponent<Collider>();
                if (col == null)
                {
                    col = puzzle.GetComponentInChildren<Collider>();
                }
                if (col == null)
                {
                    issues += "无Collider ";
                    valid = false;
                }
                
                // 检查canInteract
                if (!puzzle.canInteract)
                {
                    issues += "canInteract=false ";
                    valid = false;
                }
                
                if (valid)
                {
                    validCount++;
                }
                else
                {
                    report += $"   ⚠️ {puzzle.gameObject.name}: {issues}\n";
                }
            }
            
            if (validCount == 0)
            {
                report += "❌ 问题3: 所有拼图物体都有问题（无Collider或canInteract=false）\n";
                report += "   解决方法: 给拼图物体添加Collider，确保canInteract=true\n\n";
                hasProblems = true;
            }
            else
            {
                report += $"   其中 {validCount}/{puzzleItems.Length} 个拼图配置正确\n\n";
            }
        }
        
        // 3. 检查UIManager配置
        UIManager uiManager = Object.FindObjectOfType<UIManager>();
        if (uiManager == null)
        {
            report += "❌ 问题4: 场景中没有 UIManager\n\n";
            hasProblems = true;
        }
        else
        {
            report += "✅ UIManager 存在\n";
            
            // 检查interactionPromptPanel引用
            var panelField = typeof(UIManager).GetField("interactionPromptPanel", 
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (panelField != null)
            {
                var panelValue = panelField.GetValue(uiManager);
                if (panelValue == null)
                {
                    report += "⚠️ 提示: UIManager.interactionPromptPanel 未设置（但会自动查找，不影响）\n";
                }
            }
        }
        
        // 4. 检查InteractionPromptPanel
        GameObject panel = GameObject.Find("InteractionPromptPanel");
        if (panel == null)
        {
            report += "❌ 问题5: 场景中没有 InteractionPromptPanel UI对象\n\n";
            hasProblems = true;
        }
        else
        {
            report += "✅ InteractionPromptPanel UI存在\n";
        }
        
        // 5. 检查Camera
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Camera[] cameras = Object.FindObjectsOfType<Camera>();
            if (cameras.Length == 0)
            {
                report += "❌ 问题6: 场景中没有 Camera\n\n";
                hasProblems = true;
            }
            else
            {
                report += $"⚠️ 提示: 没有Main Camera，但有 {cameras.Length} 个Camera\n\n";
            }
        }
        else
        {
            report += "✅ Main Camera 存在\n\n";
        }
        
        // 添加使用说明
        report += "=== 使用说明 ===\n";
        report += "1. 确保已点击'开始游戏'（InteractionPrompt只在游戏开始后检测）\n";
        report += "2. 靠近拼图物体（5米内，60度角度范围内）\n";
        report += "3. 查看Console窗口的调试日志\n";
        report += "4. 如果看到'检测到碰撞体'但没显示提示，检查拼图物体是否有PuzzleItem脚本和Collider\n\n";
        
        if (hasProblems)
        {
            report += "⚠️ 发现了问题，请先解决上述问题";
        }
        else
        {
            report += "✅ 基本配置正常\n";
            report += "如果仍不显示，请：\n";
            report += "- 运行游戏\n";
            report += "- 点击'开始游戏'\n";
            report += "- 查看Console窗口的调试日志";
        }
        
        EditorUtility.DisplayDialog("排查完成", report, "确定");
        Debug.Log(report);
    }
}

