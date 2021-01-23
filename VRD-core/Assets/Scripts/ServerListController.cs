using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class ServerListController : MonoBehaviour
{
     List<UserPreferences.VNCServerDetail> serverDetailsList;
     List<GameObject> serverDetailHolders;
    public GameObject listItemPrefab;
    public  GameObject content;
    public GameObject scrollViewObject;
    public KeyboardInputHandler keyboardInputHandler;
    public TCPConnectionController tcpConnectionController;
    public Text logText;
    public Text keyboardInputField;
    public static bool testMode = false;
    public static UserPreferences.VNCServerDetail selectedDetail;
    // Start is called before the first frame update
    void Start(){  
        serverDetailsList = UserPreferences.VNCServerDetailHolderList.loadFromPlayerPrefs();
        populateServerList();
      
        tcpConnectionController.onLoginOK = ()=>{
            //Debug.Log("disbling scroll view object");
            scrollViewObject.SetActive(false);
        };
    }

    private void populateServerList(){
        serverDetailHolders = new List<GameObject>();
        for(int i=0; i<serverDetailsList.Count; i++){
            //Debug.Log("setting up server detail: " + i.ToString());
            GameObject serverDetailHolder = Instantiate(listItemPrefab, content.transform);
            serverDetailHolder.GetComponent<VNCServerDetailHolder>().serverDetail = serverDetailsList[i];
            serverDetailHolder.GetComponent<VNCServerDetailHolder>().updateDisplayTexts();
            serverDetailHolder.GetComponent<VNCServerDetailHolder>().onConnectButtonDelegate = (d)=>{
                onConnectToServer(d.serverDetail);
            };
            serverDetailHolders.Add(serverDetailHolder);
        }
    }
    private void onConnectToServer(UserPreferences.VNCServerDetail d){
        if (d.shouldUsePassword){
            tcpConnectionController.onPasswordRequestedDelegate = (onFinished)=>{
                onFinished(d.password);
            };
        }
        tcpConnectionController.onLoginOK = ()=>{
            //Debug.Log("disbling scroll view object");
            scrollViewObject.SetActive(false);
        };
        tcpConnectionController.startVNCConnection(
            d.serverName, d.serverPort
        );
           }
    public void shouldEditServerDetailAtIndex(int serverDetailIndex){
        UserPreferences.VNCServerDetail detail = serverDetailsList[serverDetailIndex];
        logText.text = "New server name: ";
        keyboardInputField.text = detail.serverName;
        SceneManager.LoadScene("EditVNCListScene");
    }
    private void OnDestroy(){
        for(int i=0; i<serverDetailHolders.Count; i++){
            Destroy(serverDetailHolders[i], 0.0f);
        }
        serverDetailsList = null;
    }
 }
