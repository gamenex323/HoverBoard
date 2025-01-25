using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GlobalData 
{
    private static string _PlayerName = "_PlayerName";
    private static string _Ammo = "Ammo";

    public static string PlayerName
    {
        set { PlayerPrefs.SetString(_PlayerName, value); }
        get { return PlayerPrefs.GetString(_PlayerName); }
    }

    public static int Ammo
    {
        set { PlayerPrefs.SetInt(_Ammo, value); }
        get { return PlayerPrefs.GetInt(_Ammo); }
    }
}
