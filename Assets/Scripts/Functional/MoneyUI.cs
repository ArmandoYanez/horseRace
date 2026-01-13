using MoreMountains.Feedbacks;
using UnityEngine;
using TMPro;

public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;
    public MMF_Player money;
    public MMF_Player moneyred;
    private int curretMoney;

    void Start()
    {
        UpdateText(EconomyManager.Instance.Money, false);
        EconomyManager.Instance.OnMoneyChanged += UpdateText;
    }

    void OnDestroy()
    {
        if (EconomyManager.Instance != null)
            EconomyManager.Instance.OnMoneyChanged -= UpdateText;
    }

    void UpdateText(int value, bool form)
    {
        if(form == false) money.PlayFeedbacks();
        else moneyred.PlayFeedbacks();
        curretMoney = value;
    }
    
    void UpdateTextBuy(int value, bool form)
    {
        moneyred.PlayFeedbacks();
        curretMoney = value;
    }

    public void updateFeelText()
    {
        moneyText.text = curretMoney.ToString()+"$";
    }
}