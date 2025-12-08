using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PushableBox: MonoBehaviour
{
    [Header("推箱子音效")]
    public AudioClip moveSound; // 拖动时的摩擦声
    public float minSpeedToPlaySound = 0.1f;

    private Rigidbody rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // 自动添加 AudioSource
        if (moveSound != null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = moveSound;
            audioSource.loop = true; // 循环播放
            audioSource.spatialBlend = 1f; // 3D音效
            audioSource.playOnAwake = false;
        }

        // 自动设置刚体约束：防止箱子被推翻滚（只允许平移）
        // 锁定 X 和 Z 轴的旋转
        rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
    }

    void Update()
    {
        // 音效控制逻辑
        if (audioSource != null)
        {
            // 计算水平速度
            float horizontalSpeed = new Vector3(rb.velocity.x, 0, rb.velocity.z).magnitude;

            if (horizontalSpeed > minSpeedToPlaySound)
            {
                if (!audioSource.isPlaying) audioSource.Play();
            }
            else
            {
                if (audioSource.isPlaying) audioSource.Stop();
            }
        }
    }
}