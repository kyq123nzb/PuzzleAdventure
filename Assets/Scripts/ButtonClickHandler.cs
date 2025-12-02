using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 按钮点击处理组件 - 解决Unity Button点击检测严格的问题
/// 在鼠标按下时直接触发点击事件，即使鼠标在按钮外释放也能工作
/// </summary>
public class ButtonClickHandler : MonoBehaviour, IPointerDownHandler
{
    private Button button;
    
    void Awake()
    {
        button = GetComponent<Button>();
    }
    
    public void OnPointerDown(PointerEventData eventData)
    {
        // 如果按钮存在且可交互，直接触发点击事件
        if (button != null && button.interactable)
        {
            button.onClick.Invoke();
        }
    }
}

