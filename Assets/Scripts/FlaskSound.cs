using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FlaskSound : XRGrabInteractable
{
    public AudioClip grabSound;
    public AudioClip dropSound;
    private AudioSource audioSource;

    void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        if (grabSound) audioSource.PlayOneShot(grabSound);
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        if (dropSound) audioSource.PlayOneShot(dropSound);
    }
}