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
    
    private bool isMuted = false;
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
        
        // 配置AudioSource
        bgmSource.loop = true;
        bgmSource.playOnAwake = false;
        bgmSource.volume = bgmVolume;
        
        // 加载保存的音量设置
        LoadAudioSettings();
    }
    
    void Start()
    {
        // 加载背景音乐（如果Inspector中没有设置）
        // 注意：背景音乐需要在Unity编辑器的Inspector中手动设置bgmClip
        if (bgmClip == null)
        {
            Debug.LogWarning("AudioManager: bgmClip未设置，请在Unity编辑器的Inspector中设置背景音乐文件（Assets/Audio/bgm.wav）");
        }
        
        // 如果设置了背景音乐，播放它
        if (bgmClip != null && bgmSource != null)
        {
            bgmSource.clip = bgmClip;
            if (playOnStart && !isMuted)
            {
                bgmSource.Play();
            }
        }
    }
    
    // 播放/暂停背景音乐
    public void ToggleBGM()
    {
        if (bgmSource == null) return;
        
        if (bgmSource.isPlaying)
        {
            PauseBGM();
        }
        else
        {
            PlayBGM();
        }
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
    }
}

