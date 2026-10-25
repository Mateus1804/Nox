using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;

public class PlaceHolderPanel : MonoBehaviour

{
    public HUDAndSounds HUD;
    public static PlaceHolderPanel PHP;
    

    public GameObject placeHolderPanel;
    public GameObject buttonNextLevel;
    public GameObject buttonRestart;
    public Text placeHolderText;

    #region Awake()
    void Awake()
    {
        if (PHP == null)
        {
            PHP = this;
        }

        else
        {
            Destroy(gameObject);
        }
    }
    #endregion

    #region GameWinLose
    public void PanelWinOrLose(string text)
    {
        placeHolderText.text = text;
        placeHolderPanel.SetActive(true);
        PlayerScript.PT.healthBar_Canvas.SetActive(false);

        //Time.timeScale = 0; *Por algum motivo o jogo começa pausado se essa linha não estiver comentada. (João)
        if (text == "Stage Completed")
        {
            buttonNextLevel.SetActive(true);
            buttonRestart.SetActive(false);
        }
        if (text == "Game Over")
        {
            buttonNextLevel.SetActive(false);
            buttonRestart.SetActive(true);
        }

        if (HUD.main_menu.activeSelf == true)
        {
            placeHolderPanel.SetActive(false);
        }
    }
    #endregion

    #region ButtonMenu
    public void BackPlaceHolder_Button()
    {
        SceneManager.LoadScene(0);
        HUD.Click();
        HUD.main_menu.SetActive(true);
        HUD.levels_menu.SetActive(true);
        placeHolderPanel.SetActive(false);
    }
    #endregion
    public void ButtonRestart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        Time.timeScale = 1;
        HUD.Click();
        PlayerScript.PT.hp = 3;
        placeHolderPanel.SetActive(false);
    }

    public void ButtonNextLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
        Time.timeScale = 1;
        placeHolderPanel.SetActive(false);
        HUD.Click();
        HUD.in_GameMusic.Stop();
        HUD.in_GameMusic2.Play();
    }

}
