using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class scr_RewardString: MonoBehaviour
{
    [SerializeField] TMP_InputField _txtRewardName;
    [SerializeField] private TMP_InputField _txtRewardYear;
    [SerializeField] private Button _btnDeleteReward;

    public TMP_InputField TxtRewardName { get => _txtRewardName;}
    public TMP_InputField TxtRewardYear { get => _txtRewardYear;}

    private void V_DeleteReward()
    {
        //delete reward from card
    }
}
