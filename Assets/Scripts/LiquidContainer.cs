using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class LiquidContainer : MonoBehaviour
{
    [Header("Liquid Settings")]
    public Color liquidColor = Color.blue;
    public float fillAmount = 0.5f; // 0-1
    public float pourThreshold = 0.7f; // Когда начинаем лить
    
    [Header("References")]
    public Renderer liquidRenderer;
    public Transform pourPoint;
    
    private bool isPouring = false;
    
    void Start()
    {
        UpdateLiquidVisual();
    }
    
    void Update()
    {
        CheckPouring();
    }
    
    private void CheckPouring()
    {
        // Проверяем угол наклона
        float pourAngle = Vector3.Angle(transform.up, Vector3.up);
        bool shouldPour = pourAngle > pourThreshold && fillAmount > 0;
        
        if (shouldPour && !isPouring)
        {
            StartPouring();
        }
        else if (!shouldPour && isPouring)
        {
            StopPouring();
        }
    }
    
    private void StartPouring()
    {
        isPouring = true;
        Debug.Log("Start pouring!");
        
        // Можно добавить частицы для визуализации
    }
    
    private void StopPouring()
    {
        isPouring = false;
        Debug.Log("Stop pouring!");
    }
    
    public void TransferLiquid(LiquidContainer targetContainer, float amount)
    {
        if (fillAmount >= amount)
        {
            fillAmount -= amount;
            targetContainer.fillAmount += amount;
            
            UpdateLiquidVisual();
            targetContainer.UpdateLiquidVisual();
        }
    }
    
    private void UpdateLiquidVisual()
    {
        if (liquidRenderer != null)
        {
            liquidRenderer.material.color = liquidColor;
            // Здесь можно анимировать уровень жидкости
        }
    }
    
    // Для отладки - рисуем точку из которой льем
    private void OnDrawGizmos()
    {
        if (pourPoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(pourPoint.position, 0.01f);
        }
    }
}