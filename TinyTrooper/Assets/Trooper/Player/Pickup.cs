using UnityEngine;

public class Pickup : MonoBehaviour
{
    public PickupType Type;
    public Material indicatorMaterial;

    [SerializeField]
    private MeshRenderer indicator;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        indicator.material = indicatorMaterial;
        if (Type == PickupType.QUEST) {
            GameManager.Instance.QuestItemsAvailable++;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnCollisionEnter(Collision collision) {
        var player = collision.gameObject.GetComponentInParent<Player>();
        if (player != null && player.PickUp(Type)) {
            Destroy(gameObject);
        }
    }
}

public enum PickupType {
    RIFLE_AMMO,
    SNIPER_AMMO,
    HEALTH,
    QUEST
}
