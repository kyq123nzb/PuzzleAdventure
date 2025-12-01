using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleDiagnostics : MonoBehaviour
{
    [Header("诊断设置")]
    [SerializeField] private float checkInterval = 0.5f;
    [SerializeField] private int maxWaitCycles = 40; // 增加到40次
    
    [Header("场景管理")]
    [SerializeField] private bool runOnStart = true; // 是否在Start时运行
    [SerializeField] private bool createGameManagerIfMissing = false; // 如果缺少GameManager是否创建
    
    void Start()
    {
        if (runOnStart)
        {
            StartCoroutine(RunDiagnostics());
        }
    }
    
    // 公开方法，可以从UI按钮调用
    public void RunDiagnosticsManual()
    {
        StartCoroutine(RunDiagnostics());
    }
    
    IEnumerator RunDiagnostics()
    {
        Debug.Log("=== 开始拼图系统诊断 ===");
        Debug.Log($"当前场景: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}");
        
        // 首先检查GameManager GameObject是否存在
        GameObject gameManagerObj = GameObject.Find("GameManager");
        if (gameManagerObj == null)
        {
            Debug.LogWarning("⚠ 场景中未找到名为'GameManager'的GameObject");
            
            // 尝试查找任何挂载了GameManager脚本的对象
            GameManager[] allManagers = FindObjectsOfType<GameManager>(true);
            if (allManagers.Length == 0)
            {
                Debug.LogError("❌ 场景中未找到任何GameManager脚本！");
                
                if (createGameManagerIfMissing)
                {
                    Debug.Log("正在创建GameManager...");
                    CreateGameManager();
                    yield return new WaitForSeconds(2f); // 等待创建完成
                }
                else
                {
                    Debug.Log("建议：在场景中创建一个名为'GameManager'的GameObject，并附加GameManager脚本");
                    yield break;
                }
            }
            else
            {
                Debug.Log($"✅ 找到 {allManagers.Length} 个GameManager脚本");
                gameManagerObj = allManagers[0].gameObject;
            }
        }
        else
        {
            Debug.Log("✅ 找到GameManager GameObject");
            Debug.Log($"GameManager激活状态: {gameManagerObj.activeSelf}");
            
            // 检查GameManager组件
            GameManager managerComponent = gameManagerObj.GetComponent<GameManager>();
            if (managerComponent == null)
            {
                Debug.LogError("❌ GameManager GameObject上没有GameManager脚本！");
                yield break;
            }
        }
        
        // 等待GameManager实例初始化
        Debug.Log("等待GameManager单例初始化...");
        int waitCount = 0;
        
        while (GameManager.Instance == null && waitCount < maxWaitCycles)
        {
            waitCount++;
            Debug.Log($"等待GameManager.Instance... 尝试 {waitCount}/{maxWaitCycles}");
            yield return new WaitForSeconds(checkInterval);
        }
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("❌ GameManager.Instance 仍然为空！");
            Debug.Log("可能的原因：");
            Debug.Log("1. GameManager脚本的Awake()方法没有执行");
            Debug.Log("2. GameManager GameObject被禁用");
            Debug.Log("3. 有多个GameManager实例导致冲突");
            Debug.Log("4. 脚本执行顺序问题");
            
            // 尝试强制查找
            GameManager[] allManagers = FindObjectsOfType<GameManager>(true);
            if (allManagers.Length > 0)
            {
                Debug.Log($"找到 {allManagers.Length} 个GameManager组件：");
                foreach (var mgr in allManagers)
                {
                    Debug.Log($"- {mgr.gameObject.name} (活跃: {mgr.gameObject.activeInHierarchy})");
                }
            }
            
            yield break;
        }
        
        Debug.Log("✅ GameManager 实例存在");
        
        // 后续诊断代码...
        yield return StartCoroutine(CheckPuzzleSystem());
    }
    
    IEnumerator CheckPuzzleSystem()
    {
        Debug.Log("=== 检查拼图系统 ===");
        
        // 检查拼图总数
        Debug.Log($"拼图总数设置: {GameManager.Instance.TotalPuzzles}");
        
        // 查找所有拼图
        PuzzleItem[] puzzles = FindObjectsOfType<PuzzleItem>(true);
        Debug.Log($"场景中找到 {puzzles.Length} 个拼图物体");
        
        if (puzzles.Length == 0)
        {
            Debug.LogWarning("⚠ 场景中没有找到任何PuzzleItem物体！");
            Debug.Log("检查点：");
            Debug.Log("1. 拼图是否都挂载了PuzzleItem脚本");
            Debug.Log("2. 拼图是否被禁用或隐藏");
            Debug.Log("3. 拼图是否在正确的层级");
            yield break;
        }
        
        // 检查拼图ID
        List<int> puzzleIds = new List<int>();
        foreach (PuzzleItem puzzle in puzzles)
        {
            int id = puzzle.puzzleId;
            
            if (id < 1 || id > GameManager.Instance.TotalPuzzles)
            {
                Debug.LogError($"拼图 '{puzzle.gameObject.name}' 的ID {id} 超出范围 (1-{GameManager.Instance.TotalPuzzles})");
            }
            
            if (puzzleIds.Contains(id))
            {
                Debug.LogError($"拼图ID {id} 重复！");
            }
            else
            {
                puzzleIds.Add(id);
            }
        }
        
        // 显示收集状态
        Debug.Log("当前拼图收集状态：");
        for (int i = 1; i <= GameManager.Instance.TotalPuzzles; i++)
        {
            bool collected = GameManager.Instance.IsPuzzleCollected(i);
            Debug.Log($"拼图 {i}: {(collected ? "✅ 已收集" : "❌ 未收集")}");
        }
        
        Debug.Log($"收集进度: {GameManager.Instance.GetCollectedPuzzlesCount()}/{GameManager.Instance.TotalPuzzles}");
        
        yield break;
    }
    
    void CreateGameManager()
    {
        GameObject gmObj = new GameObject("GameManager");
        gmObj.AddComponent<GameManager>();
        Debug.Log("✅ 已创建GameManager GameObject");
    }
}