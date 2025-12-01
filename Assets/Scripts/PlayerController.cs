using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("移动设置")]
    public float walkSpeed = 5f;
    public float runSpeed = 8f;
    public float jumpForce = 7f;
    public float gravity = -9.81f;

    [Header("视角设置")]
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 80f;

    [Header("鼠标按钮设置")]
    [Tooltip("0=左键, 1=右键, 2=中键")]
    public int rotateMouseButton = 0;

    // 组件引用
    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isRotatingView = false;

    void Start()
    {
        // 1. 获取或添加CharacterController
        controller = GetComponent<CharacterController>();
        if (controller == null)
        {
            Debug.Log("自动添加CharacterController组件");
            controller = gameObject.AddComponent<CharacterController>();
            controller.height = 2.0f;
            controller.radius = 0.3f;
            controller.center = new Vector3(0, 1f, 0);
        }

        // 2. 获取相机
        playerCamera = GetComponentInChildren<Camera>();
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            Debug.Log("使用Main Camera作为玩家相机");
        }

        // 3. 初始鼠标设置
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log("玩家控制器初始化完成");

        // 4. 确保玩家有碰撞体
        Collider playerCollider = GetComponent<Collider>();
        if (playerCollider == null)
        {
            CapsuleCollider capsule = gameObject.AddComponent<CapsuleCollider>();
            capsule.height = 2f;
            capsule.radius = 0.3f;
            capsule.center = new Vector3(0, 1f, 0);
        }
    }

    void Update()
    {
        // 如果控制器被禁用，不处理输入
        if (!enabled) return;

        HandleViewRotation();
        HandleMovement();
    }

    void HandleViewRotation()
    {
        // 检查是否按下了设置的鼠标按钮
        if (Input.GetMouseButtonDown(rotateMouseButton))
        {
            isRotatingView = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // 检查是否释放了设置的鼠标按钮
        if (Input.GetMouseButtonUp(rotateMouseButton))
        {
            isRotatingView = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        // 如果正在旋转视角，处理鼠标移动
        if (isRotatingView)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -maxLookAngle, maxLookAngle);

            playerCamera.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            transform.Rotate(Vector3.up * mouseX);
        }
    }

    void HandleMovement()
    {
        // 地面检测
        bool isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }

        // 获取输入
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // 计算移动方向
        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // 选择速度
        float currentSpeed = Input.GetKey(KeyCode.LeftShift) ? runSpeed : walkSpeed;

        // 应用移动
        controller.Move(move * currentSpeed * Time.deltaTime);

        // 跳跃
        if (Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // 应用重力
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    // 调试方法：重置玩家
    public void ResetPlayer()
    {
        enabled = true;
        velocity = Vector3.zero;

        // 重置位置到起始点
        transform.position = Vector3.zero + Vector3.up * 1f;

        Debug.Log("玩家已重置");
    }
    
}