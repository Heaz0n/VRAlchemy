using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class InteractableObject : XRGrabInteractable
{
    protected override void Awake()
    {
        base.Awake();
        
        // Настройка для более естественного хватания
        smoothPosition = true;
        smoothRotation = true;
        throwOnDetach = false;
        
        // Настраиваем взаимодействие
        interactionLayers = InteractionLayerMask.GetMask("Default", "Interactable");
    }

    protected override void OnSelectEntered(SelectEnterEventArgs args)
    {
        base.OnSelectEntered(args);
        Debug.Log($"Объект {gameObject.name} взят");
    }

    protected override void OnSelectExited(SelectExitEventArgs args)
    {
        base.OnSelectExited(args);
        Debug.Log($"Объект {gameObject} отпущен");
    }
}