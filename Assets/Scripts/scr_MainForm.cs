using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class scr_MainForm : MonoBehaviour
{
    [Header("Fields")]
    [SerializeField] private TMP_InputField _txtInputFIO;
    [SerializeField] private TMP_InputField _txtInputDateOfBirth;
    [SerializeField] private TMP_InputField _txtInputDateOfDeath;
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
    [SerializeField] private Button _btnDeleteCard;

    [Header("String prefabs")]
    [SerializeField] private scr_RewardString _rewardStringPrefab;
    [SerializeField] private scr_SearchString _veteranStringPrefab;

    [Header("Scrollview Places")]
    [SerializeField] private GameObject _rewardsListPlace;
    [SerializeField] private GameObject _veteranListPlace;

    [Header("Info panel")]
    [SerializeField] private GameObject _infoPanel;
    [SerializeField] private TMP_Text _infoText;

    private List<scr_RewardString> _rewardStrings = new();
    private List<scr_SearchString> _searchStrings = new();

    private string _databasePath;
    private string _imageURL;
    private string _currentGUID;

    private scr_WebHandler _webHandler;

    private D_JSON _currentJSON;

    void Awake()
    {
        _webHandler = new ();

        //string url = "https://vm-86bbe67b.na4u.ru/ww2/data.json";
        //StartCoroutine(_webHandler.IE_Get(url, (reply) => _currentJSON = reply));

        _btnAddPhoto.onClick.AddListener(() => V_AddPhoto());
        _btnAddReward.onClick.AddListener(() => V_AddNewReward());
        _btnSaveCard.onClick.AddListener(() => V_SaveCard());
        _btnAddCard.onClick.AddListener(() => V_NewCard());
        _btnSearch.onClick.AddListener(() => StartCoroutine(V_LoadVeteransList()));
        _btnCheckPamyat.onClick.AddListener(() => V_CheckPamyatNaroda());
        _btnDeleteCard.onClick.AddListener(() => V_DeleteCard());

        StartCoroutine(V_LoadVeteransList());

        V_NewCard();
    }

    //New Card
    private void V_NewCard()
    {
        _currentGUID = Guid.NewGuid().ToString();
        Debug.Log("Current GUID: " + _currentGUID);

        _txtInputFIO.text = string.Empty;
        _txtInputDateOfBirth.text = string.Empty;
        _txtInputDateOfDeath.text = string.Empty;
        _txtMainInfo.text = string.Empty;
        _txtPamyat.text = string.Empty;

        _imageInput.sprite = null;

        V_DeleteAllRewards();

        _imageURL = null;
    }

    //DELETE CARD
    private void V_DeleteCard()
    {
        StartCoroutine(V_DeleteDataFromJSON());
    }

    private IEnumerator V_DeleteDataFromJSON()
    {
        V_InfoPanelOpen("Loading");

        string url = "https://vm-86bbe67b.na4u.ru/ww2/data.json";
        Debug.Log("Starting to fetch JSON from server: " + url);

        // Step 1: Fetch existing JSON data from the server
        string existingJson = "{}"; // Default to an empty JSON object if fetching fails

        //using (UnityWebRequest www = UnityWebRequest.Get(url))
        //{
        //    yield return www.SendWebRequest();

        //    if (www.result == UnityWebRequest.Result.Success)
        //    {
        //        Debug.Log("Successfully fetched JSON: " + www.downloadHandler.text);
        //        existingJson = www.downloadHandler.text;  // Store the existing data
        //    }
        //    else
        //    {
        //        Debug.LogError($"Failed to fetch JSON from server: {www.error}");
        //        Debug.LogError("Full response: " + www.downloadHandler.text);
        //        yield break; // Exit if fetch fails
        //    }
        //}

        yield return StartCoroutine(_webHandler.IE_Get(url, (reply) => existingJson = reply));

        // Step 2: Parse JSON data into D_JSON object
        D_JSON jsonData = JsonUtility.FromJson<D_JSON>(existingJson);

        // Step 3: Ensure Veterans list is initialized
        if (jsonData.Veterans == null) 
        {
            Debug.Log("empty list");
            jsonData.Veterans = new List<Dm_JSON>();
        }
        
        // Step 4: Find veteran by GUID and remove it
        int removeIndex = jsonData.Veterans.FindIndex(v => v.GUID == _currentGUID);

        Debug.Log("Index: " +  removeIndex);

        if (removeIndex != -1)
        {
            jsonData.Veterans.RemoveAt(removeIndex); // Remove veteran
            Debug.Log($"Veteran with GUID {_currentGUID} removed.");
        }
        else
        {
            Debug.LogWarning($"Veteran with GUID {_currentGUID} not found.");
        }

        // Step 5: Convert updated data back to JSON
        string updatedJson = JsonUtility.ToJson(jsonData, true);

        // Step 6: Upload the updated JSON back to the server only if data has changed
        if (removeIndex != -1) // Only upload if an entry was removed
        {
            Debug.Log("Uploading updated JSON data to the server...");
            yield return StartCoroutine(UploadJSONToServer(updatedJson));
            Debug.Log("JSON data updated and uploaded.");
        }
        else
        {
            Debug.Log("No changes made to JSON data, skipping upload.");
        }

        yield return StartCoroutine(V_LoadVeteransList());

        V_NewCard();

        V_InfoPanelClose();
    }

    private void V_InfoPanelOpen(string message)
    {
        _infoPanel.SetActive(true);
        _infoText.text = message;
    }

    private void V_InfoPanelClose()
    {
        _infoText.text = string.Empty;
        _infoPanel.SetActive(false);
    }

    [DllImport("__Internal")]
    private static extern void V_AddPhoto(); // Link to JavaScript function

    // Open the file picker
    public void OpenFilePicker()
    {
        V_AddPhoto();
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

    private void V_SavePhotoOnDisk(string fileName)
    {
        StartCoroutine(UploadPhotoToServer(fileName));
    }

    private IEnumerator UploadPhotoToServer(string fileName)
    {
        V_InfoPanelOpen("Loading");

        if (_imageInput == null || _imageInput.sprite == null) yield break;

        Sprite sprite = _imageInput.sprite;
        Texture2D texture = sprite.texture;

        // Ensure the texture is readable
        if (!texture.isReadable)
        {
            texture = new Texture2D(sprite.texture.width, sprite.texture.height, sprite.texture.format, false);
            texture.SetPixels(sprite.texture.GetPixels());
            texture.Apply();
        }

        // Convert the texture to PNG
        byte[] pngData = texture.EncodeToPNG();
        if (pngData == null)
        {
            Debug.LogError("Failed to encode texture to PNG.");
            yield break;
        }

        // Prepare the form for the HTTP POST request
        WWWForm form = new WWWForm();
        form.AddField("fileName", fileName);
        form.AddBinaryData("file", pngData, fileName + ".png", "image/png");

        using (UnityWebRequest www = UnityWebRequest.Post("https://vm-86bbe67b.na4u.ru/ww2/loadimg.php", form))
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error uploading photo: " + www.error);
            }
            else
            {
                Debug.Log("Photo uploaded successfully.");
                _imageURL = "https://vm-86bbe67b.na4u.ru/ww2/img/" + fileName + ".png"; 
            }
        }

        V_InfoPanelClose();
    }

    //LOAD SPRITE
    private async Task<Sprite> LoadSpriteFromServer(string imageUrl)
    {
        V_InfoPanelOpen("Loading");

        if (string.IsNullOrEmpty(imageUrl))
        {
            Debug.LogWarning("Image URL is empty or null.");
            V_InfoPanelClose();
            return null;
        }

        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            Debug.Log("Fetching image from: " + imageUrl);
            var operation = request.SendWebRequest();

            while (!operation.isDone)
            {
                await Task.Yield(); // Asynchronously wait
            }

            if (request.result == UnityWebRequest.Result.Success)
            {
                // Get the texture from the response
                Texture2D texture = DownloadHandlerTexture.GetContent(request);

                // Create a sprite from the texture
                Rect rect = new Rect(0, 0, texture.width, texture.height);
                V_InfoPanelClose();
                return Sprite.Create(texture, rect, new Vector2(0.5f, 0.5f));
            }
            else
            {
                Debug.LogError($"Failed to load image from server: {request.error}");
                V_InfoPanelClose();
                return null;
            }
        }


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
        StartCoroutine(UploadPhotoAndSaveJSONData(_currentGUID));
    }

    private IEnumerator UploadPhotoAndSaveJSONData(string fileName)
    {
        // Step 1: Upload the photo
        yield return StartCoroutine(UploadPhotoToServer(fileName));

        // Step 2: Save the JSON data
        Debug.Log("Starting JSON save process...");

        yield return StartCoroutine(V_SaveDataToJSON());

        Debug.Log("Load veteran list...");

        yield return StartCoroutine(V_LoadVeteransList());

        Debug.Log("Current saved GUID: " + _currentGUID);
        V_NewCard();
    }

    private IEnumerator V_SaveDataToJSON()
    {
        V_InfoPanelOpen("Saving data...");

        string url = "https://vm-86bbe67b.na4u.ru/ww2/data.json";
        Debug.Log("Starting to fetch JSON from server: " + url);

        string existingJson = "{}"; // Default to an empty JSON object if fetching fails

        // Step 1: Fetch existing JSON data from the server
        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Successfully fetched JSON: " + www.downloadHandler.text);
                existingJson = www.downloadHandler.text;  // Store the existing data
            }
            else
            {
                Debug.LogError($"Failed to fetch JSON from server: {www.error}");
                Debug.LogError("Full response: " + www.downloadHandler.text);  // Log the full response
            }
        }
        //
        // Step 2: Parse the existing JSON data into the D_JSON object
        D_JSON jsonData;

        jsonData = JsonUtility.FromJson<D_JSON>(existingJson);

        // Step 3: Ensure the Veterans list is initialized
        if (jsonData.Veterans == null)
        {
            Debug.LogWarning("Invalid or empty JSON. Initializing new structure.");
            jsonData = new D_JSON { Veterans = new List<Dm_JSON>() }; // Initialize list if null
        }

        // Step 4: Search for an existing veteran by GUID or add a new one
        int existingIndex = jsonData.Veterans.FindIndex(v => v.GUID == _currentGUID);

        if (existingIndex != -1)
        {

            Debug.Log("Update existing veteran: " + _currentGUID);

            // Update existing veteran
            var existingVeteran = jsonData.Veterans[existingIndex];
            existingVeteran.GUID = _currentGUID;
            existingVeteran.FullName = _txtInputFIO.text;
            existingVeteran.ImageURL = _imageURL;
            existingVeteran.DateOfBitrh = _txtInputDateOfBirth.text;
            existingVeteran.DateOfDeath = _txtInputDateOfDeath.text;
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

            jsonData.Veterans[existingIndex] = existingVeteran;  // Update the veteran in the list
        }
        else
        {
            Debug.Log("Save new veteran: " + _currentGUID);

            if (_currentGUID == string.Empty) 
            {
                _currentGUID = Guid.NewGuid().ToString();
                Debug.Log("NEW GUID GENERATED");
            }

            // Add new veteran
            Dm_JSON newVeteran = new Dm_JSON
            {
                GUID = _currentGUID,
                FullName = _txtInputFIO.text,
                ImageURL = _imageURL,
                DateOfBitrh = _txtInputDateOfBirth.text,
                DateOfDeath = _txtInputDateOfDeath.text,
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

            jsonData.Veterans.Add(newVeteran);  // Add the new veteran to the list
        }

        // Step 5: Convert the updated data back to JSON
        string updatedJson = JsonUtility.ToJson(jsonData, true);

        // Step 6: Upload the updated JSON back to the server
        yield return StartCoroutine(UploadJSONToServer(updatedJson));

        Debug.Log("JSON data updated and uploaded.");

        V_InfoPanelClose();
    }

    private IEnumerator UploadJSONToServer(string jsonData)
    {
        V_InfoPanelOpen("Loading data...");

        // Prepare the form for the HTTP POST request
        WWWForm form = new WWWForm();
        form.AddField("JS", jsonData);  // Send the JSON data to the PHP script

        using (UnityWebRequest www = UnityWebRequest.Post("https://vm-86bbe67b.na4u.ru/ww2/save.php", form)) // Make sure the URL is correct
        {
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Error uploading data: " + www.error);
            }
            else
            {
                Debug.Log("Data uploaded successfully.");

                Debug.Log(jsonData);
            }
        }

        V_InfoPanelClose();
    }

    //Load data 
    public async void V_LoadCardFromJSON(Dm_JSON veteran)
    {
        _currentGUID = veteran.GUID;

        _txtInputFIO.text = veteran.FullName;
        _txtInputDateOfBirth.text = veteran.DateOfBitrh;
        _txtInputDateOfDeath.text = veteran.DateOfDeath;
        _txtMainInfo.text = veteran.MainInfo;
        _txtPamyat.text = string.Empty;

        _imageInput.sprite = await LoadSpriteFromServer(veteran.ImageURL);
        _imageURL = veteran.ImageURL;

        V_DeleteAllRewards();
        veteran.Rewards.ForEach(r => V_AddExistingReward(r.RewardName,r.YearOfReward));

        Debug.Log("Current loaded GUID: " + _currentGUID);
    }

    #region Search
    private IEnumerator V_LoadVeteransList()
    {
        V_InfoPanelOpen("Loading...");

        string searchTerm = _txtSearchField.text;

        string endpoint = "https://vm-86bbe67b.na4u.ru/ww2/data.json";

        yield return _webHandler.FetchAndProcessData<D_JSON>(
            endpoint,
            jsonData => HandleVeteranData(jsonData, searchTerm),
            error => Debug.LogError($"Failed to load data: {error}")
        );

        V_InfoPanelClose();
    }

    private void HandleVeteranData(D_JSON jsonData, string searchTerm)
    {
        if (jsonData.Veterans == null)
        {
            Debug.LogWarning("No data or invalid JSON structure.");
            return;
        }

        V_DeleteSearchList();

        string lowerCaseSearchTerm = searchTerm?.ToLower();

        foreach (var veteran in jsonData.Veterans)
        {
            if (string.IsNullOrEmpty(lowerCaseSearchTerm) || veteran.FullName.ToLower().Contains(lowerCaseSearchTerm))
            {
                var veteranPrefab = Instantiate(_veteranStringPrefab, _veteranListPlace.transform);
                veteranPrefab.V_INITIALIZE(this, veteran);
                _searchStrings.Add(veteranPrefab);
            }
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
    #endregion

    #region Pamyat naroda
    public void V_CheckPamyatNaroda()
    {
        _txtPamyat.text = string.Empty;

        string linkTemplate = "https://pamyat-naroda.ru/heroes/?last_name=FAMILYNAME&first_name=NAME&middle_name=MIDDLENAME" +
                              "&group=all&types=pamyat_commander:nagrady_nagrad_doc:nagrady_uchet_kartoteka:nagrady_ubilein_kartoteka:" +
                              "pdv_kart_in:pdv_kart_in_inostranec:pamyat_voenkomat:potery_vpp:pamyat_zsp_parts:kld_ran:kld_bolezn:kld_polit:kld_upk:kld_vmf:kld_partizan:potery_doneseniya_o_poteryah:" +
                              "potery_gospitali:potery_utochenie_poter:potery_spiski_zahoroneniy:potery_voennoplen:potery_iskluchenie_iz_spiskov:potery_kartoteki:potery_rvk_extra:potery_isp_extra:" +
                              "same_doroga:same_rvk:same_guk:potery_knigi_pamyati&page=1&grouppersons=1";

        string fullName = _txtInputFIO.text.Trim(); 
        string[] nameParts = fullName.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        if (nameParts.Length < 2 || nameParts.Length > 3)
        {
            _txtPamyat.onSelect.RemoveAllListeners();
            _txtPamyat.text = "Invalid name format";
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

        // Fetch the link content using the WebHandler class.
        StartCoroutine(_webHandler.FetchAndProcessData<string>(
            finalLink,
            onSuccess: (response) => _txtPamyat.text = "Click link",
            onError: (error) => _txtPamyat.text = "Click link",
            useNoCors: true
        ));

        // Clear previous listeners to prevent duplicate calls.
        _txtPamyat.onSelect.RemoveAllListeners();

        // Add the listener for opening the URL.
        _txtPamyat.onSelect.AddListener(delegate { Application.OpenURL(finalLink); });
    }
    #endregion
}
