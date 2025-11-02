using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public string ingredientName;
    public Color ingredientColor;
    
    void Start()
    {
        // Автоматически настраиваем цвет
        GetComponent<Renderer>().material.color = ingredientColor;
    }
}