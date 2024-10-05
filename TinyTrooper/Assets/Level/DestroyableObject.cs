using UnityEngine;
using UnityEngine.AI;

public class DestroyableObject : MonoBehaviour
{
    [SerializeField]
    private float health = 10;

    [SerializeField]
    public int Hardness = 5;

    [SerializeField]
    private GameObject destroyablePart;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Hurt(float damage) {
        health -= damage;
        if (health < 0) {
            Destroy(destroyablePart);
        }
    }
}
