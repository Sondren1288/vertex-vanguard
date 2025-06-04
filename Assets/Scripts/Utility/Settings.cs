using UnityEngine;


namespace VertexVanguard.Utility {
  public class Settings : MonoBehaviour {
    public float musicVolume = 1f;
    public float sfxVolume = 1f;
    public float musicPitch = 1f;
    public bool isFullscreen = false;
    public bool isBordered = false;
    public bool isBorderless = false;
    public int targetFrameRate = 60;
    public int vSyncCount = 1;
    private static Settings instance;
    public static Settings Instance {
      get{
        if(instance == null){
          instance = FindFirstObjectByType<Settings>();
        }
        return instance;
      }
    }

    public void SetVSyncCount(int vSyncCount){
      this.vSyncCount = vSyncCount;
      QualitySettings.vSyncCount = vSyncCount;
    }

    public void SetTargetFrameRate(int targetFrameRate){
      this.targetFrameRate = targetFrameRate;
      Application.targetFrameRate = targetFrameRate;
    }
    

    public void SaveSettings(){
      PlayerPrefs.SetFloat("musicVolume", musicVolume);
      PlayerPrefs.SetFloat("sfxVolume", sfxVolume);
      PlayerPrefs.SetFloat("musicPitch", musicPitch);
      PlayerPrefs.SetInt("isFullscreen", isFullscreen ? 1 : 0);
      PlayerPrefs.SetInt("isBordered", isBordered ? 1 : 0);
      PlayerPrefs.SetInt("isBorderless", isBorderless ? 1 : 0);
      PlayerPrefs.SetInt("targetFrameRate", targetFrameRate);
      PlayerPrefs.SetInt("vSyncCount", vSyncCount);
    }

    public void LoadSettings(){
      musicVolume = PlayerPrefs.GetFloat("musicVolume", 1f);
      sfxVolume = PlayerPrefs.GetFloat("sfxVolume", 1f);
      musicPitch = PlayerPrefs.GetFloat("musicPitch", 1f);
      isFullscreen = PlayerPrefs.GetInt("isFullscreen", 0) == 1;
      isBordered = PlayerPrefs.GetInt("isBordered", 0) == 1;
      isBorderless = PlayerPrefs.GetInt("isBorderless", 0) == 1;
      targetFrameRate = PlayerPrefs.GetInt("targetFrameRate", 60);
      vSyncCount = PlayerPrefs.GetInt("vSyncCount", 1);

      SetVSyncCount(vSyncCount);
      SetTargetFrameRate(targetFrameRate);
    }
  } 
}