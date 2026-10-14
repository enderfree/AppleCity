using UnityEngine;
using System.Threading.Tasks;

/// <summary>
/// Play sounds in place, once
/// </summary>
public class PlaySound : MonoBehaviour
{
    [Header("Sound")]
    // the audio source component of the game object where the sound is generating from
    [SerializeField] private AudioSource _audioPlayer;

    // the audio clip to play
    [SerializeField] private AudioClip _soundClip;

    // how long to play the audio for
    [SerializeField] private float _disableTime;

    /// <summary>
    /// play clip when this object is instanced
    /// </summary>
    private void Start()
    { 
        PlayClip(_soundClip, _disableTime);
    }

    /// <summary>
    /// play the clip and then destroy this object
    /// </summary>
    /// <param name="soundClip"> the clip to play</param>
    /// <param name="time">how long to play the clip</param>
    public  void PlayClip(AudioClip soundClip, float time)
    {
        this.GetComponent<AudioSource>().PlayOneShot(soundClip);
        Destroy(gameObject, time);
    }


}
