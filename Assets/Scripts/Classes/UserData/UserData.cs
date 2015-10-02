using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

[Serializable]
public class UserData
{
    public float currentMoney;

    public DateTime startPlay;
    public DateTime lastPlay;

    public int level;
    public int levelPack;

    public float baseMultiplier;

    public List<UserUpgrades> userUpgrades;

    public UserUpgrades GetUpgrade(int VapeID)
    {
        UserUpgrades holdVal;

        holdVal = this.userUpgrades.Find(y => y.vapeID == VapeID);

        if (holdVal == null)
        {
            // Not found, so return base level stats.
            holdVal = new UserUpgrades();
            holdVal.vapeID = 1;
            holdVal.tankLevel = 1;
            holdVal.coilLevel = 1;
            holdVal.cottonLevel = 1;
        }

        return holdVal;
    }

    //TODO: Add Vape list
    public List<Vape> UserVapes;
    public int selectedVape;

}
