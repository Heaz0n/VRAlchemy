using UnityEngine;

public class TableSound : MonoBehaviour
{
    public AudioClip tapSound;
    public AudioClip placeSound;
    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.spatialBlend = 1f;
        audioSource.volume = 0.7f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > 1.5f)
        {
            audioSource.PlayOneShot(tapSound);
        }
        else if (collision.gameObject.CompareTag("Flask"))
        {
            audioSource.PlayOneShot(placeSound);
        }
    }
}