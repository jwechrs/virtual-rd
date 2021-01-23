using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SettingsPlaneController : MonoBehaviour{
    public GameObject settingsPlaneObject;
    public GameObject keyboardObject;
    private bool previousKeyboardObjectVisibility = false;
    public bool isFocused = false;
    // Start is called before the first frame update
    void Start(){
    }

    // Update is called once per frame
    void Update(){
    }
    public void openSettingsPage(){
        previousKeyboardObjectVisibility = keyboardObject.activeSelf;
        settingsPlaneObject.SetActive(true);
        keyboardObject.SetActive(false);
        isFocused = true;
    }
    public void closeSettingsPage(){
        settingsPlaneObject.SetActive(false);
        keyboardObject.SetActive(previousKeyboardObjectVisibility);
        isFocused = false;
    }
}
