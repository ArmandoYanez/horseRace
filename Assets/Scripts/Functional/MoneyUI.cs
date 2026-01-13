using MoreMountains.Feedbacks;
using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    public MMF_Player money;
    private int curretMoney;

    void Start()
    {
        UpdateText(EconomyManager.Instance.Money);
        EconomyManager.Instance.OnMoneyChanged += UpdateText;
    }

    void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnMoneyChanged -= UpdateText;
    }

    void UpdateText(int value)
    {
        money.PlayFeedbacks();
        curretMoney = value;
    }

    public void updateFeelText()
    {
        moneyText.text = curretMoney.ToString()+"$";
    }
}