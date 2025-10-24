using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiquidFollow : MonoBehaviour
{
    public Transform flask;
    public float maxTilt = 45f;
    public float smoothness = 8f;

    void Update()
    {
        if (!flask) return;

        Vector3 euler = flask.localRotation.eulerAngles;
        float tiltX = euler.x > 180 ? euler.x - 360 : euler.x;
        float tiltZ = euler.z > 180 ? euler.z - 360 : euler.z;

        tiltX = Mathf.Clamp(tiltX, -maxTilt, maxTilt);
        tiltZ = Mathf.Clamp(tiltZ, -maxTilt, maxTilt);

        Vector3 target = new Vector3(
            (tiltZ / maxTilt) * 0.03f,
            -0.05f + (tiltX / maxTilt) * 0.03f,
            0
        );

        transform.localPosition = Vector3.Lerp(transform.localPosition, target, Time.deltaTime * smoothness);
    }
}