using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class PlayerView : MonoBehaviour
{
    public static PlayerView instance = null;

    [SerializeField]
    public Text lbnamePlayer;

    [SerializeField]
    public Text lbChip;

    [SerializeField]
    public Text lbScore;

    [SerializeField]
    public Text lbTotalBet;

    [SerializeField]
    public Text valueChipChange;

    [SerializeField]
    public List<Card> cardPlayer = new List<Card>();

    [HideInInspector]
    public List<GameObject> chipBets = new List<GameObject>();

    [HideInInspector]
    public List<GameObject> saveChipBets = new List<GameObject>();

    [SerializeField]
    public GameObject chipContainer;

    [SerializeField]
    public GameObject chipDbContainer;

    private long agPlayer;

    [HideInInspector]
    private int score;

    [HideInInspector]
    public int totalBet = 0;

    [HideInInspector]
    public int totalChip = 0;

    private Vector3 saveBeginPosBet;

    private void Awake()
    {
        PlayerView.instance = this;
    }

    public void SetPlayerName(string name)
    {
        lbnamePlayer.text = name;
    }

    public void SetPlayerChip(int chip)
    {
        totalChip += chip;
        lbChip.text = ThreeCardView.instance.FormatMoney(totalChip);
    }

    public void SetTotalScorePlayer(string totalScore)
    {
        lbScore.text = "Score: " + totalScore;
    }

    public void setValueChange(int value)
    {
        valueChipChange.text = $"-{ThreeCardView.instance.FormatMoney(value)}";
        totalChip -= value;
        lbChip.text = ThreeCardView.instance.FormatMoney(totalChip);
    }


    public void setTotalBet(int valueBet)
    {
        totalBet += valueBet;
        lbTotalBet.text = ThreeCardView.instance.FormatMoney(totalBet);
    }

    public void addChipBet(GameObject chipBet)
    {
        chipBets.Add(chipBet);
        saveChipBets.Add(chipBet);

        if (chipBets.Count > 20)
        {
            GameObject chipToRemove = chipBets[0];
            chipBets.RemoveAt(0);
            Destroy(chipToRemove);
        }

        chipBet.transform.localPosition = transform.localPosition;
        Vector3 newTargetPosition = chipContainer.transform.localPosition;
        Vector3 newTargetPosition2 = chipContainer.transform.localPosition + new Vector3(45, 0, 0);

        for (int i = 0; i < chipBets.Count; i++)
        {
            GameObject chip = chipBets[i];
            if (i < 10)
            {
                chip.transform.DOLocalMove(newTargetPosition, 1.5f).SetEase(Ease.OutQuint);
                newTargetPosition += new Vector3(0, 5, 0);
            }
            else
            {
                chip.transform.DOLocalMove(newTargetPosition2, 1.5f).SetEase(Ease.OutQuint);
                newTargetPosition2 += new Vector3(0, 5, 0);
            }
            chip.transform.DOScale(Vector3.one * 0.4f, 0.4f);
        }
    }

    public void DoubleBet()
    {
        totalBet = ThreeCardView.instance.saveTotalBet * 2;
        lbTotalBet.text = ThreeCardView.instance.FormatMoney(totalBet);

        totalChip = ThreeCardView.instance.saveTotalChipPlayer - totalBet;
        lbChip.text = ThreeCardView.instance.FormatMoney(totalChip);

        GameObject newChip = ChipBetMove.instance.InstantiateChipPlayer(totalBet);
        addChipBet(newChip);

        ThreeCardView.instance.checkBets();


        Debug.Log(newChip.GetComponentInChildren<Text>().text);
    }

    public void ReBet()
    {
        totalBet = ThreeCardView.instance.saveTotalBet;
        lbTotalBet.text = ThreeCardView.instance.FormatMoney(totalBet);

        totalChip = ThreeCardView.instance.saveTotalChipPlayer - totalBet;
        lbChip.text = ThreeCardView.instance.FormatMoney(totalChip);

        ThreeCardView.instance.checkBets();
    }

    private List<Card.Suit> listSuits = new List<Card.Suit> { Card.Suit.Diamonds, Card.Suit.Hearts, Card.Suit.Spades, Card.Suit.Clubs };

    public Card compareCards(Card card1, Card card2)
    {
        Card cardMax = null;
        if (card1.cardValue != 1 && card2.cardValue != 1)
        {
            if (card1.cardValue > card2.cardValue)
            {
                cardMax = card1;
            }
            else if (card1.cardValue < card2.cardValue)
            {
                cardMax = card2;
            }
            else
            {
                Card.Suit suitCard1 = card1.cardSuit;
                Card.Suit suitCard2 = card2.cardSuit;

                int indCard1 = listSuits.IndexOf(suitCard1);
                int indCard2 = listSuits.IndexOf(suitCard2);

                if (indCard1 < indCard2)
                {
                    cardMax = card1;
                }
                else
                {
                    cardMax = card2;
                }
            }
        }
        else if (card1.cardValue == 1 && card2.cardValue != 1)
        {
            cardMax = card1;
        }
        else if (card1.cardValue != 1 && card2.cardValue == 1)
        {
            cardMax = card2;
        }
        else
        {
            Card.Suit suitCard1 = card1.cardSuit;
            Card.Suit suitCard2 = card2.cardSuit;

            int indCard1 = listSuits.IndexOf(suitCard1);
            int indCard2 = listSuits.IndexOf(suitCard2);

            if (indCard1 < indCard2)
            {
                cardMax = card1;
            }
            else
            {
                cardMax = card2;
            }
        }
        return cardMax;
    }
}
