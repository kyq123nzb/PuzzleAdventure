using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 测试脚本：直接绑定到StartButton，用于诊断按钮点击问题
/// </summary>
public class StartButtonTest : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    private Button btn;
    
    void Start()
    {
        btn = GetComponent<Button>();
        if (btn != null)
        {
            // 确保按钮可交互
            btn.interactable = true;
            
            // 检查Image组件
            Image img = GetComponent<Image>();
            if (img != null)
            {
                img.raycastTarget = true;
            }
            
            // 清除所有监听器
            btn.onClick.RemoveAllListeners();
            
            // 添加测试监听器
            btn.onClick.AddListener(() => {
                Debug.Log("🎯 StartButtonTest: 按钮onClick事件被触发");
            });
            
            // 添加UIManager的StartGame方法
            btn.onClick.AddListener(() => {
                if (UIManager.Instance != null)
                {
                    UIManager.Instance.StartGame();
                }
                else
                {
                    Debug.LogWarning("⚠️ StartButtonTest: UIManager.Instance为null！");
                    // 尝试直接查找UIManager
                    UIManager uiManager = FindObjectOfType<UIManager>();
                    if (uiManager != null)
                    {
                        uiManager.StartGame();
                    }
                    else
                    {
                        Debug.LogError("❌ StartButtonTest: 场景中找不到UIManager！");
                    }
                }
            });
        }
        else
        {
            Debug.LogError("❌ StartButtonTest: 没有找到Button组件！");
        }
    }
    
    // 实现IPointerClickHandler接口，直接检测点击
    public void OnPointerClick(PointerEventData eventData)
    {
        // 手动触发onClick事件
        if (btn != null && btn.interactable)
        {
            try
            {
                btn.onClick.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ StartButtonTest: onClick.Invoke()执行失败: {e.Message}");
            }
        }
    }
    
    // 检测鼠标进入
    public void OnPointerEnter(PointerEventData eventData)
    {
        // 可以在这里添加鼠标悬停效果
    }
    
    // 检测鼠标离开
    public void OnPointerExit(PointerEventData eventData)
    {
        // 可以在这里移除鼠标悬停效果
    }
    
    // 添加IPointerDownHandler来检测鼠标按下
    public void OnPointerDown(PointerEventData eventData)
    {
        // 直接在按下时触发点击事件（这样即使鼠标在按钮外释放也能触发）
        if (btn != null && btn.interactable)
        {
            try
            {
                btn.onClick.Invoke();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"❌ StartButtonTest: onClick.Invoke()执行失败: {e.Message}");
            }
        }
    }
    
    // 添加IPointerUpHandler来检测鼠标释放
    public void OnPointerUp(PointerEventData eventData)
    {
        // 不需要额外处理
    }
}

