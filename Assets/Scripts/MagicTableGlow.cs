using UnityEngine;

public class MagicTableGlow : MonoBehaviour
{
    public Light tableLight;
    public Color idleColor = new Color(0.2f, 0.2f, 0.8f);
    public Color activeColor = new Color(0.6f, 0.2f, 1f);
    public float pulseSpeed = 2f;
    public float activationDistance = 1.5f;

    private Transform playerHead;
    private bool isActive = false;

    private void Start()
    {
        if (tableLight == null)
        {
            GameObject lightObj = new GameObject("TableGlow");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.up * 0.1f;
            tableLight = lightObj.AddComponent<Light>();
            tableLight.type = LightType.Point;
            tableLight.range = 3f;
            tableLight.color = idleColor;
            tableLight.intensity = 2f;
        }

        playerHead = Camera.main.transform;
    }

    private void Update()
    {
        float distance = Vector3.Distance(playerHead.position, transform.position);

        if (distance < activationDistance && !isActive)
        {
            isActive = true;
        }
        else if (distance >= activationDistance && isActive)
        {
            isActive = false;
        }

        float intensity = isActive 
            ? (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 1f) * 4f 
            : 2f;

        tableLight.intensity = intensity;
        tableLight.color = Color.Lerp(idleColor, activeColor, isActive ? 1f : 0f);
    }
}