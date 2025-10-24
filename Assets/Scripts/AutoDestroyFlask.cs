using UnityEngine;

public class AutoDestroyFlask : MonoBehaviour
{
    public float destroyDelay = 2f;

    public void DestroyAfterPour()
    {
        Invoke("DestroyFlask", destroyDelay);
    }

    void DestroyFlask()
    {
        Destroy(gameObject);
    }
}