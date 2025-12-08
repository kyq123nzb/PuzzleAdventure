using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss 基础属性")]
    public int maxHealth = 3;

    [Header("Boss 灵活走位设置 ")]
    public bool enableMovement = true;
    public Vector2 moveAreaSize = new Vector2(10f, 5f); // X轴和Z轴的移动范围
    public float moveSpeed = 5f;       // 移动最大速度
    public float movementSmoothTime = 0.5f; // 移动平滑度
    public float changePositionInterval = 2f; // 每隔几秒换一个位置
    public bool stopMovingWhileAttacking = true;

    [Header("攻击节奏")]
    public float attackInterval = 4f;
    public float laserDuration = 3f;
    public float chargeTime = 1.5f;

    [Header("部件引用")]
    public BossCore core;
    public GameObject laserObject;
    public Renderer bossBodyRenderer;

    [Header("状态颜色")]
    public Color normalColor = Color.gray;
    public Color chargeColor = Color.red;

    [Header("胜利奖励")]
    public GameObject rewardPuzzlePrefab;
    public Vector3 rewardOffset = new Vector3(0, 2f, 0);
    [TextArea]
    public string victoryMessage = "Boss Defeated! Please pick up the Puzzle Reward.";

    // 内部变量
    private int currentHealth;
    private bool isDead = false;
    private bool isAttacking = false;

    // 移动相关变量
    private Vector3 startPosition;
    private Vector3 targetPosition;
    private Vector3 currentVelocity;
    private float moveTimer = 0f;

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position;
        targetPosition = startPosition;

        if (laserObject != null) laserObject.SetActive(false);
        if (bossBodyRenderer != null) bossBodyRenderer.material.color = normalColor;
        if (core != null) core.SetVulnerable(false);

        StartCoroutine(BossBehaviorLoop());
    }

    void Update()
    {
        if (!isDead && enableMovement)
        {
            if (stopMovingWhileAttacking && isAttacking) return;

            HandleFlexibleMovement();
        }
    }

    void HandleFlexibleMovement()
    {
        moveTimer -= Time.deltaTime;
        if (moveTimer <= 0)
        {
            PickNewRandomPosition();
            moveTimer = changePositionInterval;
        }

        // 如果血量低(只剩1点)，速度加倍 (狂暴模式)
        float currentSpeedMult = (currentHealth == 1) ? 2.0f : 1.0f;

        transform.position = Vector3.SmoothDamp(
            transform.position,
            targetPosition,
            ref currentVelocity,
            movementSmoothTime / currentSpeedMult,
            moveSpeed * currentSpeedMult
        );
    }

    void PickNewRandomPosition()
    {
        // 【修改点 1】明确使用 UnityEngine.Random
        float randomX = UnityEngine.Random.Range(-moveAreaSize.x / 2f, moveAreaSize.x / 2f);
        float randomZ = UnityEngine.Random.Range(-moveAreaSize.y / 2f, moveAreaSize.y / 2f);

        targetPosition = startPosition + new Vector3(randomX, 0, randomZ);
    }

    IEnumerator BossBehaviorLoop()
    {
        while (!isDead)
        {
            // 1. 冷却/移动
            isAttacking = false;
            yield return new WaitForSeconds(attackInterval);

            // 2. 充能
            isAttacking = true;
            targetPosition = transform.position; // 停止移动

            if (bossBodyRenderer != null) bossBodyRenderer.material.color = chargeColor;
            Debug.Log("⚠️ Boss 正在充能...");
            yield return new WaitForSeconds(chargeTime);

            // 3. 发射
            FireLaser(true);
            if (core != null) core.SetVulnerable(true);
            Debug.Log("🔥 Boss 发射激光！");

            yield return new WaitForSeconds(laserDuration);

            // 4. 恢复
            FireLaser(false);
            if (core != null) core.SetVulnerable(false);
            if (bossBodyRenderer != null) bossBodyRenderer.material.color = normalColor;
        }
    }

    void FireLaser(bool isActive)
    {
        if (laserObject != null) laserObject.SetActive(isActive);
    }

    public void TakeDamage()
    {
        if (isDead) return;
        currentHealth--;
        Debug.Log($"⚔️ Boss 受到伤害！剩余血量: {currentHealth}");

        moveTimer = 0; // 受伤立即换位

        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        FireLaser(false);
        if (core != null) core.SetVulnerable(false);
        if (bossBodyRenderer != null) bossBodyRenderer.material.color = Color.black;

        Debug.Log("🎉 Boss 被击败！生成奖励...");

        if (rewardPuzzlePrefab != null)
        {
            Instantiate(rewardPuzzlePrefab, transform.position + rewardOffset, Quaternion.identity);
        }

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowInteractionPrompt(victoryMessage);
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.BossDefeated();
        }

        this.enabled = false;
    }

    // 在 Scene 窗口画出移动范围，方便调试
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        // 【修改点 2】明确使用 UnityEngine.Application
        Vector3 center = UnityEngine.Application.isPlaying ? startPosition : transform.position;
        Gizmos.DrawWireCube(center, new Vector3(moveAreaSize.x, 1f, moveAreaSize.y));
    }
}