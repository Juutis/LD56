using System.Linq;
using UnityEngine;

public class FaceCamera : MonoBehaviour
{
    private Camera cam;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = Camera.allCameras.First(it => it.gameObject.name == "Virtual Camera");
    }

    // Update is called once per frame
    void Update()
    {
        transform.LookAt(cam.transform.position, cam.transform.up);
    }
}
