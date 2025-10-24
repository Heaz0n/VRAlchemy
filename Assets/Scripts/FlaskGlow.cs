using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FlaskGlow : XRGrabInteractable
{
    public Light glowLight;
    public Color glowColor = Color.cyan;
    public float maxIntensity = 6f;

    protected override void Awake()
    {
        base.Awake();

        if (glowLight == null)
        {
            GameObject lightObj = new GameObject("FlaskGlow");
            lightObj.transform.SetParent(transform);
            lightObj.transform.localPosition = Vector3.zero;
            glowLight = lightObj.AddComponent<Light>();
            glowLight.type = LightType.Point;
            glowLight.range = 1.5f;
            glowLight.color = glowColor;
            glowLight.intensity = 0f;
        }
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        glowLight.intensity = maxIntensity;
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        glowLight.intensity = 0f;
    }
}