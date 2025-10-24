using UnityEngine;

public class DoorOpenOnStart : MonoBehaviour
{
    public Animator doorAnimator;   // Перетащи сюда Animator двери

    private void Start()
    {
        if (doorAnimator != null)
        {
            doorAnimator.SetBool("Open", true);
        }
        else
        {
            Debug.LogError("Animator не назначен!");
        }
    }
}