using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public enum PlayerType
{
	human,
	ai
}

[System.Serializable]
public class Player {
	public PlayerType type = PlayerType.ai;
	public int playerNum;
	public List<CardBartok> hand;
	public SlotDefB handSlotDef;

	public CardBartok addCard(CardBartok eCB)
	{
		if (hand == null) hand = new List<CardBartok> ();
		hand.Add (eCB);
		if (type == PlayerType.human) {
			CardBartok[] cards = hand.ToArray ();
			cards = cards.OrderBy (cd => cd.rank).ToArray ();
			hand = new List<CardBartok> (cards);
		}
		eCB.setSortingLayerName ("10");
		eCB.eventualSortLayer = handSlotDef.layerName;
		fanHand ();
		return eCB;
	}
	public CardBartok removeCard(CardBartok cb)
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
			startRot += Bartok.S.handFanDegrees * (hand.Count-1)/2;
		}
		Vector3 pos;
		float rot;
		Quaternion rotQ;
		for (int i=0;i<hand.Count;i++)
		{
			rot = startRot - Bartok.S.handFanDegrees * i;
			rotQ = Quaternion.Euler (0, 0, rot);
			pos = Vector3.up * CardBartok.CARD_HEIGHT / 2f;
			pos = rotQ * pos;
			pos += handSlotDef.pos;
			pos.z = -0.5f * i;
			if (Bartok.S.phase != TurnPhase.idle)
			{
				hand [i].timeStart = 0;
			}
			hand [i].moveTo (pos, rotQ);
			hand [i].state = CBState.toHand;
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
		Bartok.S.phase = TurnPhase.waiting;
		CardBartok cb;
		List<CardBartok> validCards = new List<CardBartok> ();
		foreach (CardBartok tCB in hand)
		{
			if (Bartok.S.validPlay (tCB))
			{
				validCards.Add (tCB);
			}
		}
		if (validCards.Count == 0)
		{
			cb = addCard (Bartok.S.draw ());
			cb.callbackPlayer = this;
			return;
		}
		cb = validCards [Random.Range (0, validCards.Count)];
		removeCard (cb);
		Bartok.S.moveToTarget (cb);
		cb.callbackPlayer = this;
	}

	public void CBCallback (CardBartok tCB)
	{
		Utils.tr (Utils.RoundToPlaces (Time.time), "Player.CBCallback()", tCB.name, "Player " + playerNum);
		Bartok.S.passTurn ();
	}


}
	