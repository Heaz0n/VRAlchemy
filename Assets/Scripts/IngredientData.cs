using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Ingredient", menuName = "Alchemy/Ingredient")]
public class IngredientData : ScriptableObject
{
    public string ingredientName;
    public GameObject prefab;
    public Color color;
    public AudioClip pickupSound;
    
    [Header("Elemental Properties")]
    public int fireValue = 0;
    public int waterValue = 0; 
    public int earthValue = 0;
    public int airValue = 0;
    
    [Header("Effects")]
    public List<EffectType> effects = new List<EffectType>();
}

public enum EffectType
{
    Healing,
    Poison,
    Strength,
    Invisibility,
    FireResistance,
    NightVision
}