using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // 单例模式，确保只有一个GameManager
    public static GameManager Instance;
    
    [Header("游戏设置")]
    public int totalPuzzles = 9;
    private int collectedPuzzles = 0;
    
    [Header("UI引用")]
    public Text puzzleCountText;
    public GameObject victoryPanel;
    
    void Awake()
    {
        // 单例模式设置
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        UpdatePuzzleUI();
    }
    
    // 收集拼图的方法
    public void CollectPuzzle(int puzzleId)
    {
        collectedPuzzles++;
        Debug.Log($"收集拼图 {puzzleId}！进度：{collectedPuzzles}/{totalPuzzles}");
        
        UpdatePuzzleUI();
        
        // 检查胜利条件
        if (collectedPuzzles >= totalPuzzles)
        {
            Victory();
        }
    }
    
    // 更新UI显示
    void UpdatePuzzleUI()
    {
        if (puzzleCountText != null)
        {
            puzzleCountText.text = $"拼图: {collectedPuzzles}/{totalPuzzles}";
        }
    }
    
    // 胜利处理
    void Victory()
    {
        Debug.Log("恭喜！你收集了所有拼图！游戏胜利！");
        
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        
        // 可以在这里暂停游戏或显示胜利画面
        Time.timeScale = 0f;
    }
    
    // 获取当前收集进度（其他脚本可以调用）
    public int GetCollectedPuzzles()
    {
        return collectedPuzzles;
    }
}