using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public enum TurnPhase
{
	idle,
	pre,
	waiting,
	post,
	gameOver
}


public class Bartok : MonoBehaviour {
	public static Bartok S;

	public TextAsset deckXML;
	public TextAsset layoutXML;
	public Vector3 layoutCenter = Vector3.zero;

	public bool __;

	public Deck deck;
	public List<CardBartok> drawPile;
	public List<CardBartok> discardPile;

	public BartokLayout layout;
	public Transform layoutAnchor;

	public float handFanDegrees = 10f;
	public List<Player> players;
	public CardBartok targetCard;

	public int numStartingCards = 7;
	public float drawTimeStagger = 0.1f;

	public static Player CURRENT_PLAYER;

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
		layout = GetComponent<BartokLayout> ();
		layout.readLayout (layoutXML.text);
		drawPile = upgradeCardsList (deck.cards);
		layoutGame ();
	}

	public void arrangeDrawPile()
	{
		CardBartok tCB;
		for(int i=0; i<drawPile.Count;i++)
		{
			tCB = drawPile[i];
			tCB.transform.parent =layoutAnchor;
			tCB.transform.localPosition = layout.drawPile.pos;
			tCB.faceUp = false;
			tCB.setSortingLayerName(layout.drawPile.layerName);
			tCB.setSortOrder(-i*4);
			tCB.state = CBState.drawpile;
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
		Player pl;
		players = new List<Player> ();
		foreach (SlotDefB tSD in layout.slotDefs)
		{
			pl = new Player ();
			pl.handSlotDef = tSD;
			players.Add (pl);
			pl.playerNum = players.Count;
		}
		players [0].type = PlayerType.human;

		CardBartok tCB;
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

	public CardBartok draw()
	{
		CardBartok cd = drawPile [0];
		drawPile.RemoveAt (0);
		return(cd);
	}

	public void drawFirstTarget()
	{
		CardBartok tCB = moveToTarget(draw());
		tCB.reportFinishTo = this.gameObject;
	}

	public CardBartok moveToTarget(CardBartok tCB)
	{
		tCB.timeStart = 0;
		tCB.moveTo(layout.discardPile.pos+Vector3.back);
		tCB.state = CBState.toTarget;
		tCB.faceUp = true;
		tCB.setSortingLayerName ("10");
		tCB.eventualSortLayer = layout.target.layerName;
		if (targetCard != null)
		{
			moveToDiscard (targetCard);
		}
		targetCard = tCB;
		return(tCB);
	}

	public CardBartok moveToDiscard(CardBartok tCB)
	{
		tCB.state = CBState.discard;
		discardPile.Add (tCB);
		tCB.setSortingLayerName(layout.discardPile.layerName);
		tCB.setSortOrder (discardPile.Count * 4);
		tCB.transform.localPosition = layout.discardPile.pos + Vector3.back / 2;
		return tCB;
	}
		
	/*
	void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha1))
		{
			players[0].addCard(draw());
		}
		if (Input.GetKeyDown (KeyCode.Alpha2))
		{
			players [1].addCard (draw ());
		}
		if (Input.GetKeyDown (KeyCode.Alpha3))
		{
			players [2].addCard (draw ());
		}
		if (Input.GetKeyDown (KeyCode.Alpha4))
		{
			players [3].addCard (draw ());
		}
	}
	*/


	List<CardBartok> upgradeCardsList(List<Card> lCD)
	{
		List<CardBartok> lCB = new List<CardBartok> ();
		foreach(Card tCD in lCD)
		{
			lCB.Add(tCD as CardBartok);
		}
		return lCB;
	}

	public void CBCallback(CardBartok cb)
	{
		Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.CBCallback()", cb.name);
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
		Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.passTurn()", "Old: " + lastPlayerNum, "New: " + CURRENT_PLAYER.playerNum);
	}

	public bool validPlay(CardBartok cb)
	{
		if (CURRENT_PLAYER.type == PlayerType.human && cb.faceUp == false) return false;
		if (cb.rank == targetCard.rank) return true;
		if (cb.suit == targetCard.suit) return true;
		return false;
	}

	public void cardClicked(CardBartok tCB)
	{
		if (CURRENT_PLAYER == null) return;
		if (CURRENT_PLAYER.type != PlayerType.human) return;
		if (phase == TurnPhase.waiting) return;
		switch (tCB.state)
		{
			case CBState.drawpile:
				CardBartok cb = CURRENT_PLAYER.addCard (draw ());
				cb.callbackPlayer = CURRENT_PLAYER;
				Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.cardClicked()", "Draw", cb.name);
				phase = TurnPhase.waiting;
				break;
			case CBState.hand:
				if (validPlay (tCB))
				{
					CURRENT_PLAYER.removeCard (tCB);
					moveToTarget (tCB);
					tCB.callbackPlayer = CURRENT_PLAYER;
					Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.cardClicked()", "Play", tCB.name, targetCard.name + " is target");
					phase = TurnPhase.waiting;
				}
				else
				{
					Utils.tr (Utils.RoundToPlaces (Time.time), "Bartok.cardClicked()", "Attempted to Play", tCB.name, targetCard.name + " is target");
				}
				break;
		}
	}

	public bool checkGameOver()
	{
		if (drawPile.Count == 0)
		{
			List<Card> cards = new List<Card> ();
			foreach (CardBartok cb in discardPile)
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
