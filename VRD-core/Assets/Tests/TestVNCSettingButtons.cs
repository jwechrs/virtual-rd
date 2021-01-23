using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

namespace Tests
{
    public class TestVNCSettingButtons{
        [UnityTest]
        public IEnumerator TestEnterAndExitVNCSettingPage()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            SceneManager.LoadScene("SpaceScene");
            yield return new WaitForSeconds(1);
            UserPreferences.VNCServerDetailHolderList.testMode = false;
            TestUtils.clickButtonOnName("Button_editServerList");
            yield return new WaitForSeconds(1);
            Assert.AreEqual("EditVNCListScene", SceneManager.GetActiveScene().name);
            TestUtils.clickButtonOnName("EditButton");
            TestUtils.clickButtonOnName("Button_editServerName");
            TestUtils.clearTextContent("VNCServerNameText");
            TestUtils.typeString("newname");
            TestUtils.clickButtonOnName("Button_save");
            yield return new WaitForSeconds(3);
            TestUtils.clickButtonOnName("GoBackButton");
            yield return new WaitForSeconds(1);
            Assert.AreNotEqual("EditVNCListScene", SceneManager.GetActiveScene().name);
             TestUtils.clickButtonOnName("Button_editServerList");
            yield return new WaitForSeconds(1);
            Assert.AreEqual("EditVNCListScene", SceneManager.GetActiveScene().name);
             TestUtils.clickButtonOnName("EditButton");
             yield return new WaitForSeconds(3);
            Assert.AreEqual("newname", TestUtils.getTextContent("VNCServerNameText"));
            yield return null;
        }
    }
}
