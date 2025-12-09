using UnityEngine;

public class PuzzleFloater : MonoBehaviour
{
    [Header("运动设置")]
    public float rotateSpeed = 50f; // 旋转速度
    public float floatAmplitude = 0.2f; // 上下浮动范围
    public float floatFrequency = 1f; // 浮动频率

    [Header("发光组件 (可选)")]
    public Light glowLight; // 拖入子物体的灯光

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // 1. 自转 (绕 Y 轴)
        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);

        // 2. 上下浮动 (悬浮感)
        float newY = startPos.y + Mathf.Sin(Time.time * floatFrequency) * floatAmplitude;
        transform.position = new Vector3(transform.position.x, newY, transform.position.z);
    }
}