using UnityEngine;

public class BossCore : MonoBehaviour
{
    [Header("引用")]
    public BossController bossBrain;

    [Header("视觉")]
    public Color shieldedColor = Color.blue;
    public Color vulnerableColor = new Color(1f, 0.5f, 0f); // 橙色
    private Renderer myRenderer;
    private bool isVulnerable = false;

    void Awake()
    {
        myRenderer = GetComponent<Renderer>();
        SetVulnerable(false);
    }

    public void SetVulnerable(bool vulnerable)
    {
        isVulnerable = vulnerable;
        if (myRenderer != null)
        {
            myRenderer.material.color = isVulnerable ? vulnerableColor : shieldedColor;
        }
    }

    // 供玩家激光脚本调用
    public bool OnHitByLaser()
    {
        if (UIManager.Instance == null) return false;

        // ========== [新增] 状态检查 ==========
        // 如果 Boss 已经死了，什么都不做，避免覆盖胜利提示
        if (bossBrain != null && bossBrain.IsDead)
        {
            return false;
        }
        // ===================================

        if (isVulnerable)
        {
            // === 情况 A: 击中橙色核心 (有效攻击) ===

            int remainingHP = 0;
            if (bossBrain != null)
            {
                remainingHP = bossBrain.TakeDamage(); // 获取剩余血量
            }

            // 如果这一击打死了 Boss，就不要显示剩余血量提示了
            // 因为 BossController.Die() 会显示 "Boss Defeated..."
            if (remainingHP <= 0)
            {
                return true;
            }

            // 只有没死的时候才提示剩余血量
            string msg = $"Attack Successful! Boss HP: {remainingHP}";
            UIManager.Instance.ShowInteractionPrompt(msg);

            return true; // 造成了伤害
        }
        else
        {
            // === 情况 B: 击中蓝色核心 (被偏折) ===
            string msg = "Laser deflected by Boss shield!";
            UIManager.Instance.ShowInteractionPrompt(msg);

            Debug.Log("🛡️ 激光被 Boss 护盾偏折！");
            return false; // 未造成伤害
        }
    }
}