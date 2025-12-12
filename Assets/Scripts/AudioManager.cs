using UnityEngine;

/// <summary>
/// 音频管理器 - 统一管理游戏中的背景音乐和音效
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    
    [Header("背景音乐")]
    public AudioSource bgmSource;
    public AudioClip bgmClip;
    [Range(0f, 1f)]
    public float bgmVolume = 0.5f;
    public bool playOnStart = true;
    
    [Header("音效音量")]
    [Range(0f, 1f)]
    public float sfxVolume = 0.7f;
    
    [Header("音效文件（自动加载）")]
    public AudioClip puzzleCollectSound;      // 收集拼图音效
    public AudioClip doorOpenSound;           // 开门音效
    public AudioClip runeActivateSound;       // 激活符文音效
    public AudioClip alarmSound;              // 警报音效
    public AudioClip guardFootstepsSound;     // 守卫脚步声
    public AudioClip playerStepsSound;        // 玩家脚步声
    public AudioClip waterDripSound;          // 水滴声
    
    private AudioSource sfxSource;            // 音效播放源（2D音效）
    private AudioSource playerSfxSource;     // 角色身上的AudioSource（3D音效）
    private bool isMuted = false;
    private bool isBGMStopped = false;        // 记录BGM是否被停止
    private bool isAllAudioStopped = false;  // 记录是否所有音频都被停止
    private float savedVolume = 0.5f;
    
    void Awake()
    {
        // 单例模式
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        
        // 如果没有AudioSource，创建一个
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
        }
        
        // 配置BGM AudioSource
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;
        
        // 创建音效播放源（用于2D音效）
        sfxSource = gameObject.AddComponent<AudioSource>();
        sfxSource.playOnAwake = false;
        sfxSource.volume = sfxVolume;
        sfxSource.spatialBlend = 0f; // 2D音效
        
        // 尝试获取角色身上的AudioSource（用于3D音效）
        FindPlayerAudioSource();
        
        // 加载保存的音量设置
        LoadAudioSettings();
        
        // 自动加载音效文件
        LoadSFXClips();
    }
    
    // 查找角色身上的AudioSource
    void FindPlayerAudioSource()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerSfxSource = playerController.GetPlayerAudioSource();
                if (playerSfxSource != null)
                {
                    playerSfxSource.volume = sfxVolume;
                    Debug.Log("✅ AudioManager: 已找到角色身上的AudioSource，可用于3D音效");
                }
            }
        }
        
        // 如果没找到，延迟查找（角色可能还没初始化）
        if (playerSfxSource == null)
        {
            StartCoroutine(DelayedFindPlayerAudioSource());
        }
    }
    
    // 延迟查找角色AudioSource
    System.Collections.IEnumerator DelayedFindPlayerAudioSource()
    {
        yield return new WaitForSeconds(0.5f);
        
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerSfxSource = playerController.GetPlayerAudioSource();
                if (playerSfxSource != null)
                {
                    playerSfxSource.volume = sfxVolume;
                    Debug.Log("✅ AudioManager: 延迟查找成功，已找到角色身上的AudioSource");
                }
            }
        }
    }
    
    void Start()
    {
        // 自动加载背景音乐（如果Inspector中没有设置）
        if (bgmClip == null)
        {
            // 尝试从Resources文件夹加载
            bgmClip = Resources.Load<AudioClip>("Audio/bgm");
            
            // 如果Resources中没有，尝试直接加载
            if (bgmClip == null)
            {
                // 使用UnityEditor的方式加载（仅在编辑器中）
                #if UNITY_EDITOR
                bgmClip = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/bgm.wav");
                #endif
            }
            
            if (bgmClip == null)
            {
                Debug.LogWarning("AudioManager: bgmClip未设置且无法自动加载，请在Unity编辑器的Inspector中设置背景音乐文件（Assets/Audio/bgm.wav）");
            }
            else
            {
                Debug.Log("✅ AudioManager: 已自动加载背景音乐 bgm.wav");
            }
        }
        
        // 如果设置了背景音乐，播放它
        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            if (playOnStart && !isMuted)
            {
                bgmSource.Play();
                Debug.Log("✅ AudioManager: 背景音乐已开始播放");
            }
        }
    }
    
    // 播放/暂停背景音乐
    public void ToggleBGM()
    {
        if (bgmSource == null)
        {
            Debug.LogWarning("⚠️ AudioManager.ToggleBGM: bgmSource为空！");
            return;
        }
        
        Debug.Log($"🔊 ToggleBGM() 被调用，当前播放状态: {bgmSource.isPlaying}");
        
        if (bgmSource.isPlaying)
        {
            Debug.Log("🔇 停止BGM");
            StopBGM();
            isBGMStopped = true;
        }
        else
        {
            Debug.Log("▶️ 播放BGM");
            PlayBGM();
            isBGMStopped = false;
        }
        
        Debug.Log($"ToggleBGM() 执行后，播放状态: {bgmSource.isPlaying}, isBGMStopped: {isBGMStopped}");
    }
    
    // 停止所有音乐（包括BGM和音效）
    public void StopAllAudio()
    {
        Debug.Log("🔇 StopAllAudio() 被调用");
        
        isAllAudioStopped = true; // 设置标志，阻止新的音效播放
        
        // 停止BGM
        if (bgmSource != null)
        {
            if (bgmSource.isPlaying)
            {
                Debug.Log("停止BGM播放");
                bgmSource.Stop();
            }
            isBGMStopped = true;
            Debug.Log($"BGM已停止，isBGMStopped设置为: {isBGMStopped}");
        }
        else
        {
            Debug.LogWarning("⚠️ StopAllAudio: bgmSource为空！");
        }
        
        // 停止AudioManager的SFX源
        if (sfxSource != null && sfxSource.isPlaying)
        {
            Debug.Log("停止SFX播放");
            sfxSource.Stop();
        }
        
        // 停止角色身上的AudioSource
        if (playerSfxSource != null && playerSfxSource.isPlaying)
        {
            Debug.Log("停止角色AudioSource播放");
            playerSfxSource.Stop();
        }
        
        // 停止场景中所有其他AudioSource
        AudioSource[] allAudioSources = FindObjectsOfType<AudioSource>();
        int stoppedCount = 0;
        foreach (AudioSource audioSource in allAudioSources)
        {
            // 跳过BGM和SFX源（已经处理过了）
            if (audioSource == bgmSource || audioSource == sfxSource || audioSource == playerSfxSource)
                continue;
            
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
                stoppedCount++;
            }
        }
        
        if (stoppedCount > 0)
        {
            Debug.Log($"✅ 停止了 {stoppedCount} 个其他AudioSource");
        }
        
        Debug.Log("✅ 所有音频已停止");
    }
    
    // 恢复所有音乐
    public void ResumeAllAudio()
    {
        Debug.Log("▶️ ResumeAllAudio() 被调用");
        
        isAllAudioStopped = false; // 清除标志，允许音效播放
        
        if (bgmSource != null && bgmClip != null)
        {
            Debug.Log($"BGM状态 - isPlaying: {bgmSource.isPlaying}, isMuted: {isMuted}, clip: {(bgmClip != null ? bgmClip.name : "null")}");
            
            if (!bgmSource.isPlaying && !isMuted)
            {
                bgmSource.clip = bgmClip;
                bgmSource.Play();
                isBGMStopped = false;
                Debug.Log($"✅ 背景音乐已恢复播放，isBGMStopped设置为: {isBGMStopped}");
            }
            else
            {
                Debug.Log($"无法恢复BGM - isPlaying: {bgmSource.isPlaying}, isMuted: {isMuted}");
            }
        }
        else
        {
            Debug.LogWarning($"⚠️ ResumeAllAudio: bgmSource或bgmClip为空 - bgmSource: {(bgmSource != null ? "存在" : "null")}, bgmClip: {(bgmClip != null ? bgmClip.name : "null")}");
        }
    }
    
    // 检查BGM是否被停止
    public bool IsBGMStopped()
    {
        return isBGMStopped;
    }
    
    // 播放背景音乐
    public void PlayBGM()
    {
        if (bgmSource == null) return;
        
        if (bgmClip != null && bgmSource.clip != bgmClip)
        {
            bgmSource.clip = bgmClip;
        }
        
        if (!isMuted && !bgmSource.isPlaying)
        {
            bgmSource.Play();
            Debug.Log("背景音乐已播放");
        }
    }
    
    // 暂停背景音乐
    public void PauseBGM()
    {
        if (bgmSource == null) return;
        
        if (bgmSource.isPlaying)
        {
            bgmSource.Pause();
            Debug.Log("背景音乐已暂停");
        }
    }
    
    // 停止背景音乐
    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }
    
    // 静音/取消静音
    public void ToggleMute()
    {
        isMuted = !isMuted;
        
        if (isMuted)
        {
            savedVolume = bgmSource.volume;
            bgmSource.volume = 0f;
            Debug.Log("音频已静音");
        }
        else
        {
            bgmSource.volume = savedVolume;
            if (!bgmSource.isPlaying && bgmClip != null)
            {
                bgmSource.Play();
            }
            Debug.Log("音频已取消静音");
        }
        
        SaveAudioSettings();
    }
    
    // 设置背景音乐音量
    public void SetBGMVolume(float volume)
    {
        bgmVolume = Mathf.Clamp01(volume);
        if (bgmSource != null && !isMuted)
        {
            bgmSource.volume = bgmVolume;
        }
        SaveAudioSettings();
    }
    
    // 设置音效音量
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        SaveAudioSettings();
    }
    
    // 检查是否正在播放
    public bool IsPlaying()
    {
        return bgmSource != null && bgmSource.isPlaying;
    }
    
    // 检查是否静音
    public bool IsMuted()
    {
        return isMuted;
    }
    
    // 获取当前音量
    public float GetVolume()
    {
        return bgmSource != null ? bgmSource.volume : 0f;
    }
    
    // 保存音频设置到PlayerPrefs
    void SaveAudioSettings()
    {
        PlayerPrefs.SetFloat("BGMVolume", bgmVolume);
        PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
        PlayerPrefs.SetInt("IsMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();
    }
    
    // 从PlayerPrefs加载音频设置
    void LoadAudioSettings()
    {
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            bgmVolume = PlayerPrefs.GetFloat("BGMVolume", 0.5f);
        }
        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            sfxVolume = PlayerPrefs.GetFloat("SFXVolume", 0.7f);
        }
        if (PlayerPrefs.HasKey("IsMuted"))
        {
            isMuted = PlayerPrefs.GetInt("IsMuted", 0) == 1;
        }
        
        // 应用设置
        if (bgmSource != null)
        {
            bgmSource.volume = isMuted ? 0f : bgmVolume;
        }
        if (sfxSource != null)
        {
            sfxSource.volume = sfxVolume;
        }
    }
    
    // 自动加载音效文件
    void LoadSFXClips()
    {
        // 优先从Resources文件夹加载（构建后可用）
        if (puzzleCollectSound == null)
        {
            puzzleCollectSound = Resources.Load<AudioClip>("Audio/puzzlepieces_collect");
            #if UNITY_EDITOR
            // 如果Resources中没有，在编辑器中尝试从Assets路径加载
            if (puzzleCollectSound == null)
                puzzleCollectSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/puzzlepieces_collect.mp3");
            #endif
        }
        
        if (doorOpenSound == null)
        {
            doorOpenSound = Resources.Load<AudioClip>("Audio/door_open");
            #if UNITY_EDITOR
            if (doorOpenSound == null)
                doorOpenSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/door_open.wav");
            #endif
        }
        
        if (runeActivateSound == null)
        {
            runeActivateSound = Resources.Load<AudioClip>("Audio/rune_activate");
            #if UNITY_EDITOR
            if (runeActivateSound == null)
                runeActivateSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/rune_activate.wav");
            #endif
        }
        
        if (alarmSound == null)
        {
            alarmSound = Resources.Load<AudioClip>("Audio/alarm");
            #if UNITY_EDITOR
            if (alarmSound == null)
                alarmSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/alarm.wav");
            #endif
        }
        
        if (guardFootstepsSound == null)
        {
            guardFootstepsSound = Resources.Load<AudioClip>("Audio/guard_footsteps");
            #if UNITY_EDITOR
            if (guardFootstepsSound == null)
                guardFootstepsSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/guard_footsteps.wav");
            #endif
        }
        
        if (playerStepsSound == null)
        {
            playerStepsSound = Resources.Load<AudioClip>("Audio/player_steps");
            #if UNITY_EDITOR
            if (playerStepsSound == null)
                playerStepsSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/player_steps.wav");
            #endif
        }
        
        if (waterDripSound == null)
        {
            waterDripSound = Resources.Load<AudioClip>("Audio/water_drip01");
            #if UNITY_EDITOR
            if (waterDripSound == null)
                waterDripSound = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/water_drip01.wav");
            #endif
        }
        
        // 输出加载结果
        Debug.Log($"音效加载结果 - puzzleCollectSound: {(puzzleCollectSound != null ? "✓" : "✗")}, " +
                  $"doorOpenSound: {(doorOpenSound != null ? "✓" : "✗")}, " +
                  $"runeActivateSound: {(runeActivateSound != null ? "✓" : "✗")}, " +
                  $"alarmSound: {(alarmSound != null ? "✓" : "✗")}");
    }
    
    // ========== 音效播放方法 ==========
    
    // 播放收集拼图音效（3D音效，从角色位置播放）
    public void PlayPuzzleCollectSound()
    {
        PlaySFX3D(puzzleCollectSound);
    }
    
    // 播放开门音效
    public void PlayDoorOpenSound()
    {
        PlaySFX(doorOpenSound);
    }
    
    // 播放激活符文音效
    public void PlayRuneActivateSound()
    {
        PlaySFX(runeActivateSound);
    }
    
    // 播放警报音效
    public void PlayAlarmSound()
    {
        PlaySFX(alarmSound);
    }
    
    // 播放守卫脚步声
    public void PlayGuardFootstepsSound()
    {
        PlaySFX(guardFootstepsSound);
    }
    
    // 播放玩家脚步声
    public void PlayPlayerStepsSound()
    {
        PlaySFX(playerStepsSound);
    }
    
    // 播放水滴声
    public void PlayWaterDripSound()
    {
        PlaySFX(waterDripSound);
    }
    
    // 通用音效播放方法（2D音效，从AudioManager播放）
    public void PlaySFX(AudioClip clip)
    {
        if (clip == null || sfxSource == null) return;
        
        // 如果所有音频都被停止，不播放音效
        if (isAllAudioStopped)
        {
            Debug.Log("🔇 音效被阻止播放（所有音频已停止）");
            return;
        }
        
        sfxSource.PlayOneShot(clip, sfxVolume);
    }
    
    // 在角色位置播放3D音效（使用角色身上的AudioSource）
    public void PlaySFX3D(AudioClip clip)
    {
        if (clip == null) return;
        
        // 如果所有音频都被停止，不播放音效
        if (isAllAudioStopped)
        {
            Debug.Log("🔇 3D音效被阻止播放（所有音频已停止）");
            return;
        }
        
        // 优先使用角色身上的AudioSource
        if (playerSfxSource != null)
        {
            playerSfxSource.PlayOneShot(clip, sfxVolume);
        }
        // 如果没有找到角色的AudioSource，使用PlayClipAtPoint
        else
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                AudioSource.PlayClipAtPoint(clip, player.transform.position, sfxVolume);
            }
            else
            {
                // 最后回退到2D音效
                PlaySFX(clip);
            }
        }
    }
    
    // 在指定位置播放音效（3D音效）
    public void PlaySFXAtPoint(AudioClip clip, Vector3 position)
    {
        if (clip == null) return;
        
        // 如果所有音频都被停止，不播放音效
        if (isAllAudioStopped)
        {
            Debug.Log("🔇 位置音效被阻止播放（所有音频已停止）");
            return;
        }
        
        AudioSource.PlayClipAtPoint(clip, position, sfxVolume);
    }
    
    // 检查是否所有音频都被停止
    public bool IsAllAudioStopped()
    {
        return isAllAudioStopped;
    }
}

