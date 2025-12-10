using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using TMPro; // 确保引入 TMP 命名空间
using Image = UnityEngine.UI.Image;
using Text = UnityEngine.UI.Text;
/// <summary>
/// UI管理器 - 统一管理游戏中的所有UI元素
/// </summary>
public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    
    [Header("主菜单UI")]
    public GameObject mainMenuPanel;
    public GameObject startButton; // 支持GameObject，自动获取Button组件
    public GameObject quitButton; // 支持GameObject，自动获取Button组件
    public GameObject soundButton; // 声音播放/暂停按钮
    
    [Header("游戏内UI")]
    public GameObject gameHUD;
    public GameObject puzzleProgressText; // 支持Text和TextMeshPro
    // public Image puzzleProgressFill; // 已移除，只使用文本显示进度
    [Tooltip("拖入用于显示玩家生命值的 TextMeshPro 对象")]
    public GameObject playerLivesText;

    public GameObject interactionPromptPanel;
    public GameObject interactionPromptText; // 支持Text和TextMeshPro
    public GameObject tutorialPanel; // 游戏教程面板（显示操作提示）
    
    [Header("暂停菜单UI")]
    public GameObject pauseMenuPanel;
    public GameObject resumeButton; // 暂停菜单里的“继续游戏”按钮（弹出菜单用）
    public GameObject pauseQuitButton; // 暂停菜单里的“返回主菜单”按钮（弹出菜单用）
    
    [Header("HUD常驻暂停按钮（可选）")]
    [Tooltip("游戏画面右上角等位置一直显示的暂停按钮")]
    public GameObject hudPauseButton;   // 常驻“暂停”按钮
    [Tooltip("游戏画面右上角等位置一直显示的继续按钮")]
    public GameObject hudResumeButton;  // 常驻“继续”按钮
    
    [Header("胜利界面UI")]
    public GameObject victoryPanel;
    public GameObject victoryText; // 支持Text和TextMeshPro
    public GameObject victoryQuitButton; // 支持GameObject，自动获取Button组件
    
    [Header("失败界面UI")]
    public GameObject defeatPanel; // 失败/游戏结束面板
    public GameObject defeatText; // 支持Text和TextMeshPro
    public GameObject defeatQuitButton; // 支持GameObject，自动获取Button组件
    
    [Header("拼图完成庆祝界面")]
    public GameObject puzzleCompletePanel; // 拼图收集完成时的庆祝界面
    public GameObject puzzleCompleteText; // 支持Text和TextMeshPro
    public PuzzleCompleteCelebration puzzleCompleteCelebration; // 庆祝界面控制器（可选，如果没有会自动查找）
    
    [Header("设置")]
    public bool lockCursorOnStart = false;

    [Header("拼图可视化 UI")]
    public GameObject puzzlePanel;        // 九宫格父对象
    public Image[] puzzleSlots = new Image[9];  // 存放 Slot_1 ~ Slot_9 的 Image
    public Sprite placeholderSprite;      // 未收集图
    public Sprite[] puzzleSprites;        // 已收集图（长度9，对应ID 1=索引0）

    private bool isPaused = false;
    private bool isGameStarted = false;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: 已存在实例，销毁新实例");
            Destroy(gameObject);
            return;
        }
        
        SetupUI();
    }
    
    void Start()
    {
        // 检查EventSystem和Canvas配置
        CheckUIInfrastructure();
        
        // 检查是否是从RestartGame来的（通过检查游戏状态）
        bool shouldShowMainMenu = true;
        if (GameManager.Instance != null)
        {
            GameManager.GameState currentState = GameManager.Instance.GetCurrentGameState();
            // 如果游戏状态是Playing或Loading，说明是重新开始，不显示主菜单
            if (currentState == GameManager.GameState.Playing || currentState == GameManager.GameState.Loading)
            {
                shouldShowMainMenu = false;
                Debug.Log("🔄 UIManager: 检测到重新开始，跳过主菜单显示，直接开始游戏");
                // 延迟一帧后开始游戏，确保所有组件都已初始化
                StartCoroutine(DelayedStartFromRestart());
            }
            // ========== [新增] 初始化显示血量 ==========
            // 游戏刚开始时同步一次当前血量
            UpdatePlayerLives(GameManager.Instance.PlayerLives);
            // =======================================
        }

        if (shouldShowMainMenu)
        {
            ShowMainMenu();
        }
        
        SetupButtonListeners();
        SubscribeToGameManagerEvents();
        InitializeProgress();
        
        // 初始化音频管理器（如果不存在）
        InitializeAudioManager();
    }
    
    // 延迟开始游戏（从RestartGame调用）
    System.Collections.IEnumerator DelayedStartFromRestart()
    {
        yield return null; // 等待一帧，确保所有Start方法都执行完毕
        
        Debug.Log("🔄 UIManager: DelayedStartFromRestart 被调用，直接开始游戏（完全重新初始化）");
        
        // 隐藏主菜单和其他UI
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (puzzleCompletePanel != null) puzzleCompletePanel.SetActive(false);
        
        // 设置游戏状态
        isGameStarted = true;
        isPaused = false;
        Time.timeScale = 1f;
        
        // 设置光标状态（与StartGame一致）
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // 显示游戏内UI
        if (gameHUD != null)
        {
            gameHUD.SetActive(true);
        }
        
        // 确保GameHUDCanvas已激活
        GameObject gameHUDCanvas = GameObject.Find("GameHUDCanvas");
        if (gameHUDCanvas == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name == "GameHUDCanvas")
                {
                    gameHUDCanvas = canvas.gameObject;
                    break;
                }
            }
        }
        if (gameHUDCanvas != null && !gameHUDCanvas.activeSelf)
        {
            gameHUDCanvas.SetActive(true);
        }
        
        // 重新查找按钮（场景重新加载后需要重新查找）
        SetupButtonListeners();
        
        // 显示HUD按钮
        if (hudPauseButton != null)
        {
            hudPauseButton.SetActive(true);
            Button pauseBtn = hudPauseButton.GetComponent<Button>();
            if (pauseBtn != null) pauseBtn.interactable = true;
        }
        if (hudResumeButton != null)
        {
            hudResumeButton.SetActive(true);
            Button resumeBtn = hudResumeButton.GetComponent<Button>();
            if (resumeBtn != null) resumeBtn.interactable = false;
        }
        
        // 显示进度文本
        if (puzzleProgressText != null)
        {
            puzzleProgressText.SetActive(true);
        }

        // ========== [新增] 重启时也更新血量 ==========
        if (GameManager.Instance != null)
        {
            UpdatePlayerLives(GameManager.Instance.PlayerLives);
        }
        // =======================================
        // 初始化进度显示（重置为0）
        InitializeProgress();
        
        // 显示教程面板
        ShowTutorial();
        
        Debug.Log("✅ UIManager: 游戏已完全重新开始（所有内容已初始化）");
    }
    
    // 从GameManager调用，直接开始游戏（不显示主菜单）
    public void StartGameFromRestart()
    {
        StartCoroutine(DelayedStartFromRestart());
    }
    
    // 初始化音频管理器
    void InitializeAudioManager()
    {
        if (AudioManager.Instance == null)
        {
            GameObject audioManagerObj = new GameObject("AudioManager");
            AudioManager audioManager = audioManagerObj.AddComponent<AudioManager>();
            
            // 注意：bgmClip需要在Unity编辑器的Inspector中手动设置
            Debug.Log("✅ AudioManager已自动创建");
            Debug.Log("提示：请在Unity编辑器的Inspector中设置AudioManager的bgmClip（Assets/Audio/bgm.wav）");
        }
        
        // 更新声音按钮状态
        if (soundButton != null)
        {
            UpdateSoundButtonText();
        }
    }
    
    void CheckUIInfrastructure()
    {
        // 检查EventSystem
        UnityEngine.EventSystems.EventSystem eventSystem = UnityEngine.EventSystems.EventSystem.current;
        if (eventSystem == null)
        {
            Debug.LogError("❌ 错误：场景中没有EventSystem！按钮无法响应点击！");
            Debug.LogError("解决方法：在Hierarchy中右键 -> UI -> Event System");
        }
        // 检查Canvas和GraphicRaycaster
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        if (canvases.Length == 0)
        {
            Debug.LogError("❌ 错误：场景中没有Canvas！");
        }
        else
        {
            foreach (Canvas canvas in canvases)
            {
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    Debug.LogWarning($"⚠️ Canvas '{canvas.name}' 没有GraphicRaycaster组件！按钮可能无法响应点击！");
                }
            }
        }
    }
    
    void OnEnable()
    {
        // 确保在启用时也订阅事件
        SubscribeToGameManagerEvents();
    }
    
    void OnDisable()
    {
        // 取消订阅事件，避免内存泄漏
        GameManager.OnPuzzleCollected -= UpdatePuzzleVisual;
        UnsubscribeFromGameManagerEvents();
    }
    
    void SubscribeToGameManagerEvents()
    {
        // 订阅拼图收集事件，实时更新进度
        GameManager.OnPuzzleCollected += OnPuzzleCollected;
        GameManager.OnPuzzleCollected += UpdatePuzzleVisual;
        // ========== [新增] 订阅生命值变化事件 ==========
        GameManager.OnPlayerLivesChanged += UpdatePlayerLives;
        // ==========================================
        Debug.Log("UIManager: 已订阅GameManager.OnPuzzleCollected事件");
    }
    
    void UnsubscribeFromGameManagerEvents()
    {
        // 取消订阅
        GameManager.OnPuzzleCollected -= OnPuzzleCollected;
        // ========== [新增] 取消订阅 ==========
        GameManager.OnPlayerLivesChanged -= UpdatePlayerLives;
        // =================================
    }

    // ========== [新增] 更新玩家生命值显示的方法 ==========
    public void UpdatePlayerLives(int lives)
    {
        if (playerLivesText != null)
        {
            string textContent = $"LIVES: {lives}";

            // 优先尝试使用 TextMeshPro
            TMPro.TextMeshProUGUI tmpText = playerLivesText.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = textContent;
                // 如果血量 <= 1，显示红色警告，否则白色
                tmpText.color = (lives <= 1) ? Color.red : Color.white;
            }
            else
            {
                // 降级使用普通 Text
                Text legacyText = playerLivesText.GetComponent<Text>();
                if (legacyText != null)
                {
                    legacyText.text = textContent;
                    legacyText.color = (lives <= 1) ? Color.red : Color.white;
                }
            }
            Debug.Log($"UI 更新生命值: {lives}");
        }
    }
    // =================================================
    void OnPuzzleCollected(int puzzleId)
    {
        // 当拼图被收集时，实时更新UI
        if (GameManager.Instance != null)
        {
            int collected = GameManager.Instance.GetCollectedPuzzlesCount();
            int total = GameManager.Instance.TotalPuzzles;
            UpdatePuzzleProgress(collected, total);
            Debug.Log($"UIManager: 拼图 {puzzleId} 被收集，更新进度: {collected}/{total}");
        }
    }
    
    void InitializeProgress()
    {
        // 初始化进度显示
        if (GameManager.Instance != null)
        {
            int collected = GameManager.Instance.GetCollectedPuzzlesCount();
            int total = GameManager.Instance.TotalPuzzles;
            UpdatePuzzleProgress(collected, total);
            Debug.Log($"UIManager: 初始化进度显示: {collected}/{total}");
        }
        else
        {
            // 如果GameManager还没初始化，先显示默认值
            UpdatePuzzleProgress(0, 9);
        }
    }
    
    void Update()
    {
        // ESC键切换暂停
        if (isGameStarted && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
        
        // 隐藏/显示交互提示
        UpdateInteractionPrompt();
        
        // 确保游戏过程中pause按钮始终显示（防止被其他脚本隐藏）
        if (isGameStarted && !isPaused)
        {
            if (hudPauseButton != null && !hudPauseButton.activeInHierarchy)
            {
                // 如果按钮被隐藏了，重新激活它
                Transform parent = hudPauseButton.transform.parent;
                while (parent != null)
                {
                    if (!parent.gameObject.activeSelf)
                    {
                        parent.gameObject.SetActive(true);
                    }
                    parent = parent.parent;
                }
                hudPauseButton.SetActive(true);
                Button pauseBtn = hudPauseButton.GetComponent<Button>();
                if (pauseBtn != null)
                {
                    pauseBtn.interactable = true;
                }
            }
            
            // 确保continue按钮也显示（但不可交互）
            if (hudResumeButton != null && !hudResumeButton.activeInHierarchy)
            {
                Transform parent = hudResumeButton.transform.parent;
                while (parent != null)
                {
                    if (!parent.gameObject.activeSelf)
                    {
                        parent.gameObject.SetActive(true);
                    }
                    parent = parent.parent;
                }
                hudResumeButton.SetActive(true);
                Button resumeBtn = hudResumeButton.GetComponent<Button>();
                if (resumeBtn != null)
                {
                    resumeBtn.interactable = false;
                }
            }
        }
    }
    
    void SetupUI()
    {
        // 初始化UI状态
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (interactionPromptPanel != null) interactionPromptPanel.SetActive(false);
        if (puzzleCompletePanel != null) puzzleCompletePanel.SetActive(false);
        if (tutorialPanel != null) tutorialPanel.SetActive(false);
        
        // 隐藏游戏内的UI元素（在主菜单时不应该显示）
        if (puzzleProgressText != null) puzzleProgressText.SetActive(false);
        if (hudPauseButton != null) hudPauseButton.SetActive(false);
        if (hudResumeButton != null) hudResumeButton.SetActive(false);

        // ========== [新增] 隐藏血量文本 (在主菜单时) ==========
        if (playerLivesText != null) playerLivesText.SetActive(false);
        // ================================================
    }

    // 禁用按钮的键盘导航，只能通过鼠标点击
    void DisableButtonNavigation(Button btn)
    {
        if (btn != null)
        {
            var nav = btn.navigation;
            nav.mode = Navigation.Mode.None; // 禁用键盘导航
            btn.navigation = nav;
        }
    }
    
    // 统一设置按钮的辅助方法：确保按钮可交互、添加ButtonClickHandler组件
    void SetupButton(GameObject buttonObj, System.Action onClickAction, string buttonName)
    {
        if (buttonObj == null) return;
        
        Button btn = buttonObj.GetComponent<Button>();
        if (btn == null) return;
        
        // 确保按钮可交互
        if (!btn.interactable)
        {
            btn.interactable = true;
        }
        
        // 检查Image组件的Raycast Target
        Image buttonImage = buttonObj.GetComponent<Image>();
        if (buttonImage != null && !buttonImage.raycastTarget)
        {
            buttonImage.raycastTarget = true;
        }
        
        // 检查Canvas Group
        CanvasGroup canvasGroup = buttonObj.GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            if (!canvasGroup.interactable)
            {
                canvasGroup.interactable = true;
            }
            if (!canvasGroup.blocksRaycasts)
            {
                canvasGroup.blocksRaycasts = true;
            }
        }
        
        // 清除所有之前的监听器
        btn.onClick.RemoveAllListeners();
        
        // 添加点击监听器
        if (onClickAction != null)
        {
            btn.onClick.AddListener(() => onClickAction());
        }
        
        // 添加ButtonClickHandler组件来确保点击事件能正常触发
        ButtonClickHandler clickHandler = buttonObj.GetComponent<ButtonClickHandler>();
        if (clickHandler == null)
        {
            clickHandler = buttonObj.AddComponent<ButtonClickHandler>();
            Debug.Log($"✅ UIManager: 已为{buttonName}添加ButtonClickHandler组件");
        }
        
        // 禁用键盘导航
        DisableButtonNavigation(btn);
    }
    
    void SetupButtonListeners()
    {
        // 主菜单按钮
        if (startButton == null)
        {
            startButton = GameObject.Find("StartButton");
            if (startButton == null)
            {
                Debug.LogError("❌ UIManager: 无法找到StartButton！请检查场景中是否有名为'StartButton'的GameObject！");
            }
        }
        
        // 设置StartButton
        if (startButton != null)
        {
            SetupButton(startButton, StartGame, "StartButton");
            Debug.Log("✅ UIManager: StartButton已连接");
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: StartButton字段为空，尝试自动查找失败");
        }
        
        // 设置QuitButton
        if (quitButton == null)
        {
            quitButton = GameObject.Find("QuitButton");
        }
        if (quitButton != null)
        {
            SetupButton(quitButton, QuitGame, "QuitButton");
            Debug.Log("✅ UIManager: QuitButton已连接");
        }
        
        // 设置失败界面按钮（如果存在）
        if (defeatQuitButton == null && defeatPanel != null)
        {
            Transform quitButton = defeatPanel.transform.Find("DefeatQuitButton");
            if (quitButton == null)
            {
                quitButton = defeatPanel.transform.Find("QuitButton");
            }
            if (quitButton != null)
            {
                defeatQuitButton = quitButton.gameObject;
            }
        }
        if (defeatQuitButton != null)
        {
            SetupButton(defeatQuitButton, QuitGame, "DefeatQuitButton");
            Debug.Log("✅ UIManager: DefeatQuitButton已连接");
        }
        
        // 设置SoundButton（声音控制按钮）
        if (soundButton == null)
        {
            soundButton = GameObject.Find("SoundButton");
            if (soundButton == null)
            {
                soundButton = GameObject.Find("MusicButton");
            }
            if (soundButton == null)
            {
                soundButton = GameObject.Find("AudioButton");
            }
        }
        if (soundButton != null)
        {
            SetupButton(soundButton, ToggleSound, "SoundButton");
            Debug.Log("✅ UIManager: SoundButton已连接");
            UpdateSoundButtonText();
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: SoundButton未找到，如需声音控制功能请在场景中创建SoundButton");
        }
        
        // 暂停菜单按钮（ESC弹出的那个菜单）
        if (resumeButton == null)
        {
            resumeButton = GameObject.Find("ResumeButton");
            if (resumeButton == null)
            {
                resumeButton = GameObject.Find("ContinueButton");
            }
        }
        if (resumeButton != null)
        {
            SetupButton(resumeButton, ResumeGame, "ResumeButton");
            Debug.Log("✅ UIManager: ResumeButton已连接");
        }
        
        if (pauseQuitButton == null)
        {
            pauseQuitButton = GameObject.Find("PauseQuitButton");
            if (pauseQuitButton == null)
            {
                pauseQuitButton = GameObject.Find("ReturnToMainMenuButton");
            }
        }
        if (pauseQuitButton != null)
        {
            SetupButton(pauseQuitButton, QuitToMainMenu, "PauseQuitButton");
            Debug.Log("✅ UIManager: PauseQuitButton已连接");
        }
        
        // HUD上一直存在的暂停/继续按钮（如果你创建了的话）
        // 如果没有在Inspector里拖拽引用，就按照名字自动查找
        if (hudPauseButton == null)
        {
            hudPauseButton = GameObject.Find("PauseButton");
            if (hudPauseButton == null)
            {
                hudPauseButton = GameObject.Find("HUD PauseButton");
            }
        }
        if (hudResumeButton == null)
        {
            hudResumeButton = GameObject.Find("ResumeButton");
            if (hudResumeButton == null)
            {
                hudResumeButton = GameObject.Find("ContinueButton");
                if (hudResumeButton == null)
                {
                    hudResumeButton = GameObject.Find("HUD ResumeButton");
                }
            }
        }
        
        if (hudPauseButton != null)
        {
            SetupButton(hudPauseButton, PauseGame, "HUD PauseButton");
            Debug.Log("✅ UIManager: HUD PauseButton已连接");
        }
        
        if (hudResumeButton != null)
        {
            SetupButton(hudResumeButton, ResumeGame, "HUD ResumeButton");
            Debug.Log("✅ UIManager: HUD ResumeButton已连接");
        }
        
        // 胜利界面按钮
        if (victoryQuitButton == null)
        {
            victoryQuitButton = GameObject.Find("VictoryQuitButton");
            if (victoryQuitButton == null)
            {
                victoryQuitButton = GameObject.Find("Victory Quit Button");
            }
        }
        if (victoryQuitButton != null)
        {
            SetupButton(victoryQuitButton, QuitGame, "VictoryQuitButton");
            Debug.Log("✅ UIManager: VictoryQuitButton已连接");
        }
        
        // 教程面板关闭按钮
        if (tutorialPanel != null)
        {
            Transform closeButton = tutorialPanel.transform.Find("CloseButton");
            if (closeButton != null)
            {
                SetupButton(closeButton.gameObject, CloseTutorial, "TutorialCloseButton");
                Debug.Log("✅ UIManager: TutorialCloseButton已连接");
            }
        }
        else
        {
            // 尝试自动查找教程面板
            GameObject foundPanel = GameObject.Find("TutorialPanel");
            if (foundPanel != null)
            {
                tutorialPanel = foundPanel;
                Transform closeButton = tutorialPanel.transform.Find("CloseButton");
                if (closeButton != null)
                {
                    SetupButton(closeButton.gameObject, CloseTutorial, "TutorialCloseButton");
                    Debug.Log("✅ UIManager: TutorialCloseButton已连接（自动查找）");
                }
            }
        }
    }
    
    public void ShowMainMenu()
    {
        isGameStarted = false;
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (mainMenuPanel != null) mainMenuPanel.SetActive(true);
        if (gameHUD != null) gameHUD.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        
        // 确保游戏内的UI元素在主菜单时隐藏
        if (puzzleProgressText != null) puzzleProgressText.SetActive(false);
        if (hudPauseButton != null) hudPauseButton.SetActive(false);
        if (hudResumeButton != null) hudResumeButton.SetActive(false);
        if (playerLivesText != null) playerLivesText.SetActive(false); // [新增]
    }
    
    // 测试方法：验证按钮点击是否工作
    public void TestButtonClick()
    {
        Debug.Log("🎯 测试：按钮被点击了！");
    }
    
    public void StartGame()
    {
        Debug.Log("🚀 UIManager: StartGame方法被调用了！");
        
        // 立即隐藏主菜单（放在最前面，确保第一时间隐藏）
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("❌ UIManager: mainMenuPanel为空（None）！无法隐藏主菜单！请检查UIManager的Inspector设置！");
            Debug.LogError("❌ 解决方法：在UIManager的Inspector中，将主菜单Panel拖拽到'Main Menu Panel'字段");
            
            // 尝试自动查找主菜单面板
            GameObject foundPanel = GameObject.Find("MainMenuPanel");
            if (foundPanel == null)
            {
                foundPanel = GameObject.Find("Main Menu Panel");
            }
            if (foundPanel != null)
            {
                mainMenuPanel = foundPanel;
                mainMenuPanel.SetActive(false);
            }
        }
        
        isGameStarted = true;
        Time.timeScale = 1f;
        
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        // 显示游戏内UI（先激活GameHUD，这样才能找到它下面的按钮）
        if (gameHUD != null)
        {
            gameHUD.SetActive(true);
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: gameHUD为空（None）！无法显示游戏内UI！请检查UIManager的Inspector设置！");
        }
        
        // 确保GameHUDCanvas已激活（PauseButton在GameHUDCanvas下）
        GameObject gameHUDCanvas = GameObject.Find("GameHUDCanvas");
        if (gameHUDCanvas == null)
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            foreach (Canvas canvas in canvases)
            {
                if (canvas.name == "GameHUDCanvas")
                {
                    gameHUDCanvas = canvas.gameObject;
                    break;
                }
            }
        }
        if (gameHUDCanvas != null && !gameHUDCanvas.activeSelf)
        {
            gameHUDCanvas.SetActive(true);
            Debug.Log("✅ UIManager: GameHUDCanvas已激活");
        }
        
        // 在StartGame中重新查找按钮（确保能找到）
        // 注意：PauseButton在GameHUDCanvas下，不在GameHUD下
        if (hudPauseButton == null)
        {
            Debug.LogWarning("⚠️ UIManager: hudPauseButton字段为空，尝试自动查找...");
            
            // 如果之前没找到GameHUDCanvas，再次尝试查找
            if (gameHUDCanvas == null)
            {
                gameHUDCanvas = GameObject.Find("GameHUDCanvas");
                if (gameHUDCanvas == null)
                {
                    // 尝试通过Canvas组件查找
                    Canvas[] canvases = FindObjectsOfType<Canvas>(true); // true表示包括非激活的
                    foreach (Canvas canvas in canvases)
                    {
                        if (canvas.name == "GameHUDCanvas")
                        {
                            gameHUDCanvas = canvas.gameObject;
                            break;
                        }
                    }
                }
            }
            
            if (gameHUDCanvas != null)
            {
                // 确保GameHUDCanvas已激活
                if (!gameHUDCanvas.activeSelf)
                {
                    gameHUDCanvas.SetActive(true);
                }
                
                // 在GameHUDCanvas下查找PauseButton
                Transform pauseBtn = gameHUDCanvas.transform.Find("PauseButton");
                if (pauseBtn == null)
                {
                    // 递归查找所有子对象（包括非激活的）
                    pauseBtn = FindChildRecursive(gameHUDCanvas.transform, "PauseButton");
                }
                if (pauseBtn != null)
                {
                    hudPauseButton = pauseBtn.gameObject;
                    Debug.Log($"✅ UIManager: 在GameHUDCanvas下找到hudPauseButton: {hudPauseButton.name}");
                }
            }
            
            // 如果还没找到，尝试全局查找（只能找到激活的对象）
            if (hudPauseButton == null)
            {
                hudPauseButton = GameObject.Find("PauseButton");
                if (hudPauseButton != null)
                {
                    Debug.Log($"✅ UIManager: 全局找到hudPauseButton: {hudPauseButton.name}");
                }
            }
            
            if (hudPauseButton == null)
            {
                hudPauseButton = GameObject.Find("HUD PauseButton");
                if (hudPauseButton != null)
                {
                    Debug.Log($"✅ UIManager: 找到hudPauseButton (HUD PauseButton): {hudPauseButton.name}");
                }
            }
            
            if (hudPauseButton == null)
            {
                Debug.LogError("❌ UIManager: 无法找到hudPauseButton！请检查场景中是否有名为'PauseButton'的GameObject！");
                Debug.LogError("❌ 提示：PauseButton应该在GameHUDCanvas -> PauseButton下");
            }
        }
        
        if (hudResumeButton == null)
        {
            Debug.LogWarning("⚠️ UIManager: hudResumeButton字段为空，尝试自动查找...");
            
            // 如果之前没找到GameHUDCanvas，再次尝试查找
            if (gameHUDCanvas == null)
            {
                gameHUDCanvas = GameObject.Find("GameHUDCanvas");
                if (gameHUDCanvas == null)
                {
                    // 尝试通过Canvas组件查找
                    Canvas[] canvases = FindObjectsOfType<Canvas>(true); // true表示包括非激活的
                    foreach (Canvas canvas in canvases)
                    {
                        if (canvas.name == "GameHUDCanvas")
                        {
                            gameHUDCanvas = canvas.gameObject;
                            break;
                        }
                    }
                }
            }
            
            if (gameHUDCanvas != null)
            {
                // 确保GameHUDCanvas已激活
                if (!gameHUDCanvas.activeSelf)
                {
                    gameHUDCanvas.SetActive(true);
                }
                
                // 在GameHUDCanvas下查找ContinueButton
                Transform resumeBtn = gameHUDCanvas.transform.Find("ResumeButton");
                if (resumeBtn == null)
                {
                    resumeBtn = gameHUDCanvas.transform.Find("ContinueButton");
                }
                if (resumeBtn == null)
                {
                    // 递归查找所有子对象（包括非激活的）
                    resumeBtn = FindChildRecursive(gameHUDCanvas.transform, "ResumeButton");
                    if (resumeBtn == null)
                    {
                        resumeBtn = FindChildRecursive(gameHUDCanvas.transform, "ContinueButton");
                    }
                }
                if (resumeBtn != null)
                {
                    hudResumeButton = resumeBtn.gameObject;
                    Debug.Log($"✅ UIManager: 在GameHUDCanvas下找到hudResumeButton: {hudResumeButton.name}");
                }
            }
            
            // 如果还没找到，尝试全局查找
            if (hudResumeButton == null)
            {
                hudResumeButton = GameObject.Find("ResumeButton");
                if (hudResumeButton == null)
                {
                    hudResumeButton = GameObject.Find("ContinueButton");
                    if (hudResumeButton == null)
                    {
                        hudResumeButton = GameObject.Find("HUD ResumeButton");
                    }
                }
                if (hudResumeButton != null)
                {
                    Debug.Log($"✅ UIManager: 全局找到hudResumeButton: {hudResumeButton.name}");
                }
            }
        }
        
        // 显示HUD上的暂停和继续按钮（游戏过程中都需要，根据状态切换可用性）
        // 先显示pause按钮，确保它一直存在
        if (hudPauseButton != null)
        {
            // 先激活所有父对象（包括Canvas等），确保按钮可见
            Transform parent = hudPauseButton.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                    Debug.Log($"✅ UIManager: 激活父对象 {parent.name} 以确保PauseButton可见");
                }
                parent = parent.parent;
            }
            // 然后激活按钮本身（即使已经激活也确保一下）
            hudPauseButton.SetActive(true);
            
            // 确保暂停按钮可交互
            Button pauseBtn = hudPauseButton.GetComponent<Button>();
            if (pauseBtn != null)
            {
                pauseBtn.interactable = true;
            }
            
            // 检查按钮的RectTransform和Canvas设置
            RectTransform rectTransform = hudPauseButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Canvas canvas = hudPauseButton.GetComponentInParent<Canvas>();
                Debug.Log($"✅ UIManager: hudPauseButton已显示");
                Debug.Log($"   位置: {GetGameObjectPath(hudPauseButton)}");
                Debug.Log($"   ActiveInHierarchy: {hudPauseButton.activeInHierarchy}");
                Debug.Log($"   屏幕位置: {rectTransform.position}, 本地位置: {rectTransform.anchoredPosition}");
                Debug.Log($"   尺寸: {rectTransform.sizeDelta}, 缩放: {rectTransform.localScale}");
                if (canvas != null)
                {
                    Debug.Log($"   Canvas: {canvas.name}, Sort Order: {canvas.sortingOrder}, Render Mode: {canvas.renderMode}");
                    RectTransform canvasRect = canvas.GetComponent<RectTransform>();
                    if (canvasRect != null)
                    {
                        Debug.Log($"   Canvas尺寸: {canvasRect.sizeDelta}");
                    }
                }
                
                // 检查Image组件
                Image img = hudPauseButton.GetComponent<Image>();
                if (img != null)
                {
                    Debug.Log($"   Image颜色: {img.color}, RaycastTarget: {img.raycastTarget}");
                    // 确保Image颜色不透明
                    if (img.color.a < 0.1f)
                    {
                        Debug.LogWarning("   ⚠️ Image颜色透明度太低，正在修复...");
                        Color newColor = img.color;
                        newColor.a = 1f;
                        img.color = newColor;
                    }
                }
                else
                {
                    Debug.LogWarning("   ⚠️ 按钮没有Image组件！按钮可能不可见！");
                }
                
                // 检查按钮是否在屏幕可见范围内
                // 如果本地位置太大，可能是锚点设置问题
                Vector2 anchoredPos = rectTransform.anchoredPosition;
                if (Mathf.Abs(anchoredPos.x) > 2000 || Mathf.Abs(anchoredPos.y) > 2000)
                {
                    Debug.LogWarning($"   ⚠️ 按钮位置可能超出屏幕范围！本地位置: {anchoredPos}");
                    Debug.LogWarning("   ⚠️ 建议：检查按钮的RectTransform锚点设置");
                }
                
                // 确保按钮的RectTransform设置正确
                // 检查是否被其他UI元素遮挡（通过检查Canvas的Sort Order）
                if (canvas != null && canvas.sortingOrder < 100)
                {
                    Debug.LogWarning($"   ⚠️ Canvas Sort Order较低 ({canvas.sortingOrder})，可能被其他Canvas遮挡！");
                }
            }
        }
        else
        {
            Debug.LogError("❌ UIManager: hudPauseButton为空！无法显示暂停按钮！");
            Debug.LogError("❌ 解决方法：1. 在UIManager的Inspector中，将PauseButton拖拽到'HUD Pause Button'字段");
            Debug.LogError("❌ 或者：2. 确保场景中有一个名为'PauseButton'的GameObject");
        }
        
        // 显示continue按钮
        if (hudResumeButton != null)
        {
            // 激活所有父对象（包括Canvas等）
            Transform parent = hudResumeButton.transform.parent;
            while (parent != null)
            {
                parent.gameObject.SetActive(true);
                parent = parent.parent;
            }
            // 然后激活按钮本身
            hudResumeButton.SetActive(true);
            // 游戏未暂停时，继续按钮不可交互
            Button resumeBtn = hudResumeButton.GetComponent<Button>();
            if (resumeBtn != null)
            {
                resumeBtn.interactable = false;
            }
            
            // 检查按钮的RectTransform和Canvas设置
            RectTransform rectTransform = hudResumeButton.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                Canvas canvas = hudResumeButton.GetComponentInParent<Canvas>();
                Debug.Log($"✅ UIManager: hudResumeButton已显示");
                Debug.Log($"   位置: {GetGameObjectPath(hudResumeButton)}");
                Debug.Log($"   ActiveInHierarchy: {hudResumeButton.activeInHierarchy}");
                Debug.Log($"   屏幕位置: {rectTransform.position}, 本地位置: {rectTransform.anchoredPosition}");
                Debug.Log($"   尺寸: {rectTransform.sizeDelta}, 缩放: {rectTransform.localScale}");
                if (canvas != null)
                {
                    Debug.Log($"   Canvas: {canvas.name}, Sort Order: {canvas.sortingOrder}, Render Mode: {canvas.renderMode}");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: hudResumeButton为空！请检查UIManager的Inspector设置！");
        }
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        
        // 调用GameManager（如果存在）
        if (GameManager.Instance != null)
        {
            // 检查当前场景名称，如果已经在GameScene，就直接开始游戏，不要重新加载场景
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            
            if (currentSceneName == "GameScene")
            {
                // 已经在GameScene，直接设置游戏状态为Playing，不要重新加载场景
                GameManager.Instance.ContinueGame();
            }
            else
            {
                // 不在GameScene，需要加载场景
                GameManager.Instance.StartNewGame();
            }
            
            // 初始化进度显示
            InitializeProgress();

            // ========== [新增] 游戏开始时刷新血量UI ==========
            UpdatePlayerLives(GameManager.Instance.PlayerLives);
            // =============================================
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: GameManager.Instance为空，跳过GameManager调用");
            // 即使没有GameManager，也初始化进度显示
            InitializeProgress();
        }
        
        // 显示进度文本（Collecting Puzzles）- 必须在游戏开始后显示
        if (puzzleProgressText != null)
        {
            puzzleProgressText.SetActive(true);
            Debug.Log("✅ UIManager: 已显示进度文本 (Collecting Puzzles)");
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: puzzleProgressText为空，尝试自动查找...");
            // 尝试自动查找
            puzzleProgressText = GameObject.Find("ProgressText");
            if (puzzleProgressText == null)
            {
                puzzleProgressText = GameObject.Find("PuzzleProgressText");
            }
            if (puzzleProgressText != null)
            {
                puzzleProgressText.SetActive(true);
                Debug.Log("✅ UIManager: 自动找到并显示进度文本");
            }

        }
        if (playerLivesText != null) playerLivesText.SetActive(true); // [新增]
        // 显示教程面板
        ShowTutorial();
        
    }
    
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }
    
    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        
        // 切换按钮可用性：暂停按钮不可用，继续按钮可用
        if (hudPauseButton != null)
        {
            hudPauseButton.SetActive(true);
            Button pauseBtn = hudPauseButton.GetComponent<Button>();
            if (pauseBtn != null)
            {
                pauseBtn.interactable = false;
            }
        }
        if (hudResumeButton != null)
        {
            hudResumeButton.SetActive(true);
            Button resumeBtn = hudResumeButton.GetComponent<Button>();
            if (resumeBtn != null)
            {
                resumeBtn.interactable = true;
            }
        }
        
        // 调用GameManager的PauseGame（如果存在）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PauseGame();
        }
    }
    
    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
        
        if (lockCursorOnStart)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        
        // 切换按钮可用性：暂停按钮可用，继续按钮不可用
        if (hudPauseButton != null)
        {
            hudPauseButton.SetActive(true);
            Button pauseBtn = hudPauseButton.GetComponent<Button>();
            if (pauseBtn != null)
            {
                pauseBtn.interactable = true;
            }
        }
        if (hudResumeButton != null)
        {
            hudResumeButton.SetActive(true);
            Button resumeBtn = hudResumeButton.GetComponent<Button>();
            if (resumeBtn != null)
            {
                resumeBtn.interactable = false;
            }
        }
        
        // 调用GameManager的ResumeGame（如果存在）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    public void ShowVictory()
    {
        Debug.Log("🎉 UIManager.ShowVictory() 被调用！");
        
        isGameStarted = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 如果victoryPanel为空，尝试自动查找
        if (victoryPanel == null)
        {
            Debug.LogWarning("⚠️ UIManager: victoryPanel为空，尝试自动查找...");
            victoryPanel = GameObject.Find("VictoryPanel");
            if (victoryPanel == null)
            {
                victoryPanel = GameObject.Find("Victory Panel");
            }
            if (victoryPanel == null)
            {
                // 尝试在所有Canvas下查找
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    Transform victoryTransform = canvas.transform.Find("VictoryPanel");
                    if (victoryTransform == null)
                    {
                        victoryTransform = canvas.transform.Find("Victory Panel");
                    }
                    if (victoryTransform != null)
                    {
                        victoryPanel = victoryTransform.gameObject;
                        break;
                    }
                }
            }
        }
        
        // 隐藏其他UI
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (interactionPromptPanel != null) interactionPromptPanel.SetActive(false);
        if (puzzleCompletePanel != null) puzzleCompletePanel.SetActive(false);
        
        // 显示胜利面板
        if (victoryPanel != null)
        {
            Debug.Log($"✅ UIManager: 找到胜利面板 - {victoryPanel.name}");
            
            // 确保所有父对象都是激活的
            Transform parent = victoryPanel.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                    Debug.Log($"✅ UIManager: 激活父对象 - {parent.name}");
                }
                parent = parent.parent;
            }
            
            victoryPanel.SetActive(true);
            
            // 确保胜利面板是全屏的
            Canvas canvas = victoryPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"✅ UIManager: 找到Canvas - {canvas.name}, RenderMode={canvas.renderMode}");
                
                // 确保Canvas覆盖整个屏幕
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // 设置最高排序，确保在最上层
                canvas.gameObject.SetActive(true);
                
                // 确保胜利面板的RectTransform覆盖整个屏幕
                RectTransform panelRect = victoryPanel.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    // 设置锚点为全屏
                    panelRect.anchorMin = Vector2.zero;
                    panelRect.anchorMax = Vector2.one;
                    panelRect.sizeDelta = Vector2.zero;
                    panelRect.anchoredPosition = Vector2.zero;
                    
                    Debug.Log($"✅ UIManager: 胜利面板RectTransform已设置为全屏");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ UIManager: 胜利面板没有找到Canvas！");
            }
            
            Debug.Log($"✅ UIManager: 胜利面板已激活！ActiveInHierarchy={victoryPanel.activeInHierarchy}");
            
            // 统一胜利界面按钮样式与主菜单按钮一致（内部会调用SetupVictoryText）
            SyncVictoryButtonStyles();
        }
        else
        {
            Debug.LogError("❌ UIManager: 无法找到VictoryPanel！请检查场景中是否有名为'VictoryPanel'的GameObject！");
        }
    }
    
    public void ShowDefeat()
    {
        Debug.Log("💀 UIManager.ShowDefeat() 被调用！");
        
        isGameStarted = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        // 如果defeatPanel为空，尝试自动查找
        if (defeatPanel == null)
        {
            Debug.LogWarning("⚠️ UIManager: defeatPanel为空，尝试自动查找...");
            defeatPanel = GameObject.Find("DefeatPanel");
            if (defeatPanel == null)
            {
                defeatPanel = GameObject.Find("Defeat Panel");
            }
            if (defeatPanel == null)
            {
                // 尝试在所有Canvas下查找
                Canvas[] canvases = FindObjectsOfType<Canvas>();
                foreach (Canvas canvas in canvases)
                {
                    Transform defeatTransform = canvas.transform.Find("DefeatPanel");
                    if (defeatTransform == null)
                    {
                        defeatTransform = canvas.transform.Find("Defeat Panel");
                    }
                    if (defeatTransform != null)
                    {
                        defeatPanel = defeatTransform.gameObject;
                        break;
                    }
                }
            }
        }
        
        // 隐藏其他UI
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (interactionPromptPanel != null) interactionPromptPanel.SetActive(false);
        if (puzzleCompletePanel != null) puzzleCompletePanel.SetActive(false);
        
        // 显示失败面板
        if (defeatPanel != null)
        {
            Debug.Log($"✅ UIManager: 找到失败面板 - {defeatPanel.name}");
            
            // 确保所有父对象都是激活的
            Transform parent = defeatPanel.transform.parent;
            while (parent != null)
            {
                if (!parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                    Debug.Log($"✅ UIManager: 激活父对象 - {parent.name}");
                }
                parent = parent.parent;
            }
            
            defeatPanel.SetActive(true);
            
            // 确保失败面板是全屏的
            Canvas canvas = defeatPanel.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                Debug.Log($"✅ UIManager: 找到Canvas - {canvas.name}, RenderMode={canvas.renderMode}");
                
                // 确保Canvas覆盖整个屏幕
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = 999; // 设置最高排序，确保在最上层
                canvas.gameObject.SetActive(true);
                
                // 确保失败面板的RectTransform覆盖整个屏幕
                RectTransform panelRect = defeatPanel.GetComponent<RectTransform>();
                if (panelRect != null)
                {
                    // 设置锚点为全屏
                    panelRect.anchorMin = Vector2.zero;
                    panelRect.anchorMax = Vector2.one;
                    panelRect.sizeDelta = Vector2.zero;
                    panelRect.anchoredPosition = Vector2.zero;
                    
                    Debug.Log($"✅ UIManager: 失败面板RectTransform已设置为全屏");
                }
            }
            else
            {
                Debug.LogWarning("⚠️ UIManager: 失败面板没有找到Canvas！");
            }
            
            // 设置失败文本
            SetupDefeatText();
            
            // 设置失败界面按钮
            SetupDefeatButtons();
            
            Debug.Log($"✅ UIManager: 失败面板已激活！ActiveInHierarchy={defeatPanel.activeInHierarchy}");
        }
        else
        {
            Debug.LogError("❌ UIManager: 无法找到DefeatPanel！请检查场景中是否有名为'DefeatPanel'的GameObject！");
        }
    }
    
    // 设置失败文本样式
    void SetupDefeatText()
    {
        if (defeatText == null)
        {
            // 尝试自动查找
            if (defeatPanel != null)
            {
                defeatText = defeatPanel.transform.Find("DefeatText")?.gameObject;
                if (defeatText == null)
                {
                    defeatText = defeatPanel.transform.Find("Defeat Text")?.gameObject;
                }
            }
        }
        
        if (defeatText != null)
        {
            RectTransform textRect = defeatText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                // 设置完全居中（水平和垂直都居中）
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                
                // 设置位置：屏幕上方
                float textY = 200f; // 距离中心上方200像素
                if (startButton != null)
                {
                    RectTransform startRect = startButton.GetComponent<RectTransform>();
                    if (startRect != null)
                    {
                        // 文本在 startButton 上方，间距约150像素
                        textY = startRect.anchoredPosition.y + 150f;
                    }
                }
                textRect.anchoredPosition = new Vector2(0, textY);
                
                Debug.Log($"✅ UIManager: DefeatText已设置为居中，位置: ({textRect.anchoredPosition.x}, {textRect.anchoredPosition.y})");
            }
            
            // 设置文本内容为 "Defeat"
            TMPro.TextMeshProUGUI tmpText = defeatText.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = "Defeat";
                // 只在字体大小小于100时才设置（避免覆盖Inspector中的设置）
                if (tmpText.fontSize < 100f)
                {
                    tmpText.fontSize = 160f; // 设置大字体
                }
                tmpText.alignment = TMPro.TextAlignmentOptions.Center;
                tmpText.enableAutoSizing = false;
                Debug.Log($"✅ UIManager: DefeatText字体大小已设置为{tmpText.fontSize}，已居中");
            }
            else
            {
                Text textLegacy = defeatText.GetComponent<Text>();
                if (textLegacy != null)
                {
                    textLegacy.text = "Defeat";
                    // 只在字体大小小于100时才设置（避免覆盖Inspector中的设置）
                    if (textLegacy.fontSize < 100)
                    {
                        textLegacy.fontSize = 160;
                    }
                    textLegacy.alignment = TextAnchor.MiddleCenter;
                    Debug.Log($"✅ UIManager: DefeatText字体大小已设置为{textLegacy.fontSize}，已居中");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: 无法找到DefeatText！");
        }
    }
    
    // 设置失败界面按钮
    void SetupDefeatButtons()
    {
        // 设置 defeatQuitButton
        if (defeatQuitButton == null)
        {
            if (defeatPanel != null)
            {
                Transform quitButton = defeatPanel.transform.Find("DefeatQuitButton");
                if (quitButton == null)
                {
                    quitButton = defeatPanel.transform.Find("QuitButton");
                }
                if (quitButton != null)
                {
                    defeatQuitButton = quitButton.gameObject;
                }
            }
        }
        if (defeatQuitButton != null)
        {
            SetupButton(defeatQuitButton, QuitGame, "DefeatQuitButton");
            Debug.Log("✅ UIManager: DefeatQuitButton已连接");
            
            // 设置 defeatQuitButton 的位置（往上移动，参考 startButton 的位置）
            RectTransform defeatQuitRect = defeatQuitButton.GetComponent<RectTransform>();
            RectTransform startRect = startButton?.GetComponent<RectTransform>();
            if (defeatQuitRect != null && startRect != null)
            {
                // 设置按钮大小：与 startButton 一样大小
                defeatQuitRect.sizeDelta = startRect.sizeDelta;
                defeatQuitRect.localScale = Vector3.one;
                
                // 设置按钮位置：水平居中，垂直位置在屏幕底部1/4处
                defeatQuitRect.anchorMin = new Vector2(0.5f, 0.5f);
                defeatQuitRect.anchorMax = new Vector2(0.5f, 0.5f);
                defeatQuitRect.pivot = new Vector2(0.5f, 0.5f);
                
                // 计算位置：屏幕底部1/4的位置
                Canvas canvas = defeatQuitRect.GetComponentInParent<Canvas>();
                float screenHeight = Screen.height;
                if (canvas != null && canvas.GetComponent<RectTransform>() != null)
                {
                    screenHeight = canvas.GetComponent<RectTransform>().rect.height;
                }
                // 屏幕中心是0，底部是-screenHeight/2，底部1/4位置是 -screenHeight/2 + screenHeight/4 = -screenHeight/4
                float buttonY = -screenHeight / 4f;
                defeatQuitRect.anchoredPosition = new Vector2(0, buttonY);
                
                Debug.Log($"✅ UIManager: DefeatQuitButton位置已设置: ({defeatQuitRect.anchoredPosition.x}, {defeatQuitRect.anchoredPosition.y})");
            }
        }
    }
    
    // 同步胜利界面按钮样式与主菜单按钮一致，并设置布局
    void SyncVictoryButtonStyles()
    {
        // 设置胜利文本样式（很大且居中）
        SetupVictoryText();
        
        // 获取参考按钮的样式信息
        RectTransform startRect = startButton?.GetComponent<RectTransform>();
        RectTransform quitRect = quitButton?.GetComponent<RectTransform>();
        Image startImage = startButton?.GetComponent<Image>();
        Image quitImage = quitButton?.GetComponent<Image>();
        
        // 设置 victoryQuitButton（参考 startButton 的样式，往上移动）
        if (victoryQuitButton != null && startRect != null)
        {
            RectTransform victoryQuitRect = victoryQuitButton.GetComponent<RectTransform>();
            if (victoryQuitRect != null)
            {
                // 设置按钮大小：与 startButton 一样大小
                victoryQuitRect.sizeDelta = startRect.sizeDelta;
                victoryQuitRect.localScale = Vector3.one;
                
                // 设置按钮位置：水平居中，垂直位置在屏幕底部1/4处
                victoryQuitRect.anchorMin = new Vector2(0.5f, 0.5f);
                victoryQuitRect.anchorMax = new Vector2(0.5f, 0.5f);
                victoryQuitRect.pivot = new Vector2(0.5f, 0.5f);
                
                // 计算位置：屏幕底部1/4的位置
                Canvas canvas = victoryQuitRect.GetComponentInParent<Canvas>();
                float screenHeight = Screen.height;
                if (canvas != null && canvas.GetComponent<RectTransform>() != null)
                {
                    screenHeight = canvas.GetComponent<RectTransform>().rect.height;
                }
                // 屏幕中心是0，底部是-screenHeight/2，底部1/4位置是 -screenHeight/2 + screenHeight/4 = -screenHeight/4
                float buttonY = -screenHeight / 4f;
                victoryQuitRect.anchoredPosition = new Vector2(0, buttonY);
                
                Debug.Log($"✅ UIManager: VictoryQuitButton位置已设置: ({victoryQuitRect.anchoredPosition.x}, {victoryQuitRect.anchoredPosition.y})");
            }
            
            // 复制颜色
            Image victoryQuitImage = victoryQuitButton.GetComponent<Image>();
            if (startImage != null && victoryQuitImage != null)
            {
                victoryQuitImage.color = startImage.color;
            }
            
            // 复制文本样式
            CopyButtonTextStyle(startButton, victoryQuitButton);
            
            // 确保按钮大小足够包裹文字
            EnsureButtonFitsText(victoryQuitButton);
        }
    }
    
    // 复制按钮文本样式
    void CopyButtonTextStyle(GameObject sourceButton, GameObject targetButton)
    {
        if (sourceButton == null || targetButton == null) return;
        
        // 尝试 TextMeshPro
        TMPro.TextMeshProUGUI sourceText = sourceButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        TMPro.TextMeshProUGUI targetText = targetButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (sourceText != null && targetText != null)
        {
            targetText.color = sourceText.color;
            targetText.fontSize = sourceText.fontSize;
            targetText.fontStyle = sourceText.fontStyle;
        }
        else
        {
            // 尝试传统 Text
            Text sourceTextLegacy = sourceButton.GetComponentInChildren<Text>();
            Text targetTextLegacy = targetButton.GetComponentInChildren<Text>();
            if (sourceTextLegacy != null && targetTextLegacy != null)
            {
                targetTextLegacy.color = sourceTextLegacy.color;
                targetTextLegacy.fontSize = sourceTextLegacy.fontSize;
                targetTextLegacy.fontStyle = sourceTextLegacy.fontStyle;
            }
        }
    }
    
    // 确保按钮大小足够包裹文字
    void EnsureButtonFitsText(GameObject button)
    {
        if (button == null) return;
        
        TMPro.TextMeshProUGUI text = button.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (text != null)
        {
            // 让文本自动调整大小以适应按钮
            text.enableAutoSizing = false;
            
            // 计算文本所需的最小宽度
            float textWidth = text.preferredWidth;
            float textHeight = text.preferredHeight;
            
            RectTransform buttonRect = button.GetComponent<RectTransform>();
            if (buttonRect != null)
            {
                // 确保按钮宽度至少是文本宽度的1.2倍，高度至少是文本高度的1.3倍
                float minWidth = Mathf.Max(buttonRect.sizeDelta.x, textWidth * 1.2f);
                float minHeight = Mathf.Max(buttonRect.sizeDelta.y, textHeight * 1.3f);
                
                buttonRect.sizeDelta = new Vector2(minWidth, minHeight);
                
                Debug.Log($"✅ UIManager: {button.name}大小已调整为({minWidth}, {minHeight})以包裹文字");
            }
        }
        else
        {
            Text textLegacy = button.GetComponentInChildren<Text>();
            if (textLegacy != null)
            {
                RectTransform buttonRect = button.GetComponent<RectTransform>();
                if (buttonRect != null)
                {
                    // 对于传统Text，使用ContentSizeFitter或手动调整
                    ContentSizeFitter fitter = button.GetComponent<ContentSizeFitter>();
                    if (fitter == null)
                    {
                        fitter = button.AddComponent<ContentSizeFitter>();
                    }
                    fitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    
                    // 添加一些边距
                    buttonRect.sizeDelta = new Vector2(
                        Mathf.Max(buttonRect.sizeDelta.x, textLegacy.preferredWidth + 40),
                        Mathf.Max(buttonRect.sizeDelta.y, textLegacy.preferredHeight + 20)
                    );
                }
            }
        }
    }
    
    // 设置胜利文本样式（很大且居中）
    void SetupVictoryText()
    {
        if (victoryText == null)
        {
            // 尝试自动查找
            if (victoryPanel != null)
            {
                victoryText = victoryPanel.transform.Find("VictoryText")?.gameObject;
                if (victoryText == null)
                {
                    victoryText = victoryPanel.transform.Find("Victory Text")?.gameObject;
                }
            }
        }
        
        if (victoryText != null)
        {
            RectTransform textRect = victoryText.GetComponent<RectTransform>();
            if (textRect != null)
            {
                // 设置完全居中（水平和垂直都居中）
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.pivot = new Vector2(0.5f, 0.5f);
                
                // 设置位置：屏幕上方，在按钮上方
                float textY = 200f; // 距离中心上方200像素
                if (startButton != null)
                {
                    RectTransform startRect = startButton.GetComponent<RectTransform>();
                    if (startRect != null)
                    {
                        // 文本在 startButton 上方，间距约150像素
                        textY = startRect.anchoredPosition.y + 150f;
                    }
                }
                textRect.anchoredPosition = new Vector2(0, textY);
                
                Debug.Log($"✅ UIManager: VictoryText已设置为居中，位置: ({textRect.anchoredPosition.x}, {textRect.anchoredPosition.y})");
            }
            
            // 设置更大的字体
            TMPro.TextMeshProUGUI tmpText = victoryText.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                // 只在字体大小小于100时才设置（避免覆盖Inspector中的设置）
                if (tmpText.fontSize < 100f)
                {
                    tmpText.fontSize = 160f; // 设置大字体
                }
                tmpText.alignment = TMPro.TextAlignmentOptions.Center;
                tmpText.enableAutoSizing = false;
                Debug.Log($"✅ UIManager: VictoryText字体大小已设置为{tmpText.fontSize}，已居中");
            }
            else
            {
                Text textLegacy = victoryText.GetComponent<Text>();
                if (textLegacy != null)
                {
                    // 只在字体大小小于100时才设置（避免覆盖Inspector中的设置）
                    if (textLegacy.fontSize < 100)
                    {
                        textLegacy.fontSize = 160;
                    }
                    textLegacy.alignment = TextAnchor.MiddleCenter;
                    Debug.Log($"✅ UIManager: VictoryText字体大小已设置为{textLegacy.fontSize}，已居中");
                }
            }
        }
        else
        {
            Debug.LogWarning("⚠️ UIManager: 无法找到VictoryText！");
        }
    }
    
    public void RestartGame()
    {
        Debug.Log("🔄 UIManager: RestartGame 被调用，准备重新开始游戏（完全重新加载场景，效果与StartGame相同）");
        
        // 隐藏所有UI面板
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (defeatPanel != null) defeatPanel.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (puzzleCompletePanel != null) puzzleCompletePanel.SetActive(false);
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        
        // 重置游戏状态
        isGameStarted = false;
        isPaused = false;
        Time.timeScale = 1f;
        
        // 调用GameManager的RestartGame，这会重新加载场景并重置所有数据
        // 场景加载后，UIManager.Start()会检测到Loading状态，自动跳过主菜单直接开始游戏
        // 效果与StartGame完全相同
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            // 如果没有GameManager，直接重新加载场景
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
    
    public void QuitToMainMenu()
    {
        // 调用GameManager的ReturnToMainMenu（如果存在）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ReturnToMainMenu();
        }
        else
        {
            // 如果没有GameManager，直接显示主菜单
            Time.timeScale = 1f;
            ShowMainMenu();
        }
    }
    
    public void QuitGame()
    {
        // 调用GameManager的QuitGame（如果存在）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.QuitGame();
        }
        else
        {
            // 如果没有GameManager，直接退出
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }
    }
    
    // 切换声音播放/暂停
    public void ToggleSound()
    {
        if (AudioManager.Instance != null)
        {
            AudioManager.Instance.ToggleBGM();
            UpdateSoundButtonText();
            Debug.Log($"声音状态: {(AudioManager.Instance.IsPlaying() ? "播放中" : "已暂停")}");
        }
        else
        {
            Debug.LogWarning("⚠️ AudioManager.Instance为空，无法控制声音！请确保场景中有AudioManager对象");
        }
    }
    
    // 更新声音按钮的文本显示
    void UpdateSoundButtonText()
    {
        if (soundButton == null) return;
        
        // 按钮文本保持为 "Music"，不随播放状态改变
        string buttonText = "Music";
        
        // 尝试更新按钮文本（支持Text和TextMeshPro）
        TMPro.TextMeshProUGUI tmpText = soundButton.GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (tmpText != null)
        {
            tmpText.text = buttonText;
        }
        else
        {
            Text textComponent = soundButton.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                textComponent.text = buttonText;
            }
        }
    }
    
    public void UpdatePuzzleProgress(int collected, int total)
    {
        Debug.Log($"UIManager.UpdatePuzzleProgress 被调用: {collected}/{total}");
        
        // 如果字段为空，尝试自动查找
        if (puzzleProgressText == null)
        {
            Debug.LogWarning("puzzleProgressText字段为空，尝试自动查找...");
            puzzleProgressText = GameObject.Find("ProgressText");
            if (puzzleProgressText == null)
            {
                // 尝试其他可能的名称
                puzzleProgressText = GameObject.Find("PuzzleProgressText");
            }
            if (puzzleProgressText != null)
            {
                Debug.Log($"自动找到进度文本对象: {puzzleProgressText.name}");
            }
        }
        
        if (puzzleProgressText != null)
        {
            string progressText = $"Collecting Puzzles: {collected}/{total}";
            
            // 尝试使用TextMeshPro（新版本）
            TMPro.TextMeshProUGUI tmpText = puzzleProgressText.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = progressText;
                Debug.Log($"✓ 已更新TextMeshPro进度文本: {progressText}");
            }
            else
            {
                // 尝试使用传统Text组件
                Text textComponent = puzzleProgressText.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = progressText;
                    Debug.Log($"✓ 已更新Text进度文本: {progressText}");
                }
                else
                {
                    Debug.LogError($"✗ puzzleProgressText对象 '{puzzleProgressText.name}' 上既没有TextMeshPro也没有Text组件！");
                }
            }
        }
        else
        {
            Debug.LogError("✗ puzzleProgressText字段为空且无法自动查找！请手动在UIManager中设置 Puzzle Progress Text 字段！");
        }
        
        // 进度条已移除，只使用文本显示进度
    }
    
    public void ShowInteractionPrompt(string text)
    {
        // 如果字段为空，尝试自动查找
        if (interactionPromptPanel == null)
        {
            interactionPromptPanel = GameObject.Find("InteractionPromptPanel");
            if (interactionPromptPanel == null)
            {
                // 尝试在GameHUD下查找
                GameObject gameHUD = GameObject.Find("GameHUD");
                if (gameHUD != null)
                {
                    Transform panelTransform = gameHUD.transform.Find("InteractionPromptPanel");
                    if (panelTransform != null)
                    {
                        interactionPromptPanel = panelTransform.gameObject;
                    }
                }
            }
        }
        
        if (interactionPromptText == null && interactionPromptPanel != null)
        {
            interactionPromptText = interactionPromptPanel.transform.Find("InteractionPromptText")?.gameObject;
        }
        
        // 添加调试信息
        Debug.Log($"UIManager.ShowInteractionPrompt 被调用，文本: {text}");
        Debug.Log($"interactionPromptPanel 是否为null: {interactionPromptPanel == null}");
        
        if (interactionPromptPanel != null)
        {
            Debug.Log($"显示面板: {interactionPromptPanel.name}, 路径: {GetGameObjectPath(interactionPromptPanel)}");
            
            // 先隐藏所有可能的其他交互提示面板（以防有多个）
            HideAllInteractionPrompts(interactionPromptPanel);
            
            // 显示正确的面板
            interactionPromptPanel.SetActive(true);
            
            if (interactionPromptText != null)
            {
                // 尝试使用TextMeshPro（新版本）
                TMPro.TextMeshProUGUI tmpText = interactionPromptText.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = text;
                    Debug.Log($"已设置TextMeshPro文本: {text}");
                }
                else
                {
                    // 尝试使用传统Text组件
                    Text textComponent = interactionPromptText.GetComponent<Text>();
                    if (textComponent != null)
                    {
                        textComponent.text = text;
                        Debug.Log($"已设置Text文本: {text}");
                    }
                    else
                    {
                        Debug.LogWarning($"interactionPromptText对象 '{interactionPromptText.name}' 上既没有TextMeshPro也没有Text组件！");
                    }
                }
            }
            else
            {
                Debug.LogWarning("interactionPromptText字段为空（None）！无法设置文本！");
            }
        }
        else
        {
            Debug.LogError("interactionPromptPanel字段为空（None）！请在UIManager的Inspector中设置 Interaction Prompt Panel 字段！");
        }
    }
    
    // 隐藏所有交互提示面板（除了指定的那个）
    private void HideAllInteractionPrompts(GameObject keepActive)
    {
        // 查找场景中所有可能的交互提示面板
        Canvas[] canvases = FindObjectsOfType<Canvas>();
        foreach (Canvas canvas in canvases)
        {
            // 在每个 Canvas 下查找 InteractionPromptPanel
            Transform[] allChildren = canvas.GetComponentsInChildren<Transform>(true);
            foreach (Transform child in allChildren)
            {
                // 查找所有可能的交互提示面板名称
                string name = child.gameObject.name.ToLower();
                if ((name.Contains("interaction") && name.Contains("prompt") && name.Contains("panel")) ||
                    name == "interactionpromptpanel" || name == "interaction prompt panel")
                {
                    // 如果不是我们要显示的面板，就隐藏它
                    if (child.gameObject != keepActive)
                    {
                        child.gameObject.SetActive(false);
                        Debug.Log($"已隐藏其他交互提示面板: {child.gameObject.name}");
                    }
                }
            }
        }
    }
    
    // 获取 GameObject 的完整路径（用于调试）
    private string GetGameObjectPath(GameObject obj)
    {
        string path = obj.name;
        Transform parent = obj.transform.parent;
        while (parent != null)
        {
            path = parent.name + "/" + path;
            parent = parent.parent;
        }
        return path;
    }
    
    // 递归查找子对象（包括非激活的）
    private Transform FindChildRecursive(Transform parent, string name)
    {
        foreach (Transform child in parent)
        {
            if (child.name == name)
            {
                return child;
            }
            Transform found = FindChildRecursive(child, name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }
    
    public void HideInteractionPrompt()
    {
        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(false);
            // 日志已移除，避免控制台刷屏
        }
    }
    
    private void UpdateInteractionPrompt()
    {
        // 这个方法会被InteractionPrompt系统调用
        // 保持这里为空，由InteractionPrompt系统管理
    }
    
    public bool IsGamePaused()
    {
        return isPaused;
    }
    
    public bool IsGameStarted()
    {
        return isGameStarted;
    }
    
    // ========== 测试方法：强制显示庆祝界面 ==========
    [ContextMenu("测试：显示庆祝界面")]
    public void TestShowCelebration()
    {
        Debug.LogWarning("🧪 测试：手动触发显示庆祝界面");
        ShowPuzzleCompleteCelebration();
    }
    
    public void ShowPuzzleCompleteCelebration()
    {
        Debug.LogWarning("UIManager.ShowPuzzleCompleteCelebration() 被调用！准备显示庆祝界面！");
        
        // 如果字段为空，尝试自动查找
        if (puzzleCompletePanel == null)
        {
            Debug.LogWarning("UIManager: puzzleCompletePanel字段为空，尝试自动查找...");
            puzzleCompletePanel = GameObject.Find("PuzzleCompletePanel");
            if (puzzleCompletePanel == null)
            {
                // 尝试其他可能的名称
                puzzleCompletePanel = GameObject.Find("Puzzle Complete Panel");
            }
            if (puzzleCompletePanel != null)
            {
                Debug.Log($"UIManager: 自动找到庆祝面板: {puzzleCompletePanel.name}");
            }
            else
            {
                Debug.LogError("UIManager: 未找到PuzzleCompletePanel！请创建庆祝界面UI或手动在UIManager中设置 Puzzle Complete Panel 字段！");
                Debug.LogError("提示：需要在Unity中创建PuzzleCompletePanel UI界面，参考：拼图完成庆祝界面详细创建步骤.md");
                return;
            }
        }
        
        // 如果文本字段为空，尝试自动查找
        if (puzzleCompleteText == null)
        {
            Debug.LogWarning("UIManager: puzzleCompleteText字段为空，尝试自动查找...");
            if (puzzleCompletePanel != null)
            {
                // 在面板下查找文本对象
                puzzleCompleteText = puzzleCompletePanel.transform.Find("PuzzleCompleteText")?.gameObject;
                if (puzzleCompleteText == null)
                {
                    puzzleCompleteText = puzzleCompletePanel.transform.Find("Puzzle Complete Text")?.gameObject;
                }
            }
            if (puzzleCompleteText != null)
            {
                Debug.Log($"UIManager: 自动找到庆祝文本: {puzzleCompleteText.name}");
            }
        }
        
        // 如果控制器为空，尝试获取
        if (puzzleCompleteCelebration == null)
        {
            puzzleCompleteCelebration = puzzleCompletePanel.GetComponent<PuzzleCompleteCelebration>();
            if (puzzleCompleteCelebration == null)
            {
                // 如果没有组件，尝试添加
                Debug.Log("UIManager: 未找到PuzzleCompleteCelebration组件，自动添加...");
                puzzleCompleteCelebration = puzzleCompletePanel.AddComponent<PuzzleCompleteCelebration>();
                puzzleCompleteCelebration.celebrationPanel = puzzleCompletePanel;
                puzzleCompleteCelebration.celebrationText = puzzleCompleteText;
                Debug.Log("UIManager: 自动添加了PuzzleCompleteCelebration组件");
            }
        }
        
        // 显示庆祝界面
        if (puzzleCompleteCelebration != null)
        {
            Debug.Log("找到PuzzleCompleteCelebration组件，调用ShowCelebration()");
            puzzleCompleteCelebration.ShowCelebration();
        }
        else if (puzzleCompletePanel != null)
        {
            // 如果没有控制器，直接显示面板
            Debug.Log("没有找到Celebration组件，直接显示面板");
            puzzleCompletePanel.SetActive(true);
            Debug.Log("✅ 庆祝面板已激活！");
        }
        else
        {
            Debug.LogError("❌ 错误：puzzleCompletePanel和puzzleCompleteCelebration都为空！无法显示庆祝界面！");
        }
    }
    
    public void HidePuzzleCompleteCelebration()
    {
        if (puzzleCompleteCelebration != null)
        {
            puzzleCompleteCelebration.HideCelebration();
        }
        else if (puzzleCompletePanel != null)
        {
            puzzleCompletePanel.SetActive(false);
        }
    }

    private void UpdatePuzzleVisual(int puzzleId)
    {
        if (puzzleSlots == null || puzzleSlots.Length < puzzleId)
        {
            Debug.LogWarning("Puzzle slot array 未正确设置！");
            return;
        }

        int index = puzzleId - 1; // puzzleId 从 1 开始，数组从 0 开始

        // 找对应slot
        Image slot = puzzleSlots[index];

        if (slot == null)
        {
            Debug.LogWarning($"Puzzle Slot {puzzleId} 没有设置 Image！");
            return;
        }

        // 设置拼图图片（Inspector 中定义）
        if (puzzleSprites != null && puzzleSprites.Length > index)
        {
            slot.sprite = puzzleSprites[index];
        }
        else
        {
            Debug.LogWarning("PuzzleSprites 未设置，无法显示拼图图片！");
            return;
        }

        // 可选：播放一个闪烁动画
        StartCoroutine(PuzzleFlash(slot.transform));
    }

    private IEnumerator PuzzleFlash(Transform t)
    {
        Vector3 originalScale = t.localScale;
        Vector3 bigScale = originalScale * 1.2f;

        float time = 0f;
        float duration = 0.15f;

        while (time < duration)
        {
            t.localScale = Vector3.Lerp(originalScale, bigScale, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        time = 0f;
        while (time < duration)
        {
            t.localScale = Vector3.Lerp(bigScale, originalScale, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        t.localScale = originalScale;
    }

    // ========== 教程面板相关方法 ==========
    public void ShowTutorial()
    {
        if (tutorialPanel == null)
        {
            // 尝试自动查找
            tutorialPanel = GameObject.Find("TutorialPanel");
            if (tutorialPanel == null)
            {
                Debug.LogWarning("⚠️ UIManager: tutorialPanel为空，尝试自动查找失败");
                return;
            }
        }
        
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(true);
            Debug.Log("✅ UIManager: 已显示教程面板");
            
            // 确保关闭按钮已连接
            Transform closeButton = tutorialPanel.transform.Find("CloseButton");
            if (closeButton != null)
            {
                Button btn = closeButton.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(CloseTutorial);
                }
            }
        }
    }
    
    public void CloseTutorial()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
            Debug.Log("✅ UIManager: 已关闭教程面板");
        }
        else
        {
            // 尝试自动查找
            GameObject foundPanel = GameObject.Find("TutorialPanel");
            if (foundPanel != null)
            {
                foundPanel.SetActive(false);
                Debug.Log("✅ UIManager: 已关闭教程面板（自动查找）");
            }
        }
    }
    
}


