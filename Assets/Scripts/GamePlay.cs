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
    public Text textCoils;
    public Text textCloudPoints;
    public Text textUserScore;
    public Slider TankBar;
    public Slider CoilBar;
    public GameObject OverheatPanel;
    public GameObject StorePanel;


    private bool _pressed = false;
    private LevelPacks currLP;
    private Levels currL;
    private UserData userData;
    private UserUpgrades userUpgrade;

    private static float timerDrag = 0.0f;
    private static float timerCountdown = 0.0f;

    private bool isOverheat = false;

    private float maxCoilTime;
    private float currCoilTime;

    private float currMultiplier;
    private float baseMultiplier;

    private float pointValue;
    private float currCloudScore;

    private List<Multipliers> multiTimer;
    private int currTimer = 0;


    // Use this for initialization
    void Start()
    {

        //Load Level Parameters
        LoadLevel();

    }

    void Awake()
    {
        //Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
    }

    // Update is called once per frame
    void Update()
    {
        float timeChange = Time.deltaTime;

        if (!_pressed)
        {
            //Cool Down
            if (currCoilTime < maxCoilTime)
            {
                currCoilTime += timeChange * 1000f;
                textCoils.text = currCoilTime.ToString();
                CoilBar.value = maxCoilTime - currCoilTime;
            }
            else if (isOverheat)
            {
                //Finished Overheating
                isOverheat = false;

                OverheatPanel.SetActive(false);
            }



            return;
        }

        //Countdown Timer
        timerCountdown -= timeChange * 1000f;

        //Check for end of tank
        if (timerCountdown <= 0)
        {
            //End Tank and Bring up modal
            currentTime.text = "0";
            TankBar.value = 0;
            _pressed = false;

            //Save Score            
            userData.currentMoney += Mathf.Round(currCloudScore);
            textUserScore.text = "Score: " + Mathf.Round(userData.currentMoney);
            currCloudScore = 0;

            //Bring Up Store Panel
            OpenStorePanel();

            return;

        }

        currentTime.text = timerCountdown.ToString();
        TankBar.value = timerCountdown;

        //Add to Drag Timer
        timerDrag += timeChange * 1000f;
        //currentTime.text = timerDrag.ToString();

        //Check for Coil 
        currCoilTime -= timeChange * 1000f;
        textCoils.text = currCoilTime.ToString();
        CoilBar.value = maxCoilTime - currCoilTime;

        if (currCoilTime <= 0)
        {
            //Overheat
            _pressed = false;

            //Reset Score - Do Not Save Score - User Failed
            currCloudScore = 0;
            textCloudPoints.text = "+0";

            //Start Overheat Process
            isOverheat = true;
            OverheatPanel.SetActive(true);


        }

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

        //Add Score
        currCloudScore += (pointValue * timeChange) * currMultiplier;

        //Show Score
        textCloudPoints.text = "+" + Mathf.RoundToInt(currCloudScore);
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

        //Save Score to Total
        userData.currentMoney += Mathf.Round(currCloudScore);
        textUserScore.text = "Score: " + Mathf.Round(userData.currentMoney);
        currCloudScore = 0;



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

        //Setup Coils
        maxCoilTime = currLP.vape.coils.Find(y => y.level == userUpgrade.coilLevel).Overheat;
        currCoilTime = maxCoilTime;

        //Setup Slider for Coil
        CoilBar.minValue = 0;
        CoilBar.maxValue = maxCoilTime;
        CoilBar.value = 0;

        //Setup Point Values (millisecond * pointvalue) = per second scoring
        pointValue = currL.pointValue;

        //Show Current User Score
        textUserScore.text = "Score: " + Mathf.Round(userData.currentMoney);

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

        if (File.Exists(Application.persistentDataPath + "/savedGame.gd"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/savedGame.gd", FileMode.Open);
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
        FileStream file;
        //Application.persistentDataPath is a string, so if you wanted you can put that into debug.log if you want to know where save games are located
        if (File.Exists(Application.persistentDataPath + "/savedGame.gd"))
        {
            file = File.Open(Application.persistentDataPath + "/savedGame.gd", FileMode.Open);
        }
        else
        {
            file = File.Create(Application.persistentDataPath + "/savedGame.gd"); //you can call it anything you want
        }
        bf.Serialize(file, userData);
        file.Close();
    }

    //Store Functions

    public void OpenStorePanel()
    {
        //Load upgrades and points

        //Open Panel
        StorePanel.SetActive(true);
    }

    public void CloseStorePanel()
    {
        StorePanel.SetActive(false);
    }

    public void RefillTank()
    {
        //Reloads all level and tank
        SaveUserData();

        //Setup Tank
        timerCountdown = currLP.vape.tanks.Find(y => y.level == userUpgrade.tankLevel).time;
        currentTime.text = timerCountdown.ToString();

        //Setup Slider for Tank 
        TankBar.minValue = 0;
        TankBar.maxValue = timerCountdown;
        TankBar.value = timerCountdown;

        CloseStorePanel();
    }



    //Close and Save Application

    void OnApplicationPause()
    {
        #if UNITY_EDITOR
            Debug.Log("EDITOR");
        #elif UNITY_ANDROID
            SaveUserData();
        #endif
    }

    void OnApplicationQuit()
    {
        SaveUserData();
    }



}
