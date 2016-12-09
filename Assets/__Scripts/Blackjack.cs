using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class Blackjack : MonoBehaviour {
	public static Blackjack S;

	public TextAsset deckXML;
	public TextAsset layoutXML;
	public Vector3 layoutCenter = Vector3.zero;

	public bool __;

	public Deck deck;
	public List<CardBlackjack> drawPile;
	public List<CardBlackjack> discardPile;

	public BlackjackLayout layout;
	public Transform layoutAnchor;

	public float handFanDegrees = 0f;
	public List<PlayerBl> players;
	public CardBlackjack targetCard;

	public int numStartingCards = 2;
	public float drawTimeStagger = 0.1f;

	public static PlayerBl CURRENT_PLAYER;

	public TurnPhase phase = TurnPhase.idle;
	public GameObject turnLight;

	public GameObject GTGameOver;
	public GameObject GTRoundResult;

	void Awake()
	{
		S = this;
		turnLight = GameObject.Find ("TurnLight");
		GTGameOver = GameObject.Find ("GTGameOver");
		GTRoundResult = GameObject.Find ("GTRoundResult");
		GTGameOver.SetActive (false);
		GTRoundResult.SetActive (false);
	}

	void Start()
	{
		deck = GetComponent<Deck> ();
		deck.initDeck (deckXML.text);
		Deck.shuffle (ref deck.cards);
		layout = GetComponent<BlackjackLayout> ();
		layout.readLayout (layoutXML.text);
		drawPile = upgradeCardsList (deck.cards);
		layoutGame ();
	}

	public void arrangeDrawPile()
	{
		CardBlackjack tCB;
		for(int i=0; i<drawPile.Count;i++)
		{
			tCB = drawPile[i];
			tCB.transform.parent =layoutAnchor;
			tCB.transform.localPosition = layout.drawPile.pos;
			tCB.faceUp = false;
			tCB.setSortingLayerName(layout.drawPile.layerName);
			tCB.setSortOrder(-i*4);
			tCB.state = CBlState.drawpile;
		}
	}

	void layoutGame()
	{
		if (layoutAnchor == null)
		{
			GameObject tGO = new GameObject ("_LayoutAnchor");
			layoutAnchor = tGO.transform;
			layoutAnchor.transform.position = layoutCenter;
		}
		arrangeDrawPile ();
		PlayerBl pl;
		players = new List<PlayerBl> ();
		List<SlotDefBl> tShowDefList = new List<SlotDefBl> ();
		foreach (SlotDefBl tSSD in layout.showDefs)
		{
			tShowDefList.Add (tSSD);
		}
		int k = 0;
		foreach (SlotDefBl tSD in layout.slotDefs)
		{
			pl = new PlayerBl ();
			pl.handSlotDef = tSD;
			pl.showSlotDef = tShowDefList [k];
			k++;
			players.Add (pl);
			pl.playerNum = players.Count-1;
		}
		players [0].type = PlayerType.human;
		deal ();
	}

	public void deal()
	{
		CardBlackjack tCB;
		for (int i = 0; i < numStartingCards; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				tCB = draw ();
				tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
				players [(j + 1) % 4].addCard (tCB);
				if (i == 1)
				{
					tCB.faceUp = true;
				}
			}
		}
		startGame ();
	}


	public CardBlackjack draw()
	{
		if (drawPile.Count <= 0)
		{
			List<Card> cards = new List<Card> ();
			if (discardPile != null)
			{
				foreach (CardBlackjack cb in discardPile)
				{
					cards.Add (cb);
				}
				discardPile.Clear ();
				Deck.shuffle (ref cards);
				drawPile = upgradeCardsList (cards);
				arrangeDrawPile ();
			}
			else
			{
				print ("It broke! Trying to shuffle in the discard Pile, but it doesn't exist!");
				Application.Quit ();
			}
		}
		CardBlackjack cd = drawPile [0];
		drawPile.RemoveAt (0);
		return(cd);
	}

	public CardBlackjack moveToDiscard(CardBlackjack tCB)
	{
		tCB.state = CBlState.discard;
		discardPile.Add (tCB);
		tCB.setSortingLayerName(layout.discardPile.layerName);
		tCB.setSortOrder (discardPile.Count * 4);
		tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;
		return tCB;
	}


	List<CardBlackjack> upgradeCardsList(List<Card> lCD)
	{
		List<CardBlackjack> lCB = new List<CardBlackjack> ();
		foreach(Card tCD in lCD)
		{
			lCB.Add(tCD as CardBlackjack);
		}
		return lCB;
	}

	public void CBCallback(CardBlackjack cb)
	{
		Utils.tr (Utils.RoundToPlaces (Time.time), "Blackjack.CBCallback()", cb.name);
		startGame ();
	}

	public void startGame()
	{
		passTurn (1);
	}

	public void passTurn(int num= -1)
	{
		if (num == -1)
		{
			int ndx = players.IndexOf (CURRENT_PLAYER);
			num = (ndx + 1) % 4;
		}
		int lastPlayerNum = -1;
		if (CURRENT_PLAYER != null)
		{
			lastPlayerNum = CURRENT_PLAYER.playerNum;
			if (checkGameOver ())
			{
				restartGame ();
			}
		}
		CURRENT_PLAYER = players [num];
		phase = TurnPhase.pre;
		CURRENT_PLAYER.takeTurn ();
		Vector3 lPos = CURRENT_PLAYER.handSlotDef.pos + Vector3.back * 5;
		turnLight.transform.position = lPos;
		Utils.tr (Utils.RoundToPlaces (Time.time), "Blackjack.passTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
	}
		

	public void cardClicked(CardBlackjack tCB)
	{
		if (CURRENT_PLAYER == null) return;
		if (CURRENT_PLAYER.type != PlayerType.human) return;
		if (phase == TurnPhase.waiting) return;
		switch (tCB.state)
		{
			case CBlState.drawpile:
				CardBlackjack cb = CURRENT_PLAYER.addCard (draw ());
				cb.callbackPlayer = CURRENT_PLAYER;
				Utils.tr (Utils.RoundToPlaces (Time.time), "Blackjack.cardClicked()", "Draw", cb.name);
				phase = TurnPhase.waiting;
				break;
			case CBlState.hand:
				if (CURRENT_PLAYER.type == PlayerType.human && tCB.player == 0)
				{
					print ("Stay");
					CURRENT_PLAYER.stay = true;
					passTurn ();
				}
				break;
		}
	}

	public bool checkGameOver()
	{
		int done = 0;
		foreach (PlayerBl pl in players)
		{
			if (pl.stay == true || pl.bust == true) done++;
		}
		if (done == 4)
		{
			phase = TurnPhase.gameOver;
			foreach (PlayerBl pl in players)
			{
				pl.showHand ();
			}
			Invoke ("calculateScores", 2);
			return true;
		}
		else
		{
			return false;
		}
	}

	public void calculateScores()
	{
			int dealerScore = 0;
			int playerScore = 0;
			foreach (CardBlackjack cb in players[0].show)
			{
				playerScore += Mathf.Min(cb.rank,10);
			}
			foreach (CardBlackjack cb in players[2].show)
			{
				dealerScore += Mathf.Min(cb.rank,10);
			}
			if (players[0].bust==true || playerScore <= dealerScore)
			{
				GTGameOver.GetComponent<Text> ().text = "You lost!";
				GTRoundResult.GetComponent<Text> ().text = ": (";
			}
			else
			{
				GTGameOver.GetComponent<Text> ().text = "You Won!";
				GTRoundResult.GetComponent<Text> ().text = "";
			}

			GTGameOver.SetActive (true);
			GTRoundResult.SetActive (true);
			Invoke ("restartGame", 2);
		}
	public void restartGame()
	{
		CURRENT_PLAYER = null;
		foreach (PlayerBl pl in players)
		{
			foreach (CardBlackjack cb in pl.show)
			{
				moveToDiscard (cb);
			}
		}
		deal ();
	}
}


