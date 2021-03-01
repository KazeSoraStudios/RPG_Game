using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.Networking;
using UnityGoogleDrive;

public class GameDataDownloader : MonoBehaviour
{
    [SerializeField] GameData GameData;
    string jsonURL = "https://drive.google.com/uc?export=download&id=1bmAVofqQuVjT_QvqyrGK6_vwzcdfymHfTYjUSSrgjQY";
    //string jsonURL = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQ-u-qq-Ze14-8XwccPNrL9xvbIv267qux7V8J9Qy5exXoIFPZC9lCCRmOFCjABQoiziWELnadD9shw/pub?output=tsv&gid=141866169";

    string expected = "[{\"name\": \"hero\",\"type\": \"weapon\",\"icon\": \"\",\"restriction\": [],\"description\": \"\",\"stats\": {},\"use\": \"\",\"use_restriction\": \"\"},{\"name\": \"hero\",\"type\": \"weapon\",\"icon\": \"\",\"restriction\": [\"hero\",\"mage\"],\"description\": \"\",\"stats\": {},\"use\": \"\",\"use_restriction\": \"\"}]";

    string s = "items,16,10,,,,,,,,Bone Blade,weapon,sword,hero,A wicked sword made from bone.,add:attack;5,,,FALSE,300,Bone Armor,armor,plate,hero,Armor made from plates of blackened bone.,add:defense;5/resist;1,,,FALSE,500,Ring of Titan,accessory,,,Grants the strength of the Titan.,add:strength;10,,,FALSE,1000,World Tree Branch,weapon,stave,mage,A hard wood branch.,add:attack;2/magic;5,,,FALSE,300,Dragon's Cloak,armor,robe,mage,A cloak of dragon scales.,add:defense;3/resist;10,,,FALSE,500,Singer's Stone,accessory,,,The stone's song resists magical attacks.,add:resist;10,,,FALSE,1000,Black Dagger,weapon,dagger,thief,A dagger made out of an unknown material.,add:attack;4,,,FALSE,100,Footpad Leathers,armor,leather,thief,Light armor for silent movement.,add:defense;3,,,FALSE,250,Swift Boots,accessory,,,Increases speed by 25%.,mult:speed;0.25,,,FALSE,2000,Heal Potion,useable,,,Heal a small amonunt of HP.,,hp_restore_light,Choose target to heal.,TRUE,50,Mega Heal Potion,useable,,,Heal a large amount of HP.,,hp_restore_med,Choose target to heal.,TRUE,250,Life Salve,useable,,,Restore a character from the brink of death.,,revive_light,Choose target to revive.,TRUE,100,Mana Potion,useable,,,Restores a small amount of MP.,,mp_restore_light,Choose target to restore mana.,TRUE,100,Mysterious Torque,accessory,,,A golden torque that glitters.,add:strength;10/speed;10,,,FALSE,1000,Gemstone,key,,,.Red gemstone shaped like a small skull.,,,,FALSE,,Keystone,key,,,A heavy stone orb.,,,,FALSE,,actions,9,3,,,,,,,,hp_restore_light,250,selector:MostHurtParty/switch_ides:true/type:one,,,,,,,,hp_restore_med,500,selector:MostHurtParty/switch_ides:true/type:one,,,,,,,,hp_restore_heavy,1000,selector:MostHurtParty/switch_ides:true/type:one,,,,,,,,revive_light,100,selector:DeadParty/switch_sides:true/type:one,,,,,,,,revive_med,250,selector:DeadParty/switch_sides:true/type:one,,,,,,,,revive_heavy,500,selector:DeadParty/switch_sides:true/type:one,,,,,,,,mp_restore_light,50,selector:MostDrainedParty/switch_sides:true/type:one,,,,,,,,mp_restore_med,100,selector:MostDrainedParty/switch_sides:true/type:one,,,,,,,,mp_restore_heavy,250,selector:MostDrainedParty/switch_sides:true/type:one,,,,,,,";

    Action OnComplete;

    public void DownLoadGameData(Action callback = null)
    {
        OnComplete = callback;
        GoogleDriveFiles.Export("1bmAVofqQuVjT_QvqyrGK6_vwzcdfymHfTYjUSSrgjQY", "text/csv").Send().OnDone += loaded;
    }

    //public delegate void OnSpreedSheetLoaded(GstuSpreadSheet sheet);
    public void loaded(UnityGoogleDrive.Data.File file)
    {
        var str = System.Text.Encoding.Default.GetString(file.Content);
        str = Regex.Replace(str, @"\r\n?|\n", ",");
        Debug.Log(str);
        var cells = str.Split(',');
        HandleData(cells);
    }

    private Dictionary<string, Action<string>> parserFunctions = new Dictionary<string, Action<string>>();

    IEnumerator Download()
    {
        var request = UnityWebRequest.Get(jsonURL);
        yield return request.SendWebRequest();

        if (request.isNetworkError || request.isHttpError)
        {
            LogManager.LogError("Error getting file.");
        }
        LogManager.LogInfo($"Json is : {request.downloadHandler.text}");
        //var ex = JsonConvert.DeserializeObject(expected);
        
       // var x = JsonConvert.DeserializeObject<List<ItemInfo>>(request.downloadHandler.text);
        //var data = JsonUtility.FromJson<string>(request.downloadHandler.text);
        request.Dispose();
    }

    public void AfterDownload(string data)
    {
        if (null == data)
        {
            Debug.LogError("Was not able to download data or retrieve stale data.");
            // TODO: Display a notification that this is likely due to poor internet connectivity
            //       Maybe ask them about if they want to report a bug over this, though if there's no internet I guess they can't
        }
        else
        {
            StartCoroutine(ProcessData(data, AfterProcessData));
        }
    }

    public IEnumerator ProcessData(string data, System.Action<string> onCompleted)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        //Debug.Log("Data: " + data);
        data = Regex.Replace(data, @"\r\n?|\n", ",");
        var cells = data.Split(',');

        HandleData(cells);

        int index = 1;
        int count = int.Parse(cells[index]);

        while (count >= 0)
        {
            Debug.Log(cells[index]);
            count--;
        }
    }

    private void HandleData(string[] data)
    {
        // First row is total number of rows and columns
        var cells = new Dictionary<string, List<List<string>>>();
        int numberOfColumns = int.Parse(data[1]);
        int index = numberOfColumns;
        // Second row is sheet name, number of rows and columns
        int numberOfCells = int.Parse(data[index + 1]);
        int cellsProcessed = 0;
        index = numberOfColumns * 2;
        var items = GameDataItemHandler.ProcessItems(index, numberOfCells, numberOfColumns, data);
        // Count the cells we just processed plus the two rows we skipped
        cellsProcessed = numberOfColumns * 2 + numberOfColumns * numberOfCells;
        // Get the new number of cells to process and skip the row
        numberOfCells = int.Parse(data[cellsProcessed + 1]);
        cellsProcessed += numberOfColumns;
        index = cellsProcessed;
        var itemUses = GameDataItemUseHandler.ProcessItems(index, numberOfCells, numberOfColumns, data);
        cellsProcessed += numberOfColumns * numberOfCells;
        // Get the new number of cells to process and skip the row
        numberOfCells = int.Parse(data[cellsProcessed + 1]);
        cellsProcessed += numberOfColumns;
        index = cellsProcessed;
        var loc = GameDataLocalizationHandler.ProcessLocaliation(index, numberOfCells, numberOfColumns, data);
        cellsProcessed += numberOfColumns * numberOfCells;
        // Get the new number of cells to process and skip the row
        numberOfCells = int.Parse(data[cellsProcessed + 1]);
        cellsProcessed += numberOfColumns;
        index = cellsProcessed;
        var party = GameDataPartyHandler.ProcessParty(index, numberOfCells, numberOfColumns, data);
        cellsProcessed += numberOfColumns * numberOfCells;
        // Get the new number of cells to process and skip the row
        numberOfCells = int.Parse(data[cellsProcessed + 1]);
        cellsProcessed += numberOfColumns;
        index = cellsProcessed;
        var stats = GameDataStatsHandler.ProcessStats(index, numberOfCells, numberOfColumns, data);
        GameData.Items = items;
        GameData.ItemUses = itemUses;
        GameData.PartyDefs = party;
        GameData.Stats= stats;
        ServiceManager.Get<LocalizationManager>().SetLocalization(loc);
        enabled = false;
        OnComplete?.Invoke();
    }

    private ItemType GetItemType(string type)
    {
        type = type.ToLower();
        switch(type)
        {
            case "accessory":
                return ItemType.Accessory;
            case "armor":
                return ItemType.Armor;
            case "key":
                return ItemType.Key;
            case "useable":
                return ItemType.Useable;
            case "weapon":
                return ItemType.Weapon;
            default:
                return ItemType.Accessory;
        }
    }

    private int GetPrice(string priceCell)
    {
        var price = 0;
        if (priceCell.IsNotEmpty())
            price = int.Parse(priceCell);
        return price;
    }

    private UseRestriction[] GetUseRestriction(string data)
    {
        if (data.IsEmpty())
            return new UseRestriction[] { UseRestriction.None };
        data = data.ToLower();
        var restrictions = data.Split(':');
        int count = restrictions.Length;
        var useRestrictions = new UseRestriction[count];
        for (int i = 0; i < count; i++)
        {
            useRestrictions[i] = (UseRestriction)Enum.Parse(typeof(UseRestriction), restrictions[0], true);
        }
        return useRestrictions;
    }

    private void AfterProcessData(string errorMessage)
    {
        if (null != errorMessage)
        {
            Debug.LogError("Was not able to process data: " + errorMessage);
            // TODO: 
        }
        else
        {

        }
    }

    public IEnumerator ProcessData2(string data, System.Action<string> onCompleted)
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();



        // Line level
        int currLineIndex = 0;
        bool inQuote = false;
        int linesSinceUpdate = 0;
        int kLinesBetweenUpdate = 15;

        // Entry level
        string currEntry = "";
        int currCharIndex = 0;
        bool currEntryContainedQuote = false;
        List<string> currLineEntries = new List<string>();

        // "\r\n" means end of line and should be only occurence of '\r' (unless on macOS/iOS in which case lines ends with just \n)
        var lineEnding = Environment.NewLine;
        var lineEndingLength = lineEnding.Length;

        while (currCharIndex < data.Length)
        {
            if (!inQuote /*&& (data[currCharIndex] == lineEnding)*/)
            {
                // Skip the line ending
                currCharIndex += lineEndingLength;

                // Wrap up the last entry
                // If we were in a quote, trim bordering quotation marks
                if (currEntryContainedQuote)
                {
                    currEntry = currEntry.Substring(1, currEntry.Length - 2);
                }

                currLineEntries.Add(currEntry);
                currEntry = "";
                currEntryContainedQuote = false;

                // Line ended
                ProcessLineFromCSV(currLineEntries, currLineIndex);
                currLineIndex++;
                currLineEntries = new List<string>();

                linesSinceUpdate++;
                if (linesSinceUpdate > kLinesBetweenUpdate)
                {
                    linesSinceUpdate = 0;
                    yield return new WaitForEndOfFrame();
                }
            }
            else
            {
                if (data[currCharIndex] == '"')
                {
                    inQuote = !inQuote;
                    currEntryContainedQuote = true;
                }

                // Entry level stuff
                {
                    if (data[currCharIndex] == ',')
                    {
                        if (inQuote)
                        {
                            currEntry += data[currCharIndex];
                        }
                        else
                        {
                            // If we were in a quote, trim bordering quotation marks
                            if (currEntryContainedQuote)
                            {
                                currEntry = currEntry.Substring(1, currEntry.Length - 2);
                            }

                            currLineEntries.Add(currEntry);
                            currEntry = "";
                            currEntryContainedQuote = false;
                        }
                    }
                    else
                    {
                        currEntry += data[currCharIndex];
                    }
                }
                currCharIndex++;
            }

            //progress = (int)((float)currCharIndex / data.Length * 100.0f);
        }

        onCompleted(null);
    }

    private void ProcessLineFromCSV(List<string> currLineElements, int currLineIndex)
    {

        //// This line contains the column headers, telling us which languages are in which column
        //if (currLineIndex == 0)
        //{
        //    languages = new List<string>();
        //    for (int columnIndex = 0; columnIndex < currLineElements.Count; columnIndex++)
        //    {
        //        string currLanguage = currLineElements[columnIndex];
        //        Assert.IsTrue((columnIndex != 0 || currLanguage == "English"), "First column first row was:" + currLanguage);
        //        Assert.IsFalse(Manager.instance.translator.termData.languageIndicies.ContainsKey(currLanguage));
        //        Manager.instance.translator.termData.languageIndicies.Add(currLanguage, currLineIndex);
        //        languages.Add(currLanguage);
        //    }
        //    UnityEngine.Assertions.Assert.IsFalse(languages.Count == 0);
        //}
        //// This is a normal node
        //else if (currLineElements.Count > 1)
        //{
        //    string englishSpelling = null;
        //    string[] nonEnglishSpellings = new string[languages.Count - 1];

        //    for (int columnIndex = 0; columnIndex < currLineElements.Count; columnIndex++)
        //    {
        //        string currentTerm = currLineElements[columnIndex];
        //        if (columnIndex == 0)
        //        {
        //            Assert.IsFalse(Manager.instance.translator.termData.termTranslations.ContainsKey(currentTerm), "Saw this term twice: " + currentTerm);
        //            englishSpelling = currentTerm;
        //        }
        //        else
        //        {
        //            nonEnglishSpellings[columnIndex - 1] = currentTerm;
        //        }
        //    }
        //    Manager.instance.translator.termData.termTranslations[englishSpelling] = nonEnglishSpellings;
        //    //print( "englishSpelling: >" + englishSpelling + "<" );
        //}
        //else
        //{
        //    Debug.LogError("Database line did not fall into one of the expected categories.");
        //}
    }

    private void parseItems(string text)
    {
        // name	type	icon	restriction	description	stats	use	use_restriction	hint	map_use	price
        var items = text.Split(',');
        if (items == null || items.Length < 1 || items[0] != "items")
        {
            LogManager.LogError("Cannot parse items.");
            return;
        }

    }
}

public static class CSVDownloader
{
    private const string k_googleSheetDocID = "13zXZxMWmS5ShIIxXd8OIOIf6JCBYmwziav9OsLdPH1U";

    //docs.google.com/spreadsheets/d/e/2PACX-1vQ-u-qq-Ze14-8XwccPNrL9xvbIv267qux7V8J9Qy5exXoIFPZC9lCCRmOFCjABQoiziWELnadD9shw/pub?output=csv
    // docs.google.com/spreadsheets/d/13zXZxMWmS5ShIIxXd8OIOIf6JCBYmwziav9OsLdPH1U/edit#gid=0
    //private const string url = "https://docs.google.com/spreadsheets/d/" + k_googleSheetDocID + "/export?format=csv";
    private const string url = "https://docs.google.com/spreadsheets/d/e/2PACX-1vQ-u-qq-Ze14-8XwccPNrL9xvbIv267qux7V8J9Qy5exXoIFPZC9lCCRmOFCjABQoiziWELnadD9shw/pub?output=csv";

    internal static IEnumerator DownloadData(System.Action<string> onCompleted)
    {
        yield return new WaitForEndOfFrame();

        string downloadData = null;
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            Debug.Log("Starting Download...");
            yield return webRequest.SendWebRequest();
            int equalsIndex = ExtractEqualsIndex(webRequest.downloadHandler);
            if (webRequest.isNetworkError || (-1 == equalsIndex))
            {
                Debug.Log("...Download Error: " + webRequest.error);
                downloadData = PlayerPrefs.GetString("LastDataDownloaded", null);
                string versionText = PlayerPrefs.GetString("LastDataDownloadedVersion", null);
                Debug.Log("Using stale data version: " + versionText);
            }
            else
            {
                string versionText = webRequest.downloadHandler.text.Substring(0, equalsIndex);
                Debug.Log(webRequest.downloadHandler.text);
                downloadData = webRequest.downloadHandler.text;//.Substring(equalsIndex + 1);
                PlayerPrefs.SetString("LastDataDownloadedVersion", versionText);
                PlayerPrefs.SetString("LastDataDownloaded", downloadData);
                Debug.Log("...Downloaded version: " + versionText);

            }
        }

        onCompleted(downloadData);
    }

    private static int ExtractEqualsIndex(DownloadHandler d)
    {
        if (d.text == null || d.text.Length < 10)
        {
            return -1;
        }
        // First term will be preceeded by version number, e.g. "100=English"
        string versionSection = d.text.Substring(0, 5);
        int equalsIndex = versionSection.IndexOf('=');
        if (equalsIndex == -1)
            Debug.Log("Could not find a '=' at the start of the CVS");
        return 10;
    }
}

public class Loader : MonoBehaviour
{

    private int progress = 0;
    List<string> languages = new List<string>();

    void Initialize()
    {

    }

    public void Load()
    {
        //StartCoroutine(CSVDownloader.DownloadData(AfterDownload));
    }

    
}