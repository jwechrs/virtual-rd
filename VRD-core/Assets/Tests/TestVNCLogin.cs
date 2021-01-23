using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tests
{
    public class VNCLoginTestScript
    {

        [SetUp]
        public void Init(){
        }
        // A Test behaves as an ordinary method
        [Test]
        public void NewTestScriptSimplePasses()
        {
            // Use the Assert class to test conditions
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        public void hitDeleteKey(){
            TestUtils.clickButtonOn(GameObject.Find("Delete"));
        }
        public void hitEnterKey(){
            TestUtils.clickButtonOn(GameObject.Find("Enter"));
        }
        [UnityTest]
        public IEnumerator TestLoginInHomeTheader()
        {
            SceneManager.LoadScene("HomeTheaterKeyboard");
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSeconds(0.1f);
            for(int i=0; i<11; i++){
                hitDeleteKey();
            }
            yield return new WaitForSeconds(0.1f);
            TestUtils.typeString("localhost");
            yield return new WaitForSeconds(0.5f);
            hitEnterKey();
            TestUtils.typeString("5900");
            yield return new WaitForSeconds(0.5f);
            hitEnterKey();
            yield return new WaitForSeconds(0.5f);
            TestUtils.typeString("aaaaaaa");
            hitEnterKey();
            yield return new WaitForSeconds(5);
            yield return null;
        }
        [UnityTest]
        public IEnumerator TestLoginInSpaceScene()
        {
            SceneManager.LoadScene("SpaceScene");
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSeconds(0.1f);
            for(int i=0; i<11; i++){
                hitDeleteKey();
            }
            yield return new WaitForSeconds(0.1f);
            TestUtils.typeString("localhost");
            yield return new WaitForSeconds(0.5f);
            hitEnterKey();
            TestUtils.typeString("5900");
            yield return new WaitForSeconds(0.5f);
            hitEnterKey();
            yield return new WaitForSeconds(0.5f);
            TestUtils.typeString("aaaaaaa");
            hitEnterKey();
            yield return new WaitForSeconds(5);
            yield return null;
        }

         [UnityTest]
        public IEnumerator TestWithSavedConnection()
        {
            UserPreferences.VNCServerDetailHolderList.testMode = true;
            SceneManager.LoadScene("SpaceScene");
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSeconds(0.1f);
            // for(int i=0; i<11; i++){
            //     hitDeleteKey();
            // }
            // yield return new WaitForSeconds(0.1f);
            // TestUtils.typeString("localhost");
            // yield return new WaitForSeconds(0.5f);
            // hitEnterKey();
            // TestUtils.typeString("5900");
            // yield return new WaitForSeconds(0.5f);
            // hitEnterKey();
            // yield return new WaitForSeconds(0.5f);
            // TestUtils.typeString("aaaaaaa");
            // hitEnterKey();
            TestUtils.clickButtonOnName("ConnectButton");
            yield return new WaitForSeconds(5);
            yield return null;
        }
        [UnityTest]
        public IEnumerator TestWithSavedConnectionHomeTheater()
        {
            UserPreferences.VNCServerDetailHolderList.testMode = true;
            SceneManager.LoadScene("HomeTheaterKeyboard");
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return new WaitForSeconds(0.1f);
            // for(int i=0; i<11; i++){
            //     hitDeleteKey();
            // }
            // yield return new WaitForSeconds(0.1f);
            // TestUtils.typeString("localhost");
            // yield return new WaitForSeconds(0.5f);
            // hitEnterKey();
            // TestUtils.typeString("5900");
            // yield return new WaitForSeconds(0.5f);
            // hitEnterKey();
            // yield return new WaitForSeconds(0.5f);
            // TestUtils.typeString("aaaaaaa");
            // hitEnterKey();
            TestUtils.clickButtonOnName("ConnectButton");
            yield return new WaitForSeconds(5);
            TestUtils.clearTextContent("KeyboardText");
            TestUtils.typeString("aaaaaaa");
            hitEnterKey();
            yield return new WaitForSeconds(5);
            yield return null;
        }

    }
}
