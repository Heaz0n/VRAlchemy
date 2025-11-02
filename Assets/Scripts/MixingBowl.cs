using UnityEngine;
using System.Collections.Generic;

public class MixingBowl : MonoBehaviour
{
    [System.Serializable]
    public class Ingredient
    {
        public string name;
        public Color color;
        public string effect;
    }
    
    [Header("Mixing Bowl")]
    public List<Ingredient> currentIngredients = new List<Ingredient>();
    public Color currentColor = Color.clear;
    
    public void AddIngredient(Ingredient newIngredient)
    {
        currentIngredients.Add(newIngredient);
        UpdateMixture();
        
        Debug.Log($"Added {newIngredient.name}. Total ingredients: {currentIngredients.Count}");
    }
    
    private void UpdateMixture()
    {
        // Простая система смешивания цветов
        Color mixedColor = Color.clear;
        foreach (var ingredient in currentIngredients)
        {
            mixedColor += ingredient.color;
        }
        mixedColor /= currentIngredients.Count;
        
        currentColor = mixedColor;
        
        // Здесь можно добавить логику определения эффектов
        CheckForPotionRecipe();
    }
    
    private void CheckForPotionRecipe()
    {
        // Простые рецепты для теста
        if (currentIngredients.Count == 2)
        {
            bool hasRed = currentIngredients.Exists(i => i.color.r > 0.5f);
            bool hasBlue = currentIngredients.Exists(i => i.color.b > 0.5f);
            
            if (hasRed && hasBlue)
            {
                Debug.Log("🎉 Создано зелье фиолетовой магии!");
            }
        }
    }
    
    public void ResetBowl()
    {
        currentIngredients.Clear();
        currentColor = Color.clear;
    }
}