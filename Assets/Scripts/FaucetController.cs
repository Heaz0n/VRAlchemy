using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class FaucetController : MonoBehaviour
{
    public ParticleSystem waterParticleSystem; // Particle System для воды
    public Transform waterSpawnPoint; // Точка, откуда льётся вода
    public GameObject cauldron; // Котёл для алхимии
    private XRGrabInteractable valveInteractable; // Вентиль для поворота
    private bool isWaterOn = false; // Состояние воды (вкл/выкл)
    public float waterIngredientDelay = 2f; // Задержка для добавления воды как ингредиента
    private float waterTimer = 0f;

    void Start()
    {
        // Находим вентиль (дочерний объект крана или сам кран)
        valveInteractable = GetComponentInChildren<XRGrabInteractable>();
        if (valveInteractable == null)
        {
            Debug.LogError("XRGrabInteractable не найден на вентиле!");
        }

        // Убедись, что Particle System выключен изначально
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Stop();
        }
    }

    void Update()
    {
        // Если вентиль повёрнут (или удерживается), включаем воду
        if (valveInteractable.isSelected)
        {
            if (!isWaterOn)
            {
                TurnOnWater();
            }
        }
        else if (isWaterOn)
        {
            TurnOffWater();
        }

        // Если вода включена и льётся в котёл, считаем её ингредиентом
        if (isWaterOn && cauldron != null)
        {
            waterTimer += Time.deltaTime;
            if (waterTimer >= waterIngredientDelay)
            {
                AddWaterToCauldron();
                waterTimer = 0f; // Сброс таймера
            }
        }
    }

    void TurnOnWater()
    {
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Play();
            isWaterOn = true;
            Debug.Log("Вода включена!");
        }
    }

    void TurnOffWater()
    {
        if (waterParticleSystem != null)
        {
            waterParticleSystem.Stop();
            isWaterOn = false;
            Debug.Log("Вода выключена!");
        }
    }

    void AddWaterToCauldron()
    {
        // Проверяем, попадает ли вода в котёл
        AlchemyCauldron cauldronScript = cauldron.GetComponent<AlchemyCauldron>();
        if (cauldronScript != null)
        {
            // Имитируем добавление ингредиента
            cauldronScript.AddIngredient("Water");
            Debug.Log("Вода добавлена в котёл как ингредиент!");
        }
    }
}