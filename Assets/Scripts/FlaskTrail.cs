using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class FlaskTrail : MonoBehaviour
{
    public TrailRenderer trail;
    public Color trailColor = Color.red;
    public float minVelocity = 2f;

    private Rigidbody rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        if (trail == null)
        {
            GameObject trailObj = new GameObject("Trail");
            trailObj.transform.SetParent(transform);
            trailObj.transform.localPosition = Vector3.zero;
            trail = trailObj.AddComponent<TrailRenderer>();
            trail.time = 1f;
            trail.minVertexDistance = 0.1f;
            trail.widthMultiplier = 0.05f;
            trail.material = new Material(Shader.Find("Unlit/Color"));
            trail.material.color = trailColor;
            trail.startWidth = 0.08f;
            trail.endWidth = 0f;
            trail.emitting = false;
        }
    }

    private void Update()
    {
        trail.emitting = rb.velocity.magnitude > minVelocity;
    }
}