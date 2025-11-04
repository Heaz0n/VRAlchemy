using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestSystem : MonoBehaviour
{
    [System.Serializable]
    public class Quest
    {
        public string questID;
        public string title;
        public string description;
        public QuestType type;
        public QuestStatus status;
        public List<QuestObjective> objectives;
        public List<QuestReward> rewards;
        public string startDialogue;
        public string completionDialogue;
        public string turnInNPC;
        public int requiredAlchemyLevel;
    }

    [System.Serializable]
    public class QuestObjective
    {
        public ObjectiveType type;
        public string targetPotionID;
        public int requiredAmount;
        public int currentAmount;
        public string targetIngredient;
        public string description;
    }

    [System.Serializable]
    public class QuestReward
    {
        public RewardType type;
        public int goldAmount;
        public string potionRecipeID;
        public string ingredientID;
        public string equipmentID;
        public int experiencePoints;
    }

    public enum QuestType { MainStory, SideQuest, Tutorial, Secret }
    public enum QuestStatus { NotStarted, InProgress, Completed, TurnedIn }
    public enum ObjectiveType { BrewPotion, CollectIngredient, DiscoverRecipe, TalkToNPC }
    public enum RewardType { Gold, Recipe, Ingredient, Equipment, Experience }

    [Header("Quest Data")]
    public List<Quest> allQuests = new List<Quest>();
    public Quest currentActiveQuest;
    
    [Header("UI References")]
    public GameObject questJournalUI;
    public TextMeshProUGUI questTitleText;
    public TextMeshProUGUI questDescriptionText;
    public TextMeshProUGUI objectiveText;
    public TextMeshProUGUI rewardText;
    public TextMeshProUGUI npcDialogueText;
    public Button acceptQuestButton;
    public Button completeQuestButton;
    public Button nextDialogueButton;
    
    [Header("Quest Indicators")]
    public GameObject newQuestIndicator;
    public GameObject questCompleteIndicator;

    // Ссылки на другие системы
    private SimpleCauldron cauldronSystem;
    
    // Упрощенные системы (заглушки)
    private int playerGold = 0;
    private int playerExperience = 0;
    private int playerLevel = 1;

    private Queue<string> currentDialogue = new Queue<string>();
    private bool isInDialogue = false;

    void Start()
    {
        cauldronSystem = FindObjectOfType<SimpleCauldron>();

        // Подписываемся на события котла
        if (cauldronSystem != null)
        {
            // Будем отслеживать создание зелий через публичный метод
        }

        InitializeStartingQuests();
        UpdateQuestUI();
        
        // Скрываем UI при старте
        if (questJournalUI != null)
            questJournalUI.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.J))
        {
            ToggleQuestJournal();
        }
    }

    void InitializeStartingQuests()
    {
        // Начальный обучающий квест
        Quest tutorialQuest = new Quest
        {
            questID = "TUTORIAL_01",
            title = "Первые шаги алхимика",
            description = "Староста гильдии алхимиков просит создать простое зелье лечения",
            type = QuestType.Tutorial,
            status = QuestStatus.NotStarted,
            startDialogue = "Приветствую, ученик! Я - староста гильдии алхимиков. Создай для меня простое зелье лечения из Красного кристалла и Синего цветка.",
            completionDialogue = "Отлично! Ты подаешь надежды. Продолжай в том же духе!",
            turnInNPC = "Староста Гильдии",
            requiredAlchemyLevel = 1,
            objectives = new List<QuestObjective>
            {
                new QuestObjective
                {
                    type = ObjectiveType.BrewPotion,
                    targetPotionID = "HealthPotion",
                    requiredAmount = 1,
                    currentAmount = 0,
                    description = "Создай зелье лечения"
                }
            },
            rewards = new List<QuestReward>
            {
                new QuestReward
                {
                    type = RewardType.Gold,
                    goldAmount = 50
                },
                new QuestReward
                {
                    type = RewardType.Experience,
                    experiencePoints = 100
                }
            }
        };

        allQuests.Add(tutorialQuest);
        currentActiveQuest = tutorialQuest;
        
        ShowNewQuestIndicator();
        StartDialogue(tutorialQuest.startDialogue);
    }

    // Публичный метод для вызова из других систем (например, из котла)
    public void OnPotionBrewed(string potionName)
    {
        if (currentActiveQuest == null || currentActiveQuest.status != QuestStatus.InProgress)
            return;

        foreach (var objective in currentActiveQuest.objectives)
        {
            if (objective.type == ObjectiveType.BrewPotion && 
                objective.targetPotionID == potionName)
            {
                objective.currentAmount++;
                UpdateQuestUI();
                
                if (IsQuestComplete())
                {
                    CompleteQuest();
                }
                else
                {
                    ShowHint($"Прогресс квеста: {objective.currentAmount}/{objective.requiredAmount}");
                }
            }
        }
    }

    public void StartDialogue(string dialogue)
    {
        if (questJournalUI != null)
            questJournalUI.SetActive(true);
            
        isInDialogue = true;
        
        string[] sentences = dialogue.Split('\n');
        currentDialogue.Clear();
        
        foreach (string sentence in sentences)
        {
            currentDialogue.Enqueue(sentence);
        }
        
        DisplayNextDialogue();
    }

    public void DisplayNextDialogue()
    {
        if (currentDialogue.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = currentDialogue.Dequeue();
        if (npcDialogueText != null)
            npcDialogueText.text = sentence;
    }

    void EndDialogue()
    {
        isInDialogue = false;
        if (currentActiveQuest.status == QuestStatus.NotStarted && acceptQuestButton != null)
        {
            acceptQuestButton.gameObject.SetActive(true);
        }
    }

    public void AcceptCurrentQuest()
    {
        if (currentActiveQuest != null && currentActiveQuest.status == QuestStatus.NotStarted)
        {
            currentActiveQuest.status = QuestStatus.InProgress;
            if (acceptQuestButton != null)
                acceptQuestButton.gameObject.SetActive(false);
                
            UpdateQuestUI();
            if (questJournalUI != null)
                questJournalUI.SetActive(false);
            
            ShowHint($"Принят квест: {currentActiveQuest.title}");
        }
    }

    bool IsQuestComplete()
    {
        if (currentActiveQuest == null) return false;

        foreach (var objective in currentActiveQuest.objectives)
        {
            if (objective.currentAmount < objective.requiredAmount)
                return false;
        }
        return true;
    }

    void CompleteQuest()
    {
        currentActiveQuest.status = QuestStatus.Completed;
        if (completeQuestButton != null)
            completeQuestButton.gameObject.SetActive(true);
            
        ShowQuestCompleteIndicator();
        
        ShowHint($"Квест завершен: {currentActiveQuest.title}");
        StartDialogue(currentActiveQuest.completionDialogue);
    }

    public void TurnInQuest()
    {
        if (currentActiveQuest != null && currentActiveQuest.status == QuestStatus.Completed)
        {
            // Выдача наград
            GiveRewards(currentActiveQuest.rewards);
            
            currentActiveQuest.status = QuestStatus.TurnedIn;
            if (completeQuestButton != null)
                completeQuestButton.gameObject.SetActive(false);
            
            ShowHint($"Награды получены за: {currentActiveQuest.title}");
            
            // Следующий квест
            StartNextQuest();
        }
    }

    void GiveRewards(List<QuestReward> rewards)
    {
        foreach (var reward in rewards)
        {
            switch (reward.type)
            {
                case RewardType.Gold:
                    playerGold += reward.goldAmount;
                    ShowHint($"Получено золото: {reward.goldAmount} (Всего: {playerGold})");
                    break;
                case RewardType.Experience:
                    playerExperience += reward.experiencePoints;
                    ShowHint($"Получено опыта: {reward.experiencePoints} (Всего: {playerExperience})");
                    break;
                case RewardType.Recipe:
                    ShowHint($"Открыт новый рецепт: {reward.potionRecipeID}");
                    break;
            }
        }
    }

    void StartNextQuest()
    {
        Quest nextQuest = FindNextAvailableQuest();
        if (nextQuest != null)
        {
            currentActiveQuest = nextQuest;
            currentActiveQuest.status = QuestStatus.NotStarted;
            ShowNewQuestIndicator();
            StartDialogue(currentActiveQuest.startDialogue);
        }
        else
        {
            currentActiveQuest = null;
            ShowHint("Все доступные квесты завершены! Проверьте журнал позже.");
        }
    }

    Quest FindNextAvailableQuest()
    {
        foreach (var quest in allQuests)
        {
            if (quest.status == QuestStatus.NotStarted && 
                quest.requiredAlchemyLevel <= playerLevel)
            {
                return quest;
            }
        }
        return null;
    }

    void UpdateQuestUI()
    {
        if (questTitleText == null) return;

        if (currentActiveQuest == null)
        {
            questTitleText.text = "Нет активных квестов";
            questDescriptionText.text = "Отдохните или посетите гильдию для получения новых заданий";
            objectiveText.text = "";
            rewardText.text = "";
            return;
        }

        questTitleText.text = currentActiveQuest.title;
        questDescriptionText.text = currentActiveQuest.description;
        
        // Обновление целей
        objectiveText.text = "<b>Цели:</b>\n";
        foreach (var objective in currentActiveQuest.objectives)
        {
            string progress = $"{objective.currentAmount}/{objective.requiredAmount}";
            objectiveText.text += $"- {objective.description} [{progress}]\n";
        }
        
        // Обновление наград
        rewardText.text = "<b>Награды:</b>\n";
        foreach (var reward in currentActiveQuest.rewards)
        {
            switch (reward.type)
            {
                case RewardType.Gold:
                    rewardText.text += $"- Золото: {reward.goldAmount}\n";
                    break;
                case RewardType.Experience:
                    rewardText.text += $"- Опыт: {reward.experiencePoints}\n";
                    break;
                case RewardType.Recipe:
                    rewardText.text += $"- Новый рецепт\n";
                    break;
            }
        }
    }

    public void ToggleQuestJournal()
    {
        if (questJournalUI != null)
        {
            bool newState = !questJournalUI.activeInHierarchy;
            questJournalUI.SetActive(newState);
            
            if (newState)
            {
                UpdateQuestUI();
            }
        }
    }

    void ShowNewQuestIndicator()
    {
        if (newQuestIndicator != null)
        {
            newQuestIndicator.SetActive(true);
            Invoke("HideNewQuestIndicator", 5f);
        }
    }

    void HideNewQuestIndicator()
    {
        if (newQuestIndicator != null)
            newQuestIndicator.SetActive(false);
    }

    void ShowQuestCompleteIndicator()
    {
        if (questCompleteIndicator != null)
        {
            questCompleteIndicator.SetActive(true);
            Invoke("HideQuestCompleteIndicator", 5f);
        }
    }

    void HideQuestCompleteIndicator()
    {
        if (questCompleteIndicator != null)
            questCompleteIndicator.SetActive(false);
    }

    void ShowHint(string message)
    {
        Debug.Log($"Подсказка квеста: {message}");
        
        // Можно интегрировать с существующей системой подсказок из SimpleCauldron
        SimpleCauldron cauldron = FindObjectOfType<SimpleCauldron>();
        if (cauldron != null)
        {
            // Вызываем метод показа подсказки, если он есть
            // cauldron.ShowHint(message);
        }
    }

    // Методы для добавления квестов извне
    public void AddQuest(Quest newQuest)
    {
        allQuests.Add(newQuest);
    }

    public Quest GetQuestByID(string questID)
    {
        return allQuests.Find(q => q.questID == questID);
    }

    // Публичные методы для получения информации о прогрессе
    public int GetPlayerGold() => playerGold;
    public int GetPlayerExperience() => playerExperience;
    public int GetPlayerLevel() => playerLevel;
}