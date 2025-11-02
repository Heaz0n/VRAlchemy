using UnityEngine;
using UnityEngine.Events;
using UnityEngine.XR.Interaction.Toolkit;

public class VRButton : MonoBehaviour
{
    public UnityEvent onPress; // Событие при нажатии
    
    private void Start()
    {
        // Добавляем обработчик взаимодействия
        XRSimpleInteractable interactable = GetComponent<XRSimpleInteractable>();
        if (interactable != null)
        {
            interactable.selectEntered.AddListener(OnButtonPressed);
        }
    }
    
    void OnButtonPressed(SelectEnterEventArgs args)
    {
        Debug.Log("Кнопка нажата!");
        onPress.Invoke(); // Вызываем событие
        
        // Анимация нажатия
        transform.localScale = Vector3.one * 0.9f;
        Invoke("ResetButton", 0.1f);
    }
    
    void ResetButton()
    {
        transform.localScale = Vector3.one;
    }
}