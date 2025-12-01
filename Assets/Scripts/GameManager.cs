using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    // ========== 单例模式 ==========
    public static GameManager Instance { get; private set; }
    
    // ========== 游戏状态枚举 ==========
    public enum GameState
    {
        MainMenu,       // 主菜单
        Loading,        // 加载中
        Playing,        // 游戏中
        Paused,         // 暂停
        PuzzleSolving,  // 解谜中（特殊状态）
        Victory,        // 胜利
        GameOver,       // 游戏结束（被守卫发现）
        Cutscene        // 过场动画
    }
    
    // ========== 事件系统 ==========
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int> OnPuzzleCollected;
    public static event Action<float> OnTimeUpdated;
    public static event Action<bool> OnPlayerDetected; // 玩家被守卫发现
    
    // ========== 游戏数据 ==========
    [Header("游戏基础设置")]
    public int totalPuzzles = 9;
    public float maxGameTime = 600f; // 10分钟限制
    
    [Header("谜题设置")]
    public string correctRuneSequence = "123"; // 符文正确顺序
    public string correctTorchSequence = "ABCD"; // 火把正确顺序
    
    private int collectedPuzzles = 0;
    private GameState currentState = GameState.MainMenu;
    private float gameTime = 0f;
    private bool isTiming = false;
    private int playerLives = 3; // 玩家生命值
    
    // ========== 游戏进度数据 ==========
    private Dictionary<int, bool> puzzleCollectionStatus = new Dictionary<int, bool>();
    private string currentRuneInput = "";
    private string currentTorchInput = "";
    private bool isBossDefeated = false;
    
    // ========== 持久化数据 ==========
    private float bestTime = float.MaxValue;
    private int gamesPlayed = 0;
    
    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeGameData();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        LoadPlayerPrefs();
        SetGameState(GameState.MainMenu);
    }
    
    void Update()
    {
        // 游戏计时
        if (isTiming)
        {
            gameTime += Time.deltaTime;
            OnTimeUpdated?.Invoke(gameTime);
            
            // 检查时间限制
            if (gameTime >= maxGameTime)
            {
                GameOver("时间耗尽！");
            }
        }
        
        // 全局输入检测
        HandleGlobalInput();
    }
    
    // ========== 初始化方法 ==========
    void InitializeGameData()
    {
        // 初始化拼图收集状态
        for (int i = 1; i <= totalPuzzles; i++)
        {
            puzzleCollectionStatus[i] = false;
        }
    }
    
    void LoadPlayerPrefs()
    {
        bestTime = PlayerPrefs.GetFloat("BestTime", float.MaxValue);
        gamesPlayed = PlayerPrefs.GetInt("GamesPlayed", 0);
    }
    
    void SavePlayerPrefs()
    {
        PlayerPrefs.SetFloat("BestTime", bestTime);
        PlayerPrefs.SetInt("GamesPlayed", gamesPlayed);
        PlayerPrefs.Save();
    }
    
    // ========== 游戏状态控制 ==========
    public void SetGameState(GameState newState)
    {
        GameState previousState = currentState;
        currentState = newState;
        
        switch (currentState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                ResetGameData();
                break;
                
            case GameState.Loading:
                Time.timeScale = 1f;
                break;
                
            case GameState.Playing:
                Time.timeScale = 1f;
                StartGameTimer();
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                StopGameTimer();
                break;
                
            case GameState.PuzzleSolving:
                Time.timeScale = 1f; // 解谜时游戏继续，但玩家控制可能被限制
                break;
                
            case GameState.Victory:
                Time.timeScale = 0f;
                StopGameTimer();
                SaveBestTime();
                break;
                
            case GameState.GameOver:
                Time.timeScale = 0f;
                StopGameTimer();
                break;
                
            case GameState.Cutscene:
                Time.timeScale = 1f; // 过场动画时游戏继续
                break;
        }
        
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log($"游戏状态: {previousState} -> {currentState}");
    }
    
    // ========== 公共接口 - 游戏流程控制 ==========
    public void StartNewGame()
    {
        gamesPlayed++;
        ResetGameData();
        SetGameState(GameState.Loading);
        // 实际场景加载在加载完成后会调用OnSceneLoaded
    }
    
    public void ContinueGame()
    {
        SetGameState(GameState.Playing);
    }
    
    public void PauseGame()
    {
        if (currentState == GameState.Playing || currentState == GameState.PuzzleSolving)
        {
            SetGameState(GameState.Paused);
        }
    }
    
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
        {
            SetGameState(GameState.Playing);
        }
    }
    
    public void ReturnToMainMenu()
    {
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }
    
    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    
    public void QuitGame()
    {
        SavePlayerPrefs();
        
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
    
    // ========== 公共接口 - 拼图收集系统 ==========
    public void CollectPuzzle(int puzzleId)
    {
        if (currentState != GameState.Playing) return;
        
        if (puzzleId >= 1 && puzzleId <= totalPuzzles && !puzzleCollectionStatus[puzzleId])
        {
            puzzleCollectionStatus[puzzleId] = true;
            collectedPuzzles++;
            
            OnPuzzleCollected?.Invoke(puzzleId);
            Debug.Log($"收集拼图 {puzzleId}！进度：{collectedPuzzles}/{totalPuzzles}");
            
            // 检查胜利条件
            if (collectedPuzzles >= totalPuzzles && isBossDefeated)
            {
                Victory();
            }
        }
    }
    
    public bool IsPuzzleCollected(int puzzleId)
    {
        return puzzleCollectionStatus.ContainsKey(puzzleId) && puzzleCollectionStatus[puzzleId];
    }
    
    // ========== 公共接口 - 谜题系统 ==========
    public void InputRune(string runeId)
    {
        if (currentState != GameState.PuzzleSolving) return;
        
        currentRuneInput += runeId;
        Debug.Log($"符文输入: {currentRuneInput}");
        
        if (currentRuneInput.Length >= correctRuneSequence.Length)
        {
            if (currentRuneInput == correctRuneSequence)
            {
                RunePuzzleSolved();
            }
            else
            {
                RunePuzzleFailed();
            }
        }
    }
    
    public void InputTorch(string torchId)
    {
        if (currentState != GameState.PuzzleSolving) return;
        
        currentTorchInput += torchId;
        Debug.Log($"火把输入: {currentTorchInput}");
        
        if (currentTorchInput.Length >= correctTorchSequence.Length)
        {
            if (currentTorchInput == correctTorchSequence)
            {
                TorchPuzzleSolved();
            }
            else
            {
                TorchPuzzleFailed();
            }
        }
    }
    
    public void StartRunePuzzle()
    {
        currentRuneInput = "";
        SetGameState(GameState.PuzzleSolving);
    }
    
    public void StartTorchPuzzle()
    {
        currentTorchInput = "";
        SetGameState(GameState.PuzzleSolving);
    }
    
    // ========== 公共接口 - 战斗系统 ==========
    public void PlayerDetectedByGuard()
    {
        playerLives--;
        OnPlayerDetected?.Invoke(true);
        
        if (playerLives <= 0)
        {
            GameOver("被守卫发现次数过多！");
        }
        else
        {
            Debug.Log($"被守卫发现！剩余生命: {playerLives}");
            // 这里可以触发重置玩家位置等逻辑
        }
    }
    
    public void BossDefeated()
    {
        isBossDefeated = true;
        Debug.Log("Boss被击败！");
        
        // 检查胜利条件
        if (collectedPuzzles >= totalPuzzles && isBossDefeated)
        {
            Victory();
        }
    }
    
    // ========== 公共接口 - 数据查询 ==========
    public int GetCollectedPuzzlesCount()
    {
        return collectedPuzzles;
    }
    
    public float GetGameTime()
    {
        return gameTime;
    }
    
    public int GetPlayerLives()
    {
        return playerLives;
    }
    
    public float GetProgressPercentage()
    {
        return (float)collectedPuzzles / totalPuzzles;
    }
    
    public GameState GetCurrentGameState()
    {
        return currentState;
    }
    
    public float GetBestTime()
    {
        return bestTime;
    }
    
    // ========== 私有方法 ==========
    void HandleGlobalInput()
    {
        // ESC键暂停/恢复游戏
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing || currentState == GameState.PuzzleSolving)
            {
                PauseGame();
            }
            else if (currentState == GameState.Paused)
            {
                ResumeGame();
            }
        }
    }
    
    void StartGameTimer()
    {
        isTiming = true;
        gameTime = 0f;
    }
    
    void StopGameTimer()
    {
        isTiming = false;
    }
    
    void ResetGameData()
    {
        collectedPuzzles = 0;
        gameTime = 0f;
        playerLives = 3;
        isBossDefeated = false;
        currentRuneInput = "";
        currentTorchInput = "";
        
        // 重置拼图收集状态
        foreach (int key in new List<int>(puzzleCollectionStatus.Keys))
        {
            puzzleCollectionStatus[key] = false;
        }
    }
    
    void SaveBestTime()
    {
        if (gameTime < bestTime)
        {
            bestTime = gameTime;
            PlayerPrefs.SetFloat("BestTime", bestTime);
            PlayerPrefs.Save();
            Debug.Log($"新纪录！最佳时间: {FormatTime(bestTime)}");
        }
    }
    
    void Victory()
    {
        SetGameState(GameState.Victory);
        Debug.Log("游戏胜利！");
    }
    
    void GameOver(string reason)
    {
        SetGameState(GameState.GameOver);
        Debug.Log($"游戏结束: {reason}");
    }
    
    void RunePuzzleSolved()
    {
        Debug.Log("符文谜题解决！");
        SetGameState(GameState.Playing);
        // 这里可以触发开门等逻辑
    }
    
    void RunePuzzleFailed()
    {
        Debug.Log("符文谜题失败！");
        currentRuneInput = "";
        // 这里可以触发惩罚机制
    }
    
    void TorchPuzzleSolved()
    {
        Debug.Log("火把谜题解决！");
        SetGameState(GameState.Playing);
        // 这里可以触发宝箱开启等逻辑
    }
    
    void TorchPuzzleFailed()
    {
        Debug.Log("火把谜题失败！");
        currentTorchInput = "";
        // 这里可以触发惩罚机制
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    
    // ========== 场景管理 ==========
    public void LoadScene(string sceneName)
    {
        SetGameState(GameState.Loading);
        SceneManager.LoadScene(sceneName);
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (currentState == GameState.Loading)
        {
            SetGameState(GameState.Playing);
        }
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}