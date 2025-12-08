using UnityEngine;

public class PuzzleHintObject : Interactable
{
    [Header("Hint Content")]
    [TextArea]
    // 这是按下 E 后显示的线索（谜语）
    public string hintContent = "Communicate with me using the correct runes.";

    [Header("Initial Prompt")]
    // 这是平时显示的交互提示
    public string defaultPrompt = "Press E to View Hint";

    private bool isShowingHint = false;

    void Start()
    {
        // 游戏开始时，显示“按E查看”
        interactionText = defaultPrompt;
    }

    public override void Interact()
    {
        // 如果不能交互（比如被禁用了），直接返回
        if (!canInteract) return;

        // 切换状态：显示线索 <-> 显示默认提示
        isShowingHint = !isShowingHint;

        if (isShowingHint)
        {
            // 【关键修改】切换到线索文本
            interactionText = hintContent;
        }
        else
        {
            // 【关键修改】切回默认提示
            interactionText = defaultPrompt;
        }

        // 立即刷新 UI，让玩家马上看到变化
        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowInteractionPrompt(interactionText);
        }
    }

    // 当物体被隐藏或禁用时重置
    void OnDisable()
    {
        isShowingHint = false;
        interactionText = defaultPrompt;
    }
}