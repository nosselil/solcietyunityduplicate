using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class KaminoUserActivityPositionUI : MonoBehaviour
{
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI apyText;
    public Button actionButton;
    public TextMeshProUGUI dateText;

    private UserPosition position;
    private KaminoUserActivityModal parentModal;

    public void Setup(UserPosition pos, KaminoUserActivityModal modal)
    {
        position = pos;
        parentModal = modal;
        titleText.text = pos.symbol + (pos.isLend ? " (Lent)" : " (Borrowed)");
        amountText.text = pos.amount.ToString("F2");
        apyText.text = pos.apy + "%";
        dateText.text = pos.date;
        actionButton.GetComponentInChildren<TextMeshProUGUI>().text = pos.isLend ? "Withdraw" : "Repay";
        actionButton.onClick.RemoveAllListeners();
        if (pos.isLend)
            actionButton.onClick.AddListener(() => parentModal.OnWithdraw(pos));
        else
            actionButton.onClick.AddListener(() => parentModal.OnRepay(pos));
    }

    // Get the position data for this UI element
    public UserPosition GetPosition()
    {
        return position;
    }
}