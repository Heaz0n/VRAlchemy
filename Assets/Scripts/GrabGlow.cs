using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GrabGlow : XRGrabInteractable
{
    public Light grabLight;
    public Color glowColor = Color.yellow;
    public float maxIntensity = 5f;

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (grabLight) grabLight.intensity = maxIntensity;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        if (grabLight) grabLight.intensity = 0f;
    }

    void Start()
    {
        if (grabLight == null)
        {
            GameObject lightObj = new GameObject("FlaskGlow");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            grabLight = lightObj.AddComponent<Light>();
            grabLight.type = LightType.Point;
            grabLight.range = 1f;
            grabLight.color = glowColor;
            grabLight.intensity = 0f;
        }
    }
}