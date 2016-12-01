using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


[System.Serializable]
public class PlayerBl {
	public PlayerType type = PlayerType.ai;
	public int playerNum;
	public List<CardBlackjack> hand;
	public SlotDefBl handSlotDef;

	public CardBlackjack addCard(CardBlackjack eCB)
	{
		if (hand == null) hand = new List<CardBlackjack> ();
		hand.Add (eCB);
		if (type == PlayerType.human) {
			CardBlackjack[] cards = hand.ToArray ();
			cards = cards.OrderBy (cd => cd.rank).ToArray ();
			hand = new List<CardBlackjack> (cards);
		}
		eCB.setSortingLayerName ("10");
		eCB.eventualSortLayer = handSlotDef.layerName;
		fanHand ();
		return eCB;
	}
	public CardBlackjack removeCard(CardBlackjack cb)
	{
		hand.Remove (cb);
		fanHand ();
		return cb;
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
			pos = rotQ * pos;
			pos += handSlotDef.pos;
			pos.z = -0.5f * i;
			if (Blackjack.S.phase != TurnPhase.idle)
			{
				hand [i].timeStart = 0;
			}
			hand [i].moveTo (pos, rotQ);
			hand [i].state = CBlState.toHand;
			/*hand[i].transform.localPosition = pos;
			hand[i].transform.rotation = rotQ;
			hand[i].state = CBState.hand;*/
			hand[i].faceUp = (type == PlayerType.human);
			hand[i].eventualSortOrder = i * 4;
			//hand[i].setSortOrder(i*4);

		}
	}

	public void takeTurn()
	{
		Utils.tr (Utils.RoundToPlaces (Time.time), "Player.takeTurn");
		if (type == PlayerType.human) return;
		Blackjack.S.phase = TurnPhase.waiting;
		CardBlackjack cb;
		List<CardBlackjack> validCards = new List<CardBlackjack> ();
		foreach (CardBlackjack tCB in hand)
		{
			if (Blackjack.S.validPlay (tCB))
			{
				validCards.Add (tCB);
			}
		}
		if (validCards.Count == 0)
		{
			cb = addCard (Blackjack.S.draw ());
			cb.callbackPlayer = this;
			return;
		}
		cb = validCards [Random.Range (0, validCards.Count)];
		removeCard (cb);


		//How the AI takes hits, shows, or busts goes here


		cb.callbackPlayer = this;
	}

	public void CBCallback (CardBlackjack tCB)
	{
		Utils.tr (Utils.RoundToPlaces (Time.time), "Player.CBCallback()", tCB.name, "Player " + playerNum);
		Blackjack.S.passTurn ();
	}

}
	