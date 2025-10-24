using UnityEngine;

public class TableParticles : MonoBehaviour
{
    public ParticleSystem sparkEffect;
    public float minVelocity = 1f;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.relativeVelocity.magnitude > minVelocity)
        {
            ContactPoint contact = collision.contacts[0];
            if (sparkEffect)
            {
                ParticleSystem ps = Instantiate(sparkEffect, contact.point, Quaternion.LookRotation(contact.normal));
                Destroy(ps.gameObject, 2f);
            }
        }
    }
}