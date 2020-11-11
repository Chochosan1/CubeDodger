using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Chochosan
{
    public class SaveLoadManager : MonoBehaviour
    {
        public static SaveData savedGameData;
        private void Awake()
        {
            savedGameData = LoadGameState();
        }

        public static void SaveGameState()
        {
            //   SeriouslyDeleteAllSaveFiles();
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.Create);
            SaveData saveData = new SaveData();
            bf.Serialize(file, saveData);
            file.Close();
            Debug.Log("Saved");
        }

        public static SaveData LoadGameState()
        {
            if (File.Exists(Application.persistentDataPath + "/save.dat"))
            {
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/save.dat", FileMode.Open);
                SaveData saveData = (SaveData)bf.Deserialize(file);
                file.Close();
                return saveData;
            }
            return null;
        }


        public static void SeriouslyDeleteAllSaveFiles()
        {
            string path = Application.persistentDataPath;
            DirectoryInfo directory = new DirectoryInfo(path);
            directory.Delete(true);
            Directory.CreateDirectory(path);
        }

        public static bool IsSaveExists()
        {
            if (savedGameData != null)
            {
                return true;
            }
            return false;
        }

        [System.Serializable]
        public class SaveData
        {
            public GameData gameData = PlayerController.Instance.GetGameDataToSave();
        }


        #region InspectorTools
        [ContextMenu("Chochosan/Delete existing save files")]
        private void InspectorToolDeleteSaveFiles()
        {
            string path = Application.persistentDataPath;
            DirectoryInfo directory = new DirectoryInfo(path);
            directory.Delete(true);
            Directory.CreateDirectory(path);
        }
        #endregion
    }
}
