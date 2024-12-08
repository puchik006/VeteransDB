using System.Collections.Generic;
using TMPro;
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

    private void V_AddPhoto()
    {
        // on press button add photo it should be possible for user to choose photo from explorer
        // photo should be saved in StreamingAssets folder
        // in JSON we need to save just path of the photo in StreamingsAssets
    }

    private void V_AddReward()
    {
        // when press button add reward should be instantiate new reward prefab
        // instantiate reward string into rewards list place
    }

    private void V_DeleteReward()
    {
        // reward should be deleted
    }

    private void V_DeleteAllRewards()
    {
        // this method need to clean reward list
    }


    private void V_SaveDataToJSON()
    {
        // when press button save should save all fields according scructure to JSON
        // fro rewards it should check all that exist in current form and save it too
    }
}
