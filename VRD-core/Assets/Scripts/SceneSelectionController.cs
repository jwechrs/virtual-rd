using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSelectionController : MonoBehaviour{
    private const string defaultSceneKey = "default_scene";
    // Start is called before the first frame update
    void Start(){
        // string defaultSceneIndex = PlayerPrefs.GetString(defaultSceneKey, "HomeTheaterKeyboard");
        // openScene(defaultSceneIndex);
    }

    public void openScene(string sceneName){
        PlayerPrefs.SetString(defaultSceneKey, sceneName);
        PlayerPrefs.Save();
        SceneManager.LoadScene(sceneName);
    }
    public void openSceneNoRecord(string sceneName){
        SceneManager.LoadScene(sceneName);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
