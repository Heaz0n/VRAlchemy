using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Potion Effect", menuName = "Alchemy/Potion Effect")]
public class PotionEffect : ScriptableObject
{
    [Header("Основные настройки")]
    public string effectName;
    [TextArea]
    public string description;
    public EffectType effectType;
    
    [Header("Параметры эффекта")]
    public float duration = 10f;
    public float intensity = 1f;
    public bool isPositive = true;
    
    [Header("Визуальные эффекты")]
    public GameObject applyEffect;
    public Color effectColor = Color.white;
    public AudioClip applySound;
    
    [Header("Игровые модификаторы")]
    public float healthChange = 0f;
    public float speedModifier = 1f;
    public float damageModifier = 1f;
    public bool grantInvisibility = false;
    
    public enum EffectType
    {
        Healing,        // Восстановление здоровья
        Mana,           // Восстановление маны
        Speed,          // Скорость
        Strength,       // Сила
        Invisibility,   // Невидимость
        Poison,         // Яд
        Confusion,      // Смятение
        Protection,     // Защита
        Luck,           // Удача
        Transformation  // Превращение
    }
    
    // Метод применения эффекта
    public void ApplyEffect(GameObject target)
    {
        if (target == null) return;
        
        // Визуальные эффекты
        if (applyEffect != null)
        {
            Instantiate(applyEffect, target.transform.position, Quaternion.identity, target.transform);
        }
        
        // Звук
        AudioSource audioSource = target.GetComponent<AudioSource>();
        if (audioSource == null) audioSource = target.AddComponent<AudioSource>();
        if (applySound != null)
        {
            audioSource.PlayOneShot(applySound);
        }
        
        // Применение игровых эффектов
        ApplyGameEffects(target);
        
        Debug.Log($"Применен эффект: {effectName} к {target.name}");
    }
    
    private void ApplyGameEffects(GameObject target)
    {
        // Здесь можно добавить логику применения эффектов к игроку/врагам
        // Например, изменение здоровья, скорости и т.д.
        
        // Пример для здоровья
        if (healthChange != 0f)
        {
            // target.GetComponent<Health>()?.ChangeHealth(healthChange * intensity);
            Debug.Log($"Изменение здоровья: {healthChange * intensity}");
        }
        
        // Пример для модификатора скорости
        if (speedModifier != 1f)
        {
            // target.GetComponent<PlayerMovement>()?.ModifySpeed(speedModifier, duration);
            Debug.Log($"Модификатор скорости: {speedModifier} на {duration} сек");
        }
        
        // Другие эффекты...
        if (grantInvisibility)
        {
            Debug.Log($"Невидимость на {duration} сек");
        }
    }
}