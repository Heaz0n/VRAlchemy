using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

// Переименовываем класс данных чтобы избежать конфликта
[System.Serializable]
public class IngredientConfig
{
    public string ingredientName;
    public Color ingredientColor;
    public IngredientType type;
    public float weight = 1.0f;
    public int value = 10;
    public GameObject pickupEffect;
    public AudioClip pickupSound;
}

public enum IngredientType
{
    Herb,       // Травы
    Mineral,    // Минералы  
    Creature,   // Части существ
    Liquid,     // Жидкости
    Crystal,    // Кристаллы
    Powder      // Порошки
}

public class Ingredient : MonoBehaviour
{
    [Header("Данные ингредиента")]
    public IngredientConfig config; // Изменено с data на config
    
    [Header("Автонастройка")]
    public bool autoSetup = true;
    
    private Rigidbody rb;
    private XRGrabInteractable grabInteractable;
    private Vector3 startPosition;
    private Quaternion startRotation;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        grabInteractable = GetComponent<XRGrabInteractable>();
        startPosition = transform.position;
        startRotation = transform.rotation;
        
        if (autoSetup)
        {
            AutoSetup();
        }
        
        // Настраиваем взаимодействие для VR
        SetupXRInteractions();
    }
    
    void AutoSetup()
    {
        // Автоматическая настройка цвета
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null && config != null)
        {
            renderer.material.color = config.ingredientColor;
        }
        
        // Автоматическая настройка физики
        if (rb != null && config != null)
        {
            rb.mass = config.weight;
            rb.drag = 1f;
            rb.angularDrag = 0.5f;
        }
        
        // Автоматическая настройка тега и слоя
        gameObject.tag = "Ingredient";
        if (LayerMask.NameToLayer("Interactable") != -1)
        {
            gameObject.layer = LayerMask.NameToLayer("Interactable");
        }
    }
    
    void SetupXRInteractions()
    {
        if (grabInteractable == null)
        {
            grabInteractable = gameObject.AddComponent<XRGrabInteractable>();
        }
        
        grabInteractable.throwOnDetach = true;
        grabInteractable.attachEaseInTime = 0.1f;
        grabInteractable.movementType = XRBaseInteractable.MovementType.Instantaneous;
        
        // Подписываемся на события взаимодействия
        grabInteractable.selectEntered.AddListener(OnGrab);
        grabInteractable.selectExited.AddListener(OnRelease);
    }
    
    void OnGrab(SelectEnterEventArgs args)
    {
        if (config == null) return;
        
        // Эффект при взятии
        if (config.pickupEffect != null)
        {
            Instantiate(config.pickupEffect, transform.position, Quaternion.identity);
        }
        
        // Звук при взятии
        if (config.pickupSound != null)
        {
            AudioSource.PlayClipAtPoint(config.pickupSound, transform.position);
        }
        
        Debug.Log($"Взят ингредиент: {config.ingredientName}");
    }
    
    void OnRelease(SelectExitEventArgs args)
    {
        if (config != null)
        {
            Debug.Log($"Отпущен ингредиент: {config.ingredientName}");
        }
    }
    
    // Метод для сброса ингредиента на стартовую позицию
    public void ResetIngredient()
    {
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        transform.position = startPosition;
        transform.rotation = startRotation;
    }
    
    // Метод для получения имени (совместимость с твоим текущим кодом)
    public string GetIngredientName()
    {
        return config != null ? config.ingredientName : gameObject.name;
    }
    
    // Метод для получения цвета (совместимость с твоим текущим кодом)
    public Color GetIngredientColor()
    {
        return config != null ? config.ingredientColor : Color.white;
    }
    
    void OnDestroy()
    {
        // Очистка событий
        if (grabInteractable != null)
        {
            grabInteractable.selectEntered.RemoveListener(OnGrab);
            grabInteractable.selectExited.RemoveListener(OnRelease);
        }
    }
}