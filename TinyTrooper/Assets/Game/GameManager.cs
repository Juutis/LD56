using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Dictionary<AmmoType, int> PlayerAmmo = new Dictionary<AmmoType, int>();
    public static GameManager Instance;

    [SerializeField]
    private TextMeshProUGUI rifleAmmo;
    [SerializeField]
    private TextMeshProUGUI sniperAmmo;

    void Awake() {
        Instance = this;
        PlayerAmmo[AmmoType.PISTOL] = 1;
        PlayerAmmo[AmmoType.RIFLE] = 10;
        PlayerAmmo[AmmoType.SNIPER] = 0;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        rifleAmmo.SetText(PlayerAmmo[AmmoType.RIFLE].ToString());
        sniperAmmo.SetText(PlayerAmmo[AmmoType.SNIPER].ToString());
    }
}
