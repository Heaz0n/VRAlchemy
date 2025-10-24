using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class RocketFlask : MonoBehaviour
{
    private Rigidbody rb; // Rigidbody колбы
    private XRGrabInteractable grabInteractable; // Компонент для VR-взаимодействия
    public float rocketForce = 50f; // Сила "взлёта" колбы
    public float playerFallForce = 20f; // Сила падения игрока
    private GameObject xrRig; // Ссылка на XR Rig (игрока)

    void Start()
    {
        // Находим компоненты
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        xrRig = GameObject.Find("XR Rig"); // Найди XR Rig в сцене (или задай вручную через Inspector)

        // Убедись, что Rigidbody настроен
        rb.useGravity = true;
        rb.isKinematic = false;

        // Подписываемся на событие взятия колбы
        grabInteractable.onSelectEntered.AddListener(OnGrabbed);
    }

    void OnGrabbed(XRBaseInteractor interactor)
    {
        // Когда колбу берут в руки
        Debug.Log("Колба взята! Запускаем ракету!");

        // Добавляем силу вверх для колбы
        rb.AddForce(Vector3.up * rocketForce, ForceMode.Impulse);

        // Находим Rigidbody XR Rig (если есть)
        Rigidbody playerRb = xrRig.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            // Добавляем силу вниз для игрока
            playerRb.AddForce(Vector3.down * playerFallForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody на XR Rig не найден! Игрок не упадёт.");
        }
    }

    void OnDestroy()
    {
        // Отписываемся от события, чтобы избежать ошибок
        grabInteractable.onSelectEntered.RemoveListener(OnGrabbed);
    }
}