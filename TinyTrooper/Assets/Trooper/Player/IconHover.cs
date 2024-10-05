
using UnityEngine;

public class IconHover : MonoBehaviour
{
    private Vector3 origOffset;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        origOffset = transform.localPosition;
    }

    // Update is called once per frame
    void Update()
    {
        transform.localPosition = origOffset + Vector3.up * Mathf.Sin(Time.time * 2.0f) * 0.2f;
    }
}
