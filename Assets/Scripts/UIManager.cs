using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    
    [Header("游戏内UI")]
    public GameObject gameHUD;
    public GameObject puzzleProgressText; // 支持Text和TextMeshPro
    public Image puzzleProgressFill;
    public GameObject interactionPromptPanel;
    public GameObject interactionPromptText; // 支持Text和TextMeshPro
    
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
    public GameObject victoryRestartButton; // 支持GameObject，自动获取Button组件
    public GameObject victoryQuitButton; // 支持GameObject，自动获取Button组件
    
    [Header("设置")]
    public bool lockCursorOnStart = false;
    
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
            Destroy(gameObject);
            return;
        }
        
        SetupUI();
    }
    
    void Start()
    {
        ShowMainMenu();
        SetupButtonListeners();
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
    }
    
    void SetupUI()
    {
        // 初始化UI状态
        if (mainMenuPanel != null) mainMenuPanel.SetActive(false);
        if (gameHUD != null) gameHUD.SetActive(false);
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        if (interactionPromptPanel != null) interactionPromptPanel.SetActive(false);
    }
    
    void SetupButtonListeners()
    {
        // 主菜单按钮
        if (startButton != null)
        {
            Button startBtn = startButton.GetComponent<Button>();
            if (startBtn != null)
            {
                startBtn.onClick.AddListener(StartGame);
                Debug.Log("UIManager: StartButton已连接");
            }
            else
            {
                Debug.LogError("UIManager: StartButton对象上没有Button组件！");
            }
        }
        else
        {
            Debug.LogError("UIManager: StartButton字段为空（None）！");
        }
        
        if (quitButton != null)
        {
            Button quitBtn = quitButton.GetComponent<Button>();
            if (quitBtn != null)
                quitBtn.onClick.AddListener(QuitGame);
        }
        
        // 暂停菜单按钮（ESC弹出的那个菜单）
        if (resumeButton != null)
        {
            Button resumeBtn = resumeButton.GetComponent<Button>();
            if (resumeBtn != null)
            {
                resumeBtn.onClick.AddListener(ResumeGame);
                Debug.Log("UIManager: PauseMenu ResumeButton已连接");
            }
        }
        
        if (pauseQuitButton != null)
        {
            Button pauseQuitBtn = pauseQuitButton.GetComponent<Button>();
            if (pauseQuitBtn != null)
            {
                pauseQuitBtn.onClick.AddListener(QuitToMainMenu);
                Debug.Log("UIManager: PauseMenu QuitButton已连接");
            }
        }
        
        // HUD上一直存在的暂停/继续按钮（如果你创建了的话）
        // 如果没有在Inspector里拖拽引用，就按照名字自动查找
        if (hudPauseButton == null)
        {
            hudPauseButton = GameObject.Find("PauseButton");
        }
        if (hudResumeButton == null)
        {
            hudResumeButton = GameObject.Find("ResumeButton");
        }
        
        if (hudPauseButton != null)
        {
            Button hudPauseBtn = hudPauseButton.GetComponent<Button>();
            if (hudPauseBtn != null)
            {
                hudPauseBtn.onClick.RemoveListener(PauseGame); // 防止重复添加
                hudPauseBtn.onClick.AddListener(PauseGame);
                Debug.Log("UIManager: HUD PauseButton已自动连接到PauseGame");
            }
            else
            {
                Debug.LogError("UIManager: HUD PauseButton对象上没有Button组件！");
            }
        }
        else
        {
            Debug.Log("UIManager: 未找到名为 \"PauseButton\" 的HUD暂停按钮（如果不需要常驻按钮可以忽略此提示）");
        }
        
        if (hudResumeButton != null)
        {
            Button hudResumeBtn = hudResumeButton.GetComponent<Button>();
            if (hudResumeBtn != null)
            {
                hudResumeBtn.onClick.RemoveListener(ResumeGame); // 防止重复添加
                hudResumeBtn.onClick.AddListener(ResumeGame);
                Debug.Log("UIManager: HUD ResumeButton已自动连接到ResumeGame");
            }
            else
            {
                Debug.LogError("UIManager: HUD ResumeButton对象上没有Button组件！");
            }
        }
        else
        {
            Debug.Log("UIManager: 未找到名为 \"ResumeButton\" 的HUD继续按钮（如果不需要常驻按钮可以忽略此提示）");
        }
        
        // 胜利界面按钮
        if (victoryRestartButton != null)
        {
            Button victoryRestartBtn = victoryRestartButton.GetComponent<Button>();
            if (victoryRestartBtn != null)
                victoryRestartBtn.onClick.AddListener(RestartGame);
        }
        
        if (victoryQuitButton != null)
        {
            Button victoryQuitBtn = victoryQuitButton.GetComponent<Button>();
            if (victoryQuitBtn != null)
                victoryQuitBtn.onClick.AddListener(QuitToMainMenu);
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
    }
    
    public void StartGame()
    {
        Debug.Log("UIManager: StartGame方法被调用了！");
        
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
        
        if (mainMenuPanel != null)
        {
            mainMenuPanel.SetActive(false);
            Debug.Log("UIManager: 主菜单已隐藏");
        }
        else
        {
            Debug.LogError("UIManager: mainMenuPanel为空（None）！无法隐藏主菜单！");
        }
        
        if (gameHUD != null)
        {
            gameHUD.SetActive(true);
            Debug.Log("UIManager: 游戏内UI已显示");
        }
        else
        {
            Debug.LogError("UIManager: gameHUD为空（None）！无法显示游戏内UI！");
        }
        
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (victoryPanel != null) victoryPanel.SetActive(false);
        
        // 调用GameManager的StartNewGame（如果存在）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame();
        }
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
        
        Debug.Log("UIManager: PauseGame 被调用，游戏已暂停");
        
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
        
        // 调用GameManager的ResumeGame（如果存在）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.ResumeGame();
        }
    }
    
    public void ShowVictory()
    {
        isGameStarted = false;
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        if (victoryPanel != null) victoryPanel.SetActive(true);
        if (gameHUD != null) gameHUD.SetActive(false);
    }
    
    public void RestartGame()
    {
        // 调用GameManager的RestartGame（如果存在）
        if (GameManager.Instance != null)
        {
            GameManager.Instance.RestartGame();
        }
        else
        {
            // 如果没有GameManager，直接重新加载场景
            Time.timeScale = 1f;
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
    
    public void UpdatePuzzleProgress(int collected, int total)
    {
        if (puzzleProgressText != null)
        {
            string progressText = $"拼图: {collected}/{total}";
            
            // 尝试使用TextMeshPro（新版本）
            TMPro.TextMeshProUGUI tmpText = puzzleProgressText.GetComponent<TMPro.TextMeshProUGUI>();
            if (tmpText != null)
            {
                tmpText.text = progressText;
            }
            else
            {
                // 尝试使用传统Text组件
                Text textComponent = puzzleProgressText.GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = progressText;
                }
            }
        }
        
        // 尝试使用ProgressBarFill脚本（不需要Image Type）
        if (puzzleProgressFill != null)
        {
            // 从Image组件所在的GameObject上获取ProgressBarFill脚本
            ProgressBarFill progressBarFill = puzzleProgressFill.GetComponent<ProgressBarFill>();
            
            if (progressBarFill != null)
            {
                // 使用ProgressBarFill脚本（不需要Image Type）
                progressBarFill.SetProgress(collected, total);
            }
            else
            {
                // 兼容旧方式：使用Image的fillAmount（需要Image Type = Filled）
                float progress = total > 0 ? (float)collected / total : 0f;
                puzzleProgressFill.fillAmount = progress;
            }
        }
    }
    
    public void ShowInteractionPrompt(string text)
    {
        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(true);
            if (interactionPromptText != null)
            {
                // 尝试使用TextMeshPro（新版本）
                TMPro.TextMeshProUGUI tmpText = interactionPromptText.GetComponent<TMPro.TextMeshProUGUI>();
                if (tmpText != null)
                {
                    tmpText.text = text;
                }
                else
                {
                    // 尝试使用传统Text组件
                    Text textComponent = interactionPromptText.GetComponent<Text>();
                    if (textComponent != null)
                    {
                        textComponent.text = text;
                    }
                }
            }
        }
    }
    
    public void HideInteractionPrompt()
    {
        if (interactionPromptPanel != null)
        {
            interactionPromptPanel.SetActive(false);
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
}
