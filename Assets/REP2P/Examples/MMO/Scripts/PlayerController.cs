using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float speed = 10;
    public float[] border = new float[4];
    void Update()
    {
        transform.Translate(new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical")) * speed * Time.deltaTime);
        float x = Mathf.Clamp(transform.position.x, border[0], border[1]);
        float z = Mathf.Clamp(transform.position.z, border[2], border[3]);
        transform.position = new Vector3(x, transform.position.y, z);
    }
}
