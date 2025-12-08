using UnityEngine;

public class SimpleClickRune : Interactable
{
    [Header("符文设置")]
    public SimpleRuneManager manager; // 经理引用
    public Color activeColor = Color.cyan; // 点亮时的颜色
    public Color inactiveColor = Color.gray; // 熄灭时的颜色

    [Header("状态")]
    public bool isActive = false; // 当前是否点亮

    private Renderer myRenderer;
    private bool isLocked = false; // 【新增】内部锁定状态

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        UpdateVisuals();
        UpdatePrompt();
    }

    public override void Interact()
    {
        // 如果被锁定了，虽然显示文字，但点击不执行任何逻辑
        if (isLocked) return;

        // 如果基类说不能交互，也不执行
        if (!canInteract) return;

        // 1. 切换状态
        isActive = !isActive;

        // 2. 更新颜色
        UpdateVisuals();
        UpdatePrompt();

        // 3. 通知经理
        if (manager != null)
        {
            manager.CheckPuzzle();
        }
    }

    void UpdateVisuals()
    {
        if (myRenderer != null)
        {
            myRenderer.material.color = isActive ? activeColor : inactiveColor;
        }
    }

    void UpdatePrompt()
    {
        // 只有没锁定时才更新默认提示，防止覆盖胜利感言
        if (!isLocked)
        {
            interactionText = isActive ? "Blow out the runes" : "Light up the runes";
        }
    }

    // 【修改】锁定符文，并接收胜利感言
    public void LockRune(string victoryMessage)
    {
        isLocked = true; // 内部锁定，禁止点击改变颜色

        // 关键点：保持 canInteract 为 true，这样 InteractionPrompt 才会继续显示文字
        canInteract = true;

        // 把交互提示直接改成胜利感言
        interactionText = victoryMessage;
    }
}