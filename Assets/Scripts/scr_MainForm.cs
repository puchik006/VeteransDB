using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class scr_MainForm : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private TMP_InputField _txtInputFIO;
    [SerializeField] private TMP_InputField _txtInputDateOfBirth;
    [SerializeField] private TMP_InputField _txtInputDateofDeath;
    [SerializeField] private Image _imageInput;

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
    //private string _photoFileSourcePath;
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
    //private void V_AddPhoto()
    //{
    //    //// Open the file picker using StandaloneFileBrowser
    //    //var extensions = new[] {
    //    //    new ExtensionFilter("Image Files", "png", "jpg", "jpeg")
    //    //};
    //    //string[] paths = StandaloneFileBrowser.OpenFilePanel("Select an Image", "", extensions, false);

    //    //if (paths.Length > 0 && !string.IsNullOrEmpty(paths[0]))
    //    //{
    //    //    _photoFileSourcePath = paths[0];
    //    //    _inputImage.sprite = LoadSpriteFromPath(paths[0]);
    //    //}
    //    //else
    //    //{
    //    //    Debug.LogWarning("No file selected.");
    //    //}
    //}

    [DllImport("__Internal")]
    private static extern void V_AddPhoto(); // Link to JavaScript function

     // Assign this in the Inspector

    // Open the file picker
    public void OpenFilePicker()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        V_AddPhoto();
#endif
    }

    // Called by JavaScript when an image is selected
    public void OnImageSelected(string base64Image)
    {
        Debug.Log("Image selected: " + base64Image);

        // Decode the base64 string to a byte array
        byte[] imageBytes = System.Convert.FromBase64String(base64Image.Substring(base64Image.IndexOf(",") + 1));

        // Create a Texture2D from the image bytes
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(imageBytes);

        // Convert the Texture2D to a Sprite
        Rect rect = new Rect(0, 0, texture.width, texture.height);
        Sprite sprite = Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));

        // Assign the Sprite to the UI Image
        if (_imageInput != null)
        {
            _imageInput.sprite = sprite;
        }
        else
        {
            Debug.LogError("_imageInput is not assigned in the Inspector.");
        }
    }

    //
    //
    //
    //private void V_SavePhotoOnDisk(string fileName)
    //{
    //    if (_photoFileSourcePath == null) return;

    //    string fileExtension = Path.GetExtension(_photoFileSourcePath);
    //    string fileNameWithExtension = fileName + fileExtension;
    //    string photoFolderPath = Path.Combine(Application.streamingAssetsPath, "Photo");
    //    string targetPath = Path.Combine(photoFolderPath, fileNameWithExtension);

    //    _imageURL = targetPath; 

    //    if (!Directory.Exists(photoFolderPath))
    //    {
    //        Directory.CreateDirectory(photoFolderPath);  
    //        Debug.Log($"Created folder at: {photoFolderPath}");
    //    }

    //    try
    //    {
    //        File.Copy(_photoFileSourcePath, targetPath, true); 
    //        Debug.Log($"File copied to: {targetPath}");
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"Failed to copy file: {e.Message}");
    //    }
    //}

    private void V_SavePhotoOnDisk(string fileName)
    {
        if (_imageInput == null || _imageInput.sprite == null) return;

        // Get the sprite from _imageInput
        Sprite sprite = _imageInput.sprite;
        Texture2D texture = sprite.texture;

        // Ensure the texture is readable (if not, copy the texture data to a new Texture2D)
        if (!texture.isReadable)
        {
            texture = new Texture2D(sprite.texture.width, sprite.texture.height, sprite.texture.format, false);
            texture.SetPixels(sprite.texture.GetPixels());
            texture.Apply();
        }

        // Create the folder if it doesn't exist
        string photoFolderPath = Path.Combine(Application.streamingAssetsPath, "Photo");
        if (!Directory.Exists(photoFolderPath))
        {
            Directory.CreateDirectory(photoFolderPath);
            Debug.Log($"Created folder at: {photoFolderPath}");
        }

        // Save the texture to a PNG file
        byte[] pngData = texture.EncodeToPNG();
        string fileNameWithExtension = fileName + ".png";  // Save as PNG
        string targetPath = Path.Combine(photoFolderPath, fileNameWithExtension);

        try
        {
            File.WriteAllBytes(targetPath, pngData);  // Save PNG to disk
            _imageURL = targetPath;  // Store the path if needed
            Debug.Log($"File saved to: {targetPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to save image: {e.Message}");
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

        //_photoFileSourcePath = null;
    }

    //New Card
    private void V_NewCard()
    {
        _txtInputFIO.text = string.Empty;
        _txtInputDateOfBirth.text = string.Empty;
        _txtInputDateofDeath.text = string.Empty;
        _txtMainInfo.text = string.Empty;

       _imageInput.sprite = null;

        V_DeleteAllRewards();

        //_photoFileSourcePath = null;
        _imageURL = null;

    }

    //Load data 
    public void V_LoadCardFromJSON(Dm_JSON veteran)
    {
        _txtInputFIO.text = veteran.FullName;
        _txtInputDateOfBirth.text = veteran.DateOfBitrh;
        _txtInputDateofDeath.text = veteran.DateOfDeath;
        _txtMainInfo.text = veteran.MainInfo;

        _imageInput.sprite = LoadSpriteFromPath(veteran.ImageURL);
        _imageURL = veteran.ImageURL;

        V_DeleteAllRewards();
        veteran.Rewards.ForEach(r => V_AddExistingReward(r.RewardName,r.YearOfReward));

        //_photoFileSourcePath = null;
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
            }
        }
    }

}
