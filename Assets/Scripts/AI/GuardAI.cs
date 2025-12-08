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

    [Header("伤害设置")]
    public float damageCooldown = 2.0f;    // 伤害冷却时间（防止一瞬间扣光血）

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

    // 伤害控制
    private bool canDealDamage = true;     // 是否可以造成伤害

    // 碰撞体组件
    private CapsuleCollider guardCollider;

    void Start()
    {
        InitializeComponents();
        SetupPatrol();
        SetupCollider();
    }

    void InitializeComponents()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null) navAgent = gameObject.AddComponent<NavMeshAgent>();

        navAgent.speed = moveSpeed;
        navAgent.stoppingDistance = 0.1f;
        navAgent.autoBraking = true;

        playerTransform = FindPlayer();
        if (playerTransform != null)
        {
            playerController = playerTransform.GetComponent<PlayerController>();
        }
    }

    void SetupCollider()
    {
        guardCollider = GetComponent<CapsuleCollider>();
        if (guardCollider == null)
        {
            guardCollider = gameObject.AddComponent<CapsuleCollider>();
            guardCollider.height = 2f;
            guardCollider.radius = 0.5f;
            guardCollider.center = new Vector3(0, 1f, 0);
        }
        guardCollider.isTrigger = true; // 设为触发器，用于检测接触
    }

    Transform FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player != null ? player.transform : null;
    }

    void SetupPatrol()
    {
        if (patrolPoints == null || patrolPoints.Length < 2)
        {
            isActive = false;
            return;
        }
        if (patrolPoints[0] != null)
        {
            navAgent.SetDestination(patrolPoints[0].position);
        }
    }

    void Update()
    {
        if (!isActive) return;

        // 如果游戏已经结束，停止守卫逻辑
        if (GameManager.Instance != null &&
            (GameManager.Instance.GetCurrentGameState() == GameManager.GameState.GameOver ||
             GameManager.Instance.GetCurrentGameState() == GameManager.GameState.Victory))
        {
            if (navAgent.enabled) navAgent.isStopped = true;
            return;
        }

        UpdatePatrol();
        DetectPlayer();
    }

    void UpdatePatrol()
    {
        if (isWaiting)
        {
            waitTimer += Time.deltaTime;
            if (waitTimer >= waitTimeAtPoint)
            {
                isWaiting = false;
                waitTimer = 0f;
                currentTargetIndex = (currentTargetIndex + 1) % Mathf.Min(2, patrolPoints.Length);
                MoveToNextPoint();
            }
            return;
        }

        if (!navAgent.pathPending && navAgent.remainingDistance <= navAgent.stoppingDistance)
        {
            if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
            {
                isWaiting = true;
                waitTimer = 0f;
            }
        }
    }

    void MoveToNextPoint()
    {
        if (patrolPoints[currentTargetIndex] != null)
        {
            navAgent.SetDestination(patrolPoints[currentTargetIndex].position);
        }
    }

    void DetectPlayer()
    {
        if (playerTransform == null) return;

        // --- 1. 获取玩家的"真"中心点 (胸口) ---
        // 尝试获取玩家的碰撞体中心，如果获取不到，就手动抬高 1.5 米
        Vector3 targetPos;
        Collider playerCol = playerTransform.GetComponent<Collider>();
        if (playerCol != null)
        {
            targetPos = playerCol.bounds.center; // 这是绝对准确的物体中心
        }
        else
        {
            // 备用方案：手动抬高 1.5 米 (一般角色身高2米，1.5米大概在胸口/头部)
            targetPos = playerTransform.position + Vector3.up * 1.5f;
        }

        // --- 2. 设定守卫的"眼睛"位置 ---
        // 从守卫头顶发出来，稍微往前一点，防止打到自己
        Vector3 startPos = transform.position + Vector3.up * 1.6f + transform.forward * 0.5f;

        // 计算距离和方向
        float distanceToPlayer = Vector3.Distance(startPos, targetPos); // 注意这里改用 startPos 计算
        Vector3 directionToPlayer = (targetPos - startPos).normalized;

        // --- 3. 距离检测 ---
        if (distanceToPlayer <= detectionRange)
        {
            RaycastHit hit;

            // 画出黄线：表示守卫想看哪里
            Debug.DrawLine(startPos, targetPos, Color.yellow);

            // --- 4. 射线检测 ---
            // 这里的重点是：终点是 targetPos，而不是无限远
            if (Physics.Raycast(startPos, directionToPlayer, out hit, distanceToPlayer + 1f)) // 多射 1米 确保穿透
            {
                // 如果打到了东西，画出红线，终点是打到的位置
                Debug.DrawLine(startPos, hit.point, Color.red);

                if (hit.collider.CompareTag("Player"))
                {
                    Debug.Log("【发现目标】守卫看见了玩家，开始追逐！");
                    navAgent.SetDestination(playerTransform.position);
                }
                else
                {
                    // 调试：如果没打中玩家，打中了谁？(很有可能是 Floor/Ground)
                    // Debug.Log($"视线被阻挡，打到了: {hit.collider.name}");
                }
            }
        }
    }
    
    // ========== 核心修改：触发器检测逻辑 ==========
    void OnTriggerEnter(Collider other)
    {
        // 只有在可以造成伤害时才检测
        if (!canDealDamage) return;

        if (other.CompareTag("Player"))
        {
            HandlePlayerContact();
        }
    }

    void HandlePlayerContact()
    {
        if (!canDealDamage) return;

        Debug.Log($"<color=red>⚔️ 守卫抓住了玩家！</color>");

        // 1. 通知 GameManager 扣血
        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDetectedByGuard(); // 这会扣除1点生命值

            // 2. 检查玩家是否还有命
            int currentLives = GameManager.Instance.PlayerLives; // 假设 GameManager 有这个属性

            if (currentLives > 0)
            {
                // === 情况 A: 玩家还有命 ===
                Debug.Log($"玩家受伤！剩余生命: {currentLives}。守卫重置...");

                // 开启冷却，防止连续扣血
                StartCoroutine(DamageCooldownRoutine());

                // 守卫重置行为 (给玩家逃跑机会)
                ResetGuardPosition();
            }
            else
            {
                // === 情况 B: 玩家没命了 (Game Over) ===
                Debug.Log("玩家生命耗尽，游戏结束！");

                // 停止玩家控制
                if (playerController != null)
                {
                    playerController.enabled = false;
                }

                // 停止守卫
                navAgent.isStopped = true;
                isActive = false;

                // GameManager 内部应该会处理 SetGameState(GameOver)
            }
        }
        else
        {
            Debug.LogError("未找到 GameManager！无法处理扣血逻辑。");
        }
    }

    // 伤害冷却协程
    IEnumerator DamageCooldownRoutine()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDealDamage = true;
    }

    // 重置守卫位置（回到巡逻点，不再追击）
    void ResetGuardPosition()
    {
        // 停止当前追击
        navAgent.ResetPath();

        // 瞬间回到最近的巡逻点 (或者你可以选择只让他停顿几秒)
        // 这里选择让他回到巡逻起始点，给玩家最大的逃跑机会
        if (patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            navAgent.Warp(patrolPoints[0].position); // 瞬移回去
            currentTargetIndex = 0;
            navAgent.SetDestination(patrolPoints[0].position);
        }

        isWaiting = true; // 让他发一会呆
        waitTimer = -2f; // 多等2秒再动
    }

    void OnDrawGizmosSelected()
    {
        if (!showDebugInfo) return;

        Gizmos.color = patrolColor;
        if (patrolPoints != null)
        {
            foreach (var point in patrolPoints)
            {
                if (point != null) Gizmos.DrawSphere(point.position, 0.3f);
            }
        }

        Gizmos.color = detectionColor;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}