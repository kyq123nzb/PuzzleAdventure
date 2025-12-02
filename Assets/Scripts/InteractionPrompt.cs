using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 交互提示系统 - 显示"按E拾取"等交互提示
/// </summary>
public class InteractionPrompt : MonoBehaviour
{
    [Header("交互提示设置")]
    public float checkInterval = 0.1f; // 检查间隔，减少性能开销
    [Tooltip("检测距离（单位：米）。建议设置为 5-8 米，这样玩家不需要太靠近就能看到提示")]
    public float interactionDistance = 5f; // 增加到 5 米，更容易检测到
    [Tooltip("检测角度范围（度）。允许在这个角度范围内检测，不需要完全正对物体")]
    public float detectionAngle = 60f; // 允许 60 度范围内的检测
    [Tooltip("检测哪些层的物体。设置为 Everything (-1) 表示检测所有层，但建议设置为 Interactable 层 (第6层)")]
    public LayerMask interactableLayerMask = -1; // 默认检测所有层，可以设置为 1 << 6 只检测第6层
    
    [Header("提示文本设置")]
    public string defaultPromptText = "按 E 拾取";
    public KeyCode interactKey = KeyCode.E;
    
    private Camera playerCamera;
    private float lastCheckTime = 0f;
    private Interactable currentInteractable = null;
    
    void Start()
    {
        // 多种方式查找Camera，确保能找到
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        if (playerCamera == null)
        {
            Debug.LogError("InteractionPrompt: 找不到Camera！请确保场景中有Camera");
        }
        else
        {
            Debug.Log($"InteractionPrompt: 成功找到Camera - {playerCamera.gameObject.name}");
        }
        
        // 如果使用默认的 -1（所有层），输出提示
        if (interactableLayerMask.value == -1)
        {
            Debug.Log("InteractionPrompt: 当前检测所有层，如果性能有问题，建议设置为特定层（如第6层 Interactable）");
        }
    }
    
    void Update()
    {
        // 降低检查频率，减少性能开销
        if (Time.time - lastCheckTime < checkInterval)
            return;
        
        lastCheckTime = Time.time;
        
        if (playerCamera == null) return;
        
        // 如果游戏暂停或未开始，不显示提示
        if (UIManager.Instance != null)
        {
            if (UIManager.Instance.IsGamePaused() || !UIManager.Instance.IsGameStarted())
            {
                HidePrompt();
                return;
            }
        }
        
        CheckForInteractable();
    }
    
    void CheckForInteractable()
    {
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 forwardDirection = playerCamera.transform.forward;
        
        // 使用 OverlapSphere 检测范围内的所有可交互物体
        Collider[] colliders = Physics.OverlapSphere(rayOrigin, interactionDistance, interactableLayerMask);
        
        Interactable closestInteractable = null;
        float closestDistance = float.MaxValue;
        float closestAngle = float.MaxValue;
        
        // 遍历所有检测到的碰撞体
        foreach (Collider col in colliders)
        {
            Interactable interactable = col.GetComponent<Interactable>();
            if (interactable == null)
            {
                interactable = col.GetComponentInParent<Interactable>();
            }
            
            if (interactable != null && interactable.canInteract)
            {
                // 计算距离
                Vector3 directionToObject = (col.bounds.center - rayOrigin);
                float distance = directionToObject.magnitude;
                
                // 计算角度（与玩家朝向的夹角）
                float angle = Vector3.Angle(forwardDirection, directionToObject.normalized);
                
                // 只考虑在角度范围内的物体
                if (angle <= detectionAngle)
                {
                    // 再做一次射线检测，确保中间没有障碍物
                    RaycastHit hit;
                    if (Physics.Raycast(rayOrigin, directionToObject.normalized, out hit, distance, interactableLayerMask))
                    {
                        if (hit.collider == col || hit.collider.transform.IsChildOf(interactable.transform))
                        {
                            // 选择最近的物体（优先选择角度更小的）
                            if (angle < closestAngle || (Mathf.Abs(angle - closestAngle) < 5f && distance < closestDistance))
                            {
                                closestInteractable = interactable;
                                closestDistance = distance;
                                closestAngle = angle;
                            }
                        }
                    }
                }
            }
        }
        
        // 更新当前可交互物体
        if (closestInteractable != null && closestInteractable != currentInteractable)
        {
            currentInteractable = closestInteractable;
            string promptText = string.IsNullOrEmpty(closestInteractable.interactionText) 
                ? defaultPromptText 
                : closestInteractable.interactionText;
            ShowPrompt(promptText);
            // 调试日志已移除（避免日志过多）
        }
        else if (closestInteractable == null && currentInteractable != null)
        {
            // 没有找到可交互物体
            currentInteractable = null;
            HidePrompt();
        }
    }
    
    void ShowPrompt(string customText = null)
    {
        if (UIManager.Instance != null)
        {
            string promptText = string.IsNullOrEmpty(customText) ? defaultPromptText : customText;
            
            // 替换按键提示
            string keyName = interactKey.ToString();
            promptText = promptText.Replace("E", keyName);
            
            UIManager.Instance.ShowInteractionPrompt(promptText);
        }
    }
    
    void HidePrompt()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideInteractionPrompt();
        }
        currentInteractable = null;
    }
    
    void OnDrawGizmosSelected()
    {
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        if (playerCamera != null)
        {
            Gizmos.color = Color.green;
            Vector3 rayOrigin = playerCamera.transform.position;
            Vector3 rayDirection = playerCamera.transform.forward;
            Gizmos.DrawRay(rayOrigin, rayDirection * interactionDistance);
            Gizmos.DrawWireSphere(rayOrigin + rayDirection * interactionDistance, 0.2f);
        }
    }
}
