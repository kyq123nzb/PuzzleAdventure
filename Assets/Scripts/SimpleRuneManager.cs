using System.Collections.Generic;
using UnityEngine;

public class SimpleRuneManager : MonoBehaviour
{
    [Header("Configuration")]
    public List<SimpleClickRune> allRunes;
    public List<SimpleClickRune> correctRunes;

    [Header("Rewards")]
    public GameObject rewardPrefab;
    public Transform rewardSpawnPoint;

    [Header("Hint Connection (关键修改)")]
    // 这里加一个变量，用来专门连接那个有提示功能的宝箱
    public PuzzleHintObject linkedHintChest;

    [Header("Victory Message")]
    [TextArea]
    public string successMessage = "Nice to meet you, please pick up the puzzle you want.";

    private bool isSolved = false;

    public void CheckPuzzle()
    {
        if (isSolved) return;

        bool isCorrect = true;
        foreach (var rune in allRunes)
        {
            bool shouldBeActive = correctRunes.Contains(rune);
            if (rune.isActive != shouldBeActive)
            {
                isCorrect = false;
                break;
            }
        }

        if (isCorrect)
        {
            Solve();
        }
    }

    void Solve()
    {
        isSolved = true;
        Debug.Log("🎉 谜题解开！");

        // 1. 锁定符文
        foreach (var rune in allRunes)
        {
            rune.LockRune(successMessage);
        }

        // 2. 生成奖励
        if (rewardPrefab != null && rewardSpawnPoint != null)
        {
            Instantiate(rewardPrefab, rewardSpawnPoint.position, Quaternion.identity);
        }

        // 3. 立即刷新 UI
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowInteractionPrompt(successMessage);
        }

        // 4. 【修改】通过拖拽的引用来修改宝箱文字
        if (linkedHintChest != null)
        {
            // 把“默认提示”和“线索内容”都改成胜利感言
            linkedHintChest.defaultPrompt = successMessage;
            linkedHintChest.hintContent = successMessage;

            // 强制更新当前显示的文字
            linkedHintChest.interactionText = successMessage;

            // 确保它还能交互（以此来显示文字）
            linkedHintChest.canInteract = true;

            Debug.Log("宝箱提示语已更新为胜利感言");
        }
        else
        {
            Debug.LogWarning("⚠️ 注意：你没有把宝箱拖入 SimpleRuneManager 的 Linked Hint Chest 槽位，所以宝箱文字没变。");
        }
    }
}