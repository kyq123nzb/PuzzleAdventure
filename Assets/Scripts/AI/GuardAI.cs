using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    [Header("巡逻设置")]
    public Transform[] patrolPoints;  // 两个巡逻点
    public float moveSpeed = 2f;
    public float waitTimeAtPoint = 2f;

    [Header("玩家检测设置")]
    public float detectionRange = 3f;      // 小范围检测（3米）
    public float catchDistance = 1f;       // 抓住玩家的距离（1米）

    [Header("状态设置")]
    public bool isActive = true;           // 守卫是否活动

    [Header("调试设置")]
    public bool showDebugInfo = true;
    public Color patrolColor = Color.yellow;
    public Color detectionColor = Color.red;

    // 私有变量
    private NavMeshAgent navAgent;
    private Transform playerTransform;
    private PlayerController playerController;

    private int currentTargetIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool hasCaughtPlayer = false;

    // 碰撞体组件
    private CapsuleCollider guardCollider;

    void Start()
    {
        // 初始化组件
        InitializeComponents();

        // 设置巡逻
        SetupPatrol();

        // 设置碰撞体为触发器
        SetupCollider();
    }

    void InitializeComponents()
    {
        // 获取或添加NavMeshAgent
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null)
        {
            navAgent = gameObject.AddComponent<NavMeshAgent>();
        }

        // 配置NavAgent
        navAgent.speed = moveSpeed;
        navAgent.stoppingDistance = 0.1f;
        navAgent.acceleration = 8f;
        navAgent.angularSpeed = 120f;
        navAgent.autoBraking = true;

        // 查找玩家
        playerTransform = FindPlayer();
        if (playerTransform != null)
        {
            playerController = playerTransform.GetComponent<PlayerController>();
        }

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: 初始化完成，使用NavMesh绕开障碍物");
        }
    }

    void SetupCollider()
    {
        // 获取或添加碰撞体
        guardCollider = GetComponent<CapsuleCollider>();
        if (guardCollider == null)
        {
            guardCollider = gameObject.AddComponent<CapsuleCollider>();
            guardCollider.height = 2f;
            guardCollider.radius = 0.5f;
            guardCollider.center = new Vector3(0, 1f, 0);
        }

        // 设置碰撞体为触发器，用于检测玩家接触
        guardCollider.isTrigger = true;

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: 碰撞体已设置为触发器");
        }
    }

    Transform FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            return player.transform;
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: 未找到玩家对象，请确保玩家有'Player'标签");
            return null;
        }
    }

    void SetupPatrol()
    {
        // 检查巡逻点设置
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            Debug.LogError($"{gameObject.name}: 需要至少2个巡逻点！");
            isActive = false;
            return;
        }

        // 检查巡逻点是否为空
        for (int i = 0; i < Mathf.Min(2, patrolPoints.Length); i++)
        {
            if (patrolPoints[i] == null)
            {
                Debug.LogError($"{gameObject.name}: 巡逻点 {i} 为空！");
                isActive = false;
                return;
            }
        }

        // 设置第一个目标
        if (patrolPoints[0] != null)
        {
            navAgent.SetDestination(patrolPoints[0].position);
        }

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: 巡逻已启动，巡逻点: {patrolPoints.Length}");
        }
    }

    void Update()
    {
        // 如果守卫不活动或已抓住玩家，不更新
        if (!isActive || hasCaughtPlayer) return;

        // 更新巡逻
        UpdatePatrol();

        // 检测玩家（小范围检测）
        DetectPlayer();
    }

    void UpdatePatrol()
    {
        // 如果正在等待，不移动
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                waitTimer = 0f;

                // 切换到下一个巡逻点
                currentTargetIndex = (currentTargetIndex + 1) % Mathf.Min(2, patrolPoints.Length);
                MoveToNextPoint();
            }
            return;
        }

        // 检查是否到达目标点
        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
            {
                // 到达巡逻点，开始等待
                isWaiting = true;
                waitTimer = 0f;

                if (showDebugInfo)
                {
                    Debug.Log($"{gameObject.name}: 到达巡逻点 {currentTargetIndex}，等待 {waitTimeAtPoint} 秒");
                }
            }
        }
    }

    void MoveToNextPoint()
    {
        if (patrolPoints[currentTargetIndex] != null)
        {
            navAgent.SetDestination(patrolPoints[currentTargetIndex].position);

            if (showDebugInfo)
            {
                Debug.Log($"{gameObject.name}: 移动到巡逻点 {currentTargetIndex}");
            }
        }
    }

    void DetectPlayer()
    {
        // 如果玩家不存在，跳过检测
        if (playerTransform == null) return;

        // 计算与玩家的距离
        float distanceToPlayer = Vector3.Distance(transform.position, playerTransform.position);

        // 小范围检测（3米内）
        if (distanceToPlayer <= detectionRange)
        {
            // 检查是否有视线
            Vector3 directionToPlayer = (playerTransform.position - transform.position).normalized;
            RaycastHit hit;

            if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer, out hit, detectionRange))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    // 检测到玩家，开始追逐
                    StartChasing();
                }
            }
        }
    }

    void StartChasing()
    {
        // 追逐玩家
        if (playerTransform != null)
        {
            navAgent.SetDestination(playerTransform.position);
        }
    }

    // ========== 触发器检测：玩家碰到守卫 ==========
    void OnTriggerEnter(Collider other)
    {
        if (hasCaughtPlayer) return;

        // 确保检测到的是玩家
        if (other.CompareTag("Player"))
        {
            Debug.Log($"<color=red>【接触检测】</color> {gameObject.name} 碰到了 {other.name}");
            PlayerCaught();
        }
    }

    void PlayerCaught()
    {
        if (hasCaughtPlayer) return;

        hasCaughtPlayer = true;

        // 1. 停止守卫逻辑
        if (navAgent != null)
        {
            navAgent.isStopped = true; // 停止寻路
            navAgent.velocity = Vector3.zero; // 立即归零速度
            navAgent.ResetPath(); // 清除路径
        }

        // 2. 停止玩家逻辑
        if (playerController != null)
        {
            playerController.enabled = false; // 禁用玩家控制脚本
            Debug.Log("玩家控制器已禁用");
        }

        // 3. 显示调试信息
        Debug.Log("======================");
        Debug.Log($"<color=red>⚠️ 玩家被抓住了！</color>");
        Debug.Log($"抓捕者: {gameObject.name}");
        Debug.Log("======================");

        // 4. 【新增】通知游戏管理器 (如果有的话)
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDetectedByGuard();
        }
    }

    // 调试可视化
    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        // 绘制巡逻点和连线
        Gizmos.color = patrolColor;
        if (patrolPoints != null && patrolPoints.Length >= 2)
        {
            for (int i = 0; i < Mathf.Min(patrolPoints.Length, 2); i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.3f);

                    // 绘制两点之间的连线
                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    }
                    else if (patrolPoints.Length > 1 && patrolPoints[0] != null)
                    {
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                    }

                    // 绘制点标签
#if UNITY_EDITOR
                    UnityEditor.Handles.Label(patrolPoints[i].position + Vector3.up * 0.5f, $"巡逻点 {i}");
#endif
                }
            }
        }

        // 绘制检测范围（小范围）
        Gizmos.color = detectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // 绘制触发器范围
        Gizmos.color = Color.magenta;
        if (guardCollider != null)
        {
            Vector3 center = transform.position + guardCollider.center;
            Gizmos.DrawWireSphere(center, guardCollider.radius);
        }

        // 绘制NavMeshAgent路径
        if (navAgent != null && navAgent.hasPath)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < navAgent.path.corners.Length - 1; i++)
            {
                Gizmos.DrawLine(navAgent.path.corners[i], navAgent.path.corners[i + 1]);
            }
        }
    }

    // 公共方法：重置守卫
    public void ResetGuard()
    {
        hasCaughtPlayer = false;
        isActive = true;

        if (navAgent != null)
        {
            navAgent.isStopped = false;
        }

        // 重置到第一个巡逻点
        currentTargetIndex = 0;
        isWaiting = false;
        waitTimer = 0f;

        if (patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            navAgent.SetDestination(patrolPoints[0].position);
        }

        if (showDebugInfo)
        {
            Debug.Log($"{gameObject.name}: 已重置");
        }
    }
}