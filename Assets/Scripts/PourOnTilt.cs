using UnityEngine;

public class PourOnTilt : MonoBehaviour
{
    public GameObject liquidObject;
    public AudioClip pourSound;
    public ParticleSystem pourEffect;
    public bool isPoured = false;

    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    void Update()
    {
        if (isPoured) return;

        float angle = Vector3.Angle(transform.up, Vector3.up);
        if (angle > 120f)
        {
            Pour();
        }
    }

    void Pour()
    {
        isPoured = true;
        liquidObject.SetActive(false);

        if (pourSound) audioSource.PlayOneShot(pourSound);
        if (pourEffect) Instantiate(pourEffect, transform.position, Quaternion.identity);

        // Уведомляем котёл
        FindObjectOfType<Cauldron>()?.AddIngredient(transform.name.Contains("Red") ? Color.red : Color.blue);
    }
    
}