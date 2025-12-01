using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 游戏管理器 - 管理游戏状态、拼图收集和胜利条件
/// </summary>
public class GameManager : MonoBehaviour
{
    // 单例模式，确保只有一个GameManager
    public static GameManager Instance;
    
    [Header("游戏设置")]
    public int totalPuzzles = 9;
    private int collectedPuzzles = 0;
    
    // 跟踪已收集的拼图ID（用于检查特定拼图是否已收集）
    private HashSet<int> collectedPuzzleIds = new HashSet<int>();
    
    [Header("UI引用（旧版兼容）")]
    public Text puzzleCountText;
    public GameObject victoryPanel;
    
    [Header("音效设置")]
    public AudioClip collectSound;
    public AudioClip victorySound;
    
    private AudioSource audioSource;
    
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
            return;
        }
        
        // 获取或添加音频组件
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
    }
    
    void Start()
    {
        UpdatePuzzleUI();
    }
    
    // 收集拼图的方法
    public void CollectPuzzle(int puzzleId)
    {
        // 如果这个拼图已经收集过，不重复计算
        if (collectedPuzzleIds.Contains(puzzleId))
        {
            Debug.LogWarning($"拼图 {puzzleId} 已经被收集过了！");
            return;
        }
        
        // 添加到已收集列表
        collectedPuzzleIds.Add(puzzleId);
        collectedPuzzles++;
        Debug.Log($"收集拼图 {puzzleId}！进度：{collectedPuzzles}/{totalPuzzles}");
        
        // 播放收集音效
        if (collectSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(collectSound);
        }
        
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
        // 更新旧版UI（兼容）
        if (puzzleCountText != null)
        {
            puzzleCountText.text = $"拼图: {collectedPuzzles}/{totalPuzzles}";
        }
        
        // 更新新版UI系统
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePuzzleProgress(collectedPuzzles, totalPuzzles);
        }
        
        // 更新拼图进度UI组件
        PuzzleProgressUI progressUI = FindObjectOfType<PuzzleProgressUI>();
        if (progressUI != null)
        {
            progressUI.UpdateProgress(collectedPuzzles, totalPuzzles);
        }
    }
    
    // 胜利处理
    void Victory()
    {
        Debug.Log("恭喜！你收集了所有拼图！游戏胜利！");
        
        // 播放胜利音效
        if (victorySound != null && audioSource != null)
        {
            audioSource.PlayOneShot(victorySound);
        }
        
        // 使用新版UI系统显示胜利界面
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowVictory();
        }
        
        // 兼容旧版UI
        if (victoryPanel != null)
        {
            victoryPanel.SetActive(true);
        }
        
        // 使用VictoryScreenController
        VictoryScreenController victoryController = FindObjectOfType<VictoryScreenController>();
        if (victoryController != null)
        {
            victoryController.Show();
        }
        
        // 暂停游戏
        Time.timeScale = 0f;
    }
    
    // 获取当前收集进度（其他脚本可以调用）
    public int GetCollectedPuzzles()
    {
        return collectedPuzzles;
    }
    
    // 重置游戏进度
    public void ResetProgress()
    {
        collectedPuzzles = 0;
        collectedPuzzleIds.Clear();
        UpdatePuzzleUI();
        Time.timeScale = 1f;
    }
    
    // ========== 游戏流程控制接口 ==========
    
    /// <summary>
    /// 开始新游戏 - 重置所有数据并开始游戏
    /// </summary>
    public void StartNewGame()
    {
        Debug.Log("开始新游戏");
        ResetProgress();
        
        // 重置游戏状态，但不调用UIManager（由UIManager调用此方法后再处理UI）
        // UI显示/隐藏由UIManager负责
    }
    
    /// <summary>
    /// 继续游戏 - 从暂停状态恢复
    /// </summary>
    public void ContinueGame()
    {
        Debug.Log("继续游戏");
        
        // 继续游戏实际上是恢复游戏
        ResumeGame();
    }
    
    /// <summary>
    /// 暂停游戏 - 暂停游戏时间并显示暂停菜单
    /// </summary>
    public void PauseGame()
    {
        Debug.Log("暂停游戏");
        
        // 暂停游戏时间（UI显示/隐藏由UIManager负责）
        Time.timeScale = 0f;
    }
    
    /// <summary>
    /// 恢复游戏 - 从暂停状态恢复
    /// </summary>
    public void ResumeGame()
    {
        Debug.Log("恢复游戏");
        
        // 恢复游戏时间（UI显示/隐藏由UIManager负责）
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// 返回主菜单 - 返回主菜单界面
    /// </summary>
    public void ReturnToMainMenu()
    {
        Debug.Log("返回主菜单");
        
        // 重置时间（UI显示/隐藏由UIManager负责）
        Time.timeScale = 1f;
    }
    
    /// <summary>
    /// 重新开始游戏 - 重新加载场景并重置进度
    /// </summary>
    public void RestartGame()
    {
        Debug.Log("重新开始游戏");
        
        // 重置进度
        ResetProgress();
        
        // 重新加载场景
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }
    
    /// <summary>
    /// 退出游戏 - 退出应用程序
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("退出游戏");
        
        // 通知UIManager退出游戏
        if (UIManager.Instance != null)
        {
            UIManager.Instance.QuitGame();
        }
        else
        {
            // 如果没有UIManager，直接退出
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    // ========== 拼图状态查询接口 ==========
    
    /// <summary>
    /// 检查指定ID的拼图是否已收集
    /// </summary>
    /// <param name="puzzleId">拼图ID</param>
    /// <returns>如果已收集返回true，否则返回false</returns>
    public bool IsPuzzleCollected(int puzzleId)
    {
        return collectedPuzzleIds.Contains(puzzleId);
    }
    
    /// <summary>
    /// 获取总拼图数量（属性）
    /// </summary>
    public int TotalPuzzles
    {
        get { return totalPuzzles; }
    }
    
    /// <summary>
    /// 获取已收集的拼图数量（用于诊断脚本）
    /// </summary>
    /// <returns>已收集的拼图数量</returns>
    public int GetCollectedPuzzlesCount()
    {
        return collectedPuzzles;
    }
    
    // ========== 游戏事件接口 ==========
    
    /// <summary>
    /// 玩家被守卫发现时调用（由GuardAI脚本调用）
    /// </summary>
    public void PlayerDetectedByGuard()
    {
        Debug.Log("玩家被守卫发现了！");
        // 可以在这里添加被发现的处理逻辑
        // 例如：触发警报、减少生命值、记录被发现次数等
    }
}