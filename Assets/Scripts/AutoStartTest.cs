using UnityEngine;

public class AutoStartTest : MonoBehaviour
{
    void Start()
    {
        // 延迟一小会儿，确保其他管理器初始化完毕
        Invoke("ForceStart", 0.1f);
    }

    void ForceStart()
    {
        Debug.Log("【测试模式】强制开始游戏");

        // 解锁时间（防止因为之前的状态导致暂停）
        Time.timeScale = 1f;

        // 告诉 UIManager 锁定鼠标并隐藏主菜单
        if (UIManager.Instance != null)
        {
            UIManager.Instance.StartGame();
        }
    }
}