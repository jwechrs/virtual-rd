using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Text.RegularExpressions;
using System;

public class KeyboardInputHandler : MonoBehaviour
{
    public Text keyboardInputText;
    private bool characterCaps = false;
    public Action onEnterDelegate = null;
    public Action<string> onTypedDelegate = null;
    public bool shouldDisplayOnTextView = true;
    public Action onDeleteDelegate = null;
   
    public void onEnterClicked(){
        //Debug.Log("onEnterClicked");
        if (onEnterDelegate != null){
            onEnterDelegate();
        } 
    }
    public void onTyped(string typedString){
        //Debug.Log(typedString);
        string shiftAwareTypedString = typedString;
        if (Regex.IsMatch(typedString, "[a-z]")){
            //Debug.Log("alphabet input");
            if (!characterCaps){
                //keyboardInputText.text = keyboardInputText.text + typedString;
            } else {
                shiftAwareTypedString = typedString.ToUpper();
                //keyboardInputText.text = keyboardInputText.text + typedString.ToUpper();
            }
        } else {
            //Debug.Log("non-alphabetic input");
            //keyboardInputText.text = keyboardInputText.text + typedString;
        }
        if (shouldDisplayOnTextView){
            keyboardInputText.text = keyboardInputText.text + shiftAwareTypedString;
        }
        if (onTypedDelegate != null){
            onTypedDelegate(shiftAwareTypedString);
        }
    }
    public void onEnter(){

    }
    public void onDelete(){
        if (keyboardInputText.text.Length > 0){
            keyboardInputText.text = keyboardInputText.text.Remove(keyboardInputText.text.Length-1);
        }
        if (onDeleteDelegate != null){
            onDeleteDelegate();
        }
    }
    public void toggleCaps(){
        characterCaps = ! characterCaps;
    }
}
