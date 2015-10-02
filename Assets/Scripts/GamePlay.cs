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

    public Image imageVape;
    public Image imageCurrentLevel;

    public Text currentTime;
    public Text textMultiplier;
    public Text textCoils;
    public Text textCloudPoints;
    public Text textUserScore;
    public Text textStoreScore;
    public Text textTankCost;
    public Text textCoilCost;
    public Text textCottonCost;
    public Text textCurrentLevel;
    public Text textPointsToNextLevel;
    
    public Slider TankBar;
    public Slider CoilBar;
    public Slider LevelBar;
    public GameObject OverheatPanel;
    public GameObject StorePanel;
    
    public GameObject SmokeController;
    public GameObject BlueSwirlController;

    public Button buttonBuyTanks;
    public Image[] tankUpgrades;

    public Button buttonBuyCoils;
    public Image[] coilUpgrades;

    public Button buttonBuyCottons;
    public Image[] cottonUpgrades;

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

    private float cooldownAmount;

    private float currMultiplier;
    private float baseMultiplier;

    private float pointValue;
    private float currCloudScore;

    private List<Multipliers> multiTimer;
    private int currTimer = 0;

    private Animator smokeAnimator;
    private Animator blueSwirlAnimator;


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
                //Update with Cotton Timer
                currCoilTime += timeChange * cooldownAmount;
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
            UpdateScore();
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

            //Reset Muliplier
            textMultiplier.text = "x" + baseMultiplier.ToString();

            //Start Overheat Process
            isOverheat = true;
            OverheatPanel.SetActive(true);


        }

        //Check for Timer Multiplier
        if (currTimer <= (multiTimer.Count - 1))
        {
            if (multiTimer[currTimer].TimeValue <= timerDrag)
            {
                if (currTimer > 0 && !isOverheat)
                {
                    blueSwirlAnimator.SetTrigger("SetoffSwirl");
                }                
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
        smokeAnimator.SetBool("SmokeOn", true);
    }

    public void ButtonUp()
    {
        _pressed = false;
        smokeAnimator.SetBool("SmokeOn", false);

        //Stop Timer
        timerDrag = 0.0f;

        //Reset Multiplers
        currTimer = 0;
        currMultiplier = baseMultiplier;
        textMultiplier.text = "x" + currMultiplier.ToString();

        //Save Score to Total
        userData.currentMoney += Mathf.Round(currCloudScore);
        UpdateScore();
        
        currCloudScore = 0;



    }

    void LoadLevel()
    {

        //Load User Data 
        LoadUserData();

        //Load Game Data from XML 
        LoadGameData(currPack, currLevel);

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

        //Setup Cooldown Timer
        cooldownAmount = currLP.vape.cottons.Find(y => y.level == userUpgrade.cottonLevel).Overheat;

        //Setup Slider for Coil
        CoilBar.minValue = 0;
        CoilBar.maxValue = maxCoilTime;
        CoilBar.value = 0;

        //Setup Level Bar
        //Setup Slider for Coil
        LevelBar.minValue = 0;
        LevelBar.maxValue = currL.pointsToNextLevel;
        LevelBar.value = userData.currentMoney;

        //Setup Point Values (millisecond * pointvalue) = per second scoring
        pointValue = currL.pointValue;

        //Show Current User Score
        UpdateScore();

        //Load Sprite of Vaporizer
        Sprite vapeSprite = Resources.Load<Sprite>(currLP.vape.sprite);

        imageVape.sprite = vapeSprite;
        imageVape.rectTransform.sizeDelta = new Vector2(vapeSprite.rect.width, vapeSprite.rect.height);

        //Setup Smoke
        smokeAnimator = SmokeController.GetComponent<Animator>() as Animator;

        //Setup BlueSwirl
        blueSwirlAnimator = BlueSwirlController.GetComponent<Animator>() as Animator;

    }

    private void UpdateScore()
    {
        //Show Current User Score
        textUserScore.text = Mathf.Round(userData.currentMoney).ToString();

        //Show Updated Store Score
        textStoreScore.text = Mathf.Round(userData.currentMoney).ToString();

        //Update Level Stats
        textCurrentLevel.text = userData.level.ToString();

        //Update next Level         
        float nextLevelPoints = currL.pointsToNextLevel;

        //Check for Next Level
        if (nextLevelPoints - 1 <= userData.currentMoney)
        {
            //Next Level
            NextLevel();
        }

        imageCurrentLevel.fillAmount = userData.currentMoney / nextLevelPoints;

        textPointsToNextLevel.text = (nextLevelPoints - userData.currentMoney).ToString();

        

    }

    private void NextLevel()
    {
        userData.level += 1;
        currLevel += 1;

        currL = currLP.levels[currLevel - 1];

        //Setup Timed Multipliers
        multiTimer = currL.multipliers;

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
            NewUser();
        }

        currPack = userData.levelPack;
        currLevel = userData.level;

        //Get Current Upgrade Level
        userUpgrade = userData.GetUpgrade(userData.selectedVape);
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
        //Load Points
        textStoreScore.text = Mathf.Round(userData.currentMoney).ToString();
        LevelBar.value = userData.currentMoney;

        LoadUpgrades();

        //Open Panel
        StorePanel.SetActive(true);
    }

    private void LoadUpgrades()
    {
        //Load Upgrades
        //Tank
        for (int i = 1; i <= userUpgrade.tankLevel; i++)
        {
            tankUpgrades[i - 1].gameObject.SetActive(true);
        }
        //Costs
        if (userUpgrade.tankLevel < currLP.vape.tanks.Count)
        {
            buttonBuyTanks.gameObject.SetActive(true);
            //Subtract 1 because of Array
            textTankCost.text = "$" + currLP.vape.tanks[userUpgrade.tankLevel].cost;
        }
        else
        {
            //Hide Button
            buttonBuyTanks.gameObject.SetActive(false);
        }

        if (userData.currentMoney < currLP.vape.tanks[userUpgrade.tankLevel].cost)
        {
            //Cannot affort, change color
            buttonBuyTanks.gameObject.SetActive(false);
        }
        else
        {
            //Can afford, turn normal color
            buttonBuyTanks.gameObject.SetActive(true);
        }

        //Coils
        for (int i = 1; i <= userUpgrade.coilLevel; i++)
        {
            coilUpgrades[i - 1].gameObject.SetActive(true);
        }
        //Costs
        if (userUpgrade.coilLevel < currLP.vape.coils.Count)
        {
            buttonBuyCoils.gameObject.SetActive(true);
            //Subtract 1 because of Array
            textCoilCost.text = "$" + currLP.vape.coils[userUpgrade.coilLevel].cost;
        }
        else
        {
            //Hide Button
            buttonBuyCoils.gameObject.SetActive(false);
        }

        if (userData.currentMoney < currLP.vape.coils[userUpgrade.coilLevel].cost)
        {
            //Cannot affort, change color
            buttonBuyCoils.gameObject.SetActive(false);
        }
        else
        {
            //Can afford, turn normal color
            buttonBuyCoils.gameObject.SetActive(true);
        }

        //Cottons
        for (int i = 1; i <= userUpgrade.cottonLevel; i++)
        {
            cottonUpgrades[i - 1].gameObject.SetActive(true);
        }
        //Costs
        if (userUpgrade.cottonLevel < currLP.vape.cottons.Count)
        {
            buttonBuyCottons.gameObject.SetActive(true);
            //Subtract 1 because of Array
            textCottonCost.text = "$" + currLP.vape.cottons[userUpgrade.cottonLevel].cost;
        }
        else
        {
            //Hide Button
            buttonBuyCottons.gameObject.SetActive(false);
        }

        if (userData.currentMoney < currLP.vape.cottons[userUpgrade.cottonLevel].cost)
        {
            //Cannot affort, change color
            buttonBuyCottons.gameObject.SetActive(false);
        }
        else
        {
            //Can afford, turn normal color
            buttonBuyCottons.gameObject.SetActive(true);
        }
    }

    public void BuyTank()
    {
        if (userData.currentMoney >= currLP.vape.tanks[userUpgrade.tankLevel].cost)
        { 
            //Purchase
            userData.currentMoney -= currLP.vape.tanks[userUpgrade.tankLevel].cost;
            userUpgrade.tankLevel += 1;

            //Update Score         
            UpdateScore();

            LoadUpgrades();

            //Update Tank            
            TankBar.maxValue = currLP.vape.tanks.Find(y => y.level == userUpgrade.tankLevel).time;

        }
    }

    public void BuyCoil()
    {
        if (userData.currentMoney >= currLP.vape.coils[userUpgrade.coilLevel].cost)
        {
            //Purchase
            userData.currentMoney -= currLP.vape.coils[userUpgrade.coilLevel].cost;
            userUpgrade.coilLevel += 1;

            //Update Score         
            UpdateScore();

            LoadUpgrades();

            //Update Coil          
            maxCoilTime = currLP.vape.coils.Find(y => y.level == userUpgrade.coilLevel).Overheat;
            currCoilTime = maxCoilTime;

        }
    }

    public void BuyCotton()
    {
        if (userData.currentMoney >= currLP.vape.cottons[userUpgrade.cottonLevel].cost)
        {
            //Purchase
            userData.currentMoney -= currLP.vape.cottons[userUpgrade.cottonLevel].cost;
            userUpgrade.cottonLevel += 1;

            //Update Score         
            UpdateScore();

            LoadUpgrades();

            //Update Cooldown
            cooldownAmount = currLP.vape.cottons.Find(y => y.level == userUpgrade.cottonLevel).Overheat;

        }
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


    public void NewUser()
    {
        //Sets Up New User

        userData.currentMoney = 0;
        userData.baseMultiplier = 1;
        userData.lastPlay = DateTime.Now;
        userData.startPlay = DateTime.Now;
        userData.level = 1;
        userData.levelPack = 1;
        userData.selectedVape = 1;

        userData.userUpgrades = new List<UserUpgrades>();

        UserUpgrades upgrade = new UserUpgrades();
        upgrade.vapeID = 1;
        upgrade.tankLevel = 1;
        upgrade.coilLevel = 1;
        upgrade.cottonLevel = 1;

        userData.userUpgrades.Add(upgrade);

        //Add First Vape
        //userData.UserVapes = new List<Vape>();

        SaveUserData();
        
    }

    public void ButtonNewUser()
    {
        NewUser();

        LoadLevel();
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
