using TMPro;
using UnityEngine;

public class scr_SearchString : MonoBehaviour
{
    [SerializeField] private TMP_InputField _txtFullName;
    [SerializeField] private TMP_InputField _txtDateOfBirth;
    [SerializeField] private TMP_InputField _txtDateOfDeath;

    private scr_MainForm _mainForm;
    private Dm_JSON _veteran;


    public void V_INITIALIZE(scr_MainForm mainForm, Dm_JSON veteran)
    {
        _mainForm = mainForm;
        _veteran = veteran;

        _txtFullName.text = veteran.FullName;
        _txtDateOfBirth.text = veteran.DateOfBitrh;
        _txtDateOfDeath.text = veteran.DateOfDeath;

        _txtFullName.onSelect.AddListener(delegate { _mainForm.V_LoadCardFromJSON(_veteran); });
        _txtDateOfBirth.onSelect.AddListener(delegate { _mainForm.V_LoadCardFromJSON(_veteran); });
        _txtDateOfDeath.onSelect.AddListener(delegate { _mainForm.V_LoadCardFromJSON(_veteran); });
    }
}
