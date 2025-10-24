using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class CauldronInteraction : MonoBehaviour
{
    [Header("Эффекты")]
    public ParticleSystem bubbleEffect;
    public AudioClip boilSound;
    public Light boilLight;
    public float boilDuration = 1.5f;

    [Header("Логика")]
    public Renderer liquidRenderer;
    private int redCount = 0, blueCount = 0;
    private AudioSource audioSource;
    private bool isBoiling = false;

    private void Start()
    {
        audioSource = gameObject.AddComponent<AudioSource>();
        audioSource.loop = true;
        audioSource.clip = boilSound;
        audioSource.volume = 0.6f;
    }

    // ЛОВИТ ЛЕТЯЩИЕ ОБЪЕКТЫ!
    private void OnTriggerEnter(Collider other)
    {
        // Логируем для отладки
        Debug.Log($"Котёл: коснулся {other.name}");

        // Проверяем тег
        if (!other.CompareTag("Flask")) return;

        // Определяем цвет
        Color color = other.name.Contains("Red") ? Color.red : Color.blue;

        // Эффекты
        StartBoiling();

        // Уничтожаем фласк
        Destroy(other.gameObject, 0.1f);

        // Засчитываем
        AddIngredient(color);
    }

    private void AddIngredient(Color color)
    {
        if (color == Color.red) redCount++;
        else if (color == Color.blue) blueCount++;

        UpdatePotion();
    }

    private void UpdatePotion()
    {
        if (redCount > 0 && blueCount > 0)
        {
            liquidRenderer.material.color = new Color(0.6f, 0.2f, 0.8f);
            OpenDoor();
        }
        else if (redCount > 0)
            liquidRenderer.material.color = Color.red;
        else if (blueCount > 0)
            liquidRenderer.material.color = Color.blue;
    }

    private void OpenDoor()
    {
        GameObject door = GameObject.Find("Door");
        if (door) door.transform.position += Vector3.up * 3f;
    }

    private void StartBoiling()
    {
        if (isBoiling) return;
        isBoiling = true;

        if (bubbleEffect) bubbleEffect.Play();
        if (boilSound) audioSource.Play();
        if (boilLight) StartCoroutine(PulseLight());

        Invoke("StopBoiling", boilDuration);
    }

    private void StopBoiling()
    {
        isBoiling = false;
        if (bubbleEffect) bubbleEffect.Stop();
        if (audioSource) audioSource.Stop();
        if (boilLight) boilLight.intensity = 0f;
    }

    private System.Collections.IEnumerator PulseLight()
    {
        float timer = 0f;
        while (timer < boilDuration)
        {
            float intensity = Mathf.Sin(Time.time * 20f) * 2f + 3f;
            boilLight.intensity = intensity;
            timer += Time.deltaTime;
            yield return null;
        }
    }
}