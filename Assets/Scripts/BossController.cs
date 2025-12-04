using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [Header("Boss 基础属性")]
    public int maxHealth = 3;

    [Header("Boss 移动设置 (Z轴移动)")]
    public bool enableMovement = true;
    public float moveSpeed = 3f;
    public float moveDistance = 5f;
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

    private int currentHealth;
    private bool isDead = false;
    private bool isAttacking = false;
    private Vector3 startPosition;

    void Start()
    {
        currentHealth = maxHealth;
        startPosition = transform.position; // 记住出生点

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
            HandleMovement();
        }
    }

    void HandleMovement()
    {
        // 计算偏移量 (-moveDistance 到 +moveDistance)
        float offset = Mathf.Sin(Time.time * moveSpeed) * moveDistance;

        // 【修改点】保持 X 和 Y 不变，只改变 Z 轴
        transform.position = new Vector3(startPosition.x, transform.position.y, startPosition.z + offset);
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
        if (currentHealth <= 0) Die();
    }

    void Die()
    {
        isDead = true;
        StopAllCoroutines();
        FireLaser(false);
        if (core != null) core.SetVulnerable(false);
        if (bossBodyRenderer != null) bossBodyRenderer.material.color = Color.black;

        Debug.Log("🎉 Boss 被击败！");
        if (GameManager.Instance != null) GameManager.Instance.BossDefeated();
    }
}