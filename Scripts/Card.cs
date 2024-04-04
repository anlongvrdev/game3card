using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    public enum Suit
    {
        Spades,//bích
        Hearts,//cơ
        Diamonds,//dô
        Clubs//tép
    }

    public Suit cardSuit;
    public Image cardSprite;
    public int cardValue;

    public Card(Suit suit, Image sprite, int value)
    {
        cardValue = value;
        cardSuit = suit;
        cardSprite = sprite;
    }

    public void SetCard(int index)
    {
        cardSprite.sprite = ThreeCardView.instance.GetSpriteCardID(index);
        cardValue = index % 13 + 1;
        if (index < 13)
        {
            cardSuit = Suit.Diamonds;
        }
        else if (index < 26)
        {
            cardSuit = Suit.Hearts;
        }
        else if (index < 39)
        {
            cardSuit = Suit.Clubs;
        }
        else
        {
            cardSuit = Suit.Spades;
        }
        //Debug.Log(cardSuit + " " + cardValue);
    }

    public int GetScore(int index)
    {
        if (index == 10 || index == 11 || index == 12 ||
            index == 23 || index == 24 || index == 25 ||
            index == 36 || index == 37 || index == 38 ||
            index == 49 || index == 50 || index == 51)
        {
            return 10;
        }
        return cardValue;
    }   
}
