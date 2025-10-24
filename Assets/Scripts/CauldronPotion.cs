using UnityEngine;

public class CauldronPotion : MonoBehaviour
{
    public Renderer liquid;
    private int red = 0, blue = 0;

    private void OnTriggerStay(Collider other)
    {
        var flask = other.GetComponentInParent<Transform>();
        if (flask == null) return;

        float angle = Vector3.Angle(flask.up, Vector3.up);
        if (angle > 120f)
        {
            if (flask.name.Contains("Red") && red == 0) { red = 1; HideLiquid(flask); }
            if (flask.name.Contains("Blue") && blue == 0) { blue = 1; HideLiquid(flask); }
            UpdatePotion();
        }
    }

    void HideLiquid(Transform flask) => flask.Find("Liquid_Red")?.gameObject.SetActive(false);
    void UpdatePotion()
    {
        if (red == 1 && blue == 1)
        {
            liquid.material.color = new Color(0.6f, 0, 0.8f);
            OpenDoor();
        }
    }

    void OpenDoor()
    {
        GameObject door = GameObject.Find("Door");
        if (door) door.transform.position += Vector3.up * 3f;
    }
}