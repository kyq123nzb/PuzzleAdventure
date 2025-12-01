using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialObject : MonoBehaviour
{
    [Header("特殊物体设置")]
    public bool isActivated = false;
    public GameObject activationEffect; // 激活时的粒子效果
    public AudioClip activationSound;   // 激活音效
    
    [Header("激活后的变化")]
    public GameObject objectToActivate; // 要激活的物体（如门、机关等）
    public bool enableOnActivate = true; // 激活后启用还是禁用
    public float activationDelay = 0f;  // 激活延迟
    
    void Start()
    {
        // 初始化状态
        if (isActivated)
        {
            OnActivated();
        }
    }
    
    public void Activate()
    {
        if (isActivated) return;
        
        isActivated = true;
        Debug.Log($"特殊物体激活: {gameObject.name}");
        
        // 播放激活效果
        StartCoroutine(ActivationSequence());
    }
    
    IEnumerator ActivationSequence()
    {
        // 等待延迟
        if (activationDelay > 0)
        {
            yield return new WaitForSeconds(activationDelay);
        }
        
        // 播放音效
        if (activationSound != null)
        {
            AudioSource.PlayClipAtPoint(activationSound, transform.position);
        }
        
        // 播放粒子效果
        if (activationEffect != null)
        {
            Instantiate(activationEffect, transform.position, Quaternion.identity);
        }
        
        // 激活关联物体
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(enableOnActivate);
        }
        
        // 触发激活完成事件
        OnActivated();
    }
    
    protected virtual void OnActivated()
    {
        // 可以被子类重写，实现特定逻辑
        // 例如：改变颜色、播放动画等
        
        // 示例：改变材质颜色
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.green;
        }
    }
    
    public void ResetActivation()
    {
        isActivated = false;
        
        // 恢复默认状态
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.white;
        }
        
        // 重置关联物体
        if (objectToActivate != null)
        {
            objectToActivate.SetActive(!enableOnActivate);
        }
    }
}