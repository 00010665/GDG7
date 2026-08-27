using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    [Header("Player Data")]
    public int currentLevel = 1;
    public int currentXP = 0;
    public int currentGold = 0;
    public int currentHP = 100;
    public int maxHP = 100;
    public int currentAttackPower = 10;
    public int currentDefense = 5;

    [Header("Chosen Chef")]
    public string chosenChefId;
    public ChefData chosenChef;

    [Header("Unlocked Passives")]
    public List<PassiveAbilityData> unlockedPassives = new List<PassiveAbilityData>();

    [Header("Unlocked Foods")]
    public List<FoodData> unlockedFoods = new List<FoodData>();

    [Header("Selected Food (Weapon)")]
    public FoodData selectedFood;

    private bool isAttacking = false;
    private float attackCooldown = 0f;
    private float attackCooldownTime = 1f;

    void Awake()
    {
        LoadPlayerData();
    }

    void Start()
    {
        UpdateUI();
    }

    void Update()
    {
        if (attackCooldown > 0)
        {
            attackCooldown -= Time.deltaTime;
        }
    }

    public void Attack()
    {
        if (attackCooldown > 0)
        {
            return;
        }

        if (selectedFood == null)
        {
            Debug.LogWarning("No food selected for attack!");
            return;
        }

        isAttacking = true;
        attackCooldown = attackCooldownTime;

        // Apply damage
        int damage = selectedFood.damage * currentAttackPower;

        // Apply passive effects
        ApplyPassiveEffects(damage);

        // Apply food-specific effects
        ApplyFoodEffects(damage);

        // Apply chef-specific passive
        if (chosenChef != null)
        {
            ApplyChefPassive();
        }

        // Apply other unlocked passives
        foreach (var passive in unlockedPassives)
        {
            passive.ApplyEffect(this, damage);
        }

        isAttacking = false;
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = damage - currentDefense;
        if (actualDamage < 0) actualDamage = 0;

        currentHP -= actualDamage;
        currentHP = Mathf.Max(0, currentHP);

        UpdateUI();

        if (currentHP <= 0)
        {
            Death();
        }
    }

    public void Heal(int amount)
    {
        currentHP += amount;
        currentHP = Mathf.Min(maxHP, currentHP);
        UpdateUI();
    }

    public void AddXP(int amount)
    {
        currentXP += amount;
        UpdateUI();
    }

    public void AddGold(int amount)
    {
        currentGold += amount;
        UpdateUI();
    }

    public void UpgradePassive(string passiveId)
    {
        foreach (var passive in unlockedPassives)
        {
            if (passive.id == passiveId)
            {
                passive.level++;
                UpdateUI();
                return;
            }
        }
    }

    public void UnlockFood(string foodId)
    {
        foreach (var food in unlockedFoods)
        {
            if (food.id == foodId)
            {
                return;
            }
        }

        foreach (var foodData in GameManager.Instance.GetAvailableFoods())
        {
            if (foodData.id == foodId)
            {
                unlockedFoods.Add(foodData);
                UpdateUI();
                return;
            }
        }
    }

    public void SelectFood(string foodId)
    {
        foreach (var food in unlockedFoods)
        {
            if (food.id == foodId)
            {
                selectedFood = food;
                UpdateUI();
                return;
            }
        }
    }

    public void SetChef(string chefId)
    {
        foreach (var chef in GameManager.Instance.GetChefs())
        {
            if (chef.id == chefId)
            {
                chosenChef = chef;
                UpdateUI();
                return;
            }
        }
    }

    private void LoadPlayerData()
    {
        // Load player data from JSON
    }

    private void SavePlayerData()
    {
        // Save player data to JSON
    }

    private void UpdateUI()
    {
        // Update UI with player data
    }

    private void Death()
    {
        Debug.Log("Player died!");
        // Handle death
    }

    private void ApplyPassiveEffects(int damage)
    {
        // Apply passive effects
    }

    private void ApplyFoodEffects(int damage)
    {
        // Apply food-specific effects
    }

    private void ApplyChefPassive()
    {
        // Apply chef-specific passive
    }
}