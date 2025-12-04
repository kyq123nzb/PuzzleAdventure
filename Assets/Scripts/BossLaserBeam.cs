using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BossLaserBeam : MonoBehaviour
{
    [Header("激光属性")]
    public float damageInterval = 1.0f; // 伤害间隔
    public float laserSpeed = 30f;      // 激光飞行速度 (米/秒)
    public float maxDistance = 50f;     // 激光最大长度
    public LayerMask hitLayerMask;      // 激光能打到什么 (墙 + 玩家)

    [Header("视觉设置")]
    public float laserWidth = 0.3f;
    public Color laserColor = Color.red;

    private LineRenderer lineRenderer;
    private float damageTimer = 0f;
    private float currentLength = 0f;   // 当前激光长度

    void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        SetupLineRenderer();
    }

    void OnEnable()
    {
        // 每次激活时，长度归零，重新发射
        currentLength = 0f;
        lineRenderer.enabled = true;
    }

    void SetupLineRenderer()
    {
        lineRenderer.useWorldSpace = true; // 使用世界坐标，方便适应Boss移动
        lineRenderer.startWidth = laserWidth;
        lineRenderer.endWidth = laserWidth;
        lineRenderer.material = new Material(Shader.Find("Sprites/Default")); // 简单材质
        lineRenderer.startColor = laserColor;
        lineRenderer.endColor = laserColor;
    }

    void Update()
    {
        // 1. 让激光变长 (传播过程)
        if (currentLength < maxDistance)
        {
            currentLength += laserSpeed * Time.deltaTime;
        }

        // 2. 射线检测 (核心逻辑)
        // 从当前物体位置，向前发射
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        // 计算这一帧激光的终点
        Vector3 endPoint = transform.position + transform.forward * currentLength;

        // 检测射线是否打到了东西 (距离使用 currentLength)
        if (Physics.Raycast(ray, out hit, currentLength, hitLayerMask))
        {
            // 如果打到了东西，激光终点就是打到的点 (不穿墙)
            endPoint = hit.point;

            // 如果打到的是玩家，尝试扣血
            if (hit.collider.CompareTag("Player"))
            {
                TryDamagePlayer();
            }
        }

        // 3. 更新画面
        lineRenderer.SetPosition(0, transform.position); // 起点
        lineRenderer.SetPosition(1, endPoint);           // 终点
    }

    void TryDamagePlayer()
    {
        if (Time.time > damageTimer)
        {
            damageTimer = Time.time + damageInterval;
            Debug.Log("💔 激光击中玩家！(阻挡检测生效中)");

            if (GameManager.Instance != null)
            {
                GameManager.Instance.PlayerDetectedByGuard();
            }
        }
    }
}