using UnityEngine;
using UnityEngine.Audio;
using System.Collections;

public class AudioManager : MonoBehaviour {

    [System.Serializable]
    public class AudioMusicTileset
    {
        [Tileset]
        public string tileset;
        public AudioClip clip;
    }

    public enum AudioType
    {
        Gun,
        Health,
        Pickups,
        Other,
        UI
    }

    public AudioMixer musicMixer;
    public AudioMixer effectsMixer;

    public AudioMixerGroup gunGroup;
    public AudioMixerGroup hitsGroup;
    public AudioMixerGroup pickupsGroup;
    public AudioMixerGroup otherGroup;
    public AudioMixerGroup uiGroup;

    public AudioMusicTileset[] musicGame;
    public AudioClip[] musicUpgrades;
    public AudioClip[] musicMenu;
    public AudioClip[] musicBoss;

    AudioSource source;
    bool lastPlaying;
    bool lastInMenu;
    bool lastInGame;
    bool lastBoss;
    bool lastGenerating;

    public static AudioManager singleton;

    public static void Assign(AudioSource source, AudioType type = AudioType.Other)
    {
        if (!singleton) singleton = FindObjectOfType<AudioManager>();

        if(source)
        {
            if (type == AudioType.Gun) source.outputAudioMixerGroup = singleton.gunGroup;
            if (type == AudioType.Health) source.outputAudioMixerGroup = singleton.hitsGroup;
            if (type == AudioType.Other) source.outputAudioMixerGroup = singleton.otherGroup;
            if (type == AudioType.Pickups) source.outputAudioMixerGroup = singleton.pickupsGroup;
            if (type == AudioType.UI) source.outputAudioMixerGroup = singleton.uiGroup;
        }
    }

    void Awake()
    {
        singleton = this;
        lastPlaying = true;
        source = GetComponent<AudioSource>();
    }

    bool IsMenuMusic
    {
        get
        {
            for(int i = 0; i < musicMenu.Length;i++)
            {
                if (musicMenu[i] == source.clip) return true;
            }
            return false;
        }
    }

    void PlayMusic()
    {
        bool inMenu = !Character.Player || GeneratorManager.Generating;
        if (inMenu)
        {
            if(!IsMenuMusic)
            {
                //main menu
                source.clip = musicMenu[Random.Range(0, musicMenu.Length)];
                source.Play();
            }
        }
        else
        {
            bool inGame = !PickerItems.Active;
            if (inGame)
            {
                bool fightingBoss = IsFighting();
                //ingame
                if(fightingBoss)
                {
                    source.clip = musicBoss[Random.Range(0, musicBoss.Length)];
                    source.Play();
                }
                else
                {
                    for(int i = 0; i < musicGame.Length;i++)
                    {
                        if(musicGame[i].tileset == Generator.singleton.preset.tileset)
                        {
                            source.clip = musicGame[i].clip;
                            source.Play();
                        }
                    }
                }
            }
            else
            {
                //picker build
                source.clip = musicUpgrades[Random.Range(0, musicUpgrades.Length)];
                source.Play();
            }
        }
    }

    bool IsFighting()
    {
        if (!Character.Player) return false;

        if (Character.Player.InsideBossRoom)
        {
            if (EnemyManager.BossesAlive > 0)
            {
                return true;
            }
        }
        return false;
    }

    void Update()
    {
        float vol = Settings.Setting.volumeMusic.Remap(0f, 100f, -80f, 20f);
        musicMixer.SetFloat("vol", vol);
        vol = Settings.Setting.volumeEffects.Remap(0f, 100f, -80f, 20f);
        effectsMixer.SetFloat("vol", vol);

        if (HUD.Layer.StartsWith("Intro")) return;

        singleton = this;
        
        bool updated = false;
        if(lastInGame != !PickerItems.Active)
        {
            lastInGame = !lastInGame;
            updated = true;
        }
        if (lastGenerating != GeneratorManager.Generating)
        {
            lastGenerating = !lastGenerating;
            updated = true;
        }
        if (lastInMenu != Character.Player)
        {
            lastInMenu = !lastInMenu;
            updated = true;
        }
        if(lastPlaying != source.isPlaying)
        {
            lastPlaying = source.isPlaying;
            updated = true;
        }
        if(lastInGame)
        {
            bool fightingBoss = IsFighting();
            if (lastBoss != fightingBoss)
            {
                lastBoss = fightingBoss;
                updated = true;
            }
        }
        if(updated)
        {
            PlayMusic();
        }
    }
}
