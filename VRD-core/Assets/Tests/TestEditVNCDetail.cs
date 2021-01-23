using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UserPreferences;
using System.Text;
using UnityEngine.UI;

namespace Tests
{
    public class TestEditVNCDetail
    {

        [SetUp]
        public void Init(){
            UnityEngine.SceneManagement.SceneManager.LoadScene("EditVNCListScene");
            //ClickEventSystem.previousSceneName = "SpaceScene";
            //ServerListController.testMode = true;
            VNCServerDetailHolderList.testMode = true;
        }
        [Test]
        public void TestEncodeDecode(){
            string originalStr = "abcd";
            byte[] encodedByte = Encoding.UTF8.GetBytes(originalStr);
            string decodeEncodeStr = Encoding.UTF8.GetString(Encoding.UTF8.GetBytes(originalStr));
            Assert.AreEqual(originalStr, decodeEncodeStr);
        }
        [Test]
        public void TestEncryptDecrypt(){
            string originalValue = "abcdefghji";
            string encryptedValue = EncryptUtil.encryptValue(originalValue);
            Assert.AreNotEqual(originalValue, encryptedValue);
            Debug.Log("encrypted=" + encryptedValue);
            string decryptedValue = EncryptUtil.decryptValue(encryptedValue);
            Assert.AreEqual(originalValue, decryptedValue);
        }
        // A Test behaves as an ordinary method
        [Test]
        public void TestEditVNCDetailSimplePasses()
        {
            // Use the Assert class to test conditions
            VNCServerDetailHolderList.testMode = false;
            var serverDetailsList = VNCServerDetailHolderList.loadFromPlayerPrefs();
            var initialServerDetailsCount = serverDetailsList.Count;
            serverDetailsList.Add(new VNCServerDetail("sample", 5900, 2, false, ""));
            VNCServerDetailHolderList.saveServerDetails(serverDetailsList);
            var reloadedServersList = VNCServerDetailHolderList.loadFromPlayerPrefs();
            Assert.AreEqual(initialServerDetailsCount + 1, reloadedServersList.Count);
            Assert.AreEqual("sample", reloadedServersList[reloadedServersList.Count-1].serverName);
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator TestEditServerName()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            TestUtils.clearTextContent("VNCServerNameText");
            TestUtils.clickButtonOnName("Button_editServerName");
            TestUtils.typeString("newservername");
            TestUtils.hitEnterKey();
            Assert.AreEqual(TestUtils.getTextContent("VNCServerNameText"), "newservername");
            //TestUtils.typeString("extra");
            //Assert.AreEqual(TestUtils.getTextContent("VNCServerNameText"), "newservername");
            TestUtils.clearTextContent("VNCPortNameText");
            TestUtils.clickButtonOnName("Button_editServerPort");
            TestUtils.typeString("1234");
            TestUtils.hitEnterKey();
            Assert.AreEqual(TestUtils.getTextContent("VNCPortNameText"), "1234");
            Assert.AreEqual(TestUtils.getTextContent("VNCServerNameText"), "newservername");
            TestUtils.clickButtonOnName("Button_save");
            TestUtils.clickButtonOnName("GoBackButton");
            yield return new WaitForSeconds(2);
            yield return null;
        }
        [UnityTest]
        public IEnumerator TestEditServerPort()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            TestUtils.clearTextContent("VNCPortNameText");
            TestUtils.clickButtonOnName("Button_editServerPort");
            TestUtils.typeString("2345");
            Assert.AreEqual("2345", TestUtils.getTextContent("VNCPortNameText"));
            TestUtils.clickButtonOnName("Delete");
            Assert.AreEqual("234", TestUtils.getTextContent("VNCPortNameText"));
            TestUtils.typeString("5");
            TestUtils.hitEnterKey();
            Assert.AreEqual(TestUtils.getTextContent("VNCPortNameText"), "2345");
            //TestUtils.typeString("extra");
            //Assert.AreEqual(TestUtils.getTextContent("VNCPortNameText"), "newportname");
            yield return null;
        }

        [UnityTest]
        public IEnumerator TestEditButtonClick(){
            TestUtils.clickButtonOnName("EditButton");
            Assert.AreEqual("127.0.0.1", TestUtils.getTextContent("VNCServerNameText"));
            yield return null;
        }
        [UnityTest]
        public IEnumerator TestDeleteButtonClick(){
            TestUtils.clickButtonOnName("DeleteButton");
            yield return new WaitForSeconds(0.1f);
            TestUtils.clickButtonOnName("EditButton");
            Assert.AreEqual("localhost2", TestUtils.getTextContent("VNCServerNameText"));
            yield return new WaitForSeconds(0.1f);
            yield return null;
        }

         [UnityTest]
        public IEnumerator TestAddButtonClick(){
            TestUtils.clickButtonOnName("AddServerListButton");
            yield return new WaitForSeconds(0.1f);
            //TestUtils.clickButtonOnName("EditButton");
            //Assert.AreEqual("localhost2", TestUtils.getTextContent("VNCServerNameText"));
            yield return new WaitForSeconds(0.1f);
            Assert.AreEqual("127.0.0.1", TestUtils.getTextContent("VNCServerNameText"));
            yield return null;
        }
        [UnityTest]
        public IEnumerator TestTogglePassword(){
              var toggleButton = GameObject.Find("Toggle").GetComponent<Toggle>();
              var initialToggle = toggleButton.isOn;
            //TestUtils.clickButtonOnName("Toggle");
            toggleButton.isOn = !initialToggle;
          
            Assert.AreEqual(toggleButton.isOn, !initialToggle);
            yield return new WaitForSeconds(2);
            TestUtils.clickButtonOnName("EditButton");
            yield return new WaitForSeconds(1);
            Assert.AreEqual(toggleButton.isOn, !initialToggle);
               //TestUtils.clickButtonOnName("Toggle");
               toggleButton.isOn = initialToggle;
                  TestUtils.clickButtonOnName("EditButton");
            Assert.AreEqual(toggleButton.isOn, initialToggle);
            yield return new WaitForSeconds(1);
            Assert.AreEqual(toggleButton.isOn, initialToggle);
        }
    }
}
