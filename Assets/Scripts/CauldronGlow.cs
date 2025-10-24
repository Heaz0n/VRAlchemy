using UnityEngine;

public class CauldronGlow : MonoBehaviour
{
    public Light successLight;
    public Color successColor = new Color(0.5f, 0.2f, 1f);
    public float pulseSpeed = 3f;

    private bool success = false;

    void Update()
    {
        if (success)
        {
            float intensity = (Mathf.Sin(Time.time * pulseSpeed) + 1) * 3f;
            successLight.intensity = intensity;
            successLight.color = Color.Lerp(Color.white, successColor, Mathf.Abs(Mathf.Sin(Time.time * pulseSpeed)));
        }
    }

    public void TriggerSuccess()
    {
        success = true;
        successLight.enabled = true;
    }
}