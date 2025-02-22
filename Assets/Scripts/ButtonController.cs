using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonController : MonoBehaviour
{
    public Animator animator;

    public GameObject optionsMenu;
    public GameObject titleObject;

    public void StartGameFade()
    {
        animator.SetTrigger("StartGame");
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Intro");
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void OptionsMenu()
    {
        if (optionsMenu.activeSelf)
        {
            optionsMenu.SetActive(false);
            titleObject.SetActive(true);
        }
        else
        {
            optionsMenu.SetActive(true);
            titleObject.SetActive(false);

        }
    }

}


