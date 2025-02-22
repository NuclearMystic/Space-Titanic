using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlaySFX : MonoBehaviour
{
    public AudioSource clipOneSFX;
    public AudioSource clipTwoSFX;
    public AudioSource clipThreeSFX;


    public void PlaySFXOne()
    {
        clipOneSFX.Play();
    }

    public void PlaySFXTwo()
    {
        clipTwoSFX.Play();
    }

    public void PlaySFXThree()
    {
        clipThreeSFX.Play();
    }
}
