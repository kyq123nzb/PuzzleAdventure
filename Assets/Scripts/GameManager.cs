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
        InitializeGameData();
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
                // 通知UIManager显示胜利面板
                Debug.Log("🎉 GameManager: 游戏状态变为Victory，准备显示胜利面板");
                if (UIManager.Instance != null)
                {
                    Debug.Log("✅ GameManager: UIManager.Instance存在，调用ShowVictory()");
                    UIManager.Instance.ShowVictory();
                }
                else
                {
                    Debug.LogError("❌ GameManager: UIManager.Instance为空！无法显示胜利面板！");
                }
                break;
                
            case GameState.GameOver:
                Time.timeScale = 0f;
                StopGameTimer();
                UpdateUIVisibility();
                // 通知UIManager显示失败面板
                Debug.Log("💀 GameManager: 游戏状态变为GameOver，准备显示失败面板");
                if (UIManager.Instance != null)
                {
                    Debug.Log("✅ GameManager: UIManager.Instance存在，调用ShowDefeat()");
                    UIManager.Instance.ShowDefeat();
                }
                else
                {
                    Debug.LogError("❌ GameManager: UIManager.Instance为空！无法显示失败面板！");
                }
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
        Debug.Log("🔄 GameManager: RestartGame 被调用，重置所有游戏数据并重新加载场景");
        
        // 重置所有游戏数据
        ResetGameData();
        
        // 重新加载当前场景（场景加载后会自动初始化）
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        
        // 注意：SetGameState 会在场景加载完成后通过 OnSceneLoaded 调用
        // 这里先设置为 Loading 状态，场景加载完成后会自动变为 Playing
        SetGameState(GameState.Loading);
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
    
    // ========== 调试方法：检查当前收集状态 ==========
    [ContextMenu("调试：检查拼图收集状态")]
    public void DebugCheckPuzzleStatus()
    {
        Debug.LogWarning($"=== 拼图收集状态调试 ===");
        Debug.LogWarning($"当前游戏状态: {currentState}");
        Debug.LogWarning($"总拼图数: {totalPuzzles}");
        Debug.LogWarning($"已收集数: {collectedPuzzles}");
        Debug.LogWarning($"收集进度: {collectedPuzzles}/{totalPuzzles}");
        Debug.LogWarning($"Boss状态: {isBossDefeated}");
        
        for (int i = 1; i <= totalPuzzles; i++)
        {
            bool collected = puzzleCollectionStatus.ContainsKey(i) && puzzleCollectionStatus[i];
            Debug.LogWarning($"拼图 {i}: {(collected ? "✅ 已收集" : "❌ 未收集")}");
        }
        
        if (collectedPuzzles >= totalPuzzles)
        {
            Debug.LogWarning($"🎉 所有拼图已收集！应该显示庆祝界面！");
            ShowPuzzleCompleteCelebration();
        }
        else
        {
            Debug.LogWarning($"还需收集 {totalPuzzles - collectedPuzzles} 块拼图");
        }
    }
    
    // ========== 公共接口 - 拼图收集系统 ==========
    public void CollectPuzzle(int puzzleId)
    {
        if (currentState != GameState.Playing)
        {
            Debug.LogWarning($"无法收集拼图 {puzzleId}: 游戏状态为 {currentState}");
            return;
        }
        
        if (puzzleId >= 1 && puzzleId <= totalPuzzles)
        {
            // 修复：先检查是否包含，如果包含就获取状态，如果不包含就初始化
            if (!puzzleCollectionStatus.ContainsKey(puzzleId))
            {
                puzzleCollectionStatus[puzzleId] = false;
            }
            
            bool isAlreadyCollected = puzzleCollectionStatus[puzzleId];
            
            if (!isAlreadyCollected)
            {
                puzzleCollectionStatus[puzzleId] = true;
                collectedPuzzles++;
                
                Debug.Log($"成功收集拼图 {puzzleId}！当前进度：{collectedPuzzles}/{totalPuzzles}");
                
                // 触发事件（供其他系统订阅）
                OnPuzzleCollected?.Invoke(puzzleId);
                
                // 直接更新UI（备用机制，确保UI更新）
                UpdateProgressUI();
                
                // 检查胜利条件（可能会触发Victory或显示庆祝界面）
                CheckVictoryCondition();
            }
            else
            {
                Debug.LogWarning($"拼图 {puzzleId} 已经被收集过了，忽略重复收集");
            }
        }
        else
        {
            Debug.LogError($"拼图ID {puzzleId} 超出有效范围 (1-{totalPuzzles})");
        }
    }
    
    public bool IsPuzzleCollected(int puzzleId)
    {
        if (!puzzleCollectionStatus.ContainsKey(puzzleId))
        {
            puzzleCollectionStatus[puzzleId] = false;
        }
        return puzzleCollectionStatus[puzzleId];
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
        Debug.Log($"=== CheckVictoryCondition 被调用 ===");
        Debug.Log($"当前收集数量: {collectedPuzzles}, 总数量: {totalPuzzles}");
        Debug.Log($"条件检查: collectedPuzzles({collectedPuzzles}) >= totalPuzzles({totalPuzzles}) = {collectedPuzzles >= totalPuzzles}");
        Debug.Log($"Boss状态: {isBossDefeated}");
        
        // 收集完所有拼图后直接显示胜利面板
        if (collectedPuzzles >= totalPuzzles)
        {
            Debug.Log($"🎉 所有拼图已收集（{collectedPuzzles}/{totalPuzzles}），触发胜利！");
            Victory();
        }
        else if (isBossDefeated)
        {
            Debug.Log($"Boss已击败，但还需收集所有拼图！当前: {collectedPuzzles}/{totalPuzzles}");
        }
        else
        {
            Debug.Log($"进度：{collectedPuzzles}/{totalPuzzles}，继续收集...");
        }
    }
    
    // 显示拼图收集完成庆祝界面
    void ShowPuzzleCompleteCelebration()
    {
        Debug.Log($"GameManager: 显示拼图完成庆祝界面，当前收集: {collectedPuzzles}/{totalPuzzles}");
        
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowPuzzleCompleteCelebration();
        }
        else
        {
            Debug.LogError("GameManager: UIManager.Instance为空！无法显示庆祝界面！");
        }
    }
    
    // 直接更新UI进度（备用机制，确保UI更新）
    void UpdateProgressUI()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePuzzleProgress(collectedPuzzles, totalPuzzles);
            Debug.Log($"GameManager: 直接更新UI进度: {collectedPuzzles}/{totalPuzzles}");
        }
        else
        {
            Debug.LogWarning("GameManager: UIManager.Instance为空，无法更新进度UI！");
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
        // 快速收集8块拼图（跳过puzzlepiece2，方便测试）
        if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("🔧 调试模式：快速收集8块拼图（跳过puzzlepiece2）");
            for (int i = 1; i <= totalPuzzles; i++)
            {
                if (i != 2 && !IsPuzzleCollected(i)) // 跳过ID为2的拼图
                {
                    CollectPuzzle(i);
                }
            }
            Debug.Log($"✅ 已收集 {GetCollectedPuzzlesCount()}/9 块拼图，还剩puzzlepiece2未收集");
        }
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
    
    // 公共方法：重置游戏数据（供外部调用）
    public void ResetGameDataPublic()
    {
        ResetGameData();
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
    
    public void GameOver(string reason)
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
        {
            SetGameState(GameState.Playing);
            
            // 如果UIManager存在，延迟一帧后自动开始游戏（不显示主菜单）
            if (UIManager.Instance != null)
            {
                StartCoroutine(DelayedStartGame());
            }
        }
    }
    
    // 延迟开始游戏（确保场景完全加载）
    System.Collections.IEnumerator DelayedStartGame()
    {
        yield return null; // 等待一帧，确保所有Start方法都执行完毕
        
        // 如果UIManager存在且游戏状态是Playing，自动开始游戏
        if (UIManager.Instance != null && currentState == GameState.Playing)
        {
            Debug.Log("🔄 GameManager: 场景加载完成，自动开始游戏（RestartGame后，效果与StartGame相同）");
            // 直接调用UIManager的StartGame方法，效果与点击StartGame按钮完全相同
            // StartGame会检查场景名称，如果在GameScene就直接ContinueGame，否则StartNewGame
            // 但这里已经在GameScene了，所以会调用ContinueGame，效果和StartGame一样
            UIManager.Instance.StartGame();
        }
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