using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData 
{
    private static string _PlayerName = "_PlayerName";

    public static string PlayerName
    {
        set { PlayerPrefs.SetString(_PlayerName, value); }
        get { return PlayerPrefs.GetString(_PlayerName); }
    }
}
