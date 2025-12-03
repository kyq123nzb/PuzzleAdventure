using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("交互设置")]
    public float interactDistance = 3f;
    public float interactRadius = 1f; // 新增：检测半径
    public KeyCode interactKey = KeyCode.E;
    
    [Header("层级设置")]
    public LayerMask interactableLayerMask = 1 << 6; // 默认第6层(Interactable)
    
    [Header("调试设置")]
    public bool showDebugRay = true;
    public bool showDebugSphere = true; // 新增：显示检测球体
    public Color debugRayColor = Color.red;
    public Color debugSphereColor = Color.blue; // 新增：球体颜色
    
    private Camera playerCamera;
    private Interactable currentInteractable; // 当前可交互物体
    
    void Start()
    {
        // 多种方式获取Camera，确保能找到
        playerCamera = GetComponentInChildren<Camera>();
        
        // 如果没找到，尝试其他方法
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        if (playerCamera == null)
        {
            Debug.LogError("PlayerInteract: 找不到任何Camera！请确保场景中有Camera");
        }
        else
        {
            Debug.Log($"PlayerInteract: 成功找到Camera - {playerCamera.gameObject.name}");
        }
    }
    
    void Update()
    {
        // 持续检测可交互物体
        CheckForInteractables();
        
        // 检测交互输入
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("E键按下，尝试交互");
            TryInteract();
        }
    }
    
    void CheckForInteractables()
    {
        if (playerCamera == null) return;
        
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;
        
        // 使用球形射线检测，检测范围内的所有物体
        RaycastHit[] hits = Physics.SphereCastAll(
            rayOrigin, 
            interactRadius, 
            rayDirection, 
            interactDistance, 
            interactableLayerMask
        );
        
        Interactable closestInteractable = null;
        float closestDistance = float.MaxValue;
        
        // 遍历所有命中的物体，找到最近的可交互物体
        foreach (RaycastHit hit in hits)
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<Interactable>();
            }
            
            if (interactable != null && interactable.canInteract)
            {
                float distance = Vector3.Distance(rayOrigin, hit.point);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestInteractable = interactable;
                }
            }
        }
        
        // 更新当前可交互物体
        currentInteractable = closestInteractable;
        
        // 调试信息已移除（避免日志过多）
    }
    
    void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerInteract: Camera为空，无法进行交互");
            return;
        }
        
        if (currentInteractable != null && currentInteractable.canInteract)
        {
            Debug.Log($"与 {currentInteractable.gameObject.name} 交互");
            currentInteractable.Interact();
        }
        else
        {
            // 调试日志已移除
        }
    }
    
    // 在Scene视图中可视化
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null)
        {
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            Vector3 rayEnd = rayOrigin + rayDirection * interactDistance;
            
            if (showDebugRay)
            {
                // 绘制中心射线
                Gizmos.color = debugRayColor;
                Gizmos.DrawLine(rayOrigin, rayEnd);
            }
            
            if (showDebugSphere)
            {
                // 绘制检测球体
                Gizmos.color = debugSphereColor;
                
                // 在射线起点绘制球体
                Gizmos.DrawWireSphere(rayOrigin, interactRadius);
                
                // 在射线终点绘制球体
                Gizmos.DrawWireSphere(rayEnd, interactRadius);
                
                // 绘制连接两个球体的胶囊形状
                DrawCapsuleGizmo(rayOrigin, rayEnd, interactRadius);
            }
        }
    }
    
    // 绘制胶囊体Gizmo（用于可视化检测范围）
    void DrawCapsuleGizmo(Vector3 start, Vector3 end, float radius)
    {
        // 绘制连接线
        Gizmos.DrawLine(start + Vector3.up * radius, end + Vector3.up * radius);
        Gizmos.DrawLine(start + Vector3.down * radius, end + Vector3.down * radius);
        Gizmos.DrawLine(start + Vector3.left * radius, end + Vector3.left * radius);
        Gizmos.DrawLine(start + Vector3.right * radius, end + Vector3.right * radius);
        
        // 绘制侧面线
        Gizmos.DrawLine(start + Vector3.forward * radius, end + Vector3.forward * radius);
        Gizmos.DrawLine(start + Vector3.back * radius, end + Vector3.back * radius);
    }
    
    // 在Scene视图中持续显示（不只是选中时）
    void OnDrawGizmos()
    {
        if (showDebugSphere && playerCamera != null)
        {
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            Vector3 rayEnd = rayOrigin + rayDirection * interactDistance;
            
            Gizmos.color = new Color(debugSphereColor.r, debugSphereColor.g, debugSphereColor.b, 0.3f);
            DrawCapsuleGizmo(rayOrigin, rayEnd, interactRadius);
        }
    }
}