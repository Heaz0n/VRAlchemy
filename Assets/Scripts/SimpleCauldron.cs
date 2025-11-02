using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SimpleCauldron : MonoBehaviour
{
    [System.Serializable]
    public class PotionRecipe
    {
        public string ingredient1;
        public string ingredient2;
        public GameObject resultPotion;
        public string potionName;
    }

    [Header("Основные настройки")]
    public List<PotionRecipe> recipes = new List<PotionRecipe>();
    public Transform spawnPoint;
    public float brewTime = 2f;

    [Header("Эффекты")]
    public ParticleSystem brewEffect;
    public Light cauldronLight;
    
    [Header("UI элементы")]
    public TextMeshProUGUI mainDisplay;
    public TextMeshProUGUI ingredientsDisplay;
    public TextMeshProUGUI timerDisplay;

    [Header("Кнопка очистки")]
    public GameObject clearButton;
    public AudioClip clearSound;

    [Header("Звуки")]
    public AudioClip addSound;
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip brewSound;

    // Приватные переменные
    private AudioSource audioSource;
    private List<string> currentIngredients = new List<string>();
    private bool isWorking = false;
    private bool hasFinishedBrewing = false; // Защита от повторного вызова
    private float currentTimer = 0f;
    private int successCount = 0;
    private int totalAttempts = 0;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        UpdateClearButton();
        Debug.Log("Котел инициализирован");
        ShowMessage("Добро пожаловать в лабораторию!\nБросьте 2 ингредиента в котел");
        UpdateIngredientsUI();
    }

    void Update()
    {
        if (isWorking && !hasFinishedBrewing)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerUI();

            if (currentTimer <= 0)
            {
                FinishBrewing();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (isWorking) 
        {
            Debug.Log("Котел занят, игнорируем ингредиент");
            return;
        }

        Ingredient item = other.GetComponent<Ingredient>();
        if (item != null && currentIngredients.Count < 2)
        {
            Debug.Log($"Обнаружен ингредиент: {item.ingredientName}");
            ProcessIngredient(item);
        }
    }

    void ProcessIngredient(Ingredient ingredient)
    {
        currentIngredients.Add(ingredient.ingredientName);
        Debug.Log($"Добавлен ингредиент: {ingredient.ingredientName}. Всего: {currentIngredients.Count}");

        PlaySound(addSound);
        Destroy(ingredient.gameObject);

        UpdateIngredientsUI();
        UpdateClearButton();
        ShowMessage($"Добавлен: {ingredient.ingredientName}\nОсталось: {2 - currentIngredients.Count}");

        if (currentIngredients.Count >= 2)
        {
            Debug.Log("Достаточно ингредиентов, начинаем варку");
            StartBrewing();
        }
    }

    void StartBrewing()
    {
        isWorking = true;
        hasFinishedBrewing = false; // Сбрасываем флаг завершения
        currentTimer = brewTime;
        totalAttempts++;

        UpdateClearButton();

        Debug.Log($"Начало варки. Ингредиенты: {currentIngredients[0]} + {currentIngredients[1]}");

        if (brewEffect != null)
            brewEffect.Play();

        if (cauldronLight != null)
        {
            cauldronLight.color = Color.yellow;
            cauldronLight.intensity = 2f;
        }

        PlaySound(brewSound);
        ShowMessage("Начинаем варку зелья...\nЖдите результат!");
    }

    void FinishBrewing()
    {
        // Защита от повторного вызова
        if (hasFinishedBrewing)
        {
            Debug.Log("Варка уже завершена, игнорируем повторный вызов");
            return;
        }

        hasFinishedBrewing = true;
        isWorking = false;
        
        Debug.Log("Завершение варки...");

        // Ищем подходящий рецепт
        PotionRecipe foundRecipe = FindRecipe();

        if (foundRecipe != null)
        {
            Debug.Log($"Найден рецепт: {foundRecipe.potionName}");
            CreateSuccessPotion(foundRecipe);
        }
        else
        {
            Debug.Log("Рецепт не найден, создаем провальное зелье");
            CreateFailedPotion();
        }

        // Очищаем котел и обновляем кнопку
        currentIngredients.Clear();
        UpdateIngredientsUI();
        UpdateClearButton();
    }

    PotionRecipe FindRecipe()
    {
        if (currentIngredients.Count != 2) 
        {
            Debug.Log("Не хватает ингредиентов для поиска рецепта");
            return null;
        }

        string item1 = currentIngredients[0];
        string item2 = currentIngredients[1];

        Debug.Log($"Поиск рецепта для: {item1} + {item2}");

        foreach (PotionRecipe recipe in recipes)
        {
            bool match = (recipe.ingredient1 == item1 && recipe.ingredient2 == item2) ||
                        (recipe.ingredient1 == item2 && recipe.ingredient2 == item1);
            
            Debug.Log($"Проверка рецепта: {recipe.ingredient1} + {recipe.ingredient2} = {match}");

            if (match)
            {
                return recipe;
            }
        }

        Debug.Log("Подходящий рецепт не найден");
        return null;
    }

    void CreateSuccessPotion(PotionRecipe recipe)
    {
        successCount++;
        Debug.Log($"Создание успешного зелья: {recipe.potionName}");

        // Создаем зелье
        if (recipe.resultPotion != null && spawnPoint != null)
        {
            GameObject newPotion = Instantiate(recipe.resultPotion, spawnPoint.position, Quaternion.identity);
            Debug.Log($"Зелье создано: {newPotion.name}");
            
            // Добавляем защиту от повторного использования префаба
            StartCoroutine(PreventDuplicateSpawn(newPotion));
        }
        else
        {
            Debug.LogError("Ошибка: префаб зелья или точка появления не назначены!");
        }

        // Эффекты успеха
        if (brewEffect != null)
        {
            var main = brewEffect.main;
            main.startColor = Color.green;
            brewEffect.Play();
        }

        if (cauldronLight != null)
        {
            cauldronLight.color = Color.green;
        }

        PlaySound(successSound);
        ShowMessage($"Успех! Создано: {recipe.potionName}\nУдачных зелий: {successCount}/{totalAttempts}");

        Invoke("ResetCauldron", 3f);
    }

    void CreateFailedPotion()
    {
        Debug.Log("Создание провального зелья");

        // Создаем провальный объект
        GameObject failedItem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        failedItem.transform.position = spawnPoint.position;
        failedItem.transform.localScale = Vector3.one * 0.2f;
        
        Renderer rend = failedItem.GetComponent<Renderer>();
        rend.material.color = new Color(0.3f, 0.3f, 0.3f);

        Rigidbody rb = failedItem.AddComponent<Rigidbody>();
        failedItem.name = "Неудачное зелье";

        Debug.Log($"Провальное зелье создано: {failedItem.name}");

        // Эффекты провала
        if (brewEffect != null)
        {
            var main = brewEffect.main;
            main.startColor = Color.red;
            brewEffect.Play();
        }

        if (cauldronLight != null)
        {
            cauldronLight.color = Color.red;
        }

        PlaySound(failSound);
        ShowMessage($"Провал! Неизвестная комбинация\nУдачных зелий: {successCount}/{totalAttempts}");

        Invoke("ResetCauldron", 3f);
    }

    // Защита от дублирования спавна
    private System.Collections.IEnumerator PreventDuplicateSpawn(GameObject spawnedObject)
    {
        // Временно отключаем коллайдер чтобы предотвратить повторное срабатывание
        Collider collider = spawnedObject.GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
            yield return new WaitForSeconds(0.5f);
            collider.enabled = true;
        }
    }

    // === МЕТОД ДЛЯ ОЧИСТКИ КОТЛА ===
    public void ClearCauldron()
    {
        Debug.Log("Вызов метода ClearCauldron");
        
        if (isWorking)
        {
            Debug.Log("Нельзя очистить котел во время варки!");
            ShowMessage("Нельзя очистить во время варки!\nДождитесь окончания");
            return;
        }

        if (currentIngredients.Count > 0)
        {
            Debug.Log($"Очистка котла. Удалено ингредиентов: {currentIngredients.Count}");
            
            PlaySound(clearSound);
            currentIngredients.Clear();
            UpdateIngredientsUI();
            UpdateClearButton();
            ShowMessage("Котел очищен!\nМожно начинать заново");
            
            if (brewEffect != null)
            {
                var main = brewEffect.main;
                main.startColor = Color.blue;
                brewEffect.Play();
            }

            Debug.Log("Котел успешно очищен");
        }
        else
        {
            Debug.Log("Котел уже пуст");
            ShowMessage("Котел уже пуст!\nДобавьте ингредиенты");
        }
    }

    void UpdateClearButton()
    {
        if (clearButton != null)
        {
            bool shouldShow = currentIngredients.Count > 0 && !isWorking;
            clearButton.SetActive(shouldShow);

            Renderer buttonRenderer = clearButton.GetComponent<Renderer>();
            if (buttonRenderer != null)
            {
                if (isWorking)
                    buttonRenderer.material.color = Color.gray;
                else if (currentIngredients.Count > 0)
                    buttonRenderer.material.color = Color.red;
                else
                    buttonRenderer.material.color = Color.white;
            }
        }
    }

    void ResetCauldron()
    {
        Debug.Log("Сброс котла");

        if (cauldronLight != null)
        {
            cauldronLight.color = Color.white;
            cauldronLight.intensity = 1f;
        }

        if (brewEffect != null)
            brewEffect.Stop();

        UpdateClearButton();
        ShowMessage("Котел готов!\nБросьте 2 новых ингредиента");
        UpdateIngredientsUI();
    }

    void ShowMessage(string text)
    {
        if (mainDisplay != null)
        {
            mainDisplay.text = text;
            Debug.Log($"UI сообщение: {text}");
        }
    }

    void UpdateIngredientsUI()
    {
        if (ingredientsDisplay != null)
        {
            if (currentIngredients.Count == 0)
            {
                ingredientsDisplay.text = "Ингредиентов: 0/2";
            }
            else
            {
                string text = "В котле:\n";
                foreach (string ingredient in currentIngredients)
                {
                    text += $"• {ingredient}\n";
                }
                text += $"\n<color=red>Нажмите кнопку для очистки</color>";
                ingredientsDisplay.text = text;
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerDisplay != null)
        {
            timerDisplay.text = $"Осталось: {currentTimer:0.0}с";
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip);
            Debug.Log($"Воспроизведен звук: {clip.name}");
        }
    }

    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.1f);
        }
    }
}