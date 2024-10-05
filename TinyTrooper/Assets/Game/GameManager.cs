using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
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

    public bool ReadyToExtract() {
        for(var i = 0; i < objectivesCompleted.Length - 1; i++) {
            if (!objectivesCompleted[i]) {
                return false;
            }
        }
        return true;
    }

    public void Extract() {
        objectivesCompleted[objectivesCompleted.Length-1] = true;
        UpdateObjectives();
    }

    void Awake() {
        Instance = this;
        PlayerAmmo[AmmoType.PISTOL] = 1;
        PlayerAmmo[AmmoType.RIFLE] = 1000;
        PlayerAmmo[AmmoType.SNIPER] = 3;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        objectivesCompleted = new bool[objectives.Length];
        CreateObjectives();
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        rifleAmmo.SetText(PlayerAmmo[AmmoType.RIFLE].ToString());
        sniperAmmo.SetText(PlayerAmmo[AmmoType.SNIPER].ToString());

        if (Input.GetKey(KeyCode.M)) {
            minimap.SetActive(false);
            minimapFull.SetActive(true);
        } else {
            minimap.SetActive(true);
            minimapFull.SetActive(false);
        }
        healthBar.value = player.Health / player.maxHealth;
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

        for (var i = 0; i < objectives.Length; i++) {
            var ui = objectiveUiContainer.transform.GetChild(i+1);
            var text = ui.GetComponentInChildren<TextMeshProUGUI>();
            if (objectivesCompleted[i]) {
                text.SetText((i+1) + ".)<s> " + objectives[i].Description + " </s>");
            }
        }
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
    EXTRACT
}
