using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class CauldronCylinder : MonoBehaviour
{
    [Header("=== ЭФФЕКТЫ ===")]
    public ParticleSystem bubbleEffect;
    public AudioClip pourSound;
    public AudioClip successSound;
    public Light boilLight;
    public float brewTime = 2f;

    [Header("=== СЛОТЫ ДЛЯ КОЛБ ===")]
    public Transform redSlot;
    public Transform blueSlot;

    [Header("=== ЖИДКОСТЬ ===")]
    public Renderer liquidRenderer;

    private GameObject redFlask = null;
    private GameObject blueFlask = null;
    private bool isBrewing = false;

    private void Start()
    {
        // Добавляем коллайдер, если нет
        if (!GetComponent<Collider>())
            gameObject.AddComponent<CapsuleCollider>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (isBrewing) return;
        if (!other.CompareTag("Flask")) return;

        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab != null && grab.isSelected) return;

        if (other.name.Contains("Red") && redFlask == null)
        {
            StartCoroutine(PlaceFlask(other.gameObject, redSlot, Color.red));
        }
        else if (other.name.Contains("Blue") && blueFlask == null)
        {
            StartCoroutine(PlaceFlask(other.gameObject, blueSlot, Color.blue));
        }
    }

    private IEnumerator PlaceFlask(GameObject flask, Transform slot, Color color)
    {
        Rigidbody rb = flask.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        Vector3 startPos = flask.transform.position;
        Quaternion startRot = flask.transform.rotation;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 4f;
            flask.transform.position = Vector3.Lerp(startPos, slot.position, t);
            flask.transform.rotation = Quaternion.Slerp(startRot, slot.rotation, t);
            yield return null;
        }

        flask.transform.SetParent(transform);
        flask.transform.position = slot.position;
        flask.transform.rotation = slot.rotation;

        if (color == Color.red) redFlask = flask;
        else blueFlask = flask;

        if (pourSound) AudioSource.PlayClipAtPoint(pourSound, slot.position);

        if (redFlask && blueFlask)
            StartCoroutine(Brew());
    }

    private IEnumerator Brew()
    {
        isBrewing = true;

        if (bubbleEffect) bubbleEffect.Play();
        if (boilLight) StartCoroutine(PulseLight());

        yield return new WaitForSeconds(brewTime);

        liquidRenderer.material.color = new Color(0.6f, 0.2f, 0.8f);
        if (successSound) AudioSource.PlayClipAtPoint(successSound, transform.position);

        DropFlask(redFlask);
        DropFlask(blueFlask);

        redFlask = blueFlask = null;
        isBrewing = false;

        OpenDoor(); // ← ЗАМЕНИЛИ DOJump НА ЭТУ ФУНКЦИЮ
    }

    private void DropFlask(GameObject flask)
    {
        if (!flask) return;

        Rigidbody rb = flask.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        flask.transform.SetParent(null);
        Vector3 dropPos = transform.position + Vector3.up * 0.5f + Random.insideUnitSphere * 0.3f;
        flask.transform.position = dropPos;
        rb.AddForce(Random.insideUnitSphere * 3f, ForceMode.Impulse);
    }

    // ← НОВАЯ ФУНКЦИЯ — ПЛАВНОЕ ОТКРЫТИЕ ДВЕРИ БЕЗ DOTWEEN
    private void OpenDoor()
    {
        GameObject door = GameObject.Find("Door");
        if (door != null)
        {
            StartCoroutine(MoveDoorUp(door.transform));
        }
    }

    private IEnumerator MoveDoorUp(Transform door)
    {
        Vector3 start = door.position;
        Vector3 target = start + Vector3.up * 3f;
        float t = 0;

        while (t < 1f)
        {
            t += Time.deltaTime * 2f;
            door.position = Vector3.Lerp(start, target, t);
            yield return null;
        }
    }

    private IEnumerator PulseLight()
    {
        float t = 0;
        while (t < brewTime)
        {
            boilLight.intensity = Mathf.Sin(Time.time * 20f) * 2f + 4f;
            t += Time.deltaTime;
            yield return null;
        }
    }
}