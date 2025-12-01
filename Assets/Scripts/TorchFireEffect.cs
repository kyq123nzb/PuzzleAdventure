using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 火炬火焰效果 - 管理火炬的粒子效果和光照
/// </summary>
public class TorchFireEffect : MonoBehaviour
{
    [Header("粒子效果")]
    public ParticleSystem fireParticles;
    public ParticleSystem smokeParticles;
    public ParticleSystem sparksParticles;
    
    [Header("光源设置")]
    public Light torchLight;
    public float lightIntensityMin = 0.8f;
    public float lightIntensityMax = 1.5f;
    public float lightFlickerSpeed = 5f;
    public Color lightColor = new Color(1f, 0.6f, 0.2f, 1f);
    public float lightRange = 8f;
    
    [Header("动画设置")]
    public bool useLightFlickering = true;
    public bool useParticleVariation = true;
    public float variationSpeed = 2f;
    
    [Header("自动创建")]
    public bool autoCreateParticles = true;
    public bool autoCreateLight = true;
    
    private float baseIntensity;
    private float flickerTimer = 0f;
    private ParticleSystem.MainModule fireMain;
    private ParticleSystem.MainModule smokeMain;
    private ParticleSystem.MainModule sparksMain;
    
    void Start()
    {
        InitializeComponents();
        SetupParticles();
        SetupLight();
    }
    
    void InitializeComponents()
    {
        // 自动创建粒子系统
        if (autoCreateParticles)
        {
            if (fireParticles == null)
            {
                GameObject fireObj = new GameObject("Fire Particles");
                fireObj.transform.SetParent(transform);
                fireObj.transform.localPosition = Vector3.zero;
                fireParticles = fireObj.AddComponent<ParticleSystem>();
            }
            
            if (smokeParticles == null)
            {
                GameObject smokeObj = new GameObject("Smoke Particles");
                smokeObj.transform.SetParent(transform);
                smokeObj.transform.localPosition = Vector3.up * 0.3f;
                smokeParticles = smokeObj.AddComponent<ParticleSystem>();
            }
            
            if (sparksParticles == null)
            {
                GameObject sparksObj = new GameObject("Sparks Particles");
                sparksObj.transform.SetParent(transform);
                sparksObj.transform.localPosition = Vector3.zero;
                sparksParticles = sparksObj.AddComponent<ParticleSystem>();
            }
        }
        
        // 自动创建光源
        if (autoCreateLight && torchLight == null)
        {
            GameObject lightObj = new GameObject("Torch Light");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.up * 0.5f;
            torchLight = lightObj.AddComponent<Light>();
        }
    }
    
    void SetupParticles()
    {
        // 设置火焰粒子
        if (fireParticles != null)
        {
            var main = fireParticles.main;
            main.startLifetime = 1.5f;
            main.startSpeed = 2f;
            main.startSize = 0.3f;
            main.startColor = new Color(1f, 0.5f, 0f, 1f);
            main.maxParticles = 50;
            main.startRotation3D = true;
            
            var emission = fireParticles.emission;
            emission.rateOverTime = 30f;
            
            var shape = fireParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 25f;
            shape.radius = 0.1f;
            
            var velocity = fireParticles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.y = new ParticleSystem.MinMaxCurve(2f, 4f);
            
            var colorOverLifetime = fireParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(1f, 0.8f, 0.2f), 0f),
                    new GradientColorKey(new Color(1f, 0.4f, 0f), 0.5f),
                    new GradientColorKey(new Color(0.5f, 0.1f, 0f), 1f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(1f, 0f),
                    new GradientAlphaKey(0.8f, 0.7f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;
            
            fireMain = main;
        }
        
        // 设置烟雾粒子
        if (smokeParticles != null)
        {
            var main = smokeParticles.main;
            main.startLifetime = 3f;
            main.startSpeed = 0.5f;
            main.startSize = 0.5f;
            main.startColor = new Color(0.2f, 0.2f, 0.2f, 0.6f);
            main.maxParticles = 20;
            
            var emission = smokeParticles.emission;
            emission.rateOverTime = 5f;
            
            var shape = smokeParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 15f;
            shape.radius = 0.1f;
            
            var velocity = smokeParticles.velocityOverLifetime;
            velocity.enabled = true;
            velocity.space = ParticleSystemSimulationSpace.Local;
            velocity.y = new ParticleSystem.MinMaxCurve(1f, 2f);
            
            var colorOverLifetime = smokeParticles.colorOverLifetime;
            colorOverLifetime.enabled = true;
            Gradient gradient = new Gradient();
            gradient.SetKeys(
                new GradientColorKey[] { 
                    new GradientColorKey(new Color(0.3f, 0.3f, 0.3f), 0f),
                    new GradientColorKey(new Color(0.2f, 0.2f, 0.2f), 1f)
                },
                new GradientAlphaKey[] { 
                    new GradientAlphaKey(0.6f, 0f),
                    new GradientAlphaKey(0f, 1f)
                }
            );
            colorOverLifetime.color = gradient;
            
            smokeMain = main;
        }
        
        // 设置火花粒子
        if (sparksParticles != null)
        {
            var main = sparksParticles.main;
            main.startLifetime = 0.5f;
            main.startSpeed = 3f;
            main.startSize = 0.1f;
            main.startColor = new Color(1f, 0.9f, 0.3f, 1f);
            main.maxParticles = 10;
            
            var emission = sparksParticles.emission;
            emission.rateOverTime = 5f;
            emission.SetBursts(new ParticleSystem.Burst[] {
                new ParticleSystem.Burst(0f, 2, 5, 1, 0.01f)
            });
            
            var shape = sparksParticles.shape;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 30f;
            shape.radius = 0.05f;
            
            sparksMain = main;
        }
    }
    
    void SetupLight()
    {
        if (torchLight != null)
        {
            torchLight.type = LightType.Point;
            torchLight.color = lightColor;
            torchLight.range = lightRange;
            torchLight.shadows = LightShadows.Soft;
            torchLight.shadowStrength = 0.8f;
            baseIntensity = (lightIntensityMin + lightIntensityMax) / 2f;
            torchLight.intensity = baseIntensity;
        }
    }
    
    void Update()
    {
        // 灯光闪烁效果
        if (useLightFlickering && torchLight != null)
        {
            flickerTimer += Time.deltaTime * lightFlickerSpeed;
            float flicker = Mathf.PerlinNoise(flickerTimer, flickerTimer * 0.5f);
            torchLight.intensity = Mathf.Lerp(lightIntensityMin, lightIntensityMax, flicker);
            
            // 轻微的灯光颜色变化
            float colorVariation = Mathf.Sin(flickerTimer * 0.5f) * 0.1f;
            torchLight.color = new Color(
                Mathf.Clamp01(lightColor.r + colorVariation),
                Mathf.Clamp01(lightColor.g + colorVariation * 0.5f),
                Mathf.Clamp01(lightColor.b - colorVariation * 0.5f),
                1f
            );
        }
        
        // 粒子系统变化
        if (useParticleVariation && fireParticles != null)
        {
            float variation = Mathf.Sin(Time.time * variationSpeed) * 0.2f + 1f;
            
            var emission = fireParticles.emission;
            emission.rateOverTime = 30f * variation;
            
            fireMain.startSize = 0.3f * variation;
        }
    }
    
    public void SetActive(bool active)
    {
        if (fireParticles != null)
        {
            if (active) fireParticles.Play();
            else fireParticles.Stop();
        }
        
        if (smokeParticles != null)
        {
            if (active) smokeParticles.Play();
            else smokeParticles.Stop();
        }
        
        if (sparksParticles != null)
        {
            if (active) sparksParticles.Play();
            else sparksParticles.Stop();
        }
        
        if (torchLight != null)
        {
            torchLight.enabled = active;
        }
    }
}
