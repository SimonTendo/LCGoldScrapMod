using System;
using System.IO;
using System.Xml.Serialization;

public class GoldScrapSaveData
{
    [Serializable]
    public class DataToSave
    {
        public int[] savedPreviousCredits;
    }

    public class SaveData
    {
        DataToSave data = new DataToSave
        {
            savedPreviousCredits = CreditsCardManager.previousCredits
        };

        public void Save()
        {
            GoldScrapPlugin.Logger.LogInfo("Saving LCGoldScrapMod SaveData...");
            string SavePath = new string(Path.Combine(GoldScrapPlugin.sSaveLocation, "LCGoldScrapMod.xml"));
            XmlSerializer serializer = new XmlSerializer(typeof(DataToSave));
            using (FileStream stream = new FileStream(SavePath, FileMode.Create))
            {
                serializer.Serialize(stream, data);
            }
        }

        public void Load()
        {
            string savePath = new string(Path.Combine(GoldScrapPlugin.sSaveLocation, "LCGoldScrapMod.xml"));
            if (File.Exists(savePath))
            {
                //Loading the SaveData
                XmlSerializer serializer = new XmlSerializer(typeof(DataToSave));
                using FileStream stream = new FileStream(savePath, FileMode.Open);
                DataToSave LoadedData = (DataToSave)serializer.Deserialize(stream);

                //The variables to set from the loaded data
                CreditsCardManager.previousCredits = LoadedData.savedPreviousCredits;

                GoldScrapPlugin.Logger.LogInfo("Loaded LCGoldScrapMod SaveData");

            }
            else
            {
                GoldScrapPlugin.Logger.LogInfo("No LCGoldScrapMod SaveData exists yet");
            }
        }
    }
}
