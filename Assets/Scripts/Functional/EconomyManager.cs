using UnityEngine;
using System;
using TMPro;

public class EconomyManager : MonoBehaviour
{
    public static EconomyManager Instance { get; private set; }

    [Header("Money")]
    [SerializeField] private int money = 0;

    public int Money => money;

    public event Action<int> OnMoneyChanged;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    #region Public API

    public void AddMoney(int amount)
    {
        if (amount <= 0) return;

        money += amount;
        Notify();
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0) return true;

        if (money < amount)
            return false;

        money -= amount;
        Notify();
        return true;
    }

    public bool CanAfford(int amount)
    {
        return money >= amount;
    }

    public void SetMoney(int amount)
    {
        money = Mathf.Max(0, amount);
        Notify();
    }

    #endregion

    void Notify()
    {
        OnMoneyChanged?.Invoke(money);
    }
}