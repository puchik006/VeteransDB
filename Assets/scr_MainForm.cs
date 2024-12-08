using System;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class scr_MainForm : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private TMP_InputField _txtInputFIO;
    [SerializeField] private TMP_InputField _txtInputDateOfBirth;
    [SerializeField] private TMP_InputField _txtInputDateofDeath;
    [SerializeField] private Image _inputImage;
    [SerializeField] private TMP_InputField _txtMainInfo;
    [SerializeField] private TMP_InputField _txtSearchField;

    [Header("Buttons")]
    [SerializeField] private Button _btnAddCard;
    [SerializeField] private Button _btnSaveCard;
    [SerializeField] private Button _btnAddPhoto;
    [SerializeField] private Button _btnAddReward;
    [SerializeField] private Button _btnSearch;

    [Header("String prefabs")]
    [SerializeField] private scr_RewardString _rewardStringPrefab;
    [SerializeField] private scr_SearchString _veteranStringPrefab;

    [Header("Scrollview Places")]
    [SerializeField] private GameObject _rewardsListPlace;
    [SerializeField] private GameObject _veteranListPlace;

    private List<scr_RewardString> _rewardStrings;
    private List<scr_SearchString> _searchStrings;

    private string _photoFileSourcePath;
    private string _imageURL;


    private void Awake()
    {
        _rewardStrings = new List<scr_RewardString>();
        _searchStrings = new List<scr_SearchString>();

        V_CheckStreamingAssets();
        V_ShowAllData();

        _btnAddPhoto.onClick.AddListener(() => V_AddPhoto());
        _btnAddReward.onClick.AddListener(() => V_AddNewReward());
        _btnSaveCard.onClick.AddListener(() => V_SaveDataToJSON());
        _btnAddCard.onClick.AddListener(() => V_NewCard());
        _btnSearch.onClick.AddListener(() => V_Search());
        
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
    private void V_SaveDataToJSON()
    {
        V_SavePhotoOnDisk(_txtInputFIO.text);

        string path = Path.Combine(Application.streamingAssetsPath, "Veterans.json");
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

        V_Search();
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
        string path = Path.Combine(Application.streamingAssetsPath, "Veterans.json");

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
            string path = Path.Combine(Application.streamingAssetsPath, "Veterans.json");

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
}
