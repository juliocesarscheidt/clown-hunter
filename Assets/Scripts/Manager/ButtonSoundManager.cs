using UnityEngine;

public class ButtonSoundManager : MonoBehaviour
{
    public AudioSource menuInteractionAudioSource;

    public void PlaySound() {
        if (menuInteractionAudioSource != null) {
            menuInteractionAudioSource.Play();
        }
    }
}
