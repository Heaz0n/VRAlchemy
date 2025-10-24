using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpinningChair : MonoBehaviour
{
    private XRGrabInteractable grabInteractable; // Для VR-взаимодействия
    private Transform chairTransform; // Трансформ стула
    private float rotationSpeed = 100f; // Скорость вращения (градусов/сек)
    private bool isGrabbed = false; // Состояние: стул держат?

    void Start()
    {
        // Находим компоненты
        grabInteractable = GetComponent<XRGrabInteractable>();
        chairTransform = transform;

        // Настройки XR Grab Interactable
        grabInteractable.movementType = XRGrabInteractable.MovementType.VelocityTracking;

        // Подписываемся на события взятия и отпускания
        grabInteractable.selectEntered.AddListener(OnGrabbed);
        grabInteractable.selectExited.AddListener(OnReleased);
    }

    void Update()
    {
        // Если стул держат, вращаем его на основе движения контроллера
        if (isGrabbed && grabInteractable.interactorsSelecting.Count > 0)
        {
            var interactor = grabInteractable.interactorsSelecting[0];
            Vector3 interactorMovement = interactor.transform.rotation.eulerAngles;
            float rotationInput = interactorMovement.y;

            // Вращаем стул вокруг оси Y
            chairTransform.Rotate(Vector3.up, rotationInput * rotationSpeed * Time.deltaTime);
        }
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        isGrabbed = true;
        Debug.Log("Стул схвачен! Начинаем вращение.");
    }

    void OnReleased(SelectExitEventArgs args)
    {
        isGrabbed = false;
        Debug.Log("Стул отпущен.");
    }

    void OnDestroy()
    {
        // Отписываемся от событий
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
        grabInteractable.selectExited.RemoveListener(OnReleased);
    }
}