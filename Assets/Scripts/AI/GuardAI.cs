using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    [Header("巡逻设置")]
    public Transform[] patrolPoints;
    public float moveSpeed = 3.5f; // 稍微快一点，增加压迫感
    public float waitTimeAtPoint = 2f;

    [Header("玩家检测设置")]
    public float detectionRange = 10f; // 视线距离
    public float damageCooldown = 2.0f; // 伤害间隔

    [Header("追逐反馈")]
    public GameObject warningUIObject; // 直接把那个红色的 Text 物体拖进来
    public float warningDuration = 3f; // 提示显示多久自动消失
    public AudioClip spotSound;        // (可选) 发现玩家时的惊叹音效

    private bool isChasing = false;    // 内部状态锁，防止警告一直闪烁

    [Header("状态设置")]
    public bool isActive = true;

    [Header("调试设置")]
    public bool showDebugInfo = true;

    // 私有组件
    private NavMeshAgent navAgent;
    private Transform playerTransform;
    private PlayerController playerController;
    private int currentTargetIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private bool canDealDamage = true;
    private CapsuleCollider guardCollider;

    void Start()
    {
        InitializeComponents();
        SetupPatrol();
        SetupCollider();

        // 游戏开始时，强制关闭警告 UI，防止穿帮
        if (warningUIObject != null)
        {
            warningUIObject.SetActive(false);
        }
    }

    void InitializeComponents()
    {
        navAgent = GetComponent<NavMeshAgent>();
        if (navAgent == null) navAgent = gameObject.AddComponent<NavMeshAgent>();

        navAgent.speed = moveSpeed;
        navAgent.stoppingDistance = 0.5f; // 稍微大一点，防止贴脸鬼畜
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
        guardCollider.isTrigger = true; // 必须是 Trigger 才能穿过并扣血
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

        // 如果游戏结束或暂停，停止逻辑
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameState() != GameManager.GameState.Playing)
        {
            if (navAgent.isOnNavMesh) navAgent.isStopped = true;
            return;
        }
        else
        {
            if (navAgent.isOnNavMesh) navAgent.isStopped = false;
        }

        DetectPlayer(); // 先检测
        UpdatePatrol(); // 再巡逻
    }

    void UpdatePatrol()
    {
        // 如果正在追逐玩家，就不执行巡逻逻辑
        if (isChasing) return;

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

    // ========== 核心检测逻辑 ==========
    void DetectPlayer()
    {
        if (playerTransform == null) return;

        // 1. 获取目标点 (玩家胸口，防止看地板)
        Vector3 targetPos;
        Collider playerCol = playerTransform.GetComponent<Collider>();
        if (playerCol != null) targetPos = playerCol.bounds.center;
        else targetPos = playerTransform.position + Vector3.up * 1.5f;

        // 2. 获取起点 (守卫眼睛，防止看自己肚皮)
        Vector3 startPos = transform.position + Vector3.up * 1.6f + transform.forward * 0.5f;

        float distanceToPlayer = Vector3.Distance(startPos, targetPos);
        Vector3 directionToPlayer = (targetPos - startPos).normalized;

        // 3. 距离判定
        if (distanceToPlayer <= detectionRange)
        {
            // 调试线 (黄色=在范围内)
            Debug.DrawLine(startPos, targetPos, Color.yellow);

            RaycastHit hit;
            // 4. 射线判定 (是否被墙挡住)
            if (Physics.Raycast(startPos, directionToPlayer, out hit, distanceToPlayer + 1f))
            {
                if (hit.collider.CompareTag("Player"))
                {
                    // === [新增] 发现玩家，触发追逐逻辑 ===
                    EngagePlayer();

                    // 持续更新追逐目标
                    navAgent.SetDestination(playerTransform.position);

                    // 调试线 (红色=已锁定)
                    Debug.DrawLine(startPos, hit.point, Color.red);
                }
            }
        }
    }

    // [新增] 触发追逐状态和警告
    void EngagePlayer()
    {
        // 如果已经是追逐状态，就不重复触发警告 (防止每帧都弹窗)
        if (isChasing) return;

        isChasing = true; // 锁定状态
        Debug.Log("【👁️ SPOTTED!】Guard is chasing!");

        // 1. 显示警告 UI
        if (warningUIObject != null)
        {
            warningUIObject.SetActive(true);
            // 开启协程，几秒后自动关闭
            StartCoroutine(HideWarningDelay());
        }

        // 2. 播放音效 (可选)
        if (spotSound != null)
        {
            AudioSource.PlayClipAtPoint(spotSound, transform.position);
        }
    }

    // [新增] 延迟关闭 UI
    IEnumerator HideWarningDelay()
    {
        yield return new WaitForSeconds(warningDuration);
        if (warningUIObject != null)
        {
            warningUIObject.SetActive(false);
        }
    }

    // ========== 伤害逻辑 ==========
    void OnTriggerEnter(Collider other)
    {
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

        if (GameManager.Instance != null)
        {
            GameManager.Instance.PlayerDetectedByGuard();
            int currentLives = GameManager.Instance.PlayerLives;

            if (currentLives > 0)
            {
                // 玩家还有命：开启无敌帧，重置守卫
                StartCoroutine(DamageCooldownRoutine());
                ResetGuardPosition();
            }
            else
            {
                // 玩家没命了：游戏结束
                if (playerController != null) playerController.enabled = false;
                navAgent.isStopped = true;
                isActive = false;
            }
        }
    }

    IEnumerator DamageCooldownRoutine()
    {
        canDealDamage = false;
        yield return new WaitForSeconds(damageCooldown);
        canDealDamage = true;
    }

    // [新增] 重置逻辑
    void ResetGuardPosition()
    {
        // 关键：重置时解除追逐状态，这样下次发现还能再弹警告
        isChasing = false;

        // 停止当前追逐
        navAgent.ResetPath();

        // 瞬移回起点 (给玩家喘息机会)
        if (patrolPoints != null && patrolPoints.Length > 0 && patrolPoints[0] != null)
        {
            navAgent.Warp(patrolPoints[0].position);
            currentTargetIndex = 0;
            navAgent.SetDestination(patrolPoints[0].position);
        }

        // 发呆一会
        isWaiting = true;
        waitTimer = -2f; // 多等2秒
    }

    // 公共重置方法 (给外部调用)
    public void ResetGuard()
    {
        ResetGuardPosition();
    }
}