using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using static scr_General;

public class scr_MainForm : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private TMP_InputField _txtInputFIO;
    [SerializeField] private TMP_InputField _txtInputDateOfBirth;
    [SerializeField] private TMP_InputField _txtInputDateofDeath;
    [SerializeField] private Image _inputImage;
    [SerializeField] private TMP_InputField _txtMainInfo;
    [SerializeField] private TMP_InputField _txtSearchField;
    [SerializeField] private TMP_InputField _txtPamyat;

    [Header("Buttons")]
    [SerializeField] private Button _btnAddCard;
    [SerializeField] private Button _btnSaveCard;
    [SerializeField] private Button _btnAddPhoto;
    [SerializeField] private Button _btnAddReward;
    [SerializeField] private Button _btnSearch;
    [SerializeField] private Button _btnCheckPamyat;

    [Header("String prefabs")]
    [SerializeField] private scr_RewardString _rewardStringPrefab;
    [SerializeField] private scr_SearchString _veteranStringPrefab;

    [Header("Scrollview Places")]
    [SerializeField] private GameObject _rewardsListPlace;
    [SerializeField] private GameObject _veteranListPlace;

    private List<scr_RewardString> _rewardStrings = new();
    private List<scr_SearchString> _searchStrings = new();

    private string _databasePath;
    private string _photoFileSourcePath;
    private string _imageURL;


    void Awake()
    {

        //_databasePath = Path.Combine(Application.streamingAssetsPath, m_General.GET_DatabaseNameJSON);
        _databasePath = Path.Combine(Application.streamingAssetsPath, "Veterans.json");

        V_CheckStreamingAssets();
        V_ShowAllData();

        _btnAddPhoto.onClick.AddListener(() => V_AddPhoto());
        _btnAddReward.onClick.AddListener(() => V_AddNewReward());
        _btnSaveCard.onClick.AddListener(() => V_SaveCard());
        _btnAddCard.onClick.AddListener(() => V_NewCard());
        _btnSearch.onClick.AddListener(() => V_Search());
        _btnCheckPamyat.onClick.AddListener(() => V_CheckPamyatNaroda());
        
    }

    //Streaming assets
    private void V_CheckStreamingAssets()
    {
        string streamingAssetsPath = Application.streamingAssetsPath;

        if (!Directory.Exists(streamingAssetsPath))
        {
            Directory.CreateDirectory(streamingAssetsPath);
        }
    }

    //Photo
    private void V_AddPhoto()
    {
        string sourceFilePath = EditorUtility.OpenFilePanel("Select an Image", "", "png,jpg,jpeg");

        if (string.IsNullOrEmpty(sourceFilePath))
        {
            Debug.LogWarning("No file selected.");
            return;
        }

        _photoFileSourcePath = sourceFilePath;
        _inputImage.sprite = LoadSpriteFromPath(sourceFilePath);
    }

    private void V_SavePhotoOnDisk(string fileName)
    {
        if (_photoFileSourcePath == null) return;

        string fileExtension = Path.GetExtension(_photoFileSourcePath);
        string fileNameWithExtension = fileName + fileExtension;
        string targetPath = Path.Combine(Application.streamingAssetsPath, fileNameWithExtension);

        _imageURL = targetPath;

        try
        {
            File.Copy(_photoFileSourcePath, targetPath, true);
            Debug.Log($"File copied to: {targetPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to copy file: {e.Message}");
            return;
        }
    }

    private Sprite LoadSpriteFromPath(string filePath)
    {
        if (filePath == string.Empty) return null;

        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    //Rewards
    private void V_AddNewReward()
    {
        scr_RewardString newReward = Instantiate(_rewardStringPrefab, _rewardsListPlace.transform);
        newReward.V_INITIALIZE(this); 
        _rewardStrings.Add(newReward);
    }

    private void V_AddExistingReward(string name,string year)
    {
        scr_RewardString newReward = Instantiate(_rewardStringPrefab, _rewardsListPlace.transform);
        newReward.V_INITIALIZE(this);
        newReward.RewardName.text = name;
        newReward.RewardYear.text = year;
        _rewardStrings.Add(newReward);
    }

    public void V_DeleteReward(scr_RewardString reward)
    {
        _rewardStrings.Remove(reward);
        Destroy(reward.gameObject);
    }

    private void V_DeleteAllRewards()
    {
        foreach (var reward in _rewardStrings)
        {
            Destroy(reward.gameObject);
        }
        _rewardStrings.Clear();
    }

    //Save
    private void V_SaveCard()
    {
        V_SavePhotoOnDisk(_txtInputFIO.text);
        V_SaveDataToJSON();
        V_Search();
    }

    private void V_SaveDataToJSON()
    {
        string path = _databasePath;

        D_JSON jsonData;

        if (File.Exists(path))
        {
            string existingJson = File.ReadAllText(path);
            jsonData = JsonUtility.FromJson<D_JSON>(existingJson);
        }
        else
        {
            jsonData = new D_JSON
            {
                Veterans = new List<Dm_JSON>()
            };
        }

        // Check if the veteran already exists
        int existingIndex = jsonData.Veterans.FindIndex(v => v.FullName == _txtInputFIO.text);

        if (existingIndex != -1)
        {
            // Update the existing veteran
            var existingVeteran = jsonData.Veterans[existingIndex];
            existingVeteran.ImageURL = _imageURL;
            existingVeteran.DateOfBitrh = _txtInputDateOfBirth.text;
            existingVeteran.DateOfDeath = _txtInputDateofDeath.text;
            existingVeteran.MainInfo = _txtMainInfo.text;
            existingVeteran.Rewards = new List<Dmm_JSON>();

            foreach (var reward in _rewardStrings)
            {
                existingVeteran.Rewards.Add(new Dmm_JSON
                {
                    RewardName = reward.RewardName.text,
                    YearOfReward = reward.RewardYear.text
                });
            }

            jsonData.Veterans[existingIndex] = existingVeteran; // Update the list
        }
        else
        {
            // Add a new veteran
            Dm_JSON newVeteran = new Dm_JSON
            {
                FullName = _txtInputFIO.text,
                ImageURL = _imageURL,
                DateOfBitrh = _txtInputDateOfBirth.text,
                DateOfDeath = _txtInputDateofDeath.text,
                MainInfo = _txtMainInfo.text,
                Rewards = new List<Dmm_JSON>()
            };

            foreach (var reward in _rewardStrings)
            {
                newVeteran.Rewards.Add(new Dmm_JSON
                {
                    RewardName = reward.RewardName.text,
                    YearOfReward = reward.RewardYear.text
                });
            }

            jsonData.Veterans.Add(newVeteran);
        }

        string json = JsonUtility.ToJson(jsonData, true);
        File.WriteAllText(path, json);

        Debug.Log($"JSON saved to: {path}");

        _photoFileSourcePath = null;
    }

    //New Card
    private void V_NewCard()
    {
        _txtInputFIO.text = string.Empty;
        _txtInputDateOfBirth.text = string.Empty;
        _txtInputDateofDeath.text = string.Empty;
        _txtMainInfo.text = string.Empty;

        _inputImage.sprite = null;

        V_DeleteAllRewards();

        _photoFileSourcePath = null;
        _imageURL = null;

    }

    //Load data 
    public void V_LoadCardFromJSON(Dm_JSON veteran)
    {
        _txtInputFIO.text = veteran.FullName;
        _txtInputDateOfBirth.text = veteran.DateOfBitrh;
        _txtInputDateofDeath.text = veteran.DateOfDeath;
        _txtMainInfo.text = veteran.MainInfo;

        _inputImage.sprite = LoadSpriteFromPath(veteran.ImageURL);
        _imageURL = veteran.ImageURL;

        V_DeleteAllRewards();
        veteran.Rewards.ForEach(r => V_AddExistingReward(r.RewardName,r.YearOfReward));

        _photoFileSourcePath = null;
      
    }

    //Search
    private void V_ShowAllData()
    {
        string path = _databasePath;

        if (File.Exists(path))
        {
            string json = File.ReadAllText(path);
            D_JSON jsonData = JsonUtility.FromJson<D_JSON>(json);

            foreach (var veteran in jsonData.Veterans)
            {
                scr_SearchString veteranPrefab = Instantiate(_veteranStringPrefab, _veteranListPlace.transform);
                veteranPrefab.V_INITIALIZE(this, veteran);

                _searchStrings.Add(veteranPrefab);
            }
        }
        else
        {
            Debug.LogWarning("Veterans.json file not found in StreamingAssets.");
        }
    }

    private void V_DeleteSearchList()
    {
        foreach (var searchString in _searchStrings)
        {
            Destroy(searchString.gameObject);
        }
       _searchStrings.Clear();
    }

    private void V_Search()
    {
        string searchTerm = _txtSearchField.text.ToLower();

        V_DeleteSearchList();

        if (_txtSearchField.text == string.Empty) 
        {
            V_ShowAllData();
        }
        else 
        {
            string path = _databasePath;

            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                D_JSON jsonData = JsonUtility.FromJson<D_JSON>(json);

                foreach (var veteran in jsonData.Veterans)
                {
                    if (veteran.FullName.ToLower().Contains(searchTerm))
                    {
                        scr_SearchString veteranPrefab = Instantiate(_veteranStringPrefab, _veteranListPlace.transform);
                        veteranPrefab.V_INITIALIZE(this, veteran);

                        _searchStrings.Add(veteranPrefab);
                    }
                }
            }
        }
    }

    //Check Pamyat Naroda
    public void V_CheckPamyatNaroda()
    {
        string linkTemplate = "https://pamyat-naroda.ru/heroes/?last_name=FAMILYNAME&first_name=NAME&middle_name=MIDDLENAME" +
                              "&group=all&types=pamyat_commander:nagrady_nagrad_doc:nagrady_uchet_kartoteka:nagrady_ubilein_kartoteka:" +
                              "pdv_kart_in:pdv_kart_in_inostranec:pamyat_voenkomat:potery_vpp:pamyat_zsp_parts:kld_ran:kld_bolezn:kld_polit:kld_upk:kld_vmf:kld_partizan:potery_doneseniya_o_poteryah:" +
                              "potery_gospitali:potery_utochenie_poter:potery_spiski_zahoroneniy:potery_voennoplen:potery_iskluchenie_iz_spiskov:potery_kartoteki:potery_rvk_extra:potery_isp_extra:" +
                              "same_doroga:same_rvk:same_guk:potery_knigi_pamyati&page=1&grouppersons=1";

        string fullName = _txtInputFIO.text;
        string[] nameParts = fullName.Split(' ');

        if (nameParts.Length < 2 || nameParts.Length > 3)
        {
            _txtPamyat.text = "Invalid name format. Please provide 'Family Name First Name [Middle Name]'.";
            return;
        }

        string familyName = nameParts[0];
        string firstName = nameParts[1];
        string middleName = nameParts.Length == 3 ? nameParts[2] : ""; 

        // Replace placeholders in the link template.
        string finalLink = linkTemplate
            .Replace("FAMILYNAME", Uri.EscapeDataString(familyName))
            .Replace("MIDDLENAME", Uri.EscapeDataString(middleName))
            .Replace("NAME", Uri.EscapeDataString(firstName));

        // Save the link in the text field.
        _txtPamyat.text = finalLink;

        // Execute the link to fetch its content.
        StartCoroutine(FetchLinkContent(finalLink));

        _txtPamyat.onSelect.AddListener(delegate { Application.OpenURL(finalLink); });
    }

    private IEnumerator FetchLinkContent(string url)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            // Set a user-agent header to mimic a browser.
            webRequest.SetRequestHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/113.0.0.0 Safari/537.36");

            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error fetching URL: {webRequest.error}");
                _txtPamyat.text = "Failed to fetch the link. Check console for details.";
            }
            else
            {
                Debug.Log($"Fetched content: {webRequest.downloadHandler.text}");
                // Process the response if needed
            }
        }
    }

}
