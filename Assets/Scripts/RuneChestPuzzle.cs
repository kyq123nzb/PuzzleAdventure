using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RuneChestPuzzle : Interactable
{
    [Header("解谜设置")]
    public string correctSequence = "123"; // 正确的顺序密码
    public List<RuneStone> runes; // 场景中所有关联的符文

    [Header("宝箱动画设置")]
    public Transform lidPivot; // 盖子的旋转轴心（父物体）
    public float openAngle = -110f; // 打开的角度

    [Header("奖励")]
    public GameObject rewardItem; // 奖励物品Prefab（比如拼图）

    private string currentInput = ""; // 当前玩家输入的序列
    private bool isSolved = false;

    void Start()
    {
        // 初始化状态
        interactionText = "箱子被魔法锁住了... (需要按顺序激活符文)";
    }

    // 重写交互逻辑：点击箱子本身时的反应
    public override void Interact()
    {
        if (isSolved)
        {
            // 如果已解开，可能什么都不做，或者提示已打开
            if (UIManager.Instance != null)
                UIManager.Instance.ShowInteractionPrompt("箱子已经打开了");
        }
        else
        {
            // 如果没解开，显示提示
            base.Interact();
            if (UIManager.Instance != null)
                UIManager.Instance.ShowInteractionPrompt("需要按正确的符文顺序才能打开！");
        }
    }

    // 接收符文输入的逻辑
    public void ReceiveRuneInput(RuneStone rune)
    {
        if (isSolved) return;

        currentInput += rune.runeID;
        Debug.Log($"当前输入序列: {currentInput}");

        CheckSequence();
    }

    void CheckSequence()
    {
        // 1. 检查当前输入是否匹配密码的前半部分
        // 例如密码是 "123"，输入 "1" -> 匹配；输入 "2" -> 不匹配
        if (correctSequence.StartsWith(currentInput))
        {
            // 输入正确，检查长度是否足够
            if (currentInput == correctSequence)
            {
                StartCoroutine(SolvePuzzle());
            }
        }
        else
        {
            // 输入错误序列
            Debug.Log("顺序错误！重置谜题。");
            StartCoroutine(ResetPuzzleDelay());
        }
    }

    IEnumerator SolvePuzzle()
    {
        isSolved = true;
        interactionText = "箱子已打开";
        Debug.Log("谜题解开！打开箱子...");

        // 播放开箱动画（旋转盖子）
        float timer = 0f;
        Quaternion startRot = lidPivot.localRotation;
        Quaternion targetRot = Quaternion.Euler(0, 0, openAngle);//沿着蓝色轴旋转

        while (timer < 1f)
        {
            timer += Time.deltaTime * 2;
            lidPivot.localRotation = Quaternion.Lerp(startRot, targetRot, timer);
            yield return null;
        }

        // 生成奖励物品
        if (rewardItem != null)
        {
            // 在箱子上方稍微高一点的位置生成
            Instantiate(rewardItem, transform.position + Vector3.up * 1.5f, Quaternion.identity);
            Debug.Log("生成奖励！");
        }

        // 既然解开了，符文就没必要再互动了，保持绿色常亮即可
    }

    IEnumerator ResetPuzzleDelay()
    {
        // 等待一小会儿让玩家意识到按错了（通常配合红色闪烁）
        yield return new WaitForSeconds(0.2f);

        // 让所有符文闪烁红色并重置
        foreach (var rune in runes)
        {
            rune.FlashError();
        }

        // 清空输入
        currentInput = "";
    }
}