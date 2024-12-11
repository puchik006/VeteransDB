using System.Collections.Generic;
using System;

[Serializable]
public struct D_JSON
{
    public List<Dm_JSON> Veterans;
}

/// <summary>
/// Represents veteran
/// </summary>
[Serializable]
public struct Dm_JSON
{
    public string GUID;
    public string FullName;
    public string ImageURL;
    public string DateOfBitrh;
    public string DateOfDeath;
    public string MainInfo;
    public List<Dmm_JSON> Rewards;
}

/// <summary>
/// Represents reward
/// </summary>
[Serializable]
public struct Dmm_JSON
{
    public string RewardName;
    public string YearOfReward;
}

[Serializable]
public struct D_ApplicaionData
{
    public string _str_DatabaseNameJSON;
}

