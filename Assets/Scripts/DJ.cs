using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Audio;

namespace DiscoJockeyNamespace{
  [System.Serializable]
    public class AudioTrack{
        public string stateName;
        public AudioClip audioClip;
    } 
  public class DJ : MonoBehaviour{
    public AudioSource firstChannel;
    public AudioSource secondChannel;
    public AudioSource sfxChannel;
    private bool isFirstChannelPlaying = true;
    public  float maxVolume = 1f;
    public readonly float fadeDuration = 2.0f; // Duration of fade transition in seconds
    private Coroutine currentFadeCoroutine;
    private static DJ instance;
    [SerializeField] private List<AudioTrack> tracks;

    private void Start(){
        AudioSource[] audioSources = GetComponents<AudioSource>();
        firstChannel = audioSources[0];
        secondChannel = audioSources[1];
        sfxChannel = audioSources[2];

        DontDestroyOnLoad(this.gameObject);
    }

    public void PlayTrack(string name)
    {
        AudioTrack track = tracks.Find(t => t.stateName == name);
        if (track == null)
        {
            Logger.Error($"Track not found: {name}");
            return;
        }

        if(isFirstChannelPlaying && firstChannel.clip == track.audioClip){
            return;
        }else if(!isFirstChannelPlaying && secondChannel.clip == track.audioClip){
            return;
        }

        // Stop any current fade coroutine
        if (currentFadeCoroutine != null)
        {
            StopCoroutine(currentFadeCoroutine);
        }

        // Start the fade transition
        currentFadeCoroutine = StartCoroutine(FadeToTrack(track.audioClip));
    }

    private IEnumerator FadeToTrack(AudioClip newClip)
    {
        AudioSource currentChannel = isFirstChannelPlaying ? firstChannel : secondChannel;
        AudioSource nextChannel = isFirstChannelPlaying ? secondChannel : firstChannel;

        // Set up the next channel
        nextChannel.clip = newClip;
        nextChannel.volume = 0f;
        nextChannel.Play();

        float timer = 0f;

        // Perform the fade
        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / fadeDuration;

            // Fade out current channel
            currentChannel.volume = Mathf.Lerp(maxVolume, 0f, progress);
            // Fade in next channel
            nextChannel.volume = Mathf.Lerp(0f, maxVolume, progress);

            yield return null;
        }

        // Ensure final volumes are set correctly
        currentChannel.volume = 0f;
        nextChannel.volume = maxVolume;

        // Stop the previous channel and switch tracking
        currentChannel.Stop();
        isFirstChannelPlaying = !isFirstChannelPlaying;

        currentFadeCoroutine = null;
    }

    public void SetMusicVolume(float volume){
      if(isFirstChannelPlaying){
        firstChannel.volume = volume;
      }else{
        secondChannel.volume = volume;
      }
      maxVolume = volume;
    }

    public void SetMusicPitch(float pitch){
        firstChannel.pitch = pitch;
        secondChannel.pitch = pitch;
    }

    public void SetSFXVolume(float volume){
        sfxChannel.volume = volume;
    }

    public static DJ Instance{
      get{
        if(instance == null){
          instance = FindFirstObjectByType<DJ>();
          if(instance == null){
            Logger.Error("DJ instance not found");
          }
        }
        return instance;
      }
    }
    
  }
}
