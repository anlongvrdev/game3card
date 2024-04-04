using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEngine.UI;

public class ChipBetMove : MonoBehaviour
{
    public static ChipBetMove instance = null;

    [SerializeField]
    public GameObject chipPrefab;

    [SerializeField]
    public List<Sprite> listChipSprite = new List<Sprite>();

    [SerializeField]
    public Transform targetPositionPlayer1;

    [SerializeField]
    public Transform targetPositionPlayer2;

    [HideInInspector]
    public List<int> listValueBets = new List<int> { 1000, 2000, 5000, 10000, 50000};

    private void Awake()
    {
        ChipBetMove.instance = this;
    }

    public void OnClickChip(int value)
    {
        GameObject newChip = InstantiateChipPlayer(value);
        ThreeCardView.instance.players[0].addChipBet(newChip);
        ThreeCardView.instance.players[0].setTotalBet(value);
        ThreeCardView.instance.players[0].setValueChange(value);
        ThreeCardView.instance.checkValidBets();
        StartCoroutine(DeleteTextChangeChip());
        //Debug.Log("Length:>>> "+ ThreeCardView.instance.players[0].GetLenChipBets());
    }

    private IEnumerator DeleteTextChangeChip()
    {
        yield return new WaitForSeconds(0.5f);
        foreach (PlayerView player in ThreeCardView.instance.players)
        {
            player.valueChipChange.text = "";
        }
    }

    public GameObject InstantiateChipPlayer(int value)
    {
        GameObject newChip = Instantiate(chipPrefab, transform);
        Image chipImage = newChip.GetComponentInChildren<Image>();
        Text valueChip = newChip.GetComponentInChildren<Text>();
        if (listValueBets.Contains(value))
        {
            chipImage.sprite = listChipSprite[listValueBets.IndexOf(value)];
        }
        else
        {
            chipImage.sprite = listChipSprite[0];
        }
        valueChip.text = ThreeCardView.instance.FormatMoney(value, true);

        switch (valueChip.text)
        {
            case "1K":
                valueChip.color = new Color32(38, 96, 29, 255);
                break;
            case "2K":
                valueChip.color = new Color32(73, 94, 171, 255);
                break;
            case "5K":
                valueChip.color = new Color32(154, 108, 15, 255);
                break;
            case "10K":
                valueChip.color = new Color32(120, 54, 120, 255);
                break;
            case "50K":
                valueChip.color = new Color32(137, 58, 46, 255);
                break;
        }
        return newChip;
    }
}
