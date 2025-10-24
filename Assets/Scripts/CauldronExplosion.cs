using UnityEngine;

public class CauldronExplosion : MonoBehaviour
{
    public GameObject explosionPrefab;
    public AudioClip boomSound;
    private int sameCount = 0;
    private Color lastColor;

    private void OnTriggerStay(Collider other)
    {
        var flask = other.GetComponentInParent<Transform>();
        if (flask == null) return;

        float angle = Vector3.Angle(flask.up, Vector3.up);
        if (angle > 120f)
        {
            Color color = flask.name.Contains("Red") ? Color.red : Color.blue;
            if (color == lastColor) sameCount++;
            else { sameCount = 1; lastColor = color; }

            if (sameCount >= 3)
            {
                Explode();
            }
        }
    }

    void Explode()
    {
        Instantiate(explosionPrefab, transform.position, Quaternion.identity);
        GetComponent<AudioSource>().PlayOneShot(boomSound);
        Destroy(gameObject, 1f);
    }
}