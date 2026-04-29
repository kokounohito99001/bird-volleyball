using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [System.Serializable]
    public class Sound
    {
        public string name;
        public AudioClip clip;
        [Range(0f, 1f)]
        public float volume = 1f;
        public bool loop = false;
    }
    
    public Sound[] sounds;
    
    public static AudioManager Instance { get; private set; }
    
    private AudioSource[] audioSources;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Create audio sources for each sound
            audioSources = new AudioSource[sounds.Length];
            for (int i = 0; i < sounds.Length; i++)
            {
                GameObject sourceObj = new GameObject($"AudioSource_{sounds[i].name}");
                sourceObj.transform.parent = transform;
                audioSources[i] = sourceObj.AddComponent<AudioSource>();
                audioSources[i].clip = sounds[i].clip;
                audioSources[i].volume = sounds[i].volume;
                audioSources[i].loop = sounds[i].loop;
                audioSources[i].playOnAwake = false;
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public void PlaySound(string soundName)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == soundName)
            {
                audioSources[i].Play();
                return;
            }
        }
        Debug.LogWarning($"Sound '{soundName}' not found!");
    }
    
    public void StopSound(string soundName)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == soundName)
            {
                audioSources[i].Stop();
                return;
            }
        }
    }
    
    public void SetVolume(string soundName, float volume)
    {
        for (int i = 0; i < sounds.Length; i++)
        {
            if (sounds[i].name == soundName)
            {
                audioSources[i].volume = Mathf.Clamp01(volume);
                return;
            }
        }
    }
}
