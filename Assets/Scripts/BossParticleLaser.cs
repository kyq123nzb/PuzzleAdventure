using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class BossParticleLaser : MonoBehaviour
{
    [Header("伤害设置")]
    public float damageInterval = 0.5f; // 连续被击中的扣血间隔

    private float lastDamageTime = 0f;
    private ParticleSystem ps;

    void Awake()
    {
        ps = GetComponent<ParticleSystem>();
    }

    void OnEnable()
    {
        // Boss 激活激光时，开始发射
        if (ps != null) ps.Play();
    }

    void OnDisable()
    {
        // Boss 关闭激光时，停止发射（已发射的粒子会飞完）
        if (ps != null) ps.Stop();
    }

    // Unity 原生回调：当粒子打到任何 Collider 时触发
    void OnParticleCollision(GameObject other)
    {
        // 1. 检查是否打中玩家
        if (other.CompareTag("Player"))
        {
            // 2. 检查伤害冷却
            if (Time.time > lastDamageTime + damageInterval)
            {
                lastDamageTime = Time.time;
                Debug.Log("⚡ 粒子激光击中玩家！");

                // 3. 扣血
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.PlayerDetectedByGuard();
                }
            }
        }
    }
}