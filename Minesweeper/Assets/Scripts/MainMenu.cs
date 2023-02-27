using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_InputField fieldX, fieldY, numBombsField;
    int number;
    public void NewGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1, LoadSceneMode.Single);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void GetCustomFieldX()
    {
        PlayerPrefs.SetInt("X", int.TryParse(fieldX.text, out number) ? int.Parse(fieldX.text) : 16);
    }

    public void GetCustomFieldY()
    {
        PlayerPrefs.SetInt("Y", int.TryParse(fieldY.text, out number) ? int.Parse(fieldY.text) : 16);
    }

    public void GetCustomFieldNumBombs()
    {
        PlayerPrefs.SetInt("NumBombs", int.TryParse(numBombsField.text, out number) ? int.Parse(numBombsField.text) : 32);
    }
}
