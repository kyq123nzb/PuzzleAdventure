using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleItem : Interactable
{
    [Header("拼图设置")]
    public int puzzleId = 1;
    public GameObject visualEffect; // 收集时的视觉效果
    public AudioClip collectSound;  // 收集音效
    
    void Start()
    {
        // 检查是否已经收集过
        if (GameManager.Instance != null && 
            GameManager.Instance.IsPuzzleCollected(puzzleId))
        {
            gameObject.SetActive(false);
            return;
        }
    }
    
    public override void Interact()
    {
        if (!canInteract) return;
        
        base.Interact(); // 调用父类的Interact方法（如果有基础逻辑）
        
        // 收集拼图
        Collect();
    }
    
    void Collect()
    {
        if (!canInteract) return;
        
        Debug.Log($"拾取拼图 {puzzleId}");
        
        // 通知游戏管理器
        if (GameManager.Instance != null)
        {
            GameManager.Instance.CollectPuzzle(puzzleId);
        }
        
        // 播放效果
        PlayCollectEffects();
        
        // 隐藏物体（不立即销毁，等效果播放完）
        StartCoroutine(DisableAfterEffects());
    }
    
    void PlayCollectEffects()
    {
        // 播放音效
        if (collectSound != null)
        {
            AudioSource.PlayClipAtPoint(collectSound, transform.position);
        }
        
        // 播放粒子效果
        if (visualEffect != null)
        {
            Instantiate(visualEffect, transform.position, Quaternion.identity);
        }
        
        // 添加UI提示（可选）
        ShowCollectMessage();
    }
    
    IEnumerator DisableAfterEffects()
    {
        // 等待0.5秒让效果播放完
        yield return new WaitForSeconds(0.5f);
        
        // 禁用交互并隐藏物体
        canInteract = false;
        gameObject.SetActive(false);
        
        // 或者销毁
        // Destroy(gameObject);
    }
    
    void ShowCollectMessage()
    {
        // 这里可以调用UI系统显示"获得拼图！"的提示
        Debug.Log($"获得拼图 {puzzleId}！");
    }
    
    // 重置状态（用于游戏重开）
    public void ResetPuzzle()
    {
        canInteract = true;
        gameObject.SetActive(true);
    }
}