using UnityEngine;

public class scr_General : MonoBehaviour
{
    public static scr_General m_General;

    [Header("Application data")]
    [SerializeField] D_ApplicaionData _d_ApplicationData;

    void Awake()
    {
        if (m_General == null)
        {
            m_General = this;
        }
    }

    void Update()
    {
        if (m_General == null)
        {
            m_General = this;
        }
    }

    #region Getters
    public string GET_DatabaseNameJSON { get => _d_ApplicationData._str_DatabaseNameJSON; }

    #endregion

    #region Setters


    #endregion

}

