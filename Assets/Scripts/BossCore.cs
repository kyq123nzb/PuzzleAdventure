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
        if (isVulnerable)
        {
            // 只有脆弱时才受伤
            if (bossBrain != null)
            {
                bossBrain.TakeDamage();
            }
            return true; // 返回 true 表示造成了伤害
        }
        else
        {
            // 护盾弹开激光效果
            Debug.Log("🛡️ 激光被 Boss 护盾偏折！");
            return false;
        }
    }
}