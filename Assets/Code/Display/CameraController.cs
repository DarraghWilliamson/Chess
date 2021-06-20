using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {

    public Transform target;
    public Vector3 offset;
    private float currentZoom = 6f;
    public float pitch = 2f;
    private readonly float zoomSpeed = 4f;
    private readonly float minZoom = 2f;
    private readonly float maxZoom = 9f;
    float yawSpeed = 100f;
    public float currentYaw = 0f;
    public float currentRoll = 0f;

    public float dragSpeed = 2;
    private Vector3 dragOrigin;

    Ray ray;
    RaycastHit hit;

    public GameObject selected;

    void Update() {

        currentZoom -= Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);
        currentYaw -= Input.GetAxis("Horizontal") * yawSpeed * Time.deltaTime;
        currentRoll -= Input.GetAxis("Vertical") * yawSpeed * Time.deltaTime;
        currentRoll = Mathf.Clamp(currentRoll, -24, 65);
       

        ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out hit)) {
            //print(hit.collider.name);
            if (Input.GetMouseButtonDown(1)) {
                    target = hit.collider.transform;
            }
        }

        
    



}

    void LateUpdate() {
        transform.position = target.position - offset * currentZoom;
        transform.LookAt(target.position + Vector3.up * pitch);
        transform.RotateAround(target.position, Vector3.up, currentYaw);

        transform.RotateAround(target.position, transform.right , currentRoll);
    }


}
