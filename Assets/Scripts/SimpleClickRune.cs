using UnityEngine;

public class SimpleClickRune : Interactable
{
    [Header("符文设置")]
    public SimpleRuneManager manager; // 经理引用
    public Color activeColor = Color.cyan; // 点亮时的颜色
    public Color inactiveColor = Color.gray; // 熄灭时的颜色

    [Header("灯光组件")]
    public Light runeLight; // 拖入子物体的 Light

    [Header("状态")]
    public bool isActive = false; // 当前是否点亮

    private Renderer myRenderer;
    private bool isLocked = false; // 内部锁定状态

    void Start()
    {
        myRenderer = GetComponent<Renderer>();
        UpdateVisuals(); // 初始化视觉（颜色+灯光）
        UpdatePrompt();  // 初始化提示语
    }

    public override void Interact()
    {
        // 如果被锁定了，不执行逻辑
        if (isLocked) return;

        if (!canInteract) return;

        // 1. 切换状态
        isActive = !isActive;

        // 2. 更新颜色和灯光
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
        // 改变材质颜色
        if (myRenderer != null)
        {
            myRenderer.material.color = isActive ? activeColor : inactiveColor;
        }

        // 【新增】开关灯光
        if (runeLight != null)
        {
            runeLight.enabled = isActive;

            // 可选：让灯光颜色跟符文激活颜色一致
            if (isActive)
            {
                runeLight.color = activeColor;
            }
        }
    }

    void UpdatePrompt()
    {
        if (!isLocked)
        {
            // 英文提示：点亮 / 熄灭
            interactionText = isActive ? "Extinguish" : "Light up";
        }
    }

    // 锁定符文
    public void LockRune(string victoryMessage)
    {
        isLocked = true;
        canInteract = true;
        interactionText = victoryMessage;
    }
}