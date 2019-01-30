using MidiPlayerTK;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace MidiPlayerTK
{
    public class GoMainMenu : MonoBehaviour
    {
        static string sceneMainMenu = "ScenesDemonstration";
       static public void Go()
        {
            SceneManager.LoadScene(sceneMainMenu, LoadSceneMode.Single);
        }
        public void GoToMainMenu()
        {
            SceneManager.LoadScene(sceneMainMenu, LoadSceneMode.Single);
        }
    }
}