using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    public AudioSource clipOneSFX;
    public AudioSource clipTwoSFX;


    public void PlaySFXOne()
    {
        clipOneSFX.Play();
    }

    public void PlaySFXTwo()
    {
        clipTwoSFX.Play();
    }
}
