using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections;

public class CauldronBrew : MonoBehaviour
{
    [Header("=== ЭФФЕКТЫ ===")]
    public ParticleSystem bubbleEffect;
    public AudioClip pourSound;
    public AudioClip successSound;
    public Light boilLight;
    public float boilDuration = 2f;

    [Header("=== КОЛБЫ ===")]
    public Transform redFlaskSlot;   // Позиция для красной колбы
    public Transform blueFlaskSlot;  // Позиция для синей колбы

    [Header("=== ЖИДКОСТЬ ===")]
    public Renderer liquidRenderer;

    private GameObject redFlask = null;
    private GameObject blueFlask = null;
    private bool isBrewing = false;

    private void OnTriggerEnter(Collider other)
    {
        if (isBrewing) return;
        if (!other.CompareTag("Flask")) return;

        XRGrabInteractable grab = other.GetComponent<XRGrabInteractable>();
        if (grab == null || grab.isSelected) return; // Не берём, если держат

        if (other.name.Contains("Red") && redFlask == null)
        {
            StartCoroutine(InsertFlask(other.gameObject, redFlaskSlot, Color.red));
        }
        else if (other.name.Contains("Blue") && blueFlask == null)
        {
            StartCoroutine(InsertFlask(other.gameObject, blueFlaskSlot, Color.blue));
        }
    }

    private IEnumerator InsertFlask(GameObject flask, Transform slot, Color color)
    {
        // Отключаем физику
        Rigidbody rb = flask.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        // Перемещаем и поворачиваем
        float t = 0;
        Vector3 startPos = flask.transform.position;
        Quaternion startRot = flask.transform.rotation;

        while (t < 1f)
        {
            t += Time.deltaTime * 3f;
            flask.transform.position = Vector3.Lerp(startPos, slot.position, t);
            flask.transform.rotation = Quaternion.Lerp(startRot, slot.rotation, t);
            yield return null;
        }

        // Фиксируем
        flask.transform.SetParent(transform);
        flask.transform.position = slot.position;
        flask.transform.rotation = slot.rotation;

        // Сохраняем ссылку
        if (color == Color.red) redFlask = flask;
        else blueFlask = flask;

        // Звук наливания
        if (pourSound) AudioSource.PlayClipAtPoint(pourSound, slot.position);

        // Проверяем, обе ли колбы на месте
        if (redFlask != null && blueFlask != null)
        {
            StartCoroutine(BrewPotion());
        }
    }

    private IEnumerator BrewPotion()
    {
        isBrewing = true;

        // Кипение
        if (bubbleEffect) bubbleEffect.Play();
        if (boilLight) StartCoroutine(PulseLight());

        // Ждём
        yield return new WaitForSeconds(boilDuration);

        // Успех!
        liquidRenderer.material.color = new Color(0.6f, 0.2f, 0.8f); // Фиолетовый
        if (successSound) AudioSource.PlayClipAtPoint(successSound, transform.position);

        // Дропаем колбы
        DropFlask(redFlask);
        DropFlask(blueFlask);

        redFlask = null;
        blueFlask = null;
        isBrewing = false;

        OpenDoor();
    }

    private void DropFlask(GameObject flask)
    {
        if (flask == null) return;

        Rigidbody rb = flask.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = false;

        flask.transform.SetParent(null);
        flask.transform.position += Vector3.up * 0.3f + Random.insideUnitSphere * 0.2f;
        rb.AddForce(Random.insideUnitSphere * 2f, ForceMode.Impulse);
    }

    private void OpenDoor()
    {
        GameObject door = GameObject.Find("Door");
        if (door) door.transform.position += Vector3.up * 3f;
    }

    private IEnumerator PulseLight()
    {
        float timer = 0;
        while (timer < boilDuration)
        {
            float intensity = Mathf.Sin(Time.time * 15f) * 2f + 3f;
            boilLight.intensity = intensity;
            timer += Time.deltaTime;
            yield return null;
        }
    }
}