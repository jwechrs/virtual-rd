using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Tests
{
    public class TestEnvironmentSelection
    {

        [SetUp]
        public void Init(){
            SceneManager.LoadScene("EnvironmentSelectionScene");
        }
        // A Test behaves as an ordinary method
        [Test]
        public void TestEnvironmentSelectionSimplePasses()
        {
            // Use the Assert class to test conditions
        }
         public void clickButtonOn(GameObject buttonObject){
            buttonObject.GetComponent<Button>().onClick.Invoke();
        }
        public void assertInScene(string sceneName){
            Assert.AreEqual(sceneName, SceneManager.GetActiveScene().name);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestEnvironmentSelectionWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            //SceneManager.LoadScene("EnvironmentSelectionScene");
            clickButtonOn(GameObject.Find("MiniTheaterButton"));
            yield return new WaitForSeconds(1);
            Assert.AreEqual("HomeTheaterKeyboard", SceneManager.GetActiveScene().name);
            clickButtonOn(GameObject.Find("Back (1)"));
            yield return new WaitForSeconds(1);
            assertInScene("EnvironmentSelectionScene");
            yield return null;
        }

         [UnityTest]
        public IEnumerator TestEnvironmentSelectionEnterCurvedScreenWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            //SceneManager.LoadScene("EnvironmentSelectionScene");
            clickButtonOn(GameObject.Find("CurvedScreenButton"));
            yield return new WaitForSeconds(1);
            Assert.AreEqual("SpaceScene", SceneManager.GetActiveScene().name);
            clickButtonOn(GameObject.Find("SelectSceneButton"));
            yield return new WaitForSeconds(1);
            assertInScene("EnvironmentSelectionScene");
            yield return null;
        }
    }
}
