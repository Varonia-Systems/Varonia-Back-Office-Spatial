using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using VaroniaBackOffice;

public class Pillier : MonoBehaviour
{

    public GameObject Obstacle_Lobby;
    public GameObject Obstacle_Map;


    void OnEnable()
    {
        SceneManager.sceneLoaded += SceneManager_sceneLoaded;
        Check();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneManager_sceneLoaded;
    }



    private void SceneManager_sceneLoaded(Scene arg0, LoadSceneMode arg1)
    {
        Check();
    }




    void Check()
    {

        if (SceneManager.GetActiveScene().name.Contains(VaroniaGlobal.VG.LobbySceneName) && Obstacle_Lobby != null)
        {
            Obstacle_Lobby.SetActive(true);
            Obstacle_Map.SetActive(false);
        }
        else if (SceneManager.GetActiveScene().name.Contains("Menu") || SceneManager.GetActiveScene().name.Contains("menu"))
        {
            if (Obstacle_Lobby != null)
                Obstacle_Lobby.SetActive(false);

            Obstacle_Map.SetActive(false);
        }
        else
        {
            if (Obstacle_Lobby != null)
                Obstacle_Lobby.SetActive(false);

            Obstacle_Map.SetActive(true);
        }
    }


}
