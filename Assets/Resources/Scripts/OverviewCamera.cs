using UnityEngine;
using System.Collections;

public class OverviewCamera : MonoBehaviour
{
    private Camera cam;
    public float speedScale = 0.5f;
    public float zoomScale = 50f;

    // Use this for initialization
    void Start()
    {        
        cam = this.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        var travel = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            travel.z += 1f;
        if (Input.GetKey(KeyCode.S))
            travel.z -= 1f;
        if (Input.GetKey(KeyCode.A))
            travel.x -= 1f;
        if (Input.GetKey(KeyCode.D))
            travel.x += 1f;
        if (Input.GetKey(KeyCode.R))
            travel.y += 1f;
        if (Input.GetKey(KeyCode.F))
            travel.y -= 1f;

        travel.y += Input.GetAxisRaw("Mouse ScrollWheel") * zoomScale;

        cam.transform.position += (travel * speedScale);
    }
}
