using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour {
    [SerializeField]
    private float maxSpeed = 1f;

    public float speed;

    private Vector3 direction;

    private Transform squirrel;
    private Camera camera;

    [SerializeField]
    private int lastPosLength;

    [SerializeField]
    private float butterflySpeedBoost = 3f;

    [SerializeField]
    private Text instructions;

    private Queue<Vector3> lastPositions;
    private Queue<float> xAngles;

    private List<Butterfly> butterflies;

    private bool alive;

    private uint themeID;
    // Start is called before the first frame update
    void Start() {
        themeID = AkSoundEngine.PostEvent("WindTheme", gameObject);
        direction = transform.right;
        foreach (Transform t in transform) {
            if (t.name == "Squirrel") {
                squirrel = t;
            }
            else if (t.name == "Main Camera") {
                camera = t.GetComponent<Camera>();
            }
        }
        speed = maxSpeed;
        lastPositions = new Queue<Vector3>();
        xAngles = new Queue<float>();
        for (int i = 0; i < lastPosLength; i++) {
            lastPositions.Enqueue(transform.position);
            xAngles.Enqueue(0f);
        }

        butterflies = new List<Butterfly>();
        alive = true;
    }

    // Update is called once per frame
    void Update() {
        AkSoundEngine.SetRTPCValue("WindVolume", ((transform.position.y * 100) - 800) / 42);
        if (!alive) {
            speed = Mathf.Clamp(speed - 0.2f * Time.deltaTime, 0f, 4f);
            transform.position += Vector3.up * Time.deltaTime * speed;
            Color c = instructions.color;
            c.a = Mathf.Clamp(c.a + Time.deltaTime * 0.1f, 0f, 1f);
            instructions.rectTransform.localPosition = new Vector3(18, (c.a * 100f) - 100f);
            instructions.color = c;
            if (Input.GetKeyDown(KeyCode.Space)) {
                UnityEngine.SceneManagement.SceneManager.LoadScene("SampleScene");
            }
            return;
        }
        direction.y = Input.GetAxis("Vertical") - (Mathf.Clamp(5f - (speed - 4f) * 0.5f, 0.2f, 7f) * 0.3f);
        if (transform.position.y < 7f) {
            // Game over!
            AkSoundEngine.StopPlayingID(themeID);
            AkSoundEngine.PostEvent("Lose", gameObject);
            Destroy(squirrel.gameObject);
            alive = false;
            speed = 4f;
            ScatterButterflies();
        }
    }

    private void FixedUpdate() {
        if (!alive) {
            return;
        }
        transform.position += direction.normalized * speed * Time.deltaTime;
        Vector3 sRotation = squirrel.localEulerAngles;
        sRotation.z = direction.y * -45f;
        squirrel.localEulerAngles = sRotation;
        speed = Mathf.Clamp(speed - ((direction.y + 0.1f) * Time.deltaTime * 3f), 0, maxSpeed);
        float fovMultiplier = Mathf.Lerp(0.5f, 1.0f, speed / maxSpeed);
        float newFov = Mathf.Lerp(camera.fieldOfView, (60 * fovMultiplier), 0.1f);
        camera.fieldOfView = newFov;
        lastPositions.Dequeue();
        lastPositions.Enqueue(transform.position);
        xAngles.Dequeue();
        xAngles.Enqueue(-squirrel.localEulerAngles.z);

        UpdateButterflies();
    }

    private void UpdateButterflies() {
        if (butterflies.Count == 0) {
            return;
        }
        Vector3[] lastPos = lastPositions.ToArray();
        float[] xAngle = xAngles.ToArray();
        foreach (Butterfly b in butterflies) {
            b.transform.position = lastPos[b.position] + b.offset;
            b.transform.localEulerAngles = new Vector3(xAngle[b.position], -90f);
        }
    }

    private void OnTriggerEnter(Collider other) {
        AkSoundEngine.PostEvent("Collect", gameObject);
        Butterfly newButterfly = other.gameObject.AddComponent<Butterfly>();
        speed = Mathf.Clamp(speed + butterflySpeedBoost, 0f, maxSpeed);
        Destroy(other.gameObject.GetComponent<BoxCollider>());
        Destroy(other.gameObject.GetComponent<ParticleSystem>());
        newButterfly.transform.localEulerAngles = new Vector3(0f, -90f);
        newButterfly.transform.SetParent(null);
        Random.InitState(transform.position.GetHashCode());
        newButterfly.PreparePosition(Random.Range(5, lastPosLength));
        butterflies.Add(newButterfly);
    }

    private void ScatterButterflies() {
        foreach (Butterfly b in butterflies) {
            b.transform.position = transform.position;
            b.Scatter();
        }
        instructions.text = "Press SPACEBAR\nto Restart\nYou collected " + butterflies.Count + " butterflies";
        butterflies.Clear();
        Destroy(GetComponent<BoxCollider>());
    }
}
