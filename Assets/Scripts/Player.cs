using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField]
    private float maxSpeed = 1f;

    public float speed;

    private Vector3 direction;

    private Transform squirrel;
    private Camera camera;

    private uint themeID;
    // Start is called before the first frame update
    void Start() {
        themeID = AkSoundEngine.PostEvent("WindTheme", gameObject);
        direction = transform.right;
        foreach (Transform t in transform) {
            if (t.name == "Squirrel") {
                squirrel = t;
            } else if (t.name == "Main Camera") {
                camera = t.GetComponent<Camera>();
            }
        }
        speed = maxSpeed;
    }

    // Update is called once per frame
    void Update() {
        direction.y = Input.GetAxis("Vertical") - (Mathf.Clamp(5f - (speed-4f) * 0.5f, 0.2f, 7f) * 0.3f);

    }

    private void FixedUpdate() {
        transform.position += direction.normalized * speed * Time.deltaTime;
        Vector3 sRotation = squirrel.localEulerAngles;
        sRotation.z = direction.y * -45f;
        squirrel.localEulerAngles = sRotation;
        speed = Mathf.Clamp(speed - ((direction.y+0.1f) * Time.deltaTime*3f), 0, maxSpeed);
        float fovMultiplier = Mathf.Lerp(0.5f, 1.0f, speed / maxSpeed);
        camera.fieldOfView = 60 * fovMultiplier;
    }
}
