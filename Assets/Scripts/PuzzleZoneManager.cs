using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuzzleZoneManager : MonoBehaviour
{
    [System.Serializable]
    public class PuzzleZone
    {
        public string zoneName;
        public Transform zoneTransform;
        public int puzzlesInZone = 3;
        public List<int> puzzleIds = new List<int>();
        public bool isComplete = false;
    }
    
    [Header("区域设置")]
    public List<PuzzleZone> puzzleZones = new List<PuzzleZone>();
    
    [Header("区域完成奖励")]
    public GameObject zoneCompleteEffect;
    public AudioClip zoneCompleteSound;
    
    void Start()
    {
        // 初始化区域
        InitializeZones();
    }
    
    void InitializeZones()
    {
        // 这里可以根据场景自动检测拼图区域
        // 或者手动在Inspector中设置
    }
    
    void Update()
    {
        CheckZoneCompletion();
    }
    
    void CheckZoneCompletion()
    {
        foreach (PuzzleZone zone in puzzleZones)
        {
            if (!zone.isComplete)
            {
                int collectedInZone = 0;
                
                foreach (int puzzleId in zone.puzzleIds)
                {
                    if (GameManager.Instance != null && 
                        GameManager.Instance.IsPuzzleCollected(puzzleId))
                    {
                        collectedInZone++;
                    }
                }
                
                if (collectedInZone >= zone.puzzlesInZone)
                {
                    ZoneComplete(zone);
                }
            }
        }
    }
    
    void ZoneComplete(PuzzleZone zone)
    {
        zone.isComplete = true;
        Debug.Log($"区域 '{zone.zoneName}' 完成！");
        
        // 播放效果
        if (zoneCompleteEffect != null && zone.zoneTransform != null)
        {
            Instantiate(zoneCompleteEffect, zone.zoneTransform.position, Quaternion.identity);
        }
        
        if (zoneCompleteSound != null)
        {
            AudioSource.PlayClipAtPoint(zoneCompleteSound, zone.zoneTransform.position);
        }
        
        // 触发区域完成事件
        OnZoneCompleted(zone);
    }
    
    void OnZoneCompleted(PuzzleZone zone)
    {
        // 这里可以触发区域完成后的特殊事件
        // 比如：打开隐藏的门、解锁新区域、给予玩家奖励等
        
        // 示例：激活区域内的特殊物体
        ActivateSpecialObjects(zone);
    }
    
    void ActivateSpecialObjects(PuzzleZone zone)
    {
        // 激活区域内的特殊物体
        SpecialObject[] specialObjects = zone.zoneTransform.GetComponentsInChildren<SpecialObject>();
        foreach (SpecialObject obj in specialObjects)
        {
            obj.Activate();
        }
    }
    
    // 获取区域进度
    public float GetZoneProgress(string zoneName)
    {
        PuzzleZone zone = puzzleZones.Find(z => z.zoneName == zoneName);
        if (zone != null)
        {
            int collected = 0;
            foreach (int puzzleId in zone.puzzleIds)
            {
                if (GameManager.Instance != null && 
                    GameManager.Instance.IsPuzzleCollected(puzzleId))
                {
                    collected++;
                }
            }
            return (float)collected / zone.puzzlesInZone;
        }
        return 0f;
    }
}