using UnityEngine;

public class RuneStone : Interactable
{
    [Header("符文属性")]
    public string runeID = "1"; // 符文的编号，例如 "1", "2", "A", "B"
    public RuneChestPuzzle puzzleController; // 引用控制器

    [Header("视觉反馈")]
    public Color activeColor = Color.green; // 激活时的颜色
    public Color errorColor = Color.red;    // 错误时的颜色

    private Renderer myRenderer;
    private Color defaultColor;

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        if (myRenderer != null)
        {
            defaultColor = myRenderer.material.color;
        }

        // 设置基类的交互提示文本
        interactionText = $"Activate Rune [{runeID}]";
    }

    public override void Interact()
    {
        // 如果已经不可交互（比如已经激活了），直接返回
        if (!canInteract) return;

        base.Interact(); // 调用基类打印日志

        // 1. 改变颜色表示激活
        if (myRenderer != null)
        {
            myRenderer.material.color = activeColor;
        }

        // 2. 暂时关闭交互，防止重复点击
        canInteract = false;
        interactionText = ""; // 激活后不再显示提示

        // 3. 通知控制器
        if (puzzleController != null)
        {
            puzzleController.ReceiveRuneInput(this);
        }
        else
        {
            Debug.LogError("Rune not bound to Puzzle Controller!");
        }
    }

    // 重置符文状态（由控制器调用）
    public void ResetRune()
    {
        canInteract = true;
        interactionText = $"Activate Rune [{runeID}]";
        if (myRenderer != null)
        {
            myRenderer.material.color = defaultColor;
        }
    }

    // 显示错误闪烁（由控制器调用）
    public void FlashError()
    {
        if (myRenderer != null)
        {
            myRenderer.material.color = errorColor;
            // 0.5秒后重置回默认状态
            Invoke("ResetRune", 0.5f);
        }
        else
        {
            ResetRune();
        }
    }
}