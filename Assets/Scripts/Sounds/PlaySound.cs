using UnityEngine;
using System.Threading.Tasks;

public class PlaySound : MonoBehaviour
{
    [Header("Sound")]
    [SerializeField] private AudioSource _audioPlayer;
    [SerializeField] private AudioClip _soundClip;

    [SerializeField] private float _disableTime;

    private void Start()
    { 
        PlayClip(_soundClip, _disableTime);
    }

    public  void PlayClip(AudioClip _soundClip, float time)
    {
        this.GetComponent<AudioSource>().PlayOneShot(_soundClip);
        Destroy(gameObject, time);
    }


}
