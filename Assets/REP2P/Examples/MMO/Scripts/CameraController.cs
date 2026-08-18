using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target;
    public Transform sniper;
    public float speed = 5;
    public float maxFOV = 90;
    public float minFOV = 10f;
    private Camera _camera;
    private void Start()
    {
        _camera = GetComponent<Camera>();
    }
    private void Update()
    {
        sniper.LookAt(target);
        transform.rotation = Quaternion.Lerp(transform.rotation, sniper.rotation, speed * Time.deltaTime);
        if (Input.GetMouseButton(1)||Input.GetButton("Jump"))
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, minFOV, Time.deltaTime * speed);
        }
        else
        {
            _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, maxFOV, Time.deltaTime * speed);
        }
    }
}
