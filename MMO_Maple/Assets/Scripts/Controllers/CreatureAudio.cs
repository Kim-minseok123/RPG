using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureAudio : MonoBehaviour
{
    public AudioSource audioSource;
    public void SfxSoundPlay(string name)
    {
        Managers.Sound.Play(name, Define.Sound.Effect, audioSource);
    }
}
