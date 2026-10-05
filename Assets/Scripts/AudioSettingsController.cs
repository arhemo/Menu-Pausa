using UnityEngine;
using UnityEngine.Audio;

public class AudioSettingsController : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    public void SetMusicVolume(float value)
    {
        audioMixer.SetFloat("MusicVolume", ConvertToDecibels(value));
    }

    public void SetSfxVolume(float value)
    {
        audioMixer.SetFloat("SFXVolume", ConvertToDecibels(value));
    }

    private float ConvertToDecibels(float value)
    {
        if (value <= 0.0001f)
            return -80f;

        return Mathf.Log10(value) * 20f;
    }
}