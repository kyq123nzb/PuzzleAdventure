using System.Collections;
using UnityEngine;

public class BossZoneHint : MonoBehaviour
{
    [Header("提示内容")]
    [TextArea]
    public string message = "WARNING: Press [F] to Fire when Core is ORANGE!";

    [Header("设置")]
    public float duration = 5.0f; // 显示持续时间
    public bool oneTimeOnly = true; // 是否只显示一次

    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 如果只触发一次且已经触发过，直接返回
        if (oneTimeOnly && hasTriggered) return;

        if (other.CompareTag("Player"))
        {
            hasTriggered = true;
            Debug.Log("进入 Boss 区域，显示战斗提示");

            if (UIManager.Instance != null)
            {
                // 1. 强制显示交互提示文字
                UIManager.Instance.ShowInteractionPrompt(message);

                // 2. 开启倒计时关闭
                StopAllCoroutines(); // 防止多次触发冲突
                StartCoroutine(HideHintRoutine());
            }
        }
    }

    IEnumerator HideHintRoutine()
    {
        yield return new WaitForSeconds(duration);

        // 时间到，关闭提示
        if (UIManager.Instance != null)
        {
            UIManager.Instance.HideInteractionPrompt();
        }
    }

    // 在 Scene 窗口画一个红色的框，方便你看在哪里
    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1, 0, 0, 0.3f); // 半透明红色
        BoxCollider box = GetComponent<BoxCollider>();
        if (box != null)
        {
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawCube(box.center, box.size);
        }
    }
}