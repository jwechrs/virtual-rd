using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class VNCServerDetailHolder : MonoBehaviour
{
    public UserPreferences.VNCServerDetail serverDetail;
    public Text serverNameText;
    public Text serverPortText;
    public int serverSettingsId;
    public Action<VNCServerDetailHolder> onEditButtonDelegate;
    public Color highlightColor;
    public Color normalColor;
    public Action<VNCServerDetailHolder> onConnectButtonDelegate;
    public Action<VNCServerDetailHolder> onDeleteButtonDelegate;
    public void updateDisplayTexts(){
        serverNameText.text = serverDetail.serverName;
        serverPortText.text = serverDetail.serverPort.ToString();
    }
    public void onEditButtonClicked(){
        //highlightBackgroundColor();
        if (onEditButtonDelegate != null){
            onEditButtonDelegate(this);
        }
    }
    public void clearBackgroundColor(){
        GetComponent<Image>().color = normalColor; //= new Color(256, 256, 256);
    }
    public void highlightBackgroundColor(){
        GetComponent<Image>().color = highlightColor;
    }
    public void onConnectButtonClicked(){
        if (onConnectButtonDelegate != null){
            onConnectButtonDelegate(this);
        }
    }
    public void onDeleteButtonClicked(){
        if (onDeleteButtonDelegate != null){
            onDeleteButtonDelegate(this);
        }
    }
    private void OnDestroy(){
        onConnectButtonDelegate = null;
        onDeleteButtonDelegate = null;
        onEditButtonDelegate = null;
        serverDetail = null;
    }
}
