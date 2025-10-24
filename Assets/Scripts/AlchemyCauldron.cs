using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlchemyCauldron : MonoBehaviour
{
    public GameObject potionPrefab; // Префаб зелья
    private int ingredientsCount = 0;
    public int requiredIngredients = 2;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Ingredient"))
        {
            Destroy(other.gameObject); // Уничтожаем ингредиент
            ingredientsCount++;
            Debug.Log("Ингредиент добавлен: " + ingredientsCount);

            CheckForPotion();
        }
    }

    public void AddIngredient(string ingredientType)
    {
        // Метод для добавления ингредиентов (например, воды)
        ingredientsCount++;
        Debug.Log($"{ingredientType} добавлен в котёл: {ingredientsCount}");

        CheckForPotion();
    }

    void CheckForPotion()
    {
        if (ingredientsCount >= requiredIngredients)
        {
            // Создаём зелье
            Instantiate(potionPrefab, transform.position + Vector3.up, Quaternion.identity);
            ingredientsCount = 0;
            Debug.Log("Зелье готово!");
        }
    }
}