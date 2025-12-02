using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Linq;

public class GameManager : MonoBehaviour
{
    // ========== 单例模式 ==========
    public static GameManager Instance { get; private set; }
    
    // ========== 游戏状态枚举 ==========
    public enum GameState
    {
        MainMenu,
        Loading,
        Playing,
        Paused,
        PuzzleSolving,
        Victory,
        GameOver,
        Cutscene
    }
    
    // ========== 事件系统 ==========
    public static event Action<GameState> OnGameStateChanged;
    public static event Action<int> OnPuzzleCollected;
    public static event Action<float> OnTimeUpdated;
    public static event Action<bool> OnPlayerDetected;
    public static event Action<int> OnPlayerLivesChanged;
    
    // ========== 游戏数据 ==========
    [Header("游戏基础设置")]
    [SerializeField] private int totalPuzzles = 9;
    [SerializeField] private float maxGameTime = 600f;
    [SerializeField] private int initialPlayerLives = 3;
    
    [Header("谜题设置")]
    [SerializeField] private string correctRuneSequence = "123";
    [SerializeField] private string correctTorchSequence = "ABCD";
    
    [Header("UI引用")]
    [SerializeField] private GameObject pauseMenuUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject victoryUI;
    
    private int collectedPuzzles = 0;
    private GameState currentState = GameState.MainMenu;
    private float gameTime = 0f;
    private bool isTiming = false;
    private int playerLives = 3;
    
    // ========== 游戏进度数据 ==========
    private Dictionary<int, bool> puzzleCollectionStatus = new Dictionary<int, bool>();
    private string currentRuneInput = "";
    private string currentTorchInput = "";
    private bool isBossDefeated = false;
    private bool[] runeSequenceCheck;
    private bool[] torchSequenceCheck;
    
    // ========== 持久化数据 ==========
    private float bestTime = float.MaxValue;
    private int gamesPlayed = 0;
    private const string BEST_TIME_KEY = "BestTime";
    private const string GAMES_PLAYED_KEY = "GamesPlayed";
    
    // ========== 属性访问器 ==========
    public int TotalPuzzles => totalPuzzles;
    public float MaxGameTime => maxGameTime;
    public int PlayerLives => playerLives;
    
    void Awake()
    {
        // 单例模式初始化
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("GameManager Awake: 实例已创建");
        }
        else
        {
            Debug.LogWarning("GameManager Awake: 已存在实例，销毁新实例");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        LoadPlayerPrefs();
        InitializeGameData(); // 移到这里，确保在Start中初始化
        SetGameState(GameState.MainMenu);
        InitializeUIReferences();
        Debug.Log($"GameManager Start: 初始化完成，总拼图数: {totalPuzzles}");
    }
    
    void Update()
    {
        // 游戏计时
        if (isTiming)
        {
            gameTime += Time.deltaTime;
            OnTimeUpdated?.Invoke(gameTime);
            
            if (gameTime >= maxGameTime)
            {
                GameOver("时间耗尽！");
            }
        }
        
        HandleGlobalInput();
    }
    
    // ========== 初始化方法 ==========
    void InitializeGameData()
    {
        // 确保字典为空再初始化
        puzzleCollectionStatus.Clear();
        
        // 初始化拼图收集状态
        for (int i = 1; i <= totalPuzzles; i++)
        {
            puzzleCollectionStatus[i] = false;
            Debug.Log($"初始化拼图 {i}: 状态为未收集");
        }
        
        // 初始化序列检查数组
        runeSequenceCheck = new bool[correctRuneSequence.Length];
        torchSequenceCheck = new bool[correctTorchSequence.Length];
        
        Debug.Log($"游戏数据初始化完成，共 {totalPuzzles} 个拼图");
    }
    
    void InitializeUIReferences()
    {
        if (pauseMenuUI == null)
            pauseMenuUI = GameObject.Find("PauseMenu");
        if (gameOverUI == null)
            gameOverUI = GameObject.Find("GameOverUI");
        if (victoryUI == null)
            victoryUI = GameObject.Find("VictoryUI");
    }
    
    void LoadPlayerPrefs()
    {
        bestTime = PlayerPrefs.GetFloat(BEST_TIME_KEY, float.MaxValue);
        gamesPlayed = PlayerPrefs.GetInt(GAMES_PLAYED_KEY, 0);
    }
    
    void SavePlayerPrefs()
    {
        PlayerPrefs.SetFloat(BEST_TIME_KEY, bestTime);
        PlayerPrefs.SetInt(GAMES_PLAYED_KEY, gamesPlayed);
        PlayerPrefs.Save();
    }
    
    // ========== 游戏状态控制 ==========
    public void SetGameState(GameState newState)
    {
        if (currentState == newState) return;
        
        GameState previousState = currentState;
        currentState = newState;
        
        switch (currentState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                ResetGameData();
                UpdateUIVisibility();
                break;
                
            case GameState.Loading:
                Time.timeScale = 1f;
                UpdateUIVisibility();
                break;
                
            case GameState.Playing:
                Time.timeScale = 1f;
                StartGameTimer();
                UpdateUIVisibility();
                break;
                
            case GameState.Paused:
                Time.timeScale = 0f;
                StopGameTimer();
                UpdateUIVisibility();
                break;
                
            case GameState.PuzzleSolving:
                Time.timeScale = 1f;
                UpdateUIVisibility();
                break;
                
            case GameState.Victory:
                Time.timeScale = 0f;
                StopGameTimer();
                SaveBestTime();
                UpdateUIVisibility();
                break;
                
            case GameState.GameOver:
                Time.timeScale = 0f;
                StopGameTimer();
                UpdateUIVisibility();
                break;
                
            case GameState.Cutscene:
                Time.timeScale = 1f;
                UpdateUIVisibility();
                break;
        }
        
        OnGameStateChanged?.Invoke(currentState);
        Debug.Log($"游戏状态: {previousState} -> {currentState} | 时间: {FormatTime(gameTime)}");
    }
    
    // ========== UI管理 ==========
    void UpdateUIVisibility()
    {
        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(currentState == GameState.Paused);
        
        if (gameOverUI != null)
            gameOverUI.SetActive(currentState == GameState.GameOver);
        
        if (victoryUI != null)
            victoryUI.SetActive(currentState == GameState.Victory);
    }
    
    // ========== 公共接口 - 游戏流程控制 ==========
    public void StartNewGame()
    {
        gamesPlayed++;
        ResetGameData();
        SetGameState(GameState.Loading);
        SceneManager.LoadScene("GameScene");
    }
    
    public void ContinueGame() => SetGameState(GameState.Playing);
    
    public void PauseGame()
    {
        if (currentState == GameState.Playing || currentState == GameState.PuzzleSolving)
            SetGameState(GameState.Paused);
    }
    
    public void ResumeGame()
    {
        if (currentState == GameState.Paused)
            SetGameState(GameState.Playing);
    }
    
    public void ReturnToMainMenu()
    {
        SetGameState(GameState.MainMenu);
        SceneManager.LoadScene("MainMenu");
    }
    
    public void RestartGame()
    {
        ResetGameData();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        SetGameState(GameState.Playing);
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
        if (currentState != GameState.Playing)
        {
            Debug.LogWarning($"无法收集拼图 {puzzleId}: 游戏状态为 {currentState}，非Playing状态");
            return;
        }
        
        // 添加详细的调试信息
        Debug.Log($"尝试收集拼图 ID: {puzzleId}");
        Debug.Log($"ID范围检查: {puzzleId >= 1 && puzzleId <= totalPuzzles}");
        Debug.Log($"是否已收集: {puzzleCollectionStatus.ContainsKey(puzzleId) && puzzleCollectionStatus[puzzleId]}");
        
        if (puzzleId >= 1 && puzzleId <= totalPuzzles)
        {
            if (!puzzleCollectionStatus.ContainsKey(puzzleId))
            {
                Debug.LogError($"拼图ID {puzzleId} 不存在于字典中！重新初始化字典...");
                puzzleCollectionStatus[puzzleId] = false;
            }
            
            if (!puzzleCollectionStatus[puzzleId])
            {
                puzzleCollectionStatus[puzzleId] = true;
                collectedPuzzles++;
                
                OnPuzzleCollected?.Invoke(puzzleId);
                Debug.Log($"✓ 成功收集拼图 {puzzleId}！进度：{collectedPuzzles}/{totalPuzzles}");
                
                // 检查胜利条件
                CheckVictoryCondition();
            }
            else
            {
                Debug.LogWarning($"拼图 {puzzleId} 已经收集过了");
            }
        }
        else
        {
            Debug.LogError($"拼图ID {puzzleId} 超出有效范围 (1-{totalPuzzles})");
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
        
        int index = currentRuneInput.Length - 1;
        if (index < runeSequenceCheck.Length)
        {
            runeSequenceCheck[index] = (currentRuneInput[index] == correctRuneSequence[index]);
        }
        
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
        
        int index = currentTorchInput.Length - 1;
        if (index < torchSequenceCheck.Length)
        {
            torchSequenceCheck[index] = (currentTorchInput[index] == correctTorchSequence[index]);
        }
        
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
        Array.Clear(runeSequenceCheck, 0, runeSequenceCheck.Length);
        SetGameState(GameState.PuzzleSolving);
    }
    
    public void StartTorchPuzzle()
    {
        currentTorchInput = "";
        Array.Clear(torchSequenceCheck, 0, torchSequenceCheck.Length);
        SetGameState(GameState.PuzzleSolving);
    }
    
    public bool[] GetRuneSequenceStatus() => runeSequenceCheck;
    public bool[] GetTorchSequenceStatus() => torchSequenceCheck;
    
    // ========== 公共接口 - 战斗系统 ==========
    public void PlayerDetectedByGuard()
    {
        playerLives--;
        OnPlayerDetected?.Invoke(true);
        OnPlayerLivesChanged?.Invoke(playerLives);
        
        if (playerLives <= 0)
        {
            GameOver("被守卫发现次数过多！");
        }
        else
        {
            Debug.Log($"被守卫发现！剩余生命: {playerLives}");
        }
    }
    
    public void BossDefeated()
    {
        isBossDefeated = true;
        Debug.Log("Boss被击败！");
        CheckVictoryCondition();
    }
    
    public void AddPlayerLife(int amount = 1)
    {
        playerLives += amount;
        OnPlayerLivesChanged?.Invoke(playerLives);
        Debug.Log($"增加 {amount} 点生命，当前生命: {playerLives}");
    }
    
    // ========== 公共接口 - 数据查询 ==========
    public int GetCollectedPuzzlesCount() => collectedPuzzles;
    public float GetGameTime() => gameTime;
    public int GetPlayerLives() => playerLives;
    
    public float GetProgressPercentage()
    {
        return totalPuzzles > 0 ? (float)collectedPuzzles / totalPuzzles : 0f;
    }
    
    public GameState GetCurrentGameState() => currentState;
    public float GetBestTime() => bestTime;
    public int GetGamesPlayed() => gamesPlayed;
    
    // ========== 检查胜利条件 ==========
    void CheckVictoryCondition()
    {
        if (collectedPuzzles >= totalPuzzles && isBossDefeated)
        {
            Victory();
        }
        else if (collectedPuzzles >= totalPuzzles)
        {
            Debug.Log("所有拼图已收集，但还需击败Boss！");
        }
        else if (isBossDefeated)
        {
            Debug.Log("Boss已击败，但还需收集所有拼图！");
        }
    }
    
    // ========== 私有方法 ==========
    void HandleGlobalInput()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (currentState == GameState.Playing || currentState == GameState.PuzzleSolving)
                PauseGame();
            else if (currentState == GameState.Paused)
                ResumeGame();
        }
        
        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.V)) Victory();
        if (Input.GetKeyDown(KeyCode.G)) GameOver("测试游戏结束");
        if (Input.GetKeyDown(KeyCode.P)) CollectPuzzle(1);
        #endif
    }
    
    void StartGameTimer()
    {
        isTiming = true;
        gameTime = 0f;
    }
    
    void StopGameTimer() => isTiming = false;
    
    void ResetGameData()
    {
        collectedPuzzles = 0;
        gameTime = 0f;
        playerLives = initialPlayerLives;
        isBossDefeated = false;
        currentRuneInput = "";
        currentTorchInput = "";
        
        // 重置拼图收集状态
        puzzleCollectionStatus.Clear();
        for (int i = 1; i <= totalPuzzles; i++)
        {
            puzzleCollectionStatus[i] = false;
        }
        
        // 重置序列检查
        if (runeSequenceCheck != null)
            Array.Clear(runeSequenceCheck, 0, runeSequenceCheck.Length);
        if (torchSequenceCheck != null)
            Array.Clear(torchSequenceCheck, 0, torchSequenceCheck.Length);
        
        Debug.Log("游戏数据已重置");
    }
    
    void SaveBestTime()
    {
        if (gameTime < bestTime)
        {
            bestTime = gameTime;
            PlayerPrefs.SetFloat(BEST_TIME_KEY, bestTime);
            PlayerPrefs.Save();
            Debug.Log($"新纪录！最佳时间: {FormatTime(bestTime)}");
        }
    }
    
    void Victory()
    {
        SetGameState(GameState.Victory);
        Debug.Log($"游戏胜利！用时: {FormatTime(gameTime)}");
    }
    
    void GameOver(string reason)
    {
        SetGameState(GameState.GameOver);
        Debug.Log($"游戏结束: {reason} | 用时: {FormatTime(gameTime)}");
    }
    
    void RunePuzzleSolved()
    {
        Debug.Log("符文谜题解决！");
        SetGameState(GameState.Playing);
    }
    
    void RunePuzzleFailed()
    {
        Debug.Log("符文谜题失败！");
        currentRuneInput = "";
        Array.Clear(runeSequenceCheck, 0, runeSequenceCheck.Length);
    }
    
    void TorchPuzzleSolved()
    {
        Debug.Log("火把谜题解决！");
        SetGameState(GameState.Playing);
    }
    
    void TorchPuzzleFailed()
    {
        Debug.Log("火把谜题失败！");
        currentTorchInput = "";
        Array.Clear(torchSequenceCheck, 0, torchSequenceCheck.Length);
    }
    
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60f);
        return $"{minutes:00}:{seconds:00}";
    }
    
    // ========== 场景管理 ==========
    public void LoadScene(string sceneName)
    {
        SetGameState(GameState.Loading);
        SceneManager.LoadScene(sceneName);
    }
    
    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        InitializeUIReferences();
        
        if (currentState == GameState.Loading)
            SetGameState(GameState.Playing);
    }
    
    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SavePlayerPrefs();
    }
    
    void OnApplicationQuit()
    {
        SavePlayerPrefs();
    }
}