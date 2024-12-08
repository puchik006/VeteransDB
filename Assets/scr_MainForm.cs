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

    [Header("Buttons")]
    [SerializeField] private Button _btnAddCard;
    [SerializeField] private Button _btnSaveCard;
    [SerializeField] private Button _btnAddPhoto;
    [SerializeField] private Button _btnAddReward;

    [Header("String prefabs")]
    [SerializeField] private GameObject _rewardStringPrefab;
    [SerializeField] private GameObject _veteranStringPrefab;


    private void V_SaveDataToJSON()
    {
        // when press button save should save all fields according scructure to JSON
    }
}
