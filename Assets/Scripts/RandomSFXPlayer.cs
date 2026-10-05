using UnityEngine;

public class RandomSFXPlayer : MonoBehaviour
{
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip[] clips;

    public void PlayRandomSFX()
    {
        if (clips == null || clips.Length == 0)
            return;

        int index = Random.Range(0, clips.Length);

        audioSource.PlayOneShot(clips[index]);
    }
}