using UnityEngine;
using System.Collections.Generic;
using UnityEngine.XR.Interaction.Toolkit;

public class PotionItem : MonoBehaviour
{
    [Header("Данные зелья")]
    public string potionName;
    [TextArea]
    public string description;
    public List<PotionEffect> effects = new List<PotionEffect>();
    public Color potionColor = Color.blue;
    
    [Header("Визуальные элементы")]
    public Renderer liquidRenderer;
    public Light potionLight;
    public ParticleSystem drinkEffect;
    
    [Header("Настройки использования")]
    public bool isConsumable = true;
    public int uses = 1;
    
    private XRGrabInteractable grabInteractable;
    private bool isBeingUsed = false;
    
    void Start()
    {
        grabInteractable = GetComponent<XRGrabInteractable>();
        
        // Настройка визуалов на основе данных зелья
        SetupVisuals();
        
        // Настройка взаимодействий
        if (grabInteractable != null)
        {
            grabInteractable.activated.AddListener(OnPotionActivated);
        }
    }
    
    public void SetupPotion(List<PotionEffect> potionEffects, string name, string desc, Color color)
    {
        effects = potionEffects;
        potionName = name;
        description = desc;
        potionColor = color;
        SetupVisuals();
    }
    
    void SetupVisuals()
    {
        // Настройка цвета жидкости
        if (liquidRenderer != null)
        {
            liquidRenderer.material.color = potionColor;
            liquidRenderer.material.SetColor("_EmissionColor", potionColor * 0.5f);
        }
        
        // Настройка света
        if (potionLight != null)
        {
            potionLight.color = potionColor;
            potionLight.intensity = 1f;
        }
        
        // Настройка эффектов частиц
        if (drinkEffect != null)
        {
            var main = drinkEffect.main;
            main.startColor = potionColor;
        }
    }
    
    // Вызывается когда игрок "активирует" зелье (пьет его)
    public void OnPotionActivated(ActivateEventArgs args)
    {
        if (isBeingUsed || !isConsumable) return;
        
        isBeingUsed = true;
        UsePotion();
    }
    
    public void UsePotion(GameObject target = null)
    {
        if (uses <= 0) return;
        
        // Если цель не указана, используем на себе (игроке)
        if (target == null)
        {
            // Найти игрока через XR Origin
            target = GameObject.Find("XR Origin");
            if (target == null)
            {
                target = GameObject.FindWithTag("Player");
            }
            
            if (target == null)
            {
                Debug.LogWarning("Цель для зелья не найдена!");
                isBeingUsed = false;
                return;
            }
        }
        
        // Применяем все эффекты зелья
        foreach (PotionEffect effect in effects)
        {
            effect.ApplyEffect(target);
        }
        
        // Визуальные эффекты использования
        if (drinkEffect != null)
        {
            drinkEffect.Play();
        }
        
        // Уменьшаем количество использований
        uses--;
        
        Debug.Log($"Использовано зелье: {potionName}. Осталось использований: {uses}");
        
        // Если использований не осталось - уничтожаем
        if (uses <= 0)
        {
            StartCoroutine(DestroyAfterDelay(1f));
        }
        
        isBeingUsed = false;
    }
    
    private System.Collections.IEnumerator DestroyAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        // Отключаем визуальные компоненты перед уничтожением
        if (liquidRenderer != null) liquidRenderer.enabled = false;
        if (potionLight != null) potionLight.enabled = false;
        
        Destroy(gameObject);
    }
    
    // Метод для отображения информации о зелье в UI
    public string GetPotionInfo()
    {
        string info = $"<b>{potionName}</b>\n";
        info += $"{description}\n\n";
        info += "<b>Эффекты:</b>\n";
        
        foreach (PotionEffect effect in effects)
        {
            string effectType = effect.isPositive ? "<color=green>+" : "<color=red>-";
            info += $"{effectType}{effect.effectName}</color> ({effect.duration}сек)\n";
        }
        
        info += $"\n<b>Использований:</b> {uses}";
        
        return info;
    }
    
    void OnDestroy()
    {
        // Очистка событий
        if (grabInteractable != null)
        {
            grabInteractable.activated.RemoveListener(OnPotionActivated);
        }
    }
}