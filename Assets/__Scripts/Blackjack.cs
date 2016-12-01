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

	public float handFanDegrees = 10f;
	public List<PlayerBl> players;
	public CardBlackjack targetCard;

	public int numStartingCards = 7;
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
		foreach (SlotDefBl tSD in layout.slotDefs)
		{
			pl = new PlayerBl ();
			pl.handSlotDef = tSD;
			players.Add (pl);
			pl.playerNum = players.Count;
		}
		players [0].type = PlayerType.human;

		CardBlackjack tCB;
		for (int i = 0; i < numStartingCards; i++)
		{
			for (int j = 0; j < 4; j++)
			{
				tCB = draw ();
				tCB.timeStart = Time.time + drawTimeStagger * (i * 4 + j);
				players [(j + 1) % 4].addCard (tCB);
			}
		}
		Invoke ("drawFirstTarget", drawTimeStagger * (numStartingCards * 4 + 4));
	}

	public CardBlackjack draw()
	{
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
				return;
			}
		}
		CURRENT_PLAYER = players [num];
		phase = TurnPhase.pre;
		CURRENT_PLAYER.takeTurn ();
		Vector3 lPos = CURRENT_PLAYER.handSlotDef.pos + Vector3.back * 5;
		turnLight.transform.position = lPos;
		Utils.tr (Utils.RoundToPlaces (Time.time), "Blackjack.passTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
	}

	public bool validPlay(CardBlackjack cb)
	{
		if (CURRENT_PLAYER.type == PlayerType.human && cb.faceUp == false) return false;
		if (cb.rank == targetCard.rank) return true;
		if (cb.suit == targetCard.suit) return true;
		return false;
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
				if (validPlay (tCB))
				{
					CURRENT_PLAYER.removeCard (tCB);


					//Do the stuff you need to do here


					tCB.callbackPlayer = CURRENT_PLAYER;
					Utils.tr (Utils.RoundToPlaces (Time.time), "Blackjack.cardClicked()", "Play", tCB.name, targetCard.name + " is target");
					phase = TurnPhase.waiting;
				}
				else
				{
					Utils.tr (Utils.RoundToPlaces (Time.time), "Blackjack.cardClicked()", "Attempted to Play", tCB.name, targetCard.name + " is target");
				}
				break;
		}
	}

	public bool checkGameOver()
	{
		if (drawPile.Count == 0)
		{
			List<Card> cards = new List<Card> ();
			foreach (CardBlackjack cb in discardPile)
			{
				cards.Add (cb);
			}
			discardPile.Clear ();
			Deck.shuffle (ref cards);
			drawPile = upgradeCardsList (cards);
			arrangeDrawPile ();
		}
		if (CURRENT_PLAYER.hand.Count == 0)
		{
			if (CURRENT_PLAYER.type == PlayerType.human)
			{
				GTGameOver.GetComponent<Text> ().text = "You Won!";
				GTRoundResult.GetComponent<Text> ().text = "";
			}
			else
			{
				GTGameOver.GetComponent<Text> ().text = "Game Over";
				GTRoundResult.GetComponent<Text> ().text = "Player " + CURRENT_PLAYER.playerNum + " won.";
			}
			GTGameOver.SetActive (true);
			GTRoundResult.SetActive (true);
			phase = TurnPhase.gameOver;
			Invoke ("restartGame", 2);
			return true;
		}
		return false;
	}
	public void restartGame()
	{
		CURRENT_PLAYER = null;
		SceneManager.LoadScene (SceneManager.GetActiveScene ().buildIndex);
	}
}


