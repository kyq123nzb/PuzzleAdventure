using System.Collections;
using UnityEngine;

public class PlayerLaserAttack : MonoBehaviour
{
    [Header("攻击设置")]
    public KeyCode fireKey = KeyCode.F;
    public float attackRange = 100f;
    public float laserDuration = 0.2f;

    [Header("视觉效果")]
    public LineRenderer lineRenderer;
    public Transform firePoint;
    public Color laserColor = Color.cyan;

    private Camera playerCam;

    void Start()
    {
        playerCam = Camera.main;
        if (playerCam == null) playerCam = GetComponentInChildren<Camera>();

        // 自动添加 LineRenderer 如果没有的话
        if (lineRenderer == null)
        {
            lineRenderer = gameObject.AddComponent<LineRenderer>();
            lineRenderer.startWidth = 0.1f;
            lineRenderer.endWidth = 0.1f;
            // 使用默认材质，防止变粉色
            lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startColor = laserColor;
            lineRenderer.endColor = laserColor;
        }
        lineRenderer.enabled = false;

        if (firePoint == null) firePoint = playerCam.transform;
    }

    void Update()
    {
        // ⚠️ 关键检查：必须强制允许射击，或者游戏状态必须是 Playing
        // 为了测试方便，我把状态检查暂时注释掉，让你能直接射击
        // if (GameManager.Instance != null && GameManager.Instance.GetCurrentGameState() != GameManager.GameState.Playing) return;

        if (Input.GetKeyDown(fireKey))
        {
            Debug.Log("按下了 F 键，发射激光！"); // 调试日志
            ShootLaser();
        }
    }

    void ShootLaser()
    {
        StartCoroutine(ShowLaserEffect());

        Ray ray = new Ray(playerCam.transform.position, playerCam.transform.forward);
        RaycastHit hit;
        Vector3 endPoint = playerCam.transform.position + playerCam.transform.forward * attackRange;

        if (Physics.Raycast(ray, out hit, attackRange))
        {
            endPoint = hit.point;
            // 尝试获取 BossCore
            BossCore core = hit.collider.GetComponent<BossCore>();
            if (core != null)
            {
                Debug.Log("打中 Boss 核心了！");
                core.OnHitByLaser();
            }
        }

        if (lineRenderer != null)
        {
            lineRenderer.SetPosition(0, firePoint.position + Vector3.down * 0.2f);
            lineRenderer.SetPosition(1, endPoint);
        }
    }

    IEnumerator ShowLaserEffect()
    {
        lineRenderer.enabled = true;
        yield return new WaitForSeconds(laserDuration);
        lineRenderer.enabled = false;
    }
}