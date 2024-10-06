using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public Dictionary<AmmoType, int> PlayerAmmo = new Dictionary<AmmoType, int>();
    public static GameManager Instance;

    [SerializeField]
    private TextMeshProUGUI rifleAmmo;
    [SerializeField]
    private TextMeshProUGUI sniperAmmo;

    private int questItemsFound = 0;
    public int QuestItemsFound {
        get { return questItemsFound; }
        set {
            questItemsFound = value;
            UpdateObjectives();
        }
    }
    public int QuestItemsAvailable = 0;

    private int questEnemiesKilled = 0;
    public int QuestEnemiesKilled {
        get { return questEnemiesKilled; }
        set {
            questEnemiesKilled = value;
            UpdateObjectives();
        }
    }
    public int QuestEnemiesTotal = 0;

    [SerializeField]
    private GameObject minimap;

    [SerializeField]
    private GameObject minimapFull;

    [SerializeField]
    private Objective[] objectives;

    private bool[] objectivesCompleted;

    [SerializeField]
    public GameObject objectiveUiPrefab;

    [SerializeField]
    public GameObject objectiveUiContainer;
    
    [SerializeField]
    public Slider healthBar;

    private Player player;

    [SerializeField]
    private Transform briefings;

    public bool BriefingActive = true;
    private TickingText currentBriefing;

    [SerializeField]
    private GameObject[] gunIndicators;

    private bool win;
    private bool lose;
    private bool readyToWin;
    private bool readyToLose;

    [SerializeField]
    private GameObject debriefing;

    [SerializeField]
    private GameObject gameOver;


    public bool ReadyToExtract() {
        for(var i = 0; i < objectivesCompleted.Length - 1; i++) {
            if (!objectivesCompleted[i]) {
                return false;
            }
        }
        return true;
    }

    public void Extract() {
        if (lose) return;
        objectivesCompleted[objectivesCompleted.Length-1] = true;
        UpdateObjectives();
        player.ObjectiveCompleted();
        win = true;
        Invoke("Win", 2.0f);
    }

    public void Win() {
        BriefingActive = true;
        debriefing.SetActive(true);
        Invoke("ReadyToWin", 0.5f);
    }

    public void ReadyToWin() {
        readyToWin = true;
    }

    public void Die() {
        if (win) return;
        lose = true;
        Invoke("Lose", 1.5f);
    }

    public void Lose() {
        BriefingActive = true;
        gameOver.SetActive(true);
        Invoke("ReadyToLose", 0.5f);
    }

    public void ReadyToLose() {
        readyToLose = true;
    }

    void Awake() {
        Instance = this;
        PlayerAmmo[AmmoType.PISTOL] = 1;
        PlayerAmmo[AmmoType.RIFLE] = 0;
        PlayerAmmo[AmmoType.SNIPER] = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectivesCompleted = new bool[objectives.Length];
        CreateObjectives();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        var currentScene = SceneManager.GetActiveScene().buildIndex;
        var briefingContainer = briefings.GetChild(currentScene);
        briefingContainer.gameObject.SetActive(true);
        BriefingActive = true;
        currentBriefing = briefingContainer.GetComponentInChildren<TickingText>();
        SelectGun(0);
    }

    // Update is called once per frame
    void Update()
    {
        rifleAmmo.SetText(PlayerAmmo[AmmoType.RIFLE].ToString());
        sniperAmmo.SetText(PlayerAmmo[AmmoType.SNIPER].ToString());
        healthBar.value = player.Health / player.maxHealth;

        if (BriefingActive) {

            if (win) {
                if (readyToWin && Input.anyKeyDown) {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
                }
                return;
            }
            if (lose) {
                if (readyToLose && Input.anyKeyDown) {
                    SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                }
                return;
            }

            if (currentBriefing.IsDone() && Input.anyKeyDown) {
                BriefingActive = false;
                currentBriefing.transform.parent.gameObject.SetActive(false);
            } else {
                return;
            }
        }

        if (Input.GetKey(KeyCode.M) || Input.GetKey(KeyCode.Tab)) {
            minimap.SetActive(false);
            minimapFull.SetActive(true);
        } else {
            minimap.SetActive(true);
            minimapFull.SetActive(false);
        }
    }

    public void CreateObjectives() {
        for (var i = 0; i < objectives.Length; i++) {
            var ui = Instantiate(objectiveUiPrefab, objectiveUiContainer.transform);
            ui.GetComponentInChildren<TextMeshProUGUI>().SetText((i+1) + ".) " + objectives[i].Description + " ");
        }
    }

    public void UpdateObjectives() {
        if (questItemsFound >= QuestItemsAvailable) {
            for (var i = 0; i < objectives.Length; i++) {
                if (objectives[i].Type == ObjectiveType.GATHER_ITEMS) {
                    objectivesCompleted[i] = true;
                }
            }
        }

        if (questEnemiesKilled >= QuestEnemiesTotal) {
            for (var i = 0; i < objectives.Length; i++) {
                if (objectives[i].Type == ObjectiveType.ELIMINATION) {
                    objectivesCompleted[i] = true;
                }
            }
        }

        for (var i = 0; i < objectives.Length; i++) {
            var ui = objectiveUiContainer.transform.GetChild(i+1);
            var text = ui.GetComponentInChildren<TextMeshProUGUI>();
            if (objectivesCompleted[i]) {
                text.SetText((i+1) + ".)<s> " + objectives[i].Description + " </s>");
            }
        }
    }

    public void SelectGun(int gunIndex) {
        foreach(var item in gunIndicators) {
            item.SetActive(false);
        }
        gunIndicators[gunIndex].SetActive(true);
    }
}


[Serializable]
public struct Objective {
    public ObjectiveType Type;
    public string Description;
    public bool Completed;
}

[Serializable]
public enum ObjectiveType {
    GATHER_ITEMS,
    EXTRACT,
    ELIMINATION
}
