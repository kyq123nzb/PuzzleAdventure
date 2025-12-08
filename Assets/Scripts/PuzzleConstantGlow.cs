using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleConstantGlow : MonoBehaviour
{
    [Header("持续发光设置")]
    [ColorUsage(false, true)] // 启用HDR
    public Color glowColor = Color.yellow;
    
    [Range(0.1f, 5f)]
    public float glowIntensity = 2f; // 发光强度
    
    [Range(0.1f, 2f)]
    public float pulseSpeed = 1f; // 脉动速度（0=不脉动）
    
    [Range(0, 0.5f)]
    public float pulseAmount = 0.2f; // 脉动幅度
    
    private Material material;
    private Renderer objectRenderer;
    private float timeCounter = 0f;
    
    void Start()
    {
        // 获取组件
        objectRenderer = GetComponent<Renderer>();
        if (objectRenderer == null)
        {
            Debug.LogError($"拼图 {gameObject.name} 没有Renderer组件！");
            return;
        }
        
        // 使用材质实例，避免影响其他物体
        material = objectRenderer.material;
        
        // 确保启用自发光
        material.EnableKeyword("_EMISSION");
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        
        // 初始发光
        UpdateGlow();
        
        Debug.Log($"拼图 {gameObject.name} 开始持续发光");
    }
    
    void Update()
    {
        // 如果需要脉动效果，则每帧更新
        if (pulseSpeed > 0.01f)
        {
            timeCounter += Time.deltaTime * pulseSpeed;
            UpdateGlow();
        }
    }
    
    void UpdateGlow()
    {
        if (material == null || objectRenderer == null) return;

        // 脉动
        float pulse = 1f;
        if (pulseAmount > 0)
            pulse = 1f + Mathf.Sin(timeCounter) * pulseAmount;

        // 获取主贴图颜色（非常重要！）
        Color baseColor = material.GetColor("_Color");

        // 发光 = 原贴图颜色 × 发光颜色 × 强度
        Color finalEmission =
            baseColor *
            glowColor *
            (glowIntensity * pulse);

        material.SetColor("_EmissionColor", finalEmission);

        DynamicGI.SetEmissive(objectRenderer, finalEmission);
    }

    
    void OnDestroy()
    {
        // 清理：如果使用材质实例，销毁它
        if (material != null && objectRenderer != null)
        {
            // 如果是材质实例，销毁它
            if (material != objectRenderer.sharedMaterial)
            {
                Destroy(material);
            }
        }
    }
    
    // 可选：在编辑器中也预览效果
    #if UNITY_EDITOR
    void OnValidate()
    {
        if (Application.isPlaying && material != null)
        {
            UpdateGlow();
        }
    }
    #endif
}