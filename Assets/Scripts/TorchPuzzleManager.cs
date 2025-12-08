using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorchPuzzleManager : Interactable
{
    [Header("谜题设置")]
    // 正确的ID顺序，例如 {1, 4, 2, 6, 3, 5}
    public List<int> correctSequence;

    [Header("场景对象引用")]
    public List<TorchPuzzleObject> allTorches; // 场景里那6个火把

    [Header("宝箱设置")]
    public Transform lidPivot; // 箱盖轴心
    public float openAngle = -110f; // 打开角度
    public GameObject rewardPrefab; // 拼图Prefab

    [Header("反馈音效")]
    public AudioClip igniteSound; // 点燃音效
    public AudioClip errorSound;  // 顺序错误音效
    public AudioClip solveSound;  // 解开谜题音效

    private List<int> currentInputSequence = new List<int>();
    private bool isSolved = false;
    private AudioSource audioSource;

    void Start()
    {
        interactionText = "The treasure chest has been sealed by some kind of flame magic...";
        audioSource = gameObject.AddComponent<AudioSource>();

        // 确保火把都引用了管理器
        foreach (var torch in allTorches)
        {
            torch.puzzleManager = this;
            torch.SetState(false); // 初始全灭
        }
    }

    // 重写交互：点击箱子时的提示
    public override void Interact()
    {
        if (isSolved) return;

        if (UIManager.Instance != null)
        {
            UIManager.Instance.ShowInteractionPrompt("The torches around need to be lit in a specific order...");
        }
    }

    // 当某个火把被点燃时调用
    public void OnTorchIgnited(TorchPuzzleObject torch)
    {
        if (isSolved) return;

        // 播放点燃音效
        PlaySound(igniteSound);

        // 记录输入
        currentInputSequence.Add(torch.torchID);
        Debug.Log($"Current input sequence:{string.Join(",", currentInputSequence)}");

        // 检查当前这一步是否正确
        int currentIndex = currentInputSequence.Count - 1;

        // 1. 检查当前按下的这个火把，是不是正确序列里对应位置的那个？
        if (correctSequence[currentIndex] == torch.torchID)
        {
            // 正确！

            // 2. 检查是否全部按完了
            if (currentInputSequence.Count == correctSequence.Count)
            {
                StartCoroutine(SolvePuzzle());
            }
        }
        else
        {
            // 错误！重置谜题
            Debug.Log("The sequence is wrong! The torch has gone out...The sequence is wrong! The torch has gone out...");
            StartCoroutine(ResetPuzzle());
        }
    }

    IEnumerator ResetPuzzle()
    {
        // 暂停一下让玩家看到最后点的那个火把亮起（你是错的，但先让你看清楚）
        yield return new WaitForSeconds(0.5f);

        PlaySound(errorSound);

        // 熄灭所有火把
        foreach (var torch in allTorches)
        {
            torch.SetState(false);
        }

        // 清空输入记录
        currentInputSequence.Clear();
    }

    IEnumerator SolvePuzzle()
    {
        isSolved = true;
        interactionText = "The seal has been lifted.";
        PlaySound(solveSound);
        Debug.Log("The puzzle has been solved!");

        yield return new WaitForSeconds(0.5f);

        // 开箱动画
        float timer = 0f;
        Quaternion startRot = lidPivot.localRotation;
        Quaternion targetRot = Quaternion.Euler(0, 0, openAngle);

        while (timer < 1f)
        {
            timer += Time.deltaTime * 2;
            lidPivot.localRotation = Quaternion.Lerp(startRot, targetRot, timer);
            yield return null;
        }

        // 生成奖励
        if (rewardPrefab != null)
        {
            Instantiate(rewardPrefab, transform.position + Vector3.up, Quaternion.identity);
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }
}