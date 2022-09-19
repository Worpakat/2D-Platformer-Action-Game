using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject mainMenuPanel;
    [SerializeField] private GameObject introductionPanel;

    //Resolution resolution;

    //private void Start()
    //{
    //    resolution = Screen.currentResolution;
    //    Screen.SetResolution(resolution.width, resolution.height, true);
    //    print("Resolution fixed");
    //}

    public void OnPlayButton()
    {
        mainMenuPanel.SetActive(false);
        introductionPanel.SetActive(true);
    }
    public void OnExitButton()
    {
        print("quitting");
        Application.Quit();
    }
    public void OnStartGameButton()
    {
        SceneManager.LoadScene("SampleScene");
    }
    public void OnBackButton()
    {
        mainMenuPanel.SetActive(true);
        introductionPanel.SetActive(false);
    }
}
