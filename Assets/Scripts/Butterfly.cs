using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Butterfly : MonoBehaviour
{
    public int position;
    public Vector3 offset;
    
    private bool scattering = false;
    private Vector3 direction;

    public void PreparePosition(int position) {
        this.position = position;
        offset = Random.insideUnitSphere;
    }

    private void Update() {
        if (scattering) {
            transform.position += direction.normalized * Time.deltaTime * 4f;
        }
    }

    public void Scatter() {
        scattering = true;
        Random.InitState(transform.GetHashCode());
        direction = new Vector3(Random.Range(-1f, 1f), 2f, Random.Range(-1f, 1f));
        transform.LookAt(transform.position - direction);
    }
}
