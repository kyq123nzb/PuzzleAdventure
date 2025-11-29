using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleItem : Interactable
{
    [Header("拼图设置")]
    public int puzzleId = 1;
    
    public override void Interact()
    {
        if (!canInteract) return;
        
        Debug.Log($"拾取拼图 {puzzleId}");
        
        // 通知游戏管理器
        GameManager gameManager = FindObjectOfType<GameManager>();
        if (gameManager != null)
        {
            gameManager.CollectPuzzle(puzzleId);
        }
        
        // 播放音效（如果有）
        // 播放粒子效果（如果有）
        
        // 禁用交互并隐藏物体
        canInteract = false;
        gameObject.SetActive(false);
        
        // 或者直接销毁
        // Destroy(gameObject);
    }
}