using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionManager : MonoBehaviour
{
    [Header("碰撞关系设置")]
    public bool ignorePlayerGuardCollision = true;

    [Header("调试设置")]
    public bool debugMode = true;

    void Start()
    {
        SetupCollisionSystem();
    }

    void SetupCollisionSystem()
    {
        if (debugMode) Debug.Log("=== 开始配置碰撞系统 ===");

        // 1. 设置玩家层级为Player（第7层）
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            player.layer = 7; // Player层
            if (debugMode) Debug.Log($"玩家对象层级设置为: Player (Layer 7)");
        }

        // 2. 设置所有守卫的层级为Guard（第8层）
        GuardAI[] guards = FindObjectsOfType<GuardAI>();
        foreach (GuardAI guard in guards)
        {
            guard.gameObject.layer = 8; // Guard层
        }
        if (debugMode) Debug.Log($"{guards.Length} 个守卫对象层级设置为: Guard (Layer 8)");

        if (debugMode) Debug.Log("=== 碰撞系统配置完成 ===");
    }
}