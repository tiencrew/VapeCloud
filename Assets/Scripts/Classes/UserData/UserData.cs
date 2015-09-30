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

    public float baseMultiplier;

    public List<UserUpgrades> userUpgrades;

    public UserUpgrades GetUpgrade(int LevelPack, int Level)
    {
        UserUpgrades holdVal;

        holdVal = this.userUpgrades.Find(y => y.levelPack == LevelPack && y.level == Level);

        if (holdVal == null)
        {
            // Not found, so return base level stats.
            holdVal = new UserUpgrades();
            holdVal.levelPack = LevelPack;
            holdVal.level = Level;
            holdVal.tankLevel = 1;
            holdVal.coilLevel = 1;
            holdVal.cottonLevel = 1;
        }

        return holdVal;
    }

	
}
