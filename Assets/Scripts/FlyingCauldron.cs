using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCauldron : MonoBehaviour
{
    public float height = 1.3f;
    public float pulseSpeed = 2f;
    public float pulseAmplitude = 0.1f;
    public Vector3 rotationSpeed = new Vector3(0, 30, 0);

    private Vector3 startPos;

    void Start() => startPos = transform.position;

    void Update()
    {
        float y = Mathf.Sin(Time.time * pulseSpeed) * pulseAmplitude;
        transform.position = startPos + Vector3.up * (height + y);
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}