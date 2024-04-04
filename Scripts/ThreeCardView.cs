using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public enum GAME_STATE
{
    WAITING,
    PLAYING,
    FINISH,
    REPLAY
}

public class ThreeCardView : MonoBehaviour
{
    public static ThreeCardView instance = null;

    [SerializeField]
    public List<Sprite> cardImages = new List<Sprite>();

    [SerializeField]
    public GameObject cardPrefab;

    [SerializeField]
    public ChipBetMove chipBetControl;

    [SerializeField]
    public List<PlayerView> players = new List<PlayerView>();

    [SerializeField]
    private List<ChipView> chips = new List<ChipView>();

    [SerializeField]
    public List<Button> BtnChipBet = new List<Button>();

    [SerializeField]
    public List<Button> listButtonRsAndDb = new List<Button>();

    [HideInInspector]
    public GAME_STATE GameState = GAME_STATE.WAITING;

    [SerializeField]
    private Image uiFill;

    [SerializeField]
    private Text uiText;

    [HideInInspector]
    public int maxVisibleChips = 20;

    public Sprite cardSprite;

    public Transform targetPosition1;

    public Transform targetPosition2;

    private int cardCount = 0;

    private int duration_bet = 10;

    private int duration_deal = 5;

    private int duration_waiting = 5;

    private int duration_replay = 5;

    private int totalScorePlayer1 = 0;

    private int totalScorePlayer2 = 0;

    private bool isPlayer2Active = false;

    private string checkBtn = "";

    [HideInInspector]
    public Text textGameState;

    private List<int> listValueBets = new List<int> { 1000, 2000, 5000, 10000, 50000 };

    [HideInInspector]
    public int saveTotalChipBot = 0;

    [HideInInspector]
    public int saveTotalChipPlayer = 0;

    private Vector3 savePosCardBeginPl1;

    private Vector3 savePosCardBeginPl2;

    [HideInInspector]
    public int saveTotalBet = 0;

    private void Awake()
    {
        ThreeCardView.instance = this;
    }

    private void initPosCardPlayers()
    {
        cardCount = 0;
        savePosCardBeginPl1 = targetPosition1.localPosition;
        savePosCardBeginPl2 = targetPosition2.localPosition;
    }

    private void Start()
    {
        SetPlayerInfo();
        SetChipInfo();
        initPosCardPlayers();
        StartCoroutine(CountToStartGame());
    }

    public void checkContainsChips()
    {
        if (players[0].saveChipBets.Count == 0)
        {
            listButtonRsAndDb[0].interactable = false;
            listButtonRsAndDb[1].interactable = false;

        }
        else
        {
            listButtonRsAndDb[0].interactable = true;
            listButtonRsAndDb[1].interactable = true;
        }
    }

    private IEnumerator CountToStartGame()
    {
        textGameState.text = "Đợi người chơi";
        GameState = GAME_STATE.WAITING;

        foreach (Button btn in BtnChipBet)
        {
            btn.interactable = false;
        }

        foreach (Button btn in listButtonRsAndDb)
        {
            btn.interactable = false;
        }

        int remainingDuration = duration_waiting;

        while (remainingDuration >= 0)
        {
            uiText.text = $"{remainingDuration % 60:00}";
            uiFill.fillAmount = Mathf.InverseLerp(0, duration_waiting, remainingDuration);
            remainingDuration--;
            yield return new WaitForSeconds(1f);
        }
        StartCoroutine(StartTimerAfterWaitingGame());
        yield return new WaitForSeconds(1f);
        AutoClickChips();
    }

    private IEnumerator StartTimerAfterWaitingGame()
    {
        textGameState.text = "Đặt cược";
        GameState = GAME_STATE.PLAYING;

        foreach (Button btn in BtnChipBet)
        {
            btn.interactable = true;
        }

        foreach (Button btn in listButtonRsAndDb)
        {
            btn.interactable = false;
        }

        int remainingDuration = duration_bet;
        while (remainingDuration >= 0)
        {
            uiText.text = $"{remainingDuration % 60:00}";
            uiFill.fillAmount = Mathf.InverseLerp(0, duration_bet, remainingDuration);
            remainingDuration--;
            yield return new WaitForSeconds(1f);
        }

        StopAutoClickChips();

        foreach (PlayerView player in players)
        {
            player.valueChipChange.text = "";
        }

        yield return new WaitForSeconds(1f);
        OnDealButtonClicked();
    }

    private void StopAutoClickChips()
    {
        isPlayer2Active = false;
    }

    public Sprite GetSpriteCardID(int id)
    {
        return cardImages[id];
    }

    public void OnDealButtonClicked()
    {
        DealCardsCoroutine();
        StartCoroutine(StartTimerAfterDelayDealCard());
    }

    private IEnumerator StartTimerAfterDelayDealCard()
    {
        textGameState.text = "";
        GameState = GAME_STATE.FINISH;

        foreach (Button btn in BtnChipBet)
        {
            btn.interactable = false;
        }

        foreach (Button btn in listButtonRsAndDb)
        {
            btn.interactable = false;
        }

        yield return new WaitForSeconds(2.5f);
        int remainingDuration = duration_deal;
        while (remainingDuration >= 0)
        {
            uiText.text = $"{remainingDuration % 60:00}";
            uiFill.fillAmount = Mathf.InverseLerp(0, duration_deal, remainingDuration);
            remainingDuration--;
            yield return new WaitForSeconds(1f);
        }

        OnFlipButtonClicked();

        yield return new WaitForSeconds(1f);
        compareResults();
        UpdateTotalChips();
        MoveChipAfterFinish();

        yield return new WaitForSeconds(5f);
        NewGame();
    }

    private void DealCardsCoroutine()
    {
        if (cardCount == 6)
        {
            return;
        }

        for (int i = 0; i < 6; i++)
        {
            DOTween.Sequence()
                .AppendInterval(0.3f * i)
                .AppendCallback(() =>
                {
                    GameObject cardObject = Instantiate(cardPrefab, transform);
                    Image cardRenderer = cardObject.GetComponentInChildren<Image>();
                    cardRenderer.sprite = cardSprite;

                    if (cardCount % 2 == 0)
                    {
                        cardObject.transform.DOLocalMove(savePosCardBeginPl1, 1.0f).SetEase(Ease.OutQuint);
                        savePosCardBeginPl1 += new Vector3(25, 0, 0);
                        players[0].cardPlayer.Add(cardObject.GetComponent<Card>());
                    }
                    else
                    {
                        cardObject.transform.DOLocalMove(savePosCardBeginPl2, 1.0f).SetEase(Ease.OutQuint);
                        savePosCardBeginPl2 += new Vector3(25, 0, 0);
                        players[1].cardPlayer.Add(cardObject.GetComponent<Card>());
                    }
                    cardCount++;
                });
        }
    }

    private Card FindCardMax1()
    {
        Card cardMax = players[0].cardPlayer[0];
        for (int i = 1; i < players[0].cardPlayer.Count; i++)
        {
            cardMax = players[0].compareCards(cardMax, players[0].cardPlayer[i - 1]);
        }

        return cardMax;
    }

    private Card FindCardMax2()
    {
        Card cardMax = players[1].cardPlayer[0];
        for (int i = 1; i < players[1].cardPlayer.Count; i++)
        {
            cardMax = players[1].compareCards(cardMax, players[1].cardPlayer[i - 1]);
        }
        return cardMax;
    }

    private void compareResults()
    {
        if (totalScorePlayer1 > totalScorePlayer2)
        {
            textGameState.text = "BẠN THẮNG";
            textGameState.color = Color.green;
        }
        else if (totalScorePlayer1 < totalScorePlayer2)
        {
            textGameState.text = "BẠN THUA";
            textGameState.color = Color.red;
        }
        else
        {
            Card cardMax = PlayerView.instance.compareCards(FindCardMax1(), FindCardMax2());
            if (cardMax == FindCardMax1())
            {
                textGameState.text = "BẠN THẮNG";
                textGameState.color = Color.green;
            }
            else
            {
                textGameState.text = "BẠN THUA";
                textGameState.color = Color.red;
            }
            //Debug.Log(cardMax.cardValue + " " + cardMax.cardSuit);
        }
    }

    public void OnFlipButtonClicked()
    {
        flipCards();
        SetTotalScorePlayer();
        Debug.Log(FindCardMax1().cardValue + " " + FindCardMax1().cardSuit);
        Debug.Log(FindCardMax2().cardValue + " " + FindCardMax2().cardSuit);
    }

    private void flipCards()
    {
        List<int> allCards = new List<int>();

        for (int i = 0; i < 52; i++)
        {
            allCards.Add(i);
        }

        List<int> flipCardPlayer1 = new List<int>();
        List<int> flipCardPlayer2 = new List<int>();

        for (int i = 0; i < 3; i++)
        {
            int randomIndx = UnityEngine.Random.Range(0, allCards.Count);
            int randomCard = allCards[randomIndx];
            flipCardPlayer1.Add(randomCard);
            allCards.RemoveAt(randomIndx);
        }

        for (int i = 0; i < 3; i++)
        {
            int randomIndx = UnityEngine.Random.Range(0, allCards.Count);
            int randomCard = allCards[randomIndx];
            flipCardPlayer2.Add(randomCard);
            allCards.RemoveAt(randomIndx);
        }

        /* Debug.Log("Player 1 cards: " + string.Join(", ", player1));
         Debug.Log("Player 2 cards: " + string.Join(", ", player2));*/

        for (int i = 0; i < flipCardPlayer1.Count; i++)
        {
            int cardIndex = flipCardPlayer1[i];
            Card card = players[0].cardPlayer[i].GetComponent<Card>();
            card.SetCard(cardIndex);
            totalScorePlayer1 += card.GetScore(cardIndex);
            card.transform.localScale = new Vector3(0f, 0.5f, 1f);

            card.transform.DOScaleX(0.5f, 0.5f);

        }

        for (int i = 0; i < flipCardPlayer2.Count; i++)
        {
            int cardIndex = flipCardPlayer2[i];
            Card card = players[1].cardPlayer[i].GetComponent<Card>();
            card.SetCard(cardIndex);
            totalScorePlayer2 += card.GetScore(cardIndex);
            card.transform.localScale = new Vector3(0f, 0.5f, 1f);

            card.transform.DOScaleX(0.5f, 0.5f);
        }

        /*Debug.Log(totalScorePlayer1);
        Debug.Log(totalScorePlayer2);*/

    }

    private void SetTotalScorePlayer()
    {
        if (totalScorePlayer1 > 10)
        {
            if (totalScorePlayer1 % 10 == 0)
            {
                totalScorePlayer1 = 10;
            }
            else
            {
                totalScorePlayer1 = totalScorePlayer1 % 10;
            }
        }

        if (totalScorePlayer2 > 10)
        {
            if (totalScorePlayer2 % 10 == 0)
            {
                totalScorePlayer2 = 10;
            }
            else
            {
                totalScorePlayer2 = totalScorePlayer2 % 10;
            }
        }
        players[0].SetTotalScorePlayer(totalScorePlayer1.ToString());
        players[1].SetTotalScorePlayer(totalScorePlayer2.ToString());
    }


    private void UpdateTotalChips()
    {
        if (textGameState.text.Equals("BẠN THẮNG"))
        {
            int totalChipPl1 = players[0].totalChip + players[0].totalBet + players[1].totalBet;
            saveTotalChipPlayer = totalChipPl1;
            saveTotalChipBot = players[1].totalChip;
            saveTotalBet = players[0].totalBet;
            players[0].valueChipChange.text = $"+{FormatMoney(players[0].totalBet + players[1].totalBet)}";
            players[0].valueChipChange.color = Color.green;
            players[1].valueChipChange.text = $"-{FormatMoney(players[1].totalBet)}";
            players[1].valueChipChange.color = Color.red;
            players[0].lbChip.text = FormatMoney(totalChipPl1);
            players[1].lbChip.text = FormatMoney(players[1].totalChip);
        }
        else if (textGameState.text.Equals("BẠN THUA"))
        {
            int totalChipPl2 = players[1].totalChip + players[0].totalBet + players[1].totalBet;
            saveTotalChipBot = totalChipPl2;
            saveTotalChipPlayer = players[0].totalChip;
            saveTotalBet = players[0].totalBet;
            players[1].valueChipChange.text = $"+{FormatMoney(players[0].totalBet + players[1].totalBet)}";
            players[1].valueChipChange.color = Color.green;
            players[0].valueChipChange.text = $"-{FormatMoney(players[0].totalBet)}";
            players[0].valueChipChange.color = Color.red;
            players[1].lbChip.text = FormatMoney(totalChipPl2);
            players[0].lbChip.text = FormatMoney(players[0].totalChip);
        }
    }

    public void MoveChipAfterFinish()
    {

        StartCoroutine(MoveToPlayerWin());
    }

    private IEnumerator MoveToPlayerWin()
    {
        List<GameObject> listChipWinPlayer = new List<GameObject>();
        List<GameObject> listChipWinBot = new List<GameObject>();
        if (textGameState.text.Equals("BẠN THẮNG"))
        {
            foreach (GameObject chipbet in players[1].chipBets)
            {
                chipbet.transform.DOLocalMove(cardPrefab.transform.localPosition, 0.5f).SetEase(Ease.OutQuint);
                yield return new WaitForSeconds(0.2f);
                chipbet.SetActive(false);
                listChipWinPlayer.Add(chipbet);
            }

            yield return new WaitForSeconds(0.5f);

            foreach (GameObject chipbet in listChipWinPlayer)
            {
                chipbet.SetActive(true);
                chipbet.transform.DOLocalMove(ChipBetMove.instance.targetPositionPlayer1.localPosition, 0.2f).SetEase(Ease.OutQuint);

                yield return new WaitForSeconds(0.5f);
                chipbet.SetActive(false);
            }

        }
        else if (textGameState.text.Equals("BẠN THUA"))
        {
            foreach (GameObject chipbet in players[0].chipBets)
            {
                chipbet.transform.DOLocalMove(cardPrefab.transform.localPosition, 0.5f).SetEase(Ease.OutQuint);
                yield return new WaitForSeconds(0.2f);
                chipbet.SetActive(false);
                listChipWinBot.Add(chipbet);
            }

            yield return new WaitForSeconds(0.5f);

            foreach (GameObject chipbet in listChipWinBot)
            {
                chipbet.SetActive(true);
                chipbet.transform.DOLocalMove(ChipBetMove.instance.targetPositionPlayer2.localPosition, 0.2f).SetEase(Ease.OutQuint);

                yield return new WaitForSeconds(0.5f);
                chipbet.SetActive(false);
            }
        }
    }

    private void AutoClickChips()
    {
        isPlayer2Active = true;
        StartCoroutine(BotAutoBets());
    }

    private IEnumerator BotAutoBets()
    {
        while (isPlayer2Active)
        {
            int randomValue = listValueBets[UnityEngine.Random.Range(0, listValueBets.Count)];
            // Debug.Log(":>>>>>>>>>>.randomValue=" + randomValue);
            GameObject chip = chipBetControl.InstantiateChipPlayer(randomValue);
            players[1].setTotalBet(randomValue);
            players[1].addChipBet(chip);
            players[1].setValueChange(randomValue);
            players[1].valueChipChange.color = Color.red;
            //checkBets();
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 3f));
        }
    }

    public void clickResetBet()
    {
        players[0].ReBet();
        listButtonRsAndDb[1].interactable = false;
        DisableClick(listButtonRsAndDb[0]);
    }

    public void clickDoubeBet()
    {
        players[0].DoubleBet();
        listButtonRsAndDb[0].interactable = false;
        DisableClick(listButtonRsAndDb[1]);
    }

    public void DisableClick(Button btn)
    {
        btn.interactable = false;
    }

    public void checkValidBets()
    {
        foreach (Button btn in BtnChipBet)
        {
            Text textChipBet = btn.GetComponentInChildren<Text>();
            if (players[0].totalChip < ConvertStringToNumber(textChipBet.text))
            {
                btn.interactable = false;
            }
            else
            {
                btn.interactable = true;
            }
        }
    }

    public void checkBets()
    {
        if (saveTotalChipPlayer >= saveTotalBet && saveTotalChipPlayer < saveTotalBet * 2)
        {
            listButtonRsAndDb[0].interactable = true;
            listButtonRsAndDb[1].interactable = false;
        }
        else if (saveTotalChipPlayer >= saveTotalBet * 2)
        {
            listButtonRsAndDb[0].interactable = true;
            listButtonRsAndDb[1].interactable = true;
        }
        else
        {
            listButtonRsAndDb[0].interactable = false;
            listButtonRsAndDb[1].interactable = false;
        }
    }

    public int ConvertStringToNumber(string input)
    {
        string numberString = input.Substring(0, input.Length - 1);
        int number = int.Parse(numberString);

        char unit = input[input.Length - 1];
        int multiplier = GetMultiplier(unit);

        return number * multiplier;
    }

    public int GetMultiplier(char unit)
    {
        switch (unit)
        {
            case 'K':
                return 1000;
            case 'M':
                return 1000000;
            case 'B':
                return 1000000000;
            default:
                return 1;
        }
    }

    private void SetPlayerInfo()
    {
        players[0].SetPlayerName("Player 1");
        players[0].SetPlayerChip(1000000);

        players[1].SetPlayerName("Player 2");
        players[1].SetPlayerChip(100000000);
    }

    private void SetChipInfo()
    {
        chips[0].SetBetScore(1000);
        chips[1].SetBetScore(2000);
        chips[2].SetBetScore(5000);
        chips[3].SetBetScore(10000);
        chips[4].SetBetScore(50000);
    }

    public string FormatMoney(int money, bool isK = false)
    {
        var isNeg = money < 0 ? true : false;
        var format = "";
        var mo = System.Math.Abs(money);
        money = mo;
        int du = 0;
        if (mo >= 1000000000)
        {
            mo /= 1000000000;
            du = money - mo * 1000000000;
            format = "B";
        }
        else if (mo >= 1000000)
        {
            mo /= 1000000;
            format = "M";
            du = money - mo * 1000000;
        }
        else
        {
            if (isK)
            {
                if (mo >= 1000)
                {
                    mo /= 1000;
                    du = money - mo * 1000;
                    format = "K";
                }
                else
                {
                    return FormatNumber(money);
                }
            }
            else
                return (isNeg == true ? "-" : "") + FormatNumber(money);
        }

        var neg = (isNeg == true ? "-" : "");
        if (du != 0)
        {
            var strDu = du.ToString();
            if (strDu.Length > 2)
            {
                strDu = strDu.Substring(0, 2);
                if (strDu[1] == '0')
                {
                    strDu.Remove(1);
                }
            }
            else
            {
                strDu = strDu.Substring(0, 1);
            }

            return (neg + mo + "." + strDu + format.ToUpper());
        }
        else
            return (neg + mo + format.ToUpper());
    }

    public static string FormatNumber(int number)
    {
        //return o.toString().replace(/\B(?= (\d{ 3})+(? !\d))/ g, ",")
        return String.Format("{0:n0}", number);
    }

    public void NewGame()
    {
        if (saveTotalChipPlayer > 0)
        {
            StartCoroutine(ReplayGameAfterTime());
        }
        else
        {
            checkOutOfMoney();
        }
    }

    private IEnumerator ReplayGameAfterTime()
    {
        textGameState.text = "Chuẩn bị ván mới";
        textGameState.color = Color.white;
        GameState = GAME_STATE.REPLAY;

        players[0].totalChip = saveTotalChipPlayer;
        players[0].lbChip.text = FormatMoney(players[0].totalChip);
        players[0].totalBet = 0;
        players[0].lbTotalBet.text = FormatMoney(players[0].totalBet);
        players[1].totalBet = 0;
        players[1].totalChip = saveTotalChipBot;
        players[1].lbChip.text = FormatMoney(players[1].totalChip);
        players[1].lbTotalBet.text = FormatMoney(players[1].totalBet);

        ClearChipBetAndCard();
        initPosCardPlayers();

        int remainingDuration = duration_replay;
        while (remainingDuration >= 0)
        {
            uiText.text = $"{remainingDuration % 60:00}";
            uiFill.fillAmount = Mathf.InverseLerp(0, duration_replay, remainingDuration);
            remainingDuration--;
            yield return new WaitForSeconds(1f);
        }

        StartCoroutine(StartTimerAfterWaitingGame());
        checkContainsChips();
        checkBets();
        yield return new WaitForSeconds(1f);
        AutoClickChips();
    }

    private void ClearChipBetAndCard()
    {
        foreach (GameObject chipbet in players[0].chipBets)
        {
            Destroy(chipbet);
        }
        players[0].chipBets.Clear();

        foreach (GameObject chipbet in players[1].chipBets)
        {
            Destroy(chipbet);
        }
        players[1].chipBets.Clear();

        foreach (Card card in players[0].cardPlayer)
        {
            Destroy(card.gameObject);
        }
        players[0].cardPlayer.Clear();

        foreach (Card card in players[1].cardPlayer)
        {
            Destroy(card.gameObject);
        }
        players[1].cardPlayer.Clear();

        players[0].lbScore.text = "";
        players[1].lbScore.text = "";
        players[0].valueChipChange.text = "";
        players[1].valueChipChange.text = "";
    }

    public void checkOutOfMoney()
    {
        if (saveTotalChipPlayer == 0)
        {
            textGameState.text = "HẾT TIỀN. THOÁT GAME!!!";

            Application.Quit();
            Debug.Log("Quit");
        }
    }

}








