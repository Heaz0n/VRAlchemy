using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class SpiritLampController : MonoBehaviour
{
    private Rigidbody rb; // Rigidbody бутылки
    private XRGrabInteractable grabInteractable; // Для VR-взаимодействия
    public float rocketForce = 50f; // Сила взлёта бутылки
    public float playerFallForce = 20f; // Сила падения игрока
    private GameObject xrRig; // Ссылка на XR Rig

    void Start()
    {
        // Находим компоненты
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        xrRig = GameObject.Find("XR Origin"); // Имя XR Rig (или задай в Inspector)

        // Настройки Rigidbody
        rb.useGravity = true;
        rb.isKinematic = false;

        // Событие взятия бутылки
        grabInteractable.selectEntered.AddListener(OnGrabbed);
    }

    void OnGrabbed(SelectEnterEventArgs args)
    {
        // Когда бутылку берут
        Debug.Log("Spirit_Lamp взята! Взлетаем!");

        // Взлёт бутылки
        rb.AddForce(Vector3.up * rocketForce, ForceMode.Impulse);

        // Падение игрока
        Rigidbody playerRb = xrRig.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            playerRb.AddForce(Vector3.down * playerFallForce, ForceMode.Impulse);
        }
        else
        {
            Debug.LogWarning("Rigidbody на XR Rig не найден!");
        }
    }

    void OnDestroy()
    {
        // Отписка от события
        grabInteractable.selectEntered.RemoveListener(OnGrabbed);
    }
}