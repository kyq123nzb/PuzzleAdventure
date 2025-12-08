using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class PlayerParticleAttack : MonoBehaviour
{
    [Header("攻击设置")]
    public KeyCode fireKey = KeyCode.F;
    public float fireRate = 0.2f; // 射击间隔（防止按太快）

    private ParticleSystem ps;
    private float lastFireTime = 0f;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void Update()
    {
        // 游戏状态检查 (可选，确保 AutoStartTest 已挂载)
        if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameState() != GameManager.GameState.Playing)
            return;

        // 检测输入 & 冷却
        if (Input.GetKeyDown(fireKey) && Time.time > lastFireTime + fireRate)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        lastFireTime = Time.time;

        // 手动发射 1 颗粒子 (像开枪一样)
        if (ps != null)
        {
            ps.Emit(1);
            // 播放一个音效 (如果你有的话)
            // AudioSource.PlayClipAtPoint(shootSound, transform.position);
        }
    }

    // Unity 原生回调：粒子撞到东西时触发
    void OnParticleCollision(GameObject other)
    {
        // 尝试获取 BossCore 组件
        BossCore core = other.GetComponent<BossCore>();

        if (core != null)
        {
            Debug.Log("🎯 命中 Boss 核心！");
            // 调用之前写好的伤害逻辑
            bool isHit = core.OnHitByLaser();

            if (isHit)
            {
                // 这里可以加一个击中特效，比如火花
                // Instantiate(hitEffect, transform.position, Quaternion.identity);
            }
        }
        else
        {
            // Debug.Log($"打到了墙壁: {other.name}");
        }
    }
}