using UnityEngine;
using System.Collections.Generic;

public class Cauldron : MonoBehaviour
{
    [System.Serializable]
    public class IngredientMix
    {
        public string ingredientName;
        public Color liquidColor;
        public ParticleSystem effect;
    }

    public List<IngredientMix> currentMix = new List<IngredientMix>();
    public Renderer liquidRenderer;
    public ParticleSystem bubbleEffect;

    private void Start()
    {
        if (liquidRenderer != null)
            liquidRenderer.material.color = Color.clear;
    }

    private void OnTriggerEnter(Collider other)
    {
        InteractableObject ingredient = other.GetComponent<InteractableObject>();
        if (ingredient != null)
        {
            AddIngredient(ingredient.gameObject.name);
            Destroy(ingredient.gameObject);
        }
    }

    private void AddIngredient(string ingredientName)
    {
        IngredientMix newIngredient = new IngredientMix
        {
            ingredientName = ingredientName,
            liquidColor = GetColorForIngredient(ingredientName)
        };

        currentMix.Add(newIngredient);
        UpdateCauldronAppearance();
        
        Debug.Log($"Добавлен ингредиент: {ingredientName}. Всего в котле: {currentMix.Count}");
    }

    private Color GetColorForIngredient(string ingredientName)
    {
        // Простая система цветов для демонстрации
        switch (ingredientName.ToLower())
        {
            case "herb": return Color.green;
            case "crystal": return Color.blue;
            case "mushroom": return Color.red;
            default: return Color.gray;
        }
    }

    private void UpdateCauldronAppearance()
    {
        if (liquidRenderer == null) return;

        Color mixedColor = Color.clear;
        foreach (var ingredient in currentMix)
        {
            mixedColor += ingredient.liquidColor;
        }
        mixedColor /= currentMix.Count;

        liquidRenderer.material.color = mixedColor;

        // Включаем эффект пузырьков при добавлении ингредиентов
        if (bubbleEffect != null && !bubbleEffect.isPlaying)
            bubbleEffect.Play();
    }

    public void ClearCauldron()
    {
        currentMix.Clear();
        UpdateCauldronAppearance();
        Debug.Log("Котел очищен");
    }
}