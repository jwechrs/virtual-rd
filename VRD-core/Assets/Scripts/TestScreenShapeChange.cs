using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

public class TestScreenShapeChange : MonoBehaviour
{
    // Start is called before the first frame update

    public PlaneCurveController planeCurveController;
    public SettingsPlaneController settingsPlaneController;
    public Slider radiusSlider;
    public Slider distanceSlider;
    public bool testMode = false;
    void Start()
    {
        if (!testMode){
            return;
        }
        StartCoroutine(runDelayed(3.5f, ()=>{
               radiusSlider.value = 1.0f;
               distanceSlider.value = 1.0f;
        }
        ));
        StartCoroutine(runDelayed(5.0f, ()=>{
            settingsPlaneController.openSettingsPage();
        }));
        StartCoroutine(runDelayed(6.0f, ()=>{
            settingsPlaneController.closeSettingsPage();
        }));
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    private IEnumerator runDelayed(float waitSeconds, Action action){
        yield return new WaitForSeconds(waitSeconds);
        action();
    }
}
