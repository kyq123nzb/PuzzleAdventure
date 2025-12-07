using UnityEngine;
using UnityEditor;

/// <summary>
/// 快速检查：InteractionPrompt对象是否存在
/// </summary>
public class QuickCheckInteractionPrompt
{
    [MenuItem("Tools/快速检查：InteractionPrompt对象")]
    static void QuickCheck()
    {
        GameObject interactionPrompt = GameObject.Find("InteractionPrompt");
        
        if (interactionPrompt == null)
        {
            bool create = EditorUtility.DisplayDialog("未找到 InteractionPrompt 对象", 
                "场景中没有找到 InteractionPrompt 对象！\n\n" +
                "这是导致交互提示不显示的主要原因。\n\n" +
                "是否要创建一个 InteractionPrompt 对象？", 
                "是，创建", "取消");
            
            if (create)
            {
                GameObject newObj = new GameObject("InteractionPrompt");
                newObj.AddComponent<InteractionPrompt>();
                Selection.activeGameObject = newObj;
                EditorUtility.DisplayDialog("成功", 
                    "InteractionPrompt 对象已创建！\n\n" +
                    "现在请：\n" +
                    "1. 运行游戏\n" +
                    "2. 点击开始游戏\n" +
                    "3. 靠近拼图测试交互提示\n\n" +
                    "如果仍然不显示，请启用 InteractionPrompt 的 'Enable Debug Log' 选项查看日志。", 
                    "确定");
                Debug.Log("✅ InteractionPrompt 对象已创建！");
            }
        }
        else
        {
            InteractionPrompt script = interactionPrompt.GetComponent<InteractionPrompt>();
            if (script == null)
            {
                EditorUtility.DisplayDialog("错误", 
                    "InteractionPrompt 对象存在，但没有 InteractionPrompt 脚本！\n\n" +
                    "正在添加脚本...", 
                    "确定");
                interactionPrompt.AddComponent<InteractionPrompt>();
            }
            else
            {
                Selection.activeGameObject = interactionPrompt;
                bool enableDebug = EditorUtility.DisplayDialog("InteractionPrompt 对象存在", 
                    "InteractionPrompt 对象已找到！\n\n" +
                    "当前设置：\n" +
                    $"检测距离: {script.interactionDistance}m\n" +
                    $"检测角度: {script.detectionAngle}°\n" +
                    $"调试日志: {(script.enableDebugLog ? "已启用" : "未启用")}\n\n" +
                    "是否启用调试日志以便排查问题？", 
                    "启用调试", "取消");
                
                if (enableDebug)
                {
                    script.enableDebugLog = true;
                    EditorUtility.SetDirty(interactionPrompt);
                    Debug.Log("✅ 已启用 InteractionPrompt 调试日志");
                }
            }
        }
    }
}

