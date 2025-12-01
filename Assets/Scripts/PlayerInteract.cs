using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteract : MonoBehaviour
{
    [Header("交互设置")]
    public float interactDistance = 3f;
    public KeyCode interactKey = KeyCode.E;
    
    [Header("层级设置")] // ← 新增这部分
    public LayerMask interactableLayerMask = 1 << 6; // 默认第6层(Interactable)
    
    [Header("调试设置")]
    public bool showDebugRay = true;
    public Color debugRayColor = Color.red;
    
    private Camera playerCamera;
    
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
        
        // 自动设置LayerMask（如果使用第6层）
        if (interactableLayerMask.value == (1 << 6))
        {
            Debug.Log("使用默认Interactable层 (第6层)");
        }
    }
    
    void Update()
    {
        // 在编辑时持续显示射线
        if (showDebugRay && playerCamera != null)
        {
            Debug.DrawRay(playerCamera.transform.position, 
                         playerCamera.transform.forward * interactDistance, 
                         debugRayColor);
        }
        
        // 检测交互输入
        if (Input.GetKeyDown(interactKey))
        {
            Debug.Log("E键按下，尝试交互");
            TryInteract();
        }
    }
    
    void TryInteract()
    {
        if (playerCamera == null)
        {
            Debug.LogError("PlayerInteract: Camera为空，无法进行交互");
            return;
        }
        
        RaycastHit hit;
        Vector3 rayOrigin = playerCamera.transform.position;
        Vector3 rayDirection = playerCamera.transform.forward;
        
        // 使用在Inspector中设置的interactableLayerMask ← 修改这行
        if (Physics.Raycast(rayOrigin, rayDirection, out hit, interactDistance, interactableLayerMask))
        {
            Debug.Log($"射线击中: {hit.collider.gameObject.name} (距离: {hit.distance:F2})");
            
            Interactable interactable = hit.collider.GetComponent<Interactable>();
            if (interactable == null)
            {
                interactable = hit.collider.GetComponentInParent<Interactable>();
            }
            
            if (interactable != null)
            {
                Debug.Log($"找到Interactable组件: {interactable.gameObject.name}, canInteract = {interactable.canInteract}");
                if (interactable.canInteract)
                {
                    interactable.Interact();
                }
                else
                {
                    Debug.Log($"物体 {interactable.gameObject.name} 当前不可交互");
                }
            }
            else
            {
                Debug.Log($"物体 {hit.collider.gameObject.name} 没有Interactable组件");
            }
        }
        else
        {
            Debug.Log($"射线没有击中任何可交互物体");
        }
    }
    
    // 在编辑器中可视化交互距离
    void OnDrawGizmosSelected()
    {
        if (playerCamera != null && showDebugRay)
        {
            Gizmos.color = debugRayColor;
            Gizmos.DrawRay(playerCamera.transform.position, 
                          playerCamera.transform.forward * interactDistance);
            Gizmos.DrawWireSphere(playerCamera.transform.position + 
                                 playerCamera.transform.forward * interactDistance, 0.1f);
        }
    }
}