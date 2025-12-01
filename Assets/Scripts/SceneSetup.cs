using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 场景设置辅助脚本 - 用于自动化场景的灯光和碰撞体设置
/// </summary>
public class SceneSetup : MonoBehaviour
{
    [Header("灯光设置")]
    public bool setupLighting = true;
    public Color ambientColor = new Color(0.2f, 0.2f, 0.25f, 1f);
    public float ambientIntensity = 0.3f;
    public bool useFog = true;
    public Color fogColor = new Color(0.1f, 0.1f, 0.15f, 1f);
    public FogMode fogMode = FogMode.ExponentialSquared;
    public float fogDensity = 0.01f;
    
    [Header("主光源设置")]
    public bool setupMainLight = true;
    public Light mainLight;
    public Color mainLightColor = new Color(1f, 0.9f, 0.7f, 1f);
    public float mainLightIntensity = 1.2f;
    public LightType mainLightType = LightType.Directional;
    
    [Header("碰撞体设置")]
    public bool autoAddColliders = false;
    public LayerMask groundLayer = 8; // 默认第8层为Ground
    
    [Header("后处理提示")]
    public bool showPostProcessingTip = true;
    
    void Start()
    {
        if (setupLighting)
        {
            SetupLighting();
        }
        
        if (setupMainLight)
        {
            SetupMainLight();
        }
        
        if (autoAddColliders)
        {
            SetupColliders();
        }
        
        if (showPostProcessingTip)
        {
            Debug.Log("提示: 可以添加Post Processing Volume来增强视觉效果！");
        }
    }
    
    void SetupLighting()
    {
        // 设置环境光
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = ambientColor;
        RenderSettings.ambientEquatorColor = ambientColor * 0.7f;
        RenderSettings.ambientGroundColor = ambientColor * 0.4f;
        RenderSettings.ambientIntensity = ambientIntensity;
        
        // 设置雾效
        RenderSettings.fog = useFog;
        if (useFog)
        {
            RenderSettings.fogColor = fogColor;
            RenderSettings.fogMode = fogMode;
            RenderSettings.fogDensity = fogDensity;
        }
        
        Debug.Log("场景灯光设置完成！");
    }
    
    void SetupMainLight()
    {
        // 如果没有指定主光源，尝试找到场景中的Directional Light
        if (mainLight == null)
        {
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    mainLight = light;
                    break;
                }
            }
        }
        
        // 如果还没有找到，创建一个新的主光源
        if (mainLight == null)
        {
            GameObject lightObj = new GameObject("Main Light");
            mainLight = lightObj.AddComponent<Light>();
        }
        
        // 配置主光源
        mainLight.type = mainLightType;
        mainLight.color = mainLightColor;
        mainLight.intensity = mainLightIntensity;
        mainLight.shadows = LightShadows.Soft;
        
        // 设置旋转（如果是方向光）
        if (mainLightType == LightType.Directional)
        {
            mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }
        
        Debug.Log($"主光源设置完成: {mainLight.gameObject.name}");
    }
    
    void SetupColliders()
    {
        // 这个方法需要在场景中手动添加，因为需要知道哪些物体需要碰撞体
        Debug.Log("提示: 自动添加碰撞体功能需要在特定场景中配置！");
    }
    
    // 静态方法：快速设置地牢场景灯光
    public static void SetupDungeonLighting()
    {
        // 环境光
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.15f, 0.15f, 0.2f, 1f);
        RenderSettings.ambientEquatorColor = new Color(0.1f, 0.1f, 0.15f, 1f);
        RenderSettings.ambientGroundColor = new Color(0.05f, 0.05f, 0.1f, 1f);
        RenderSettings.ambientIntensity = 0.25f;
        
        // 雾效
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.08f, 0.08f, 0.12f, 1f);
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogDensity = 0.015f;
        
        Debug.Log("地牢场景灯光设置完成！");
    }
    
    // 在编辑器中可以调用此方法
    [ContextMenu("应用地牢场景设置")]
    void ApplyDungeonSettings()
    {
        SetupDungeonLighting();
        SetupMainLight();
    }
}
