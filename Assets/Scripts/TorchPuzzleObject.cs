using UnityEngine;

public class TorchPuzzleObject : Interactable
{
    [Header("火把设置")]
    public int torchID; // 火把的编号（例如 1, 2, 3... 6）
    public TorchPuzzleManager puzzleManager; // 引用管理器
    
    [Header("视觉组件")]
    public TorchFireEffect fireEffect; // 引用你之前的火焰特效脚本
    
    [Header("状态")]
    public bool isLit = false;

    void Start()
    {
        // 初始交互提示
        interactionText = $"Light the torch {torchID}";
        
        // 确保初始状态是灭的
        if (fireEffect != null)
        {
            fireEffect.SetActive(isLit);
        }
    }

    public override void Interact()
    {
        // 如果已经点燃了，或者谜题已解开，就不再响应
        if (isLit || !canInteract) return;

        base.Interact();

        // 1. 点燃自己
        SetState(true);

        // 2. 通知管理器检查顺序
        if (puzzleManager != null)
        {
            puzzleManager.OnTorchIgnited(this);
        }
    }

    public void SetState(bool lit)
    {
        isLit = lit;
        
        // 控制特效开关
        if (fireEffect != null)
        {
            fireEffect.SetActive(lit);
        }

        // 更新交互状态
        if (isLit)
        {
            interactionText = ""; // 点燃后不显示文字
            canInteract = false;  // 暂时不可再交互
        }
        else
        {
            interactionText = $"Light the torch {torchID}";
            canInteract = true;
        }
    }
}