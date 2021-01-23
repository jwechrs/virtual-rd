

using UnityEngine.UI;
using UnityEngine;
public class TestUtils{
    public static void clickButtonOn(GameObject buttonObject){
        buttonObject.GetComponent<Button>().onClick.Invoke();
    }
    public static void clickButtonOnName(string buttonObjectName){
        //buttonObject.GetComponent<Button>().onClick.Invoke();
        clickButtonOn(GameObject.Find(buttonObjectName));
    }
    public static void hitEnterKey(){
        TestUtils.clickButtonOnName("Enter");
    }
    public static void typeString(string typeContent){
        for(int i=0; i<typeContent.Length; i++){
            //GameObject.Find(typeContent[i].ToString()).GetComponent<Button>().onClick.Invoke();
            clickButtonOn(GameObject.Find(typeContent[i].ToString()));
        }
    }
    public static Text findTextObject(string textObjectName){
        return GameObject.Find(textObjectName).GetComponent<Text>();
    }
    public static string getTextContent(string textObjectName){
        string textViewContent = (GameObject.Find(textObjectName)).GetComponent<Text>().text;
        return textViewContent;
    }
    public static void clearTextContent(string textObjectName){
        findTextObject(textObjectName).text = "";
    }
}
