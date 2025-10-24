using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using TMPro;

public class MagicBook : XRGrabInteractable
{
    [Header("Открытие")]
    public Transform coverFront;
    public float openAngle = 120f;
    public float openSpeed = 5f;

    [Header("Магия")]
    public Light bookLight;
    public ParticleSystem sparkles;
    public Vector3 floatHeight = new Vector3(0, 1.3f, 0);
    public Vector3 rotationSpeed = new Vector3(0, 30, 0);

    [Header("Рецепт")]
    public TextMeshPro recipeText;
    public Color successColor = new Color(0.5f, 0.2f, 1f);

    private bool isOpen = false;
    private Quaternion closedRot;
    private Vector3 startPos;
    private Cauldron cauldron;

    protected override void Awake()
    {
        base.Awake();
        closedRot = coverFront.localRotation;
        startPos = transform.position;
        cauldron = FindObjectOfType<Cauldron>();
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        OpenBook();
        StartFloating();
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        CloseBook();
        StopFloating();
    }

    private void OpenBook()
    {
        if (isOpen) return;
        isOpen = true;
        StopAllCoroutines();
        StartCoroutine(RotateCover(Quaternion.Euler(0, -openAngle, 0)));
        bookLight.intensity = 5f;
        sparkles.Play();
    }

    private void CloseBook()
    {
        if (!isOpen) return;
        isOpen = false;
        StopAllCoroutines();
        StartCoroutine(RotateCover(closedRot));
        bookLight.intensity = 0f;
        sparkles.Stop();
    }

    private System.Collections.IEnumerator RotateCover(Quaternion target)
    {
        Quaternion start = coverFront.localRotation;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * openSpeed;
            coverFront.localRotation = Quaternion.Lerp(start, target, t);
            yield return null;
        }
    }

    private void StartFloating()
    {
        enabled = true;
    }

    private void StopFloating()
    {
        enabled = false;
        transform.position = startPos;
    }

    private void Update()
    {
        if (!isOpen) return;

        // Левитация
        float y = Mathf.Sin(Time.time * 2f) * 0.05f;
        transform.position = startPos + floatHeight + Vector3.up * y;

        // Вращение
        transform.Rotate(rotationSpeed * Time.deltaTime);

        // Подсветка при успехе
        if (cauldron != null && cauldron.IsSuccess())
        {
            recipeText.color = successColor;
            bookLight.color = successColor;
        }
    }
}