using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Interactable : MonoBehaviour
{
    [Header("交互设置")]
    public string interactionText = "按 E 拾取";
    public bool canInteract = true;
    
    public virtual void Interact()
    {
        Debug.Log($"与 {gameObject.name} 交互");
        // 基础交互逻辑，在子类中重写
    }
    
    // 在编辑器中显示交互范围
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}