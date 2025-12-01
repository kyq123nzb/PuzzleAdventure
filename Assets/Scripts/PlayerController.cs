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
    public int rotateMouseButton = 0; // 选择按键
    
    private CharacterController controller;
    private Camera playerCamera;
    private Vector3 velocity;
    private float xRotation = 0f;
    private bool isRotatingView = false;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // 初始时不锁定鼠标
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        
        Debug.Log($"当前使用鼠标按钮 {rotateMouseButton} 进行视角旋转");
    }
    
    void Update()
    {
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
            Debug.Log("开始视角旋转");
        }
        
        // 检查是否释放了设置的鼠标按钮
        if (Input.GetMouseButtonUp(rotateMouseButton))
        {
            isRotatingView = false;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            Debug.Log("停止视角旋转");
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
}