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
    public float interactionDistance = 3f;
    public LayerMask interactableLayerMask = -1;
    
    [Header("提示文本设置")]
    public string defaultPromptText = "按 E 拾取";
    public KeyCode interactKey = KeyCode.E;
    
    private Camera playerCamera;
    private float lastCheckTime = 0f;
    private Interactable currentInteractable = null;
    
    void Start()
    {
        playerCamera = Camera.main;
        if (playerCamera == null)
        {
            playerCamera = FindObjectOfType<Camera>();
        }
        
        if (playerCamera == null)
        {
            Debug.LogError("InteractionPrompt: 找不到Camera！");
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
        RaycastHit hit;
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;
        
        // 使用射线检测可交互物体
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, interactionDistance, interactableLayerMask))
        {
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<Interactable>();
            }
            
            if (interactable != null && interactable.canInteract)
            {
                // 找到可交互物体
                if (currentInteractable != interactable)
                {
                    currentInteractable = interactable;
                    ShowPrompt(interactable.interactionText);
                }
                return;
            }
        }
        
        // 没有找到可交互物体
        if (currentInteractable != null)
        {
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
