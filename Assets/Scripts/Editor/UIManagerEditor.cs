using UnityEngine;
using UnityEditor;

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
        
        EditorGUILayout.HelpBox("点击上面的按钮可以直接测试庆祝界面是否能正常显示。", MessageType.Info);
    }
}

