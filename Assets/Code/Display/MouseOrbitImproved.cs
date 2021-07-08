using UnityEngine;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbitImproved : MonoBehaviour {
    public Transform target;
    private float distance = 150f;
    private float xSpeed = 37f;
    private float ySpeed = 30f;
    private float yMinLimit = 5f;
    private float yMaxLimit = 90f;
    public readonly float distanceMin = 100f;
    private float distanceMax = 250f;

    private Rigidbody rigidbody;

    private float x = 0.0f;
    private float y = 0.0f;

    private float mouseX = 0f;
    private float mouseY = 0f;

    // Use this for initialization
    private void Start() {
        rigidbody = new Rigidbody();
        Vector3 angles = transform.eulerAngles;
        x = angles.y;
        y = angles.x;

        rigidbody = GetComponent<Rigidbody>();

        // Make the rigid body not change rotation
        if (rigidbody != null) {
            rigidbody.freezeRotation = true;
        }
    }

    private void LateUpdate() {
        if (target) {
            GetMouseButtonDown_XY();
            float d = distance / 50;
            x += mouseX * (xSpeed + d) * 0.02f;
            y -= mouseY * ySpeed * 0.02f;

            y = ClampAngle(y, yMinLimit, yMaxLimit);

            Quaternion rotation = Quaternion.Euler(y, x, 0);

            distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 50, distanceMin, distanceMax);

            RaycastHit hit;
            if (Physics.Linecast(target.position, transform.position, out hit)) {
            }
            Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
            Vector3 position = rotation * negDistance + target.position;

            transform.rotation = rotation;
            transform.position = position;
        }
    }

    public static float ClampAngle(float angle, float min, float max) {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }

    private Vector3 mousePosPrev;

    private void GetMouseButtonDown_XY() {
        if (Input.GetMouseButtonDown(2)) {
            mousePosPrev = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }

        if (Input.GetMouseButton(2)) {
            Vector3 newMousePos = Camera.main.ScreenToViewportPoint(Input.mousePosition);

            if (newMousePos.x < mousePosPrev.x) {
                mouseX = -1;
            } else if (newMousePos.x > mousePosPrev.x) {
                mouseX = 1;
            } else {
                mouseX = -0;
            }

            if (newMousePos.y < mousePosPrev.y) {
                mouseY = -1;
            } else if (newMousePos.y > mousePosPrev.y) {
                mouseY = 1;
            } else {
                mouseY = -0;
            }

            mousePosPrev = Camera.main.ScreenToViewportPoint(Input.mousePosition);
        }
    }
}