using UnityEngine;

public class MinimapObstacle : MonoBehaviour
{
    [SerializeField]
    private Material material;

    public bool IsCopy = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (IsCopy) return;
        var copy = Instantiate(gameObject, transform.parent);
        copy.GetComponent<MinimapObstacle>().IsCopy = true;
        var layer = LayerMask.NameToLayer("Minimap");
        copy.layer = layer;
        var rends = copy.GetComponentsInChildren<MeshRenderer>();
        foreach(var rend in rends) {
            rend.material = material;
            rend.gameObject.layer = layer;
            var collider = rend.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
