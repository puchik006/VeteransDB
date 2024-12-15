using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class scr_RewardString : MonoBehaviour
{
    [SerializeField] TMP_InputField _txtRewardName;
    [SerializeField] TMP_InputField _txtRewardYear;
    [SerializeField] private Button _btnDeleteReward;

    private scr_MainForm _mainForm;

    public TMP_InputField RewardName { get => _txtRewardName; set => _txtRewardName = value; }
    public TMP_InputField RewardYear { get => _txtRewardYear; set => _txtRewardYear = value; }
    

    private void Awake()
    {
        _btnDeleteReward.onClick.AddListener(() => V_DeleteReward());
    }

    public void V_INITIALIZE(scr_MainForm mainForm)
    {
        _mainForm = mainForm;
    }

    private void V_DeleteReward()
    {
        if (_mainForm != null)
        {
            _mainForm.V_DeleteReward(this); 
        }
    }

    private void OnDestroy()
    {
        _btnDeleteReward.onClick.RemoveAllListeners();
    }
}
