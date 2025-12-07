using UnityEngine;
using UnityEditor;

/// <summary>
/// 编辑器工具：添加InteractionPrompt对象到场景（如果不存在）
/// </summary>
public class AddInteractionPrompt
{
    [MenuItem("Tools/Add Interaction Prompt to Scene")]
    static void AddInteractionPromptToScene()
    {
        // 检查是否已存在InteractionPrompt对象
        GameObject existingPrompt = GameObject.Find("InteractionPrompt");
        if (existingPrompt != null)
        {
            if (EditorUtility.DisplayDialog("InteractionPrompt已存在", 
                "场景中已存在InteractionPrompt对象，是否要删除并重新创建？", "是", "否"))
            {
                Object.DestroyImmediate(existingPrompt);
            }
            else
            {
                return;
            }
        }

        // 创建InteractionPrompt对象
        GameObject interactionPrompt = new GameObject("InteractionPrompt");
        
        // 添加InteractionPrompt脚本
        interactionPrompt.AddComponent<InteractionPrompt>();
        
        Debug.Log("✅ InteractionPrompt对象已添加到场景！");
        EditorUtility.DisplayDialog("成功", 
            "InteractionPrompt对象已添加到场景中！\n\n" +
            "功能说明：\n" +
            "- 当玩家靠近可交互物体（如拼图）时，会自动显示交互提示\n" +
            "- 默认提示文本：Press E To Collect（针对拼图）\n" +
            "- 检测距离：5米\n" +
            "- 检测角度：60度\n\n" +
            "提示：\n" +
            "- 确保场景中有InteractionPromptPanel UI对象\n" +
            "- 确保拼图物体有PuzzleItem脚本和Collider\n" +
            "- 可以在Inspector中调整检测距离和角度", 
            "确定");

        // 选中新创建的对象
        Selection.activeGameObject = interactionPrompt;
    }
}

