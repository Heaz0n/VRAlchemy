using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections;

public class SimpleCauldron : MonoBehaviour
{
    [System.Serializable]
    public class PotionRecipe
    {
        public string ingredient1;
        public string ingredient2;
        public GameObject resultPotion;
        public string potionName;
        public Color potionColor;
        public string description;
    }

    [Header("Рецепты зелий")]
    public List<PotionRecipe> recipes = new List<PotionRecipe>();
    
    [Header("Настройки котла")]
    public Transform spawnPoint;
    public float brewTime = 2f;
    public float spawnHeight = 1.5f;

    [Header("Эффекты")]
    public ParticleSystem brewEffect;
    public Light cauldronLight;
    
    [Header("UI элементы")]
    public TextMeshProUGUI mainDisplay;
    public TextMeshProUGUI ingredientsDisplay;
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI hintDisplay;

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
    private bool hasFinishedBrewing = false;
    private float currentTimer = 0f;
    private int successCount = 0;
    private int totalAttempts = 0;
    private bool isSpawningPotion = false;
    private HashSet<GameObject> processedIngredients = new HashSet<GameObject>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (spawnPoint == null)
        {
            // Создаем точку спавна над котлом, но не меняем environment
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnPoint = spawnObj.transform;
            spawnPoint.position = transform.position + Vector3.up * spawnHeight;
            spawnPoint.SetParent(transform);
        }

        UpdateClearButton();
        ShowWelcomeMessage();
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
        if (isWorking || isSpawningPotion) 
        {
            ShowHint("Котел занят! Дождитесь окончания варки.");
            return;
        }

        if (processedIngredients.Contains(other.gameObject))
        {
            return;
        }

        // Работаем только с объектами, у которых есть компонент Ingredient
        Ingredient item = other.GetComponent<Ingredient>();
        if (item != null && currentIngredients.Count < 2)
        {
            ProcessIngredient(item, other.gameObject);
        }
        else if (currentIngredients.Count >= 2)
        {
            ShowHint("Котел полон! Начните варку или очистите котел.");
        }
    }

    void ProcessIngredient(Ingredient ingredient, GameObject ingredientObject)
    {
        processedIngredients.Add(ingredientObject);
        currentIngredients.Add(ingredient.ingredientName);

        PlaySound(addSound);
        Destroy(ingredientObject);

        UpdateIngredientsUI();
        UpdateClearButton();
        
        string message = $"Добавлен: {ingredient.ingredientName}\n";
        message += currentIngredients.Count == 1 ? "Нужен еще один ингредиент" : "Готово к варке!";
        ShowMessage(message);

        ShowHint($"Ингредиентов: {currentIngredients.Count}/2\nДобавьте еще один предмет");

        if (currentIngredients.Count >= 2)
        {
            StartBrewing();
        }
    }

    void StartBrewing()
    {
        isWorking = true;
        hasFinishedBrewing = false;
        isSpawningPotion = false;
        currentTimer = brewTime;
        totalAttempts++;

        UpdateClearButton();

        string combo = $"{currentIngredients[0]} + {currentIngredients[1]}";
        ShowMessage($"Варка началась!\nКомбинация: {combo}");
        ShowHint("Идет процесс варки... Ждите результата!");

        if (brewEffect != null)
            brewEffect.Play();

        if (cauldronLight != null)
        {
            cauldronLight.color = Color.yellow;
            cauldronLight.intensity = 2f;
        }

        PlaySound(brewSound);
    }

    void FinishBrewing()
    {
        if (hasFinishedBrewing || isSpawningPotion) return;

        hasFinishedBrewing = true;
        isWorking = false;
        isSpawningPotion = true;

        PotionRecipe foundRecipe = FindRecipe();

        if (foundRecipe != null)
        {
            CreateSuccessPotion(foundRecipe);
        }
        else
        {
            CreateFailedPotion();
        }

        currentIngredients.Clear();
        processedIngredients.Clear();
        UpdateIngredientsUI();
        UpdateClearButton();
    }

    PotionRecipe FindRecipe()
    {
        if (currentIngredients.Count != 2) return null;

        string item1 = currentIngredients[0];
        string item2 = currentIngredients[1];

        foreach (PotionRecipe recipe in recipes)
        {
            bool match = (recipe.ingredient1 == item1 && recipe.ingredient2 == item2) ||
                        (recipe.ingredient1 == item2 && recipe.ingredient2 == item1);

            if (match)
            {
                return recipe;
            }
        }

        return null;
    }

    void CreateSuccessPotion(PotionRecipe recipe)
    {
        successCount++;

        if (recipe.resultPotion != null && spawnPoint != null)
        {
            GameObject newPotion = Instantiate(recipe.resultPotion, spawnPoint.position, Quaternion.identity);
            
            // Добавляем физику если нет
            Rigidbody rb = newPotion.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = newPotion.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                rb.drag = 0.5f;
            }

            // Добавляем возможность брать в VR если нет
            XRGrabInteractable grab = newPotion.GetComponent<XRGrabInteractable>();
            if (grab == null)
            {
                grab = newPotion.AddComponent<XRGrabInteractable>();
            }

            StartCoroutine(PreventDuplicateInteractions(newPotion));
        }

        // Эффекты успеха
        if (brewEffect != null)
        {
            var main = brewEffect.main;
            main.startColor = recipe.potionColor;
            brewEffect.Play();
        }

        if (cauldronLight != null)
        {
            cauldronLight.color = recipe.potionColor;
        }

        PlaySound(successSound);
        
        string successMessage = $"УСПЕХ!\n";
        successMessage += $"Создано: {recipe.potionName}\n";
        successMessage += $"{recipe.description}\n";
        successMessage += $"Удачных зелий: {successCount}/{totalAttempts}";
        
        ShowMessage(successMessage);
        ShowHint("Отличная работа! Зелье готово.");

        StartCoroutine(ResetCauldronAfterDelay(3f));
    }

    void CreateFailedPotion()
    {
        if (spawnPoint != null)
        {
            GameObject failedItem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            failedItem.transform.position = spawnPoint.position;
            failedItem.transform.localScale = Vector3.one * 0.1f;
            
            Renderer rend = failedItem.GetComponent<Renderer>();
            rend.material.color = Color.gray;

            Rigidbody rb = failedItem.AddComponent<Rigidbody>();
            failedItem.name = "Неудачное зелье";

            StartCoroutine(PreventDuplicateInteractions(failedItem));
        }

        if (brewEffect != null)
        {
            var main = brewEffect.main;
            main.startColor = Color.gray;
            brewEffect.Play();
        }

        if (cauldronLight != null)
        {
            cauldronLight.color = Color.gray;
        }

        PlaySound(failSound);
        
        string failMessage = $"НЕУДАЧА\n";
        failMessage += $"Неизвестная комбинация:\n";
        failMessage += $"{currentIngredients[0]} + {currentIngredients[1]}\n";
        failMessage += $"Удачных зелий: {successCount}/{totalAttempts}";
        
        ShowMessage(failMessage);
        ShowHint("Эта комбинация не работает. Попробуйте другие предметы!");

        StartCoroutine(ResetCauldronAfterDelay(3f));
    }

    private IEnumerator PreventDuplicateInteractions(GameObject spawnedObject)
    {
        Collider[] colliders = spawnedObject.GetComponentsInChildren<Collider>();
        foreach (Collider collider in colliders) 
            collider.enabled = false;

        XRGrabInteractable grabInteractable = spawnedObject.GetComponent<XRGrabInteractable>();
        if (grabInteractable != null) 
            grabInteractable.enabled = false;

        yield return new WaitForSeconds(1.0f);

        foreach (Collider collider in colliders) 
            collider.enabled = true;
        if (grabInteractable != null) 
            grabInteractable.enabled = true;

        isSpawningPotion = false;
    }

    public void ClearCauldron()
    {
        if (isWorking || isSpawningPotion)
        {
            ShowHint("Нельзя очистить во время варки!");
            return;
        }

        if (currentIngredients.Count > 0)
        {
            PlaySound(clearSound);
            currentIngredients.Clear();
            processedIngredients.Clear();
            UpdateIngredientsUI();
            UpdateClearButton();
            
            ShowMessage("Котел очищен!\nМожно начинать новый эксперимент");
            ShowHint("Котел пуст. Добавьте два ингредиента для варки.");

            if (brewEffect != null)
            {
                var main = brewEffect.main;
                main.startColor = Color.blue;
                brewEffect.Play();
            }
        }
        else
        {
            ShowHint("Котел уже пуст! Добавьте ингредиенты для варки.");
        }
    }

    void UpdateClearButton()
    {
        if (clearButton != null)
        {
            bool shouldShow = currentIngredients.Count > 0 && !isWorking && !isSpawningPotion;
            clearButton.SetActive(shouldShow);
        }
    }

    private IEnumerator ResetCauldronAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        ResetCauldron();
    }

    void ResetCauldron()
    {
        isWorking = false;
        hasFinishedBrewing = false;
        isSpawningPotion = false;
        processedIngredients.Clear();

        if (cauldronLight != null)
        {
            cauldronLight.color = Color.white;
            cauldronLight.intensity = 1f;
        }

        if (brewEffect != null)
            brewEffect.Stop();

        UpdateClearButton();
        ShowMessage("Котел готов к работе!\nБросьте 2 ингредиента для варки");
        ShowHint("Используйте разные комбинации ингредиентов");
    }

    void ShowWelcomeMessage()
    {
        string welcomeMsg = "Алхимическая лаборатория\n\n";
        welcomeMsg += "Бросьте два ингредиента в котел\n";
        welcomeMsg += "для создания зелий";
        
        ShowMessage(welcomeMsg);
        ShowHint("Бросьте 2 ингредиента в котел чтобы начать");
    }

    void ShowMessage(string text)
    {
        if (mainDisplay != null)
        {
            mainDisplay.text = text;
        }
    }

    void ShowHint(string text)
    {
        if (hintDisplay != null)
        {
            hintDisplay.text = text;
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
        }
    }
}