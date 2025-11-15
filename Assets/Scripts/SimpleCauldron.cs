using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using System.Collections.Generic;
using TMPro;
using System.Collections;
using System.Linq;

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

    [System.Serializable]
    public class ObjectAnalysis
    {
        public string objectName;
        public string objectTag;
        public string layerName;
        public bool hasRigidbody;
        public bool hasRenderer;
        public bool hasCollider;
        public string colliderType;
        public Vector3 objectScale;
        public Color objectColor;
        public List<string> components;
        public bool isIngredient;
        public string ingredientType;
    }

    [System.Serializable]
    public class PuzzleSequence
    {
        public string puzzleName;
        public string[] ingredientSequence;
        public GameObject specialReward;
        public string hintText;
        public Color puzzleColor;
        public float timeLimit = 60f;
    }

    [Header("Рецепты зелий")]
    public List<PotionRecipe> recipes = new List<PotionRecipe>();
    
    [Header("Настройки котла")]
    public Transform spawnPoint;
    public float brewTime = 2f;
    public float spawnHeight = 1.5f;
    public bool destroyObjects = false;

    [Header("Встроенные VFX эффекты")]
    public bool enableVFX = true;
    public float vfxIntensity = 1.0f;
    
    [Header("UI элементы")]
    public TextMeshProUGUI mainDisplay;
    public TextMeshProUGUI ingredientsDisplay;
    public TextMeshProUGUI timerDisplay;
    public TextMeshProUGUI hintDisplay;
    public TextMeshProUGUI analysisDisplay;

    [Header("UI головоломок")]
    public TextMeshProUGUI puzzleDisplay;
    public TextMeshProUGUI sequenceDisplay;
    public TextMeshProUGUI timeDisplay;
    public TextMeshProUGUI puzzleProgressDisplay;
    public GameObject puzzlePanel;
    public GameObject puzzleButton;

    [Header("Кнопка очистки")]
    public GameObject clearButton;
    public AudioClip clearSound;

    [Header("VR Кнопки")]
    public XRSimpleInteractable vrClearButton;
    public XRSimpleInteractable vrPuzzleButton;
    public XRSimpleInteractable vrVFXButton;
    public XRSimpleInteractable vrResetButton;
    public float vrButtonPressDelay = 0.5f;

    [Header("Звуки")]
    public AudioClip addSound;
    public AudioClip successSound;
    public AudioClip failSound;
    public AudioClip brewSound;
    public AudioClip puzzleCompleteSound;

    [Header("Настройки анализа")]
    public bool showDetailedAnalysis = true;
    public bool acceptAllObjects = true;

    [Header("Система головоломок")]
    public bool enablePuzzles = true;
    public List<PuzzleSequence> puzzles = new List<PuzzleSequence>();
    public int currentPuzzleIndex = 0;
    private float puzzleTimer = 0f;
    private bool puzzleActive = false;
    private List<string> puzzleSequence = new List<string>();
    private List<string> playerSequence = new List<string>();
    private int puzzlesSolved = 0;

    private AudioSource audioSource;
    private List<string> currentIngredients = new List<string>();
    private List<ObjectAnalysis> analyzedObjects = new List<ObjectAnalysis>();
    private bool isWorking = false;
    private bool hasFinishedBrewing = false;
    private float currentTimer = 0f;
    private int successCount = 0;
    private int totalAttempts = 0;
    private bool isSpawningPotion = false;
    private HashSet<GameObject> processedIngredients = new HashSet<GameObject>();
    
    private List<GameObject> absorbedObjects = new List<GameObject>();
    private List<Vector3> originalPositions = new List<Vector3>();
    private List<Quaternion> originalRotations = new List<Quaternion>();
    private List<bool> originalKinematicStates = new List<bool>();
    private List<bool> originalActiveStates = new List<bool>();
    
    private ParticleSystem absorptionEffect;
    private ParticleSystem brewingEffect;
    private ParticleSystem successEffect;
    private ParticleSystem failureEffect;
    private ParticleSystem bubbleEffect;
    private Light cauldronLight;
    private Material liquidMaterial;

    // VR переменные
    private bool vrButtonsInitialized = false;
    private Dictionary<XRSimpleInteractable, Color> originalButtonColors = new Dictionary<XRSimpleInteractable, Color>();

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        if (spawnPoint == null)
        {
            GameObject spawnObj = new GameObject("SpawnPoint");
            spawnPoint = spawnObj.transform;
            spawnPoint.position = transform.position + Vector3.up * spawnHeight;
            spawnPoint.SetParent(transform);
        }

        InitializeAllVFX();
        InitializePuzzles();
        InitializeVRButtons(); // Инициализация VR кнопок
        CheckSceneSetup();
        UpdateClearButton();
        UpdatePuzzleButton();
        ShowWelcomeMessage();
    }

    // ДОБАВЛЕНО: Инициализация VR кнопок
    void InitializeVRButtons()
    {
        // Настройка кнопки очистки для VR
        if (vrClearButton != null)
        {
            vrClearButton.selectEntered.AddListener((args) => 
            {
                if (!isWorking && !isSpawningPotion)
                {
                    StartCoroutine(VRButtonPressEffect(vrClearButton, ClearCauldron));
                }
            });
            StoreOriginalButtonColor(vrClearButton);
        }

        // Настройка кнопки головоломки для VR
        if (vrPuzzleButton != null)
        {
            vrPuzzleButton.selectEntered.AddListener((args) => 
            {
                if (!isWorking && !isSpawningPotion && !puzzleActive)
                {
                    StartCoroutine(VRButtonPressEffect(vrPuzzleButton, StartNextPuzzle));
                }
            });
            StoreOriginalButtonColor(vrPuzzleButton);
        }

        // Настройка кнопки VFX для VR
        if (vrVFXButton != null)
        {
            vrVFXButton.selectEntered.AddListener((args) => 
            {
                StartCoroutine(VRButtonPressEffect(vrVFXButton, ToggleVFX));
            });
            StoreOriginalButtonColor(vrVFXButton);
        }

        // Настройка кнопки сброса для VR
        if (vrResetButton != null)
        {
            vrResetButton.selectEntered.AddListener((args) => 
            {
                if (!isWorking && !isSpawningPotion)
                {
                    StartCoroutine(VRButtonPressEffect(vrResetButton, ResetAllProgress));
                }
            });
            StoreOriginalButtonColor(vrResetButton);
        }

        vrButtonsInitialized = true;
        UpdateVRButtonsVisibility();
    }

    // ДОБАВЛЕНО: Эффект нажатия VR кнопки
    private IEnumerator VRButtonPressEffect(XRSimpleInteractable button, System.Action action)
    {
        // Визуальная обратная связь
        if (button.TryGetComponent<Renderer>(out var renderer))
        {
            Color originalColor = GetOriginalButtonColor(button);
            renderer.material.color = Color.green;
            
            // Звук нажатия
            PlaySound(addSound);
            
            yield return new WaitForSeconds(vrButtonPressDelay);
            
            // Возврат исходного цвета
            renderer.material.color = originalColor;
        }

        // Выполнение действия
        action?.Invoke();
    }

    // ДОБАВЛЕНО: Сохранение оригинального цвета кнопки
    private void StoreOriginalButtonColor(XRSimpleInteractable button)
    {
        if (button.TryGetComponent<Renderer>(out var renderer))
        {
            originalButtonColors[button] = renderer.material.color;
        }
    }

    // ДОБАВЛЕНО: Получение оригинального цвета кнопки
    private Color GetOriginalButtonColor(XRSimpleInteractable button)
    {
        if (originalButtonColors.ContainsKey(button))
        {
            return originalButtonColors[button];
        }
        return Color.white;
    }

    // ДОБАВЛЕНО: Обновление видимости VR кнопок
    void UpdateVRButtonsVisibility()
    {
        if (!vrButtonsInitialized) return;

        // Кнопка очистки видна только когда есть что очищать
        if (vrClearButton != null)
        {
            bool shouldShow = currentIngredients.Count > 0 && !isWorking && !isSpawningPotion && !puzzleActive;
            vrClearButton.gameObject.SetActive(shouldShow);
        }

        // Кнопка головоломки видна только когда доступны головоломки
        if (vrPuzzleButton != null)
        {
            bool shouldShow = enablePuzzles && puzzles.Count > 0 && currentPuzzleIndex < puzzles.Count && !puzzleActive;
            vrPuzzleButton.gameObject.SetActive(shouldShow);
        }
    }

    // ДОБАВЛЕНО: Автоматическое создание VR кнопок если их нет
    void CreateVRButtonsIfNeeded()
    {
        if (vrClearButton == null)
        {
            vrClearButton = CreateVRButton("VR_ClearButton", new Vector3(1f, 1f, 0f), "ОЧИСТИТЬ", Color.red);
        }
        
        if (vrPuzzleButton == null)
        {
            vrPuzzleButton = CreateVRButton("VR_PuzzleButton", new Vector3(-1f, 1f, 0f), "ГОЛОВОЛОМКА", Color.blue);
        }
        
        if (vrVFXButton == null)
        {
            vrVFXButton = CreateVRButton("VR_VFXButton", new Vector3(0f, 1f, 1f), "ЭФФЕКТЫ", Color.yellow);
        }
        
        if (vrResetButton == null)
        {
            vrResetButton = CreateVRButton("VR_ResetButton", new Vector3(0f, 1f, -1f), "СБРОС", Color.gray);
        }
    }

    // ДОБАВЛЕНО: Создание VR кнопки
    XRSimpleInteractable CreateVRButton(string name, Vector3 localPosition, string buttonText, Color color)
    {
        GameObject buttonObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        buttonObj.name = name;
        buttonObj.transform.SetParent(transform);
        buttonObj.transform.localPosition = localPosition;
        buttonObj.transform.localScale = new Vector3(0.3f, 0.1f, 0.05f);
        
        // Настройка материала
        Renderer renderer = buttonObj.GetComponent<Renderer>();
        renderer.material.color = color;
        
        // Добавление текста
        GameObject textObj = new GameObject("ButtonText");
        textObj.transform.SetParent(buttonObj.transform);
        textObj.transform.localPosition = new Vector3(0, 0, -0.03f);
        textObj.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        
        TextMesh textMesh = textObj.AddComponent<TextMesh>();
        textMesh.text = buttonText;
        textMesh.fontSize = 20;
        textMesh.anchor = TextAnchor.MiddleCenter;
        textMesh.alignment = TextAlignment.Center;
        textMesh.color = Color.white;
        
        // Добавление компонентов для VR взаимодействия
        XRSimpleInteractable interactable = buttonObj.AddComponent<XRSimpleInteractable>();
        
        // Настройка коллайдера
        BoxCollider collider = buttonObj.GetComponent<BoxCollider>();
        collider.isTrigger = false;
        
        return interactable;
    }

    // ДОБАВЛЕНО: Новый метод для сброса всего прогресса
    public void ResetAllProgress()
    {
        if (isWorking || isSpawningPotion)
        {
            ShowHint("Нельзя сбросить во время варки!");
            return;
        }

        successCount = 0;
        totalAttempts = 0;
        puzzlesSolved = 0;
        currentPuzzleIndex = 0;
        
        // Очистка текущих ингредиентов
        if (currentIngredients.Count > 0)
        {
            if (!destroyObjects)
            {
                RestoreAbsorbedObjects();
            }
            currentIngredients.Clear();
            processedIngredients.Clear();
            analyzedObjects.Clear();
        }
        
        ShowMessage("<color=yellow>Весь прогресс сброшен!</color>\nНачинаем заново!");
        ShowHint("Прогресс обнулен. Можно начинать новые эксперименты!");
        
        // Переинициализация головоломок
        InitializePuzzles();
        
        // Обновление UI
        UpdateClearButton();
        UpdatePuzzleButton();
        UpdateVRButtonsVisibility();
        
        // Визуальный эффект
        if (enableVFX)
        {
            for (int i = 0; i < 3; i++)
            {
                StartCoroutine(CreateResetEffect(i * 0.3f));
            }
        }
        
        PlaySound(clearSound);
    }

    // ДОБАВЛЕНО: Эффект сброса
    private IEnumerator CreateResetEffect(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Vector3 position = transform.position + 
                          new Vector3(Random.Range(-1f, 1f), 
                                     Random.Range(1f, 2f), 
                                     Random.Range(-1f, 1f));
        
        CreateFlashEffect(position, Color.yellow);
    }

    void InitializePuzzles()
    {
        if (!enablePuzzles || puzzles.Count == 0) return;
        
        currentPuzzleIndex = 0;
        puzzlesSolved = 0;
        
        if (puzzlePanel != null) 
            puzzlePanel.SetActive(false);
            
        if (puzzleProgressDisplay != null)
            puzzleProgressDisplay.text = $"Головоломки: 0/{puzzles.Count}";
    }

    void StartPuzzle(int puzzleIndex)
    {
        if (puzzleIndex >= puzzles.Count) 
        {
            CompleteAllPuzzles();
            return;
        }
        
        puzzleActive = true;
        puzzleTimer = puzzles[puzzleIndex].timeLimit;
        playerSequence.Clear();
        puzzleSequence = new List<string>(puzzles[puzzleIndex].ingredientSequence);
        
        UpdatePuzzleUI();
        ShowPuzzleMessage($"ГОЛОВОЛОМКА: {puzzles[puzzleIndex].puzzleName}");
        ShowHint(puzzles[puzzleIndex].hintText);
        
        if (puzzlePanel != null) 
            puzzlePanel.SetActive(true);
            
        PlaySound(brewSound);
        
        UpdateVRButtonsVisibility(); // Обновляем видимость VR кнопок
    }

    void UpdatePuzzleUI()
    {
        if (!puzzleActive) return;
        
        if (puzzleDisplay != null)
        {
            puzzleDisplay.text = $"Головоломка {currentPuzzleIndex + 1}/{puzzles.Count}\n" +
                               $"<size=70%>{puzzles[currentPuzzleIndex].puzzleName}</size>";
            puzzleDisplay.color = puzzles[currentPuzzleIndex].puzzleColor;
        }
        
        if (sequenceDisplay != null)
        {
            string sequenceText = "Последовательность:\n";
            for (int i = 0; i < puzzleSequence.Count; i++)
            {
                string status = i < playerSequence.Count ? "✓" : "?";
                Color statusColor = i < playerSequence.Count ? Color.green : Color.yellow;
                sequenceText += $"<color=#{ColorUtility.ToHtmlStringRGB(statusColor)}>{i+1}. {puzzleSequence[i]} {status}</color>\n";
            }
            sequenceDisplay.text = sequenceText;
        }
        
        if (timeDisplay != null)
        {
            timeDisplay.text = $"Время: {puzzleTimer:0}с";
            timeDisplay.color = puzzleTimer < 10f ? Color.red : Color.white;
        }
        
        if (puzzleProgressDisplay != null)
        {
            puzzleProgressDisplay.text = $"Головоломки: {puzzlesSolved}/{puzzles.Count}";
        }
    }

    void CheckPuzzleSequence(string addedIngredient)
    {
        if (!puzzleActive) return;
        
        playerSequence.Add(addedIngredient);
        
        // Проверяем правильность последовательности
        bool sequenceCorrect = true;
        for (int i = 0; i < playerSequence.Count; i++)
        {
            if (i >= puzzleSequence.Count || playerSequence[i] != puzzleSequence[i])
            {
                sequenceCorrect = false;
                break;
            }
        }
        
        if (!sequenceCorrect)
        {
            FailPuzzle();
            return;
        }
        
        UpdatePuzzleUI();
        
        // Проверяем завершение головоломки
        if (playerSequence.Count == puzzleSequence.Count)
        {
            SuccessPuzzle();
        }
    }

    void SuccessPuzzle()
    {
        puzzleActive = false;
        puzzlesSolved++;
        
        PlayPuzzleSuccessVFX();
        PlaySound(puzzleCompleteSound);
        
        string successMsg = $"<color=#{ColorUtility.ToHtmlStringRGB(Color.green)}>ГОЛОВОЛОМКА РЕШЕНА!</color>\n" +
                           $"{puzzles[currentPuzzleIndex].puzzleName}\n" +
                           $"Награда: особое зелье!";
        
        ShowMessage(successMsg);
        
        // Создаем особую награду
        if (puzzles[currentPuzzleIndex].specialReward != null && spawnPoint != null)
        {
            GameObject reward = Instantiate(puzzles[currentPuzzleIndex].specialReward, 
                                          spawnPoint.position, Quaternion.identity);
            SetupPotionPhysics(reward);
            SetupXRInteractions(reward);
            
            // Добавляем специальные эффекты к награде
            Light rewardLight = reward.AddComponent<Light>();
            rewardLight.color = puzzles[currentPuzzleIndex].puzzleColor;
            rewardLight.intensity = 3f;
            rewardLight.range = 2f;
            
            StartCoroutine(PreventDuplicateInteractions(reward));
        }
        
        // Переходим к следующей головоломке
        currentPuzzleIndex++;
        if (currentPuzzleIndex < puzzles.Count)
        {
            StartCoroutine(StartNextPuzzleAfterDelay(3f));
        }
        else
        {
            CompleteAllPuzzles();
        }
        
        UpdatePuzzleButton();
        UpdateVRButtonsVisibility(); // Обновляем видимость VR кнопок
    }

    void FailPuzzle()
    {
        puzzleActive = false;
        PlayPuzzleFailVFX();
        PlaySound(failSound);
        
        ShowMessage("<color=red>Неправильная последовательность!</color>\nПопробуйте снова.");
        
        // Перезапускаем текущую головоломку
        StartCoroutine(RestartPuzzleAfterDelay(2f));
        
        UpdateVRButtonsVisibility(); // Обновляем видимость VR кнопок
    }

    void CompleteAllPuzzles()
    {
        ShowMessage("<color=green>ВСЕ ГОЛОВОЛОМКИ РЕШЕНЫ!</color>\nВы мастер алхимии!");
        if (puzzlePanel != null) 
            puzzlePanel.SetActive(false);
            
        PlayVictoryEffects();
        UpdateVRButtonsVisibility(); // Обновляем видимость VR кнопок
    }

    IEnumerator StartNextPuzzleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartPuzzle(currentPuzzleIndex);
    }

    IEnumerator RestartPuzzleAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartPuzzle(currentPuzzleIndex);
    }

    void PlayPuzzleSuccessVFX()
    {
        if (!enableVFX) return;
        
        // Специальные эффекты для головоломки
        for (int i = 0; i < 5; i++)
        {
            StartCoroutine(CreatePuzzleFirework(i * 0.2f));
        }
        
        if (cauldronLight != null)
        {
            cauldronLight.color = puzzles[currentPuzzleIndex].puzzleColor;
            cauldronLight.intensity = 8f * vfxIntensity;
            StartCoroutine(FadeLightIntensity(cauldronLight, 8f, 1f, 2f));
        }
    }

    IEnumerator CreatePuzzleFirework(float delay)
    {
        yield return new WaitForSeconds(delay);
        
        Vector3 position = transform.position + 
                          new Vector3(Random.Range(-1f, 1f), 
                                     Random.Range(1f, 2f), 
                                     Random.Range(-1f, 1f));
        
        CreateFlashEffect(position, puzzles[currentPuzzleIndex].puzzleColor);
        
        // Создаем дополнительные частицы
        CreateSparkleEffect(position, puzzles[currentPuzzleIndex].puzzleColor);
    }

    void PlayPuzzleFailVFX()
    {
        if (!enableVFX) return;
        
        // Эффекты провала
        for (int i = 0; i < 3; i++)
        {
            Vector3 position = transform.position + Vector3.up * 0.5f;
            CreateFlashEffect(position, Color.red);
        }
        
        if (cauldronLight != null)
        {
            cauldronLight.color = Color.red;
            cauldronLight.intensity = 5f * vfxIntensity;
            StartCoroutine(FadeLightIntensity(cauldronLight, 5f, 0.5f, 1f));
        }
    }

    void PlayVictoryEffects()
    {
        if (!enableVFX) return;
        
        StartCoroutine(VictoryFireworks());
        
        if (cauldronLight != null)
        {
            cauldronLight.color = Color.yellow;
            cauldronLight.intensity = 10f * vfxIntensity;
        }
    }

    IEnumerator VictoryFireworks()
    {
        for (int i = 0; i < 10; i++)
        {
            Vector3 position = transform.position + 
                              new Vector3(Random.Range(-2f, 2f), 
                                         Random.Range(2f, 4f), 
                                         Random.Range(-2f, 2f));
            
            Color randomColor = new Color(Random.value, Random.value, Random.value);
            CreateFlashEffect(position, randomColor);
            CreateSparkleEffect(position, randomColor);
            
            yield return new WaitForSeconds(0.3f);
        }
    }

    void CreateSparkleEffect(Vector3 position, Color color)
    {
        GameObject sparkle = new GameObject("SparkleEffect");
        sparkle.transform.position = position;
        
        ParticleSystem sparklePS = sparkle.AddComponent<ParticleSystem>();
        
        var main = sparklePS.main;
        main.startSpeed = 2f;
        main.startLifetime = 1f;
        main.startSize = 0.1f;
        main.startColor = color;
        main.maxParticles = 10;
        
        var emission = sparklePS.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 8)
        });
        
        var shape = sparklePS.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.1f;
        
        Destroy(sparkle, 2f);
    }

    IEnumerator FadeLightIntensity(Light light, float startIntensity, float endIntensity, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            light.intensity = Mathf.Lerp(startIntensity, endIntensity, elapsed / duration);
            yield return null;
        }
    }

    void InitializeAllVFX()
    {
        CreateCauldronLight();
        CreateLiquidMaterial();
        CreateAbsorptionEffect();
        CreateBrewingEffect();
        CreateSuccessEffect();
        CreateFailureEffect();
        CreateBubbleEffect();
    }

    void CreateCauldronLight()
    {
        GameObject lightObj = new GameObject("CauldronLight");
        lightObj.transform.SetParent(transform);
        lightObj.transform.localPosition = Vector3.up * 0.5f;
        
        cauldronLight = lightObj.AddComponent<Light>();
        cauldronLight.type = LightType.Point;
        cauldronLight.range = 3f;
        cauldronLight.intensity = 0f;
        cauldronLight.color = Color.white;
    }

    void CreateLiquidMaterial()
    {
        GameObject liquidObj = new GameObject("LiquidSurface");
        liquidObj.transform.SetParent(transform);
        liquidObj.transform.localPosition = Vector3.up * 0.2f;
        liquidObj.transform.localScale = new Vector3(0.8f, 0.1f, 0.8f);
        
        MeshRenderer renderer = liquidObj.AddComponent<MeshRenderer>();
        MeshFilter filter = liquidObj.AddComponent<MeshFilter>();
        filter.mesh = CreateQuadMesh();
        
        liquidMaterial = new Material(Shader.Find("Standard"));
        liquidMaterial.SetFloat("_Metallic", 0.3f);
        liquidMaterial.SetFloat("_Glossiness", 0.8f);
        liquidMaterial.color = new Color(0.3f, 0.3f, 0.8f, 0.7f);
        liquidMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
        liquidMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        liquidMaterial.SetInt("_ZWrite", 0);
        liquidMaterial.DisableKeyword("_ALPHATEST_ON");
        liquidMaterial.EnableKeyword("_ALPHABLEND_ON");
        liquidMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        liquidMaterial.renderQueue = 3000;
        
        renderer.material = liquidMaterial;
    }

    Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, -0.5f),
            new Vector3(0.5f, 0, 0.5f),
            new Vector3(-0.5f, 0, 0.5f)
        };
        mesh.triangles = new int[] { 0, 1, 2, 0, 2, 3 };
        mesh.normals = new Vector3[] {
            Vector3.up, Vector3.up, Vector3.up, Vector3.up
        };
        mesh.uv = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(1, 1),
            new Vector2(0, 1)
        };
        return mesh;
    }

    void CreateAbsorptionEffect()
    {
        GameObject absorptionObj = new GameObject("AbsorptionEffect");
        absorptionObj.transform.SetParent(transform);
        absorptionObj.transform.localPosition = Vector3.zero;
        
        absorptionEffect = absorptionObj.AddComponent<ParticleSystem>();
        
        var main = absorptionEffect.main;
        main.duration = 1.0f;
        main.loop = false;
        main.playOnAwake = false;
        main.startSpeed = 2f;
        main.startLifetime = 1f;
        main.startSize = 0.05f;
        main.startColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        
        var emission = absorptionEffect.emission;
        emission.rateOverTime = 0;
        emission.enabled = true;
        emission.SetBurst(0, new ParticleSystem.Burst(0f, 30));
        
        var shape = absorptionEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        shape.radiusThickness = 0.5f;
        
        var velocity = absorptionEffect.velocityOverLifetime;
        velocity.enabled = true;
        velocity.space = ParticleSystemSimulationSpace.World;
        
        var color = absorptionEffect.colorOverLifetime;
        color.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(new Color(0.2f, 1f, 0.2f), 0.0f), 
                new GradientColorKey(new Color(0.8f, 1f, 0.2f), 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        color.color = gradient;
        
        var renderer = absorptionEffect.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(new Color(0.3f, 1f, 0.3f));
    }

    void CreateBrewingEffect()
    {
        GameObject brewingObj = new GameObject("BrewingEffect");
        brewingObj.transform.SetParent(transform);
        brewingObj.transform.localPosition = Vector3.up * 0.3f;
        
        brewingEffect = brewingObj.AddComponent<ParticleSystem>();
        
        var main = brewingEffect.main;
        main.duration = brewTime;
        main.loop = true;
        main.playOnAwake = false;
        main.startSpeed = 0.5f;
        main.startLifetime = 2f;
        main.startSize = 0.02f;
        main.startColor = new Color(0.8f, 0.6f, 0.2f, 1f);
        main.maxParticles = 100;
        
        var emission = brewingEffect.emission;
        emission.rateOverTime = 20f;
        
        var shape = brewingEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;
        shape.radiusThickness = 0.1f;
        
        var velocity = brewingEffect.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.5f, 1.5f);
        
        var color = brewingEffect.colorOverLifetime;
        color.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.yellow, 0.0f), 
                new GradientColorKey(Color.red, 0.5f),
                new GradientColorKey(Color.blue, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.8f, 0.5f),
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        color.color = gradient;
        
        var renderer = brewingEffect.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(Color.yellow);
    }

    void CreateSuccessEffect()
    {
        GameObject successObj = new GameObject("SuccessEffect");
        successObj.transform.SetParent(transform);
        successObj.transform.localPosition = Vector3.up * 0.5f;
        
        successEffect = successObj.AddComponent<ParticleSystem>();
        
        var main = successEffect.main;
        main.duration = 3.0f;
        main.loop = false;
        main.playOnAwake = false;
        main.startSpeed = 3f;
        main.startLifetime = 2f;
        main.startSize = 0.1f;
        main.startColor = Color.green;
        main.maxParticles = 50;
        
        var emission = successEffect.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 25),
            new ParticleSystem.Burst(0.5f, 15)
        });
        
        var shape = successEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.5f;
        
        var velocity = successEffect.velocityOverLifetime;
        velocity.enabled = true;
        velocity.speedModifier = new ParticleSystem.MinMaxCurve(1f, 3f);
        
        var color = successEffect.colorOverLifetime;
        color.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.green, 0.0f), 
                new GradientColorKey(Color.cyan, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        color.color = gradient;
        
        var renderer = successEffect.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(Color.green);
    }

    void CreateFailureEffect()
    {
        GameObject failureObj = new GameObject("FailureEffect");
        failureObj.transform.SetParent(transform);
        failureObj.transform.localPosition = Vector3.up * 0.3f;
        
        failureEffect = failureObj.AddComponent<ParticleSystem>();
        
        var main = failureEffect.main;
        main.duration = 2.0f;
        main.loop = false;
        main.playOnAwake = false;
        main.startSpeed = 2f;
        main.startLifetime = 1.5f;
        main.startSize = 0.08f;
        main.startColor = Color.gray;
        main.maxParticles = 30;
        
        var emission = failureEffect.emission;
        emission.rateOverTime = 0;
        emission.SetBursts(new ParticleSystem.Burst[] {
            new ParticleSystem.Burst(0f, 20)
        });
        
        var shape = failureEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;
        
        var velocity = failureEffect.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(-0.5f, -2f);
        
        var color = failureEffect.colorOverLifetime;
        color.enabled = true;
        
        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new GradientColorKey[] { 
                new GradientColorKey(Color.gray, 0.0f), 
                new GradientColorKey(Color.black, 1.0f) 
            },
            new GradientAlphaKey[] { 
                new GradientAlphaKey(1.0f, 0.0f), 
                new GradientAlphaKey(0.0f, 1.0f) 
            }
        );
        color.color = gradient;
        
        var renderer = failureEffect.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(Color.gray);
    }

    void CreateBubbleEffect()
    {
        GameObject bubbleObj = new GameObject("BubbleEffect");
        bubbleObj.transform.SetParent(transform);
        bubbleObj.transform.localPosition = Vector3.up * 0.2f;
        
        bubbleEffect = bubbleObj.AddComponent<ParticleSystem>();
        
        var main = bubbleEffect.main;
        main.loop = true;
        main.playOnAwake = true;
        main.startSpeed = 0.1f;
        main.startLifetime = 3f;
        main.startSize = 0.03f;
        main.startColor = new Color(1f, 1f, 1f, 0.3f);
        main.maxParticles = 20;
        
        var emission = bubbleEffect.emission;
        emission.rateOverTime = 5f;
        
        var shape = bubbleEffect.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        shape.radiusThickness = 0.1f;
        
        var velocity = bubbleEffect.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.2f, 0.5f);
        
        var size = bubbleEffect.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 0f),
            new Keyframe(0.3f, 1f),
            new Keyframe(0.7f, 1f),
            new Keyframe(1f, 0f)
        ));
        
        var renderer = bubbleEffect.GetComponent<ParticleSystemRenderer>();
        renderer.material = CreateParticleMaterial(new Color(1f, 1f, 1f, 0.5f));
    }

    Material CreateParticleMaterial(Color color)
    {
        Material mat = new Material(Shader.Find("Particles/Standard Unlit"));
        mat.color = color;
        return mat;
    }

    void Update()
    {
        if (isWorking && !hasFinishedBrewing)
        {
            currentTimer -= Time.deltaTime;
            UpdateTimerUI();
            UpdateBrewingVFX();

            if (currentTimer <= 0)
            {
                FinishBrewing();
            }
        }
        
        // Таймер головоломки
        if (puzzleActive)
        {
            puzzleTimer -= Time.deltaTime;
            UpdatePuzzleUI();
            
            if (puzzleTimer <= 0)
            {
                FailPuzzle();
            }
        }
        
        UpdateIdleVFX();
        
        // ДОБАВЛЕНО: Автоматическое создание VR кнопок если нужно
        if (!vrButtonsInitialized)
        {
            CreateVRButtonsIfNeeded();
            InitializeVRButtons();
        }
    }

    void UpdateIdleVFX()
    {
        if (!isWorking && bubbleEffect != null && !bubbleEffect.isPlaying)
        {
            bubbleEffect.Play();
        }
        
        if (cauldronLight != null && !isWorking && !puzzleActive)
        {
            float intensity = Mathf.PingPong(Time.time * 0.5f, 0.2f) + 0.1f;
            cauldronLight.intensity = intensity * vfxIntensity;
        }
    }

    void UpdateBrewingVFX()
    {
        if (!enableVFX) return;
        
        if (liquidMaterial != null)
        {
            float pulse = Mathf.PingPong(Time.time * 2f, 0.3f) + 0.5f;
            Color brewColor = Color.Lerp(Color.yellow, Color.red, currentTimer / brewTime);
            liquidMaterial.color = new Color(brewColor.r, brewColor.g, brewColor.b, 0.7f);
            liquidMaterial.SetColor("_EmissionColor", brewColor * pulse);
        }
        
        if (cauldronLight != null)
        {
            cauldronLight.intensity = Mathf.PingPong(Time.time * 3f, 1f) + 1f;
            cauldronLight.color = Color.Lerp(Color.yellow, Color.red, (brewTime - currentTimer) / brewTime);
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

        ObjectAnalysis analysis = AnalyzeObject(other.gameObject);
        analyzedObjects.Add(analysis);
        ShowObjectAnalysis(analysis);

        if (CanUseObject(other.gameObject) && currentIngredients.Count < 2)
        {
            ProcessObject(other.gameObject, analysis);
        }
        else if (currentIngredients.Count >= 2)
        {
            ShowHint("Котел полон! Начните варку или очистите котел.");
        }
        else
        {
            ShowHint($"Объект '{other.gameObject.name}' не подходит для алхимии");
        }
    }

    ObjectAnalysis AnalyzeObject(GameObject obj)
    {
        ObjectAnalysis analysis = new ObjectAnalysis();
        
        analysis.objectName = obj.name;
        analysis.objectTag = obj.tag;
        analysis.layerName = LayerMask.LayerToName(obj.layer);
        analysis.objectScale = obj.transform.localScale;
        
        analysis.hasRigidbody = obj.GetComponent<Rigidbody>() != null;
        analysis.hasRenderer = obj.GetComponent<Renderer>() != null;
        analysis.hasCollider = obj.GetComponent<Collider>() != null;
        
        Collider collider = obj.GetComponent<Collider>();
        if (collider != null)
        {
            analysis.colliderType = collider.GetType().Name;
        }
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            analysis.objectColor = renderer.material.color;
        }
        
        analysis.components = new List<string>();
        Component[] allComponents = obj.GetComponents<Component>();
        foreach (Component component in allComponents)
        {
            if (component != null)
            {
                analysis.components.Add(component.GetType().Name);
            }
        }
        
        Ingredient ingredient = obj.GetComponent<Ingredient>();
        analysis.isIngredient = ingredient != null;
        analysis.ingredientType = ingredient != null ? ingredient.GetIngredientName() : "Не ингредиент";
        
        return analysis;
    }

    bool CanUseObject(GameObject obj)
    {
        return acceptAllObjects;
    }

    void ProcessObject(GameObject obj, ObjectAnalysis analysis)
    {
        processedIngredients.Add(obj);
        
        string ingredientName = analysis.isIngredient ? analysis.ingredientType : analysis.objectName;
        currentIngredients.Add(ingredientName);

        // Проверяем головоломку
        if (puzzleActive)
        {
            CheckPuzzleSequence(ingredientName);
        }

        PlaySound(addSound);
        CreateAbsorptionEffect(obj.transform.position);
        
        if (destroyObjects)
        {
            Destroy(obj);
        }
        else
        {
            AbsorbObject(obj);
        }

        UpdateIngredientsUI();
        UpdateClearButton();
        UpdateVRButtonsVisibility(); // ДОБАВЛЕНО: Обновление VR кнопок
        
        string message = $"Добавлен: {ingredientName}\n";
        message += currentIngredients.Count == 1 ? "Нужен еще один объект" : "Готово к варке!";
        ShowMessage(message);

        ShowHint($"Объектов: {currentIngredients.Count}/2\nДобавьте еще один предмет");

        if (currentIngredients.Count >= 2 && !puzzleActive)
        {
            StartBrewing();
        }
    }

    void AbsorbObject(GameObject obj)
    {
        absorbedObjects.Add(obj);
        originalPositions.Add(obj.transform.position);
        originalRotations.Add(obj.transform.rotation);
        originalActiveStates.Add(obj.activeSelf);
        
        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            originalKinematicStates.Add(rb.isKinematic);
            rb.isKinematic = true;
        }
        else
        {
            originalKinematicStates.Add(false);
        }
        
        obj.SetActive(false);
        obj.transform.position = transform.position;
    }

    void RestoreAbsorbedObjects()
    {
        for (int i = 0; i < absorbedObjects.Count; i++)
        {
            if (absorbedObjects[i] != null)
            {
                absorbedObjects[i].transform.position = originalPositions[i];
                absorbedObjects[i].transform.rotation = originalRotations[i];

                Rigidbody rb = absorbedObjects[i].GetComponent<Rigidbody>();
                if (rb != null)
                {
                    rb.isKinematic = originalKinematicStates[i];
                }
                
                absorbedObjects[i].SetActive(originalActiveStates[i]);
            }
        }
        
        absorbedObjects.Clear();
        originalPositions.Clear();
        originalRotations.Clear();
        originalKinematicStates.Clear();
        originalActiveStates.Clear();
    }

    void CreateAbsorptionEffect(Vector3 position)
    {
        if (!enableVFX || absorptionEffect == null) return;
        
        absorptionEffect.transform.position = position;
        absorptionEffect.Stop();
        absorptionEffect.Play();
        
        CreateFlashEffect(position, new Color(0.3f, 1f, 0.3f));
    }

    void CreateFlashEffect(Vector3 position, Color color)
    {
        GameObject flash = new GameObject("FlashEffect");
        flash.transform.position = position;
        
        Light flashLight = flash.AddComponent<Light>();
        flashLight.type = LightType.Point;
        flashLight.range = 2f;
        flashLight.intensity = 3f * vfxIntensity;
        flashLight.color = color;
        
        Destroy(flash, 0.2f);
    }

    void ShowObjectAnalysis(ObjectAnalysis analysis)
    {
        if (analysisDisplay != null && showDetailedAnalysis)
        {
            string analysisText = $"<b>Анализ объекта:</b>\n";
            analysisText += $"Имя: {analysis.objectName}\n";
            analysisText += $"Тег: {analysis.objectTag}\n";
            analysisText += $"Слой: {analysis.layerName}\n";
            analysisText += $"Размер: {analysis.objectScale}\n";
            analysisText += $"Цвет: {analysis.objectColor}\n";
            analysisText += $"Тип: {(analysis.isIngredient ? "Ингредиент" : "Обычный объект")}\n";
            
            if (analysis.isIngredient)
            {
                analysisText += $"Тип ингредиента: {analysis.ingredientType}\n";
            }
            
            analysisText += $"\n<b>Компоненты:</b>\n";
            foreach (string component in analysis.components.Take(8))
            {
                analysisText += $"• {component}\n";
            }
            
            if (analysis.components.Count > 8)
            {
                analysisText += $"... и еще {analysis.components.Count - 8}\n";
            }
            
            analysisDisplay.text = analysisText;
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
        UpdateVRButtonsVisibility(); // ДОБАВЛЕНО: Обновление VR кнопок

        string combo = $"{currentIngredients[0]} + {currentIngredients[1]}";
        ShowMessage($"Варка началась!\nКомбинация: {combo}");
        ShowHint("Идет процесс варки... Ждите результата!");

        StartBrewingVFX();
        ShowBrewingAnalysis();
        PlaySound(brewSound);
    }

    void StartBrewingVFX()
    {
        if (!enableVFX) return;
        
        if (brewingEffect != null) brewingEffect.Play();
        if (bubbleEffect != null) bubbleEffect.Stop();
        if (cauldronLight != null)
        {
            cauldronLight.intensity = 1f;
            cauldronLight.color = Color.yellow;
        }
    }

    void ShowBrewingAnalysis()
    {
        if (analysisDisplay != null)
        {
            string analysisText = "<b>Анализ комбинации:</b>\n";
            
            foreach (ObjectAnalysis analysis in analyzedObjects)
            {
                analysisText += $"{analysis.objectName}:\n";
                analysisText += $"• Тег: {analysis.objectTag}\n";
                analysisText += $"• Слой: {analysis.layerName}\n";
                analysisText += $"• Компонентов: {analysis.components.Count}\n";
                analysisText += $"• Тип: {(analysis.isIngredient ? "Ингредиент" : "Объект")}\n\n";
            }
            
            analysisText += $"Всего проанализировано объектов: {analyzedObjects.Count}";
            analysisDisplay.text = analysisText;
        }
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

        if (!destroyObjects && foundRecipe == null)
        {
            StartCoroutine(RestoreObjectsAfterDelay(1f));
        }

        currentIngredients.Clear();
        processedIngredients.Clear();
        UpdateIngredientsUI();
        UpdateClearButton();
        UpdateVRButtonsVisibility(); // ДОБАВЛЕНО: Обновление VR кнопок
    }

    private IEnumerator RestoreObjectsAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        RestoreAbsorbedObjects();
        analyzedObjects.Clear();
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
            SetupPotionPhysics(newPotion);
            SetupXRInteractions(newPotion);
            StartCoroutine(PreventDuplicateInteractions(newPotion));
        }

        PlaySuccessVFX(recipe.potionColor);
        PlaySound(successSound);
        
        string successMessage = $"<color=green>УСПЕХ!</color>\n";
        successMessage += $"Создано: {recipe.potionName}\n";
        successMessage += $"{recipe.description}\n";
        successMessage += $"Удачных зелий: {successCount}/{totalAttempts}";
        
        ShowMessage(successMessage);
        ShowHint("Отличная работа! Зелье готово.");
        ShowFinalAnalysis(true, recipe.potionName);

        if (!destroyObjects)
        {
            absorbedObjects.Clear();
            originalPositions.Clear();
            originalRotations.Clear();
            originalKinematicStates.Clear();
            originalActiveStates.Clear();
        }

        StartCoroutine(ResetCauldronAfterDelay(3f));
    }

    void PlaySuccessVFX(Color potionColor)
    {
        if (!enableVFX) return;
        
        if (brewingEffect != null) brewingEffect.Stop();
        if (successEffect != null)
        {
            var main = successEffect.main;
            main.startColor = potionColor;
            successEffect.Play();
        }
        if (cauldronLight != null)
        {
            cauldronLight.color = potionColor;
            cauldronLight.intensity = 5f * vfxIntensity;
        }
        if (liquidMaterial != null)
        {
            liquidMaterial.color = new Color(potionColor.r, potionColor.g, potionColor.b, 0.8f);
            liquidMaterial.SetColor("_EmissionColor", potionColor * 2f);
        }
        CreateFlashEffect(transform.position + Vector3.up * 0.5f, potionColor);
    }

    void CreateFailedPotion()
    {
        if (spawnPoint != null)
        {
            GameObject failedItem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            failedItem.transform.position = spawnPoint.position;
            failedItem.transform.localScale = Vector3.one * 0.1f;
            failedItem.name = "Неудачное зелье";
            
            SetupPotionPhysics(failedItem);
            SetupXRInteractions(failedItem);

            Renderer rend = failedItem.GetComponent<Renderer>();
            rend.material.color = Color.gray;
            StartCoroutine(PreventDuplicateInteractions(failedItem));
        }

        PlayFailureVFX();
        PlaySound(failSound);
        
        string failMessage = $"<color=red>НЕУДАЧА</color>\n";
        failMessage += $"Неизвестная комбинация:\n";
        failMessage += $"{currentIngredients[0]} + {currentIngredients[1]}\n";
        failMessage += $"Удачных зелий: {successCount}/{totalAttempts}";
        
        ShowMessage(failMessage);
        ShowHint("Эта комбинация не работает. Попробуйте другие предметы!");
        ShowFinalAnalysis(false, "Неизвестное зелье");
        StartCoroutine(ResetCauldronAfterDelay(3f));
    }

    void PlayFailureVFX()
    {
        if (!enableVFX) return;
        
        if (brewingEffect != null) brewingEffect.Stop();
        if (failureEffect != null) failureEffect.Play();
        if (cauldronLight != null)
        {
            cauldronLight.color = Color.gray;
            cauldronLight.intensity = 3f * vfxIntensity;
        }
        if (liquidMaterial != null)
        {
            liquidMaterial.color = new Color(0.3f, 0.3f, 0.3f, 0.6f);
            liquidMaterial.SetColor("_EmissionColor", Color.gray);
        }
        CreateSmokeEffect();
    }

    void CreateSmokeEffect()
    {
        GameObject smoke = new GameObject("SmokeEffect");
        smoke.transform.position = transform.position + Vector3.up * 0.5f;
        
        ParticleSystem smokePS = smoke.AddComponent<ParticleSystem>();
        
        var main = smokePS.main;
        main.startSpeed = 0.5f;
        main.startLifetime = 3f;
        main.startSize = 0.1f;
        main.startColor = Color.gray;
        main.maxParticles = 20;
        
        var emission = smokePS.emission;
        emission.rateOverTime = 10f;
        
        var shape = smokePS.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.3f;
        
        var velocity = smokePS.velocityOverLifetime;
        velocity.enabled = true;
        velocity.y = new ParticleSystem.MinMaxCurve(0.5f, 1f);
        
        Destroy(smoke, 4f);
    }

    void SetupPotionPhysics(GameObject potionObject)
    {
        Rigidbody rb = potionObject.GetComponent<Rigidbody>();
        if (rb == null) rb = potionObject.AddComponent<Rigidbody>();
        rb.mass = 0.5f;
        rb.drag = 0.5f;
        rb.angularDrag = 0.5f;
        rb.isKinematic = false;

        Collider collider = potionObject.GetComponent<Collider>();
        if (collider == null)
        {
            MeshFilter meshFilter = potionObject.GetComponent<MeshFilter>();
            if (meshFilter != null && meshFilter.mesh != null)
            {
                MeshCollider meshCollider = potionObject.AddComponent<MeshCollider>();
                meshCollider.convex = true;
            }
            else
            {
                potionObject.AddComponent<BoxCollider>();
            }
        }
        
        Collider[] allColliders = potionObject.GetComponentsInChildren<Collider>();
        foreach (Collider col in allColliders) col.isTrigger = false;
    }

    void SetupXRInteractions(GameObject potionObject)
    {
        XRGrabInteractable grab = potionObject.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = potionObject.AddComponent<XRGrabInteractable>();
        grab.throwOnDetach = true;
        grab.attachEaseInTime = 0.1f;
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        potionObject.layer = LayerMask.NameToLayer("Interactable");
    }

    private IEnumerator PreventDuplicateInteractions(GameObject spawnedObject)
    {
        Collider[] colliders = spawnedObject.GetComponentsInChildren<Collider>(true);
        XRGrabInteractable grabInteractable = spawnedObject.GetComponent<XRGrabInteractable>();
        Rigidbody rb = spawnedObject.GetComponent<Rigidbody>();
        
        bool[] colliderStates = new bool[colliders.Length];
        for (int i = 0; i < colliders.Length; i++)
        {
            colliderStates[i] = colliders[i].enabled;
            colliders[i].enabled = false;
        }
        
        bool wasGrabEnabled = grabInteractable != null ? grabInteractable.enabled : false;
        bool wasKinematic = rb != null ? rb.isKinematic : false;
        
        if (grabInteractable != null) grabInteractable.enabled = false;
        if (rb != null) rb.isKinematic = true;
    
        yield return new WaitForSeconds(1.0f);
    
        for (int i = 0; i < colliders.Length; i++) colliders[i].enabled = colliderStates[i];
        if (grabInteractable != null) grabInteractable.enabled = wasGrabEnabled;
        if (rb != null) rb.isKinematic = wasKinematic;
    
        isSpawningPotion = false;
    }

    void ShowFinalAnalysis(bool success, string resultName)
    {
        if (analysisDisplay != null)
        {
            string analysisText = $"<b>ФИНАЛЬНЫЙ АНАЛИЗ:</b>\n";
            analysisText += $"Результат: {(success ? "УСПЕХ" : "НЕУДАЧА")}\n";
            analysisText += $"Создано: {resultName}\n\n";
            
            analysisText += $"<b>Использованные объекты:</b>\n";
            for (int i = 0; i < Mathf.Min(analyzedObjects.Count, currentIngredients.Count); i++)
            {
                var analysis = analyzedObjects[i];
                analysisText += $"{i+1}. {analysis.objectName}\n";
                analysisText += $"   Тег: {analysis.objectTag}\n";
                analysisText += $"   Слой: {analysis.layerName}\n";
                analysisText += $"   Компонентов: {analysis.components.Count}\n";
            }
            
            analysisText += $"\n<b>Статистика:</b>\n";
            analysisText += $"Всего объектов: {analyzedObjects.Count}\n";
            analysisText += $"Ингредиентов: {analyzedObjects.Count(a => a.isIngredient)}\n";
            analysisText += $"Обычных объектов: {analyzedObjects.Count(a => !a.isIngredient)}\n";
            analysisText += $"Успешных зелий: {successCount}\n";
            analysisText += $"Всего попыток: {totalAttempts}";
            
            analysisDisplay.text = analysisText;
        }
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
            
            if (!destroyObjects)
            {
                RestoreAbsorbedObjects();
            }
            
            currentIngredients.Clear();
            processedIngredients.Clear();
            analyzedObjects.Clear();
            UpdateIngredientsUI();
            UpdateClearButton();
            UpdateVRButtonsVisibility(); // ДОБАВЛЕНО: Обновление VR кнопок
            
            ShowMessage("Котел очищен!\nМожно начинать новый эксперимент");
            ShowHint("Котел пуст. Добавьте два ингредиента для варки.");

            if (analysisDisplay != null) analysisDisplay.text = "Готов к анализу объектов...";
            if (enableVFX) CreateFlashEffect(transform.position, Color.blue);
        }
        else ShowHint("Котел уже пуст! Добавьте ингредиенты для варки.");
    }

    void UpdateClearButton()
    {
        if (clearButton != null)
        {
            bool shouldShow = currentIngredients.Count > 0 && !isWorking && !isSpawningPotion && !puzzleActive;
            clearButton.SetActive(shouldShow);
        }
    }

    void UpdatePuzzleButton()
    {
        if (puzzleButton != null)
        {
            bool shouldShow = enablePuzzles && puzzles.Count > 0 && currentPuzzleIndex < puzzles.Count && !puzzleActive;
            puzzleButton.SetActive(shouldShow);
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

        if (enableVFX)
        {
            if (cauldronLight != null)
            {
                cauldronLight.color = Color.white;
                cauldronLight.intensity = 0.1f;
            }
            if (liquidMaterial != null)
            {
                liquidMaterial.color = new Color(0.3f, 0.3f, 0.8f, 0.7f);
                liquidMaterial.SetColor("_EmissionColor", Color.black);
            }
            if (bubbleEffect != null && !bubbleEffect.isPlaying)
                bubbleEffect.Play();
        }

        UpdateClearButton();
        UpdatePuzzleButton();
        UpdateVRButtonsVisibility(); // ДОБАВЛЕНО: Обновление VR кнопок
        ShowMessage("Котел готов к работе!\nБросьте 2 объекта для варки");
        ShowHint("Используйте разные комбинации объектов");

        if (analysisDisplay != null && !showDetailedAnalysis)
            analysisDisplay.text = "Бросьте объект в котел для анализа...";
    }

    void CheckSceneSetup()
    {
        GameObject floor = GameObject.FindWithTag("Floor");
        if (floor == null)
        {
            Debug.LogWarning("Не найден объект с тегом 'Floor'. Создаю автоматически...");
            CreateDefaultFloor();
        }

        XRInteractionManager xrManager = FindObjectOfType<XRInteractionManager>();
        if (xrManager == null) Debug.LogWarning("XR Interaction Manager не найден в сцене!");

        if (LayerMask.NameToLayer("Interactable") == -1)
            Debug.LogWarning("Слой 'Interactable' не создан. Создайте слой для интерактивных объектов.");
    }

    void CreateDefaultFloor()
    {
        GameObject floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
        floor.name = "DefaultFloor";
        floor.tag = "Floor";
        floor.transform.position = new Vector3(0, -1, 0);
        floor.transform.localScale = Vector3.one * 5f;
        
        PhysicMaterial physicMat = new PhysicMaterial();
        physicMat.bounciness = 0.1f;
        physicMat.dynamicFriction = 0.6f;
        physicMat.staticFriction = 0.6f;
        
        Collider floorCollider = floor.GetComponent<Collider>();
        floorCollider.material = physicMat;

        Renderer floorRenderer = floor.GetComponent<Renderer>();
        floorRenderer.material.color = new Color(0.3f, 0.2f, 0.1f);
    }

    void ShowWelcomeMessage()
    {
        string welcomeMsg = "УНИВЕРСАЛЬНАЯ АЛХИМИЧЕСКАЯ ЛАБОРАТОРИЯ\n\n";
        welcomeMsg += "Бросьте ЛЮБЫЕ два объекта в котел\n";
        welcomeMsg += "для создания зелий и анализа";
        
        if (enablePuzzles && puzzles.Count > 0)
        {
            welcomeMsg += $"\n\nДоступно головоломок: {puzzles.Count}";
        }
        
        ShowMessage(welcomeMsg);
        ShowHint("Бросьте 2 объекта в котел чтобы начать");

        if (analysisDisplay != null)
            analysisDisplay.text = "Ожидание объектов...\nКотел проанализирует любой брошенный предмет!";
    }

    void ShowPuzzleMessage(string text)
    {
        if (mainDisplay != null) 
        {
            mainDisplay.text = text;
        }
    }

    void ShowMessage(string text)
    {
        if (mainDisplay != null) mainDisplay.text = text;
    }

    void ShowHint(string text)
    {
        if (hintDisplay != null) hintDisplay.text = text;
    }

    void UpdateIngredientsUI()
    {
        if (ingredientsDisplay != null)
        {
            if (currentIngredients.Count == 0) ingredientsDisplay.text = "Объектов: 0/2";
            else
            {
                string text = "В котле:\n";
                foreach (string ingredient in currentIngredients) text += $"• {ingredient}\n";
                ingredientsDisplay.text = text;
            }
        }
    }

    void UpdateTimerUI()
    {
        if (timerDisplay != null) timerDisplay.text = $"Осталось: {currentTimer:0.0}с";
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && audioSource != null) audioSource.PlayOneShot(clip);
    }

    // Публичные методы для UI кнопок
    public void StartNextPuzzle()
    {
        if (currentPuzzleIndex < puzzles.Count && !puzzleActive && !isWorking && !isSpawningPotion)
        {
            StartPuzzle(currentPuzzleIndex);
            UpdatePuzzleButton();
            UpdateVRButtonsVisibility(); // ДОБАВЛЕНО: Обновление VR кнопок
        }
    }

    public void SkipCurrentPuzzle()
    {
        if (puzzleActive)
        {
            puzzleActive = false;
            currentPuzzleIndex++;
            if (currentPuzzleIndex < puzzles.Count)
            {
                StartPuzzle(currentPuzzleIndex);
            }
            else
            {
                CompleteAllPuzzles();
            }
            UpdatePuzzleButton();
            UpdateVRButtonsVisibility(); // ДОБАВЛЕНО: Обновление VR кнопок
        }
    }

    public void TogglePuzzleMode()
    {
        enablePuzzles = !enablePuzzles;
        if (enablePuzzles && !puzzleActive && currentPuzzleIndex < puzzles.Count)
        {
            StartPuzzle(currentPuzzleIndex);
        }
        else
        {
            puzzleActive = false;
            if (puzzlePanel != null) puzzlePanel.SetActive(false);
        }
        
        ShowHint($"Режим головоломок: {(enablePuzzles ? "ВКЛ" : "ВЫКЛ")}");
        UpdatePuzzleButton();
        UpdateVRButtonsVisibility(); // ДОБАВЛЕНО: Обновление VR кнопок
    }

    public void SetVFXIntensity(float intensity)
    {
        vfxIntensity = Mathf.Clamp01(intensity);
    }

    public void ToggleVFX()
    {
        enableVFX = !enableVFX;
        ShowHint($"VFX эффекты: {(enableVFX ? "ВКЛ" : "ВЫКЛ")}");
    }

    public void ToggleDestroyMode()
    {
        destroyObjects = !destroyObjects;
        ShowHint($"Режим уничтожения: {(destroyObjects ? "ВКЛ" : "ВЫКЛ")}");
    }

    void OnDrawGizmos()
    {
        if (spawnPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(spawnPoint.position, 0.1f);
            Gizmos.DrawIcon(spawnPoint.position, "spawn_icon", true);
        }

        Collider collider = GetComponent<Collider>();
        if (collider != null && collider.isTrigger)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.matrix = transform.localToWorldMatrix;
            
            if (collider is BoxCollider boxCollider) Gizmos.DrawCube(boxCollider.center, boxCollider.size);
            else if (collider is SphereCollider sphereCollider) Gizmos.DrawSphere(sphereCollider.center, sphereCollider.radius);
        }
        
        // ДОБАВЛЕНО: Отображение позиций VR кнопок
        Gizmos.color = Color.cyan;
        if (vrClearButton != null) Gizmos.DrawWireCube(vrClearButton.transform.position, new Vector3(0.3f, 0.1f, 0.05f));
        if (vrPuzzleButton != null) Gizmos.DrawWireCube(vrPuzzleButton.transform.position, new Vector3(0.3f, 0.1f, 0.05f));
        if (vrVFXButton != null) Gizmos.DrawWireCube(vrVFXButton.transform.position, new Vector3(0.3f, 0.1f, 0.05f));
        if (vrResetButton != null) Gizmos.DrawWireCube(vrResetButton.transform.position, new Vector3(0.3f, 0.1f, 0.05f));
    }
}