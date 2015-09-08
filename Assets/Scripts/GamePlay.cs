using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GamePlay : MonoBehaviour
{

    public int currPack = 1;
    public int currLevel = 1;

    public Image vapeImage;
    public Text currentTime;
    public Text textMultiplier;
    public Slider TankBar;

    
    private bool _pressed = false;
    private LevelPacks currLP;
    private Levels currL;
    private UserData userData;
    private UserUpgrades userUpgrade;
    
    private static float timerDrag = 0.0f;
    private static float timerCountdown = 0.0f;

    private float tankTime;
    private float coilTime;

    private float currMultiplier;
    private float baseMultiplier;

    private List<Multipliers> multiTimer;
    private int currTimer = 0;


    // Use this for initialization
    void Start()
    {

        //Load Level Parameters
        LoadLevel();
        
    }

    // Update is called once per frame
    void Update()
    {

        if (!_pressed)
            return;

        //Add to Drag Timer
        timerDrag += Time.deltaTime * 1000f;
        //currentTime.text = timerDrag.ToString();

        //Countdown Timer
        timerCountdown -= Time.deltaTime * 1000f;
        currentTime.text = timerCountdown.ToString();
        TankBar.value = timerCountdown;

        //Debug.Log("Curr Timer: " + currTimer);
        //Debug.Log("Count: " + (multiTimer.Count - 1));

        //Check for Timer Multiplier
        if (currTimer <= (multiTimer.Count - 1))
        {            
            if (multiTimer[currTimer].TimeValue <= timerDrag)
            { 
                //Set Multiplier
                currMultiplier += multiTimer[currTimer].MultiplierValue;
                textMultiplier.text = "x" + currMultiplier.ToString();

                //Hit the timer and move to next
                currTimer++;
            }
        }        
    }

    //Button Presses Events
    public void ButtonDown()
    {
        _pressed = true;
    }

    public void ButtonUp()
    {
        _pressed = false;

        //Stop Timer
        timerDrag = 0.0f;

        //Reset Multiplers
        currTimer = 0;
        currMultiplier = baseMultiplier;
        textMultiplier.text = "x" + currMultiplier.ToString();

    }

    void LoadLevel()
    {
        //Load Game Data from XML 
        LoadGameData(currPack, currLevel);

        //Load User Data 
        LoadUserData();

        //Setup Game Parameters
        //Tank
        timerCountdown = currLP.vape.tanks.Find(y => y.level == userUpgrade.tankLevel).time;
        currentTime.text = timerCountdown.ToString();

        //Setup Slider for Tank 
        TankBar.minValue = 0;
        TankBar.maxValue = timerCountdown;
        TankBar.value = timerCountdown;
        
        //Setup Multipliers
        currMultiplier = userData.baseMultiplier;
        baseMultiplier = userData.baseMultiplier;


        //Load Sprite of Vaporizer
        Sprite vapeSprite = Resources.Load<Sprite>(currLP.vape.sprite);
        
        vapeImage.sprite = vapeSprite;
        vapeImage.rectTransform.sizeDelta = new Vector2(vapeSprite.rect.width, vapeSprite.rect.height);
            
        
    }

    void LoadGameData(int packID, int levelID)
    {
        GameData gameData = new GameData();
        gameData = XmlIO.LoadXml<GameData>("LevelData");

        //Using Array so need to start at zero
        //TO DO: CHECK FOR level not found
        currLP = gameData.levelPacks[packID - 1];

        currL = currLP.levels[levelID - 1];

        //Setup Timed Multipliers
        multiTimer = currL.multipliers;

        Debug.Log(currL.name);
    }

    void LoadUserData()
    {
        userData = new UserData();

        if (File.Exists(Application.persistentDataPath + "/savedGames.gd"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedGames.gd", FileMode.Open);
            userData = (UserData)bf.Deserialize(file);
            file.Close();
                        
        }
        else
        {
            //Create new save
            //Add
            userData.currentMoney = 0;
            userData.baseMultiplier = 1;
            userData.lastPlay = DateTime.Now;
            userData.startPlay = DateTime.Now;

            userData.userUpgrades = new List<UserUpgrades>();

            UserUpgrades upgrade = new UserUpgrades();
            upgrade.levelPack = 1;
            upgrade.level = 1;
            upgrade.tankLevel = 1;
            upgrade.coilLevel = 1;
            upgrade.cottonLevel = 1;

            userData.userUpgrades.Add(upgrade);

            SaveUserData();
        }

        //Get Current Upgrade Level
        userUpgrade = userData.GetUpgrade(currPack, currLevel);
    }

    void SaveUserData()
    {
        BinaryFormatter bf = new BinaryFormatter();
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        FileStream file = File.Create(Application.persistentDataPath + "/savedGames.gd"); //you can call it anything you want
        bf.Serialize(file, userData);
        file.Close();
    }


    //Count Down Timer


   
}
