using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour {
    [SerializeField]
    private float speed = 1f;

    private uint themeID;
    // Start is called before the first frame update
    void Start() {
        themeID = AkSoundEngine.PostEvent("WindTheme", gameObject);
    }

    // Update is called once per frame
    void Update() {

    }

    private void FixedUpdate() {
        transform.position += new Vector3(speed * Time.deltaTime, 0f);
    }
}
