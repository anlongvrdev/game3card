using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChipView : MonoBehaviour
{
    [SerializeField]
    private Text lbBet;

    public void SetBetScore(int value)
    {
        lbBet.text = ThreeCardView.instance.FormatMoney(value, true);
    }
}
