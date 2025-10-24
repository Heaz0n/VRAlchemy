using UnityEngine;

public class CauldronHeat : MonoBehaviour
{
    public ParticleSystem steam;
    public Light fireLight;
    public float heatTime = 5f;
    private float timer = 0f;
    private bool isHeating = false;

    void Update()
    {
        if (isHeating)
        {
            timer += Time.deltaTime;
            if (timer > heatTime)
            {
                steam.Play();
                fireLight.intensity = 5f;
            }
        }
    }

    public void StartHeating() => isHeating = true;
    public void StopHeating() => isHeating = false;
}