using System.Collections.Generic;
using UnityEngine;
using System;
using System.Security.Cryptography;
using System.Text;

namespace UserPreferences {
    [Serializable]
    public class VNCServerPreferences {
        // contains all server details as a list
        // this is necessary for JSON serialization.
        public List<VNCServerDetail> serverDetails;
    }
    [Serializable]
    public class VNCServerDetail {
        public string serverName;
        public int serverPort;
        public int serverDetailsId;
        public bool shouldUsePassword = false;
        public string password;
        public VNCServerDetail(string sName, int sPort, int detailsId,
                               bool shouldUsePassword,
                               string password){
            serverName = sName;
            serverPort = sPort;
            serverDetailsId = detailsId;
            this.shouldUsePassword = shouldUsePassword;
            this.password = password;
        }
    }
    public class VNCServerDetailHolderList{
        public static bool testMode = false;
        private static void initializePreference(){
            var initialPrefs = new VNCServerPreferences();
            initialPrefs.serverDetails = new List<VNCServerDetail>();
            saveStringToPreference("ServerDetailsPreferences", JsonUtility.ToJson(initialPrefs), true);
        }
        private static List<UserPreferences.VNCServerDetail> laodDummyData(){
            var serverDetailsList = new List<UserPreferences.VNCServerDetail>();
            //serverDetailsList.Add(new UserPreferences.VNCServerDetail("127.0.0.1", 5900, 0, true, "vncpass"));
            serverDetailsList.Add(new UserPreferences.VNCServerDetail("127.0.0.1", 5900, 0, false, ""));
            serverDetailsList.Add(new UserPreferences.VNCServerDetail("localhost2", 5900, 1, false, "")) ;
            return serverDetailsList;
        }
        public static List<VNCServerDetail> loadFromPlayerPrefs(){
            if (testMode){
                Debug.Log("loading dummy data");
                return laodDummyData();
            }
            //initializePreference();
            if (!PlayerPrefs.HasKey("ServerDetailsPreferences")){
                initializePreference();
            }
            var allPreferences = JsonUtility.FromJson<VNCServerPreferences>(
                getStringFromPreference("ServerDetailsPreferences", true)
                );
            //return details;
            return allPreferences.serverDetails;
        }
       
        public static void saveServerDetails(List<VNCServerDetail> details){
            var allDetails = new VNCServerPreferences();
            allDetails.serverDetails = details;
            //PlayerPrefs.SetString("ServerDetailsPreferences", JsonUtility.ToJson(allDetails));
            saveStringToPreference("ServerDetailsPreferences", JsonUtility.ToJson(allDetails));
        }
        public static void saveStringToPreference(string keyName, string value, bool shouldEncrypt = true){
            if (shouldEncrypt){
                string encryptedValue = EncryptUtil.encryptValue(value);
                PlayerPrefs.SetString(keyName, encryptedValue);
            }
            else {
                PlayerPrefs.SetString(keyName, value);
            }
        }
        public static string getStringFromPreference(string keyName, bool isEncrypted = true){
            if (isEncrypted){
                return EncryptUtil.decryptValue(PlayerPrefs.GetString(keyName));
            } else {
                return PlayerPrefs.GetString(keyName);
            }
        }
    }
    public class EncryptUtil {
        public static string encryptValue(string value){
            // TODO: Use your own password
            byte[] encryptKey = Encoding.UTF8.GetBytes("passpass");
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = encryptKey;
            des.IV = encryptKey;
            byte[] valueBytes = Encoding.UTF8.GetBytes(value);
            //byte[] valueBytes = Convert.FromBase64String(value);
            var transform = des.CreateEncryptor();
            byte[] encryptedValue = transform.TransformFinalBlock(
                valueBytes, 0, valueBytes.Length
            );
            transform.Dispose();
            //return Encoding.ASCII.GetString(encryptedValue);
            return Convert.ToBase64String(encryptedValue);
        }
        public static string decryptValue(string encryptedValue){
            byte[] encryptedBytes = Convert.FromBase64String(encryptedValue);
            byte[] encryptKey = Encoding.UTF8.GetBytes("passpass");
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            des.Key = encryptKey;
            des.IV = encryptKey;
            var detransform = des.CreateDecryptor();
            byte[] decryptedValue = detransform.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
            detransform.Dispose();
            return Encoding.UTF8.GetString(decryptedValue);
        }
    }
}
