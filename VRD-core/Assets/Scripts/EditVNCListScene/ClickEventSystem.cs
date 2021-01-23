using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Threading;

public class ClickEventSystem : MonoBehaviour
{
    //public static UserPreferences.VNCServerDetail serverDetail;
    List<UserPreferences.VNCServerDetail> serverDetailsList;
    List<GameObject> serverDetailHolders;
    private UserPreferences.VNCServerDetail serverDetail;
    private VNCServerDetailHolder currentServerDetailHolder;
    public Text serverHostNameText;
    public Text serverPortNameText;
    public Toggle shouldUsePasswordToggle;
    public Text passwordText;
    public KeyboardInputHandler keyboardInputHandler;
    private Text activeTextView = null;
    public static string previousSceneName = "SpaceScene";
    public GameObject content;
    public GameObject listItemPrefab;
    public GameObject keyboardObj;
    private SynchronizationContext UIThreadContext;

    public GameObject editPasswordButton;
    public GameObject editServerNameButton;
    public GameObject editPortButton;
   
    void Start(){
        UIThreadContext = SynchronizationContext.Current;
        
        //serverDetail = ServerListController.selectedDetail;
        if (serverDetail == null){
            serverDetail = new UserPreferences.VNCServerDetail
                ("sampleHost", 5900, 10, false, "");
        }
       
        keyboardObj.SetActive(false);
        keyboardInputHandler.onDeleteDelegate = ()=>{onKeyboardDelete();};
        var asyncPrepareThread = new Thread(new ThreadStart(asyncPrepare));
        asyncPrepareThread.IsBackground = true;
        asyncPrepareThread.Start();
        //asyncPrepare();
    }
    private void asyncPrepare(){
        serverDetailHolders = new List<GameObject>();
        //showServerDetail();
        keyboardInputHandler.onTypedDelegate = (typedContent)=>{
            onKeyboardTyped(typedContent);
        };
        keyboardInputHandler.onEnterDelegate = ()=>{
            onKeyboardEnter();
        };
        runOnUIThread(()=>{
            serverDetailsList = UserPreferences.VNCServerDetailHolderList.loadFromPlayerPrefs();
            populateServerList();
            if (serverDetailHolders.Count >= 0){
             onEditClickedFor(serverDetailHolders[0].GetComponent<VNCServerDetailHolder>());
            }
        });
    }
    private void clearServerList(){
        for(int i=0; i<serverDetailHolders.Count; i++){
            Destroy(serverDetailHolders[i], 0);
        }
        serverDetailHolders = new List<GameObject>();
    }
    private void populateServerList(){
        for(int i=0; i<serverDetailsList.Count; i++){
            addListItemPrefab(serverDetailsList[i]);
        }
    }
    private void runOnUIThread(Action action){
        //StartCoroutine(runInLateUpdate(action));
        UIThreadContext.Post(__ =>{
            action();
        }, null);
    }
    private GameObject addListItemPrefab(UserPreferences.VNCServerDetail detail){
        GameObject listItem = Instantiate(listItemPrefab, content.transform);
        listItem.GetComponent<VNCServerDetailHolder>().serverDetail = detail;
        listItem.GetComponent<VNCServerDetailHolder>().updateDisplayTexts();
        listItem.GetComponent<VNCServerDetailHolder>().onEditButtonDelegate = (d)=>{
            onEditClickedFor(d);
        };
        listItem.GetComponent<VNCServerDetailHolder>().onDeleteButtonDelegate = (d)=>{
            onDeleteClickedFor(d);
        };
        serverDetailHolders.Add(listItem);
        return listItem;
    }
    public void onEditClickedFor(VNCServerDetailHolder d){
        serverDetail = d.serverDetail;
        currentServerDetailHolder = d;
        clearBackgroundColors();
        d.highlightBackgroundColor();
        showServerDetail();
        //keyboardObj.SetActive(true);
        editPasswordButton.SetActive(true);
        editServerNameButton.SetActive(true);
        editPortButton.SetActive(true);
    }
    
    private void clearBackgroundColors(){
        for(int i=0; i<serverDetailHolders.Count; i++){
            serverDetailHolders[i].GetComponent<VNCServerDetailHolder>().clearBackgroundColor();
        }
    }
    public void onDeleteClickedFor(VNCServerDetailHolder d){
        //Debug.Log("deleting server details list...");
        serverDetailsList.Remove(d.serverDetail);
        clearServerList();
        populateServerList();
        onEditClickedFor(serverDetailHolders[0].GetComponent<VNCServerDetailHolder>());
    }

    public void onKeyboardEnter(){
        activeTextView = null;
        onSaveClicked();
    }

    public void onKeyboardDelete(){
        if (activeTextView == null){
            return;
        }
        if (activeTextView.text.Length > 0){
            activeTextView.text = activeTextView.text.Remove(activeTextView.text.Length-1);
        }
    }

    public void onKeyboardTyped(string typedContent){
        if (activeTextView == null){
            return;
        }
        activeTextView.text = activeTextView.text + typedContent;
    }
    private void showServerDetail(){
        serverHostNameText.text = serverDetail.serverName;
        serverPortNameText.text = serverDetail.serverPort.ToString();
        shouldUsePasswordToggle.isOn = serverDetail.shouldUsePassword;
        if (serverDetail.shouldUsePassword){
            passwordText.text = serverDetail.password;
        } else {
            passwordText.text = "";
        }
    }
    public void onEditServerNameClicked(){
        keyboardObj.SetActive(true);
        activeTextView = serverHostNameText;
    }
    public void onEditServerPortClicked(){
        keyboardObj.SetActive(true);
        activeTextView = serverPortNameText;
    }
    public void onEditPasswordClicked(){
        if (!shouldUsePasswordToggle.isOn){
            activeTextView = null;
            return;
        }
        keyboardObj.SetActive(true);
        activeTextView = passwordText;
    }
    public void onGoBackClicked(){
        //SceneManager.LoadScene(previousSceneName);
        string defaultSceneName = PlayerPrefs.GetString("default_scene");
        if (defaultSceneName.Equals("")){
            defaultSceneName = "HomeTheaterKeyboard";
        }
        //Debug.Log("next scene=" + defaultSceneName);
        SceneManager.LoadScene(defaultSceneName);
        Destroy(this, 0.0f);
    }
    public void toggleShouldUsePassword(){
        serverDetail.shouldUsePassword = shouldUsePasswordToggle.isOn;
    }
    public void onSaveClicked(){
        if (serverDetail != null){
            if (serverDetail.serverDetailsId < serverDetailsList.Count){
                serverDetail.serverName = serverHostNameText.text;
                //Debug.Log("portName=" + serverPortNameText.text.ToString());
                serverDetail.serverPort = Int32.Parse(serverPortNameText.text);
                serverDetail.shouldUsePassword = shouldUsePasswordToggle.isOn;
                if (shouldUsePasswordToggle.isOn){
                    serverDetail.password = passwordText.text;
                } else {
                    serverDetail.password = "";
                }
                serverDetailsList[serverDetail.serverDetailsId] = serverDetail;
            }
            //serverDetail.updateDisplayTexts();
            currentServerDetailHolder.serverDetail = serverDetail;
            currentServerDetailHolder.updateDisplayTexts();
        }
        UserPreferences.VNCServerDetailHolderList.saveServerDetails(serverDetailsList);
        //clearServerList();
        //populateServerList();
        activeTextView = null;
        keyboardObj.SetActive(false);
    }
    public void onCancelClicked(){
    }
    public void onAddButtonClicked(){
        var newServerDetail = new UserPreferences.VNCServerDetail(
            "127.0.0.1", 5900, 0, false, ""
        );
        serverDetailsList.Add(newServerDetail);
        VNCServerDetailHolder addedItemPrefab = addListItemPrefab(newServerDetail).GetComponent<VNCServerDetailHolder>();
        onEditClickedFor(addedItemPrefab);
    }

    private void OnDestroy(){
        for(int i=0; i<serverDetailHolders.Count; i++){
            serverDetailHolders[i] = null;
            //Destroy(serverDetailHolders[i], 0.0f);
        }
        serverDetailsList = null;
        serverDetailHolders = null;
        keyboardInputHandler = null;
    }
}
