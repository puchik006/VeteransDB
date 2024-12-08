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
    //[SerializeField] private GameObject _veteranStringPrefab;

    [Header("Scrollview Places")]
    [SerializeField] private GameObject _rewardsListPlace;

    private List<scr_RewardString> _rewardStrings;


    private void Awake()
    {
        _rewardStrings = new List<scr_RewardString>();


        _btnAddPhoto.onClick.AddListener(() => V_AddPhoto());
        _btnAddReward.onClick.AddListener(() => V_AddReward());
        _btnSaveCard.onClick.AddListener(() => V_SaveDataToJSON());
    }


    private void V_AddPhoto()
    {
        // Open a file dialog for the user to select a photo
        string sourceFilePath = UnityEditor.EditorUtility.OpenFilePanel("Select an Image", "", "png,jpg,jpeg");
        if (string.IsNullOrEmpty(sourceFilePath))
        {
            Debug.LogWarning("No file selected.");
            return;
        }

        // Get the file name and define the target path
        string fileName = System.IO.Path.GetFileName(sourceFilePath);
        string targetPath = System.IO.Path.Combine(Application.streamingAssetsPath, fileName);

        // Copy the file to StreamingAssets
        try
        {
            System.IO.File.Copy(sourceFilePath, targetPath, true);
            Debug.Log($"File copied to: {targetPath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to copy file: {e.Message}");
            return;
        }

        // Save the relative path
        string relativePath = $"StreamingAssets/{fileName}";

        _inputImage.sprite = LoadSpriteFromPath(targetPath);

        // Update your JSON object or field
        Debug.Log($"Saved relative path: {relativePath}");
    }


    private Sprite LoadSpriteFromPath(string filePath)
    {
        byte[] fileData = File.ReadAllBytes(filePath);
        Texture2D texture = new Texture2D(2, 2);
        texture.LoadImage(fileData);
        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }

    private void V_AddReward()
    {
        scr_RewardString newReward = Instantiate(_rewardStringPrefab, _rewardsListPlace.transform);
        _rewardStrings.Add(newReward);
    }

    private void V_DeleteReward(scr_RewardString reward)
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

    private void V_SaveDataToJSON()
    {
        D_JSON jsonData = new D_JSON
        {
            Veterans = new List<Dm_JSON>
        {
            new Dm_JSON
            {
                FullName = _txtInputFIO.text,
                ImageURL = $"StreamingAssets/{System.IO.Path.GetFileName(_inputImage.sprite.name)}", // Use the saved relative path
                DateOfBitrh = _txtInputDateOfBirth.text,
                DateOfDeath = _txtInputDateofDeath.text,
                Discription = _txtMainInfo.text,
                Rewards = new List<Dmm_JSON>()
            }
        }
        };

        // Add rewards from the form
        foreach (var reward in _rewardStrings)
        {
            jsonData.Veterans[0].Rewards.Add(new Dmm_JSON
            {
                RewardName = reward.TxtRewardName.text,
                YearOfReward = reward.TxtRewardYear.text
            });
        }

        // Convert to JSON and save
        string json = JsonUtility.ToJson(jsonData, true);
        string path = System.IO.Path.Combine(Application.streamingAssetsPath, "data.json");
        System.IO.File.WriteAllText(path, json);

        Debug.Log($"JSON saved to: {path}");
    }

}
