using UnityEngine;

public class ExtractionZone : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter(Collider other) {
        if (Time.timeSinceLevelLoad < 1.0f) return;
        if (other.gameObject.tag == "Player")
        {
            if (GameManager.Instance.ReadyToExtract()) {
                GameManager.Instance.Extract();
            }
        }
    }
}
