using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class PlayerBl {
	public PlayerType type = PlayerType.ai;
	public int playerNum;
	public List<CardBlackjack> hand;
	public List<CardBlackjack> show;
	public SlotDefBl handSlotDef;
	public SlotDefBl showSlotDef;
	public bool bust = false;
	public bool stay = false;

	public CardBlackjack addCard(CardBlackjack eCB)
	{
		if (hand == null) hand = new List<CardBlackjack> ();
		hand.Add (eCB);
		eCB.player = playerNum;
		eCB.setSortingLayerName ("10");
		eCB.eventualSortLayer = handSlotDef.layerName;
		fanHand ();
		return eCB;
	}
	public CardBlackjack removeCard(CardBlackjack cb)
	{
		hand.Remove (cb);
		cb.player = -1;
		fanHand ();
		return cb;
	}

	public void showHand()
	{
		foreach(CardBlackjack tCB in hand)
		{
			if (show == null) show = new List<CardBlackjack>();
			show.Add (tCB);
			tCB.setSortingLayerName ("10");
			tCB.eventualSortLayer = handSlotDef.layerName;
		}
	}

	public void fanHand()
	{
		float startRot = handSlotDef.rot;
		if (hand.Count > 1)
		{
			startRot += Blackjack.S.handFanDegrees * (hand.Count-1)/2;
		}
		Vector3 pos;
		float rot;
		Quaternion rotQ;
		for (int i=0;i<hand.Count;i++)
		{
			rot = startRot - Blackjack.S.handFanDegrees * i;
			rotQ = Quaternion.Euler (0, 0, rot);
			pos = Vector3.up * CardBlackjack.CARD_HEIGHT / 2f;
			pos += Vector3.right * CardBlackjack.CARD_WIDTH;
			pos = rotQ * pos;
			pos += handSlotDef.pos;
			pos.z = -0.5f * i;
			if (Blackjack.S.phase != TurnPhase.idle)
			{
				hand [i].timeStart = 0;
			}
			hand[i].moveTo (pos, rotQ);
			hand[i].state = CBlState.toHand;
			hand[i].faceUp = (type == PlayerType.human);
			if (i == 0) hand [i].faceUp = true;
			hand[i].eventualSortOrder = i * 4;
		}
	}


	public void fanShow()
	{
		float startRot = showSlotDef.rot;
		if (hand.Count > 1)
		{
			startRot += Blackjack.S.handFanDegrees * (hand.Count-1)/2;
		}
		Vector3 pos;
		float rot;
		Quaternion rotQ;
		for (int i=0;i<hand.Count;i++)
		{
			rot = startRot - Blackjack.S.handFanDegrees * i;
			rotQ = Quaternion.Euler (0, 0, rot);
			pos = Vector3.up * CardBlackjack.CARD_HEIGHT / 2f;
			pos += Vector3.right * CardBlackjack.CARD_WIDTH;
			pos = rotQ * pos;
			pos += showSlotDef.pos;
			pos.z = -0.5f * i;
			if (Blackjack.S.phase != TurnPhase.idle)
			{
				hand [i].timeStart = 0;
			}
			show[i].moveTo (pos, rotQ);
			show[i].state = CBlState.toShow;
			show[i].faceUp = true;
			show[i].eventualSortOrder = i * 4;

		}
	}

	public void takeTurn()
	{
		if (bust == true)
		Utils.tr (Utils.RoundToPlaces (Time.time), "Player.takeTurn");
		if (type == PlayerType.human && bust == false) return;
		else if (bust == true) Blackjack.S.passTurn ();
		else if (stay == true) Blackjack.S.passTurn ();
		Blackjack.S.phase = TurnPhase.waiting;
		CardBlackjack cb;
		int handValue = 0;
		foreach (CardBlackjack tCB in hand)
		{
			handValue += Mathf.Min (tCB.rank, 10);
		}
		if (handValue > 21)
		{
			bust = true;
			Blackjack.S.passTurn ();
		}
		else if (handValue == 21)
		{
			stay = true;
			Blackjack.S.passTurn ();
		}
		else if (handValue < 21)
		{
			if (21 - handValue >= Random.Range (1, 6))
			{
				cb= Blackjack.S.draw ();
				addCard (cb);
				cb.callbackPlayer = this;
			}
			else
			{
				stay = true;
				Blackjack.S.passTurn ();
			}
		}
	}

	public void CBCallback (CardBlackjack tCB)
	{
		Utils.tr (Utils.RoundToPlaces (Time.time), "Player.CBCallback()", tCB.name, "Player " + playerNum);
		Blackjack.S.passTurn ();
	}
}
	