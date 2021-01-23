using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class OpenDefaultScene : MonoBehaviour
{
     private const string defaultSceneKey = "default_scene";
    // Start is called before the first frame update
    void Start()
    {
        string defaultSceneIndex = PlayerPrefs.GetString(defaultSceneKey, "HomeTheaterKeyboard");
        //openScene(defaultSceneIndex);
        if (defaultSceneIndex == "EditVNCListScene"){
            defaultSceneIndex = "EnvironmentSelectionScene";
        }
        SceneManager.LoadScene(defaultSceneIndex);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
