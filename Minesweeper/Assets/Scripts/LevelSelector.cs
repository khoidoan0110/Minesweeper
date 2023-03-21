using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelSelector : MonoBehaviour
{
    public static int selectedDifficulty;
    public void OpenGameplayScene1(){
        selectedDifficulty = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenGameplayScene2(){
        selectedDifficulty = 2;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void OpenGameplayScene3(){
        selectedDifficulty = 3;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
