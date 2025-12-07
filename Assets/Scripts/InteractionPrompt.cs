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
    [Tooltip("检测距离（单位：米）。在这个距离内检测可交互物体（用于OverlapSphere，如果失败会改用FindObjectsOfType）")]
    public float interactionDistance = 15f; // 检测范围：15米（增大以匹配实际游戏场景）
    [Tooltip("最小显示距离（米）。只有在这个距离以上才显示提示，避免太近时显示")]
    public float minDisplayDistance = 0.5f; // 最小0.5米才显示
    [Tooltip("最大显示距离（米）。超过这个距离不显示提示，避免太远时显示")]
    public float maxDisplayDistance = 5f; // 最大5米才显示（靠近时显示）
    [Tooltip("检测角度范围（度）。允许在这个角度范围内检测，不需要完全正对物体")]
    public float detectionAngle = 75f; // 默认 75 度，玩家左右75度范围内才能检测到
    [Tooltip("检测哪些层的物体。设置为 Everything (-1) 表示检测所有层，但建议设置为 Interactable 层 (第6层)")]
    public LayerMask interactableLayerMask = -1; // 默认检测所有层，可以设置为 1 << 6 只检测第6层
    
    [Header("调试设置")]
    public bool enableDebugLog = false; // 启用调试日志
    
    [Header("提示文本设置")]
    public string defaultPromptText = "Press E to Interact";
    public KeyCode interactKey = KeyCode.E;
    
    private Camera playerCamera;
    private float lastCheckTime = 0f;
    private Interactable currentInteractable = null;
    
    void Start()
    {
        // 多种方式查找Camera，确保能找到正确的玩家相机
        // 优先查找标签为"MainCamera"的相机
        GameObject mainCameraObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (mainCameraObj != null)
        {
            playerCamera = mainCameraObj.GetComponent<Camera>();
        }
        
        // 如果没找到，使用Camera.main
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }
        
        // 如果还没找到，查找所有相机，优先选择非UI相机和非编辑器相机
        if (playerCamera == null)
        {
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in allCameras)
            {
                // 跳过UI相机（通常名字包含"UI"或"Canvas"）
                if (cam.name.Contains("UI") || cam.name.Contains("Canvas"))
                    continue;
                // 跳过编辑器相机（名字包含"Editor"或"CINEMA"）
                if (cam.name.Contains("Editor") || cam.name.Contains("CINEMA"))
                    continue;
                // 跳过场景视图相机
                if (cam.name.Contains("Scene") || cam.name.Contains("Preview"))
                    continue;
                playerCamera = cam;
                break;
            }
        }
        
        // 如果还没找到，尝试查找玩家对象下的相机
        if (playerCamera == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerCamera = player.GetComponentInChildren<Camera>();
            }
        }
        
        // 最后尝试：查找任何相机（但排除编辑器相机）
        if (playerCamera == null)
        {
            Camera[] allCameras = FindObjectsOfType<Camera>();
            foreach (Camera cam in allCameras)
            {
                // 排除编辑器相关相机
                if (cam.name.Contains("Editor") || cam.name.Contains("CINEMA") || 
                    cam.name.Contains("Scene") || cam.name.Contains("Preview"))
                    continue;
                playerCamera = cam;
                break;
            }
        }
        
        if (playerCamera == null)
        {
            Debug.LogError("InteractionPrompt: 找不到Camera！请确保场景中有Camera");
        }
        
        // 检查UIManager
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("InteractionPrompt: UIManager.Instance 为空！交互提示可能无法显示");
        }
        
        // 确保游戏开始时提示是隐藏的
        HidePrompt();
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
            bool isPaused = UIManager.Instance.IsGamePaused();
            bool isGameStarted = UIManager.Instance.IsGameStarted();
            
            if (isPaused || !isGameStarted)
            {
                HidePrompt();
                return;
            }
        }
        
        CheckForInteractable();
    }
    
    void CheckForInteractable()
    {
        if (playerCamera == null)
        {
            return;
        }
        
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 forwardDirection = playerCamera.transform.forward;
        
        // 方法1：使用 OverlapSphere 检测范围内的所有可交互物体
        Collider[] colliders = Physics.OverlapSphere(rayOrigin, interactionDistance, interactableLayerMask);
        
        // 方法2：同时使用FindObjectsOfType查找所有Interactable对象（更可靠，作为备用）
        Interactable[] allInteractables = FindObjectsOfType<Interactable>();
        
        Interactable closestInteractable = null;
        float closestDistance = float.MaxValue;
        float closestAngle = float.MaxValue;
        
        // 先检查OverlapSphere找到的碰撞体
        if (colliders.Length > 0)
        {
            foreach (Collider col in colliders)
            {
                Interactable interactable = col.GetComponent<Interactable>();
                if (interactable == null)
                {
                    interactable = col.GetComponentInParent<Interactable>();
                }
                
                if (interactable != null && interactable.canInteract)
                {
                    // 计算距离（使用碰撞体中心）
                    Vector3 directionToObject = (col.bounds.center - rayOrigin);
                    float distance = directionToObject.magnitude;
                    
                    // 计算角度（与玩家朝向的夹角）
                    float angle = Vector3.Angle(forwardDirection, directionToObject.normalized);
                    
                    // 只考虑在角度范围内的物体，距离检查放宽到maxDisplayDistance
                    if (angle <= detectionAngle && distance <= maxDisplayDistance)
                    {
                        if (distance < closestDistance)
                        {
                            closestInteractable = interactable;
                            closestDistance = distance;
                            closestAngle = angle;
                        }
                    }
                }
            }
        }
        
        // 同时检查所有Interactable对象（确保不会漏掉任何对象）
        if (allInteractables != null && allInteractables.Length > 0)
        {
            foreach (Interactable interactable in allInteractables)
            {
                if (interactable == null || !interactable.canInteract || !interactable.gameObject.activeInHierarchy)
                    continue;
                
                // 计算距离：优先使用碰撞体的最近点，如果没有碰撞体则使用物体位置
                Collider col = interactable.GetComponent<Collider>();
                if (col == null) col = interactable.GetComponentInChildren<Collider>();
                
                Vector3 targetPosition;
                float distance;
                if (col != null)
                {
                    // 使用碰撞体的最近点（最准确，计算玩家到碰撞体的最短距离）
                    targetPosition = col.ClosestPoint(rayOrigin);
                    // 如果最近点在碰撞体内部，使用中心点
                    if (targetPosition == rayOrigin)
                    {
                        targetPosition = col.bounds.center;
                    }
                    distance = Vector3.Distance(rayOrigin, targetPosition);
                }
                else
                {
                    // 没有碰撞体，使用物体位置
                    targetPosition = interactable.transform.position;
                    distance = Vector3.Distance(rayOrigin, targetPosition);
                }
                
                Vector3 directionToObject = (targetPosition - rayOrigin);
                
                // 计算角度（与玩家朝向的夹角）
                float angle = Vector3.Angle(forwardDirection, directionToObject.normalized);
                
                // 放宽距离检查：只要在maxDisplayDistance内就考虑（而不是interactionDistance）
                if (distance > maxDisplayDistance)
                    continue;
                
                // 只考虑在角度范围内的物体
                if (angle <= detectionAngle)
                {
                    if (distance < closestDistance)
                    {
                        closestInteractable = interactable;
                        closestDistance = distance;
                        closestAngle = angle;
                    }
                }
            }
        }
        
        // 更新当前可交互物体
        if (closestInteractable != null)
        {
            // 检查是否在显示距离范围内（最小和最大之间）且角度合适
            // 确保 closestAngle 是有效值（不是 float.MaxValue）
            bool distanceOK = closestDistance >= minDisplayDistance && closestDistance <= maxDisplayDistance;
            bool angleOK = closestAngle != float.MaxValue && closestAngle <= detectionAngle;
            
            if (distanceOK && angleOK)
            {
                // 如果找到了新的可交互物体，或者物体变化了，更新提示
                if (closestInteractable != currentInteractable)
                {
                    currentInteractable = closestInteractable;
                    string promptText = string.IsNullOrEmpty(closestInteractable.interactionText) 
                        ? defaultPromptText 
                        : closestInteractable.interactionText;
                    
                    ShowPrompt(promptText);
                }
            }
            else
            {
                // 距离或角度不满足，隐藏提示
                if (currentInteractable != null)
                {
                    currentInteractable = null;
                    HidePrompt();
                }
            }
        }
        else if (currentInteractable != null)
        {
            // 没有找到可交互物体，隐藏提示
            currentInteractable = null;
            HidePrompt();
        }
    }
    
    void ShowPrompt(string customText = null)
    {
        if (UIManager.Instance == null)
        {
            Debug.LogWarning("InteractionPrompt: UIManager.Instance 为空！无法显示提示");
            return;
        }
        
        string promptText = string.IsNullOrEmpty(customText) ? defaultPromptText : customText;
        
        // 替换按键提示
        string keyName = interactKey.ToString();
        promptText = promptText.Replace("E", keyName);
        
        UIManager.Instance.ShowInteractionPrompt(promptText);
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
