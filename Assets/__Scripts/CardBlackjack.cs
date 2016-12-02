using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public enum CBlState
{
	drawpile,
	toHand,
	hand,
	toShow,
	show,
	discard,
	to,
	idle
}

public class CardBlackjack : Card {
	public static float MOVE_DURATION = 0.5f;
	public static string MOVE_EASING = Easing.InOut;
	public static float CARD_HEIGHT = 3.5f;
	public static float CARD_WIDTH = 2f;
	public CBlState state = CBlState.drawpile;
	public int player;


	public List<Vector3> bezierPts;
	public List<Quaternion> bezierRots;
	public float timeStart, timeDuration;

	public GameObject reportFinishTo = null;
	public PlayerBl callbackPlayer = null;

	public int eventualSortOrder;
	public string eventualSortLayer;

	void Awake()
	{
		callbackPlayer = null;
	}

	public void moveTo (Vector3 ePos, Quaternion eRot)
	{
		bezierPts = new List<Vector3> ();
		bezierPts.Add (transform.localPosition);
		bezierPts.Add (ePos);
		bezierRots = new List<Quaternion> ();
		bezierRots.Add (transform.rotation);
		bezierRots.Add (eRot);

		if (timeStart == 0)
		{
			timeStart = Time.time;
		}
		timeDuration = MOVE_DURATION;
		state = CBlState.to;
	}


	public void moveTo(Vector3 ePos)
	{
		moveTo (ePos, Quaternion.identity);
	}

	void Update()
	{
		switch (state)
		{
			case CBlState.toHand:
			case CBlState.to:
				float u = (Time.time - timeStart) / timeDuration;
				float uC = Easing.Ease (u, MOVE_EASING);
				if (u < 0)
				{
					transform.localPosition = bezierPts [0];
					transform.rotation = bezierRots [0];
					return;
				}
				if (u >= 1)
				{
					uC = 1;
					if (state == CBlState.toHand) state = CBlState.hand;
					if (state == CBlState.to) state = CBlState.idle;
					if (state == CBlState.toShow) state = CBlState.show;
					transform.localPosition = bezierPts [bezierPts.Count - 1];
					transform.rotation = bezierRots [bezierPts.Count - 1];
					timeStart = 0;
					if (reportFinishTo != null)
					{
						reportFinishTo.SendMessage ("CBCallback", this);
						reportFinishTo = null;
					}
					else if (callbackPlayer != null)
					{
						callbackPlayer.CBCallback (this);
						callbackPlayer = null;
					}
					else
					{
						//do nothing
					}
				}
				else
				{
					Vector3 pos = Utils.Bezier (uC, bezierPts);
					transform.localPosition = pos;
					Quaternion rotQ = Utils.Bezier(uC, bezierRots);//This doesn't work because Utils.Bezier is looking for a list of floats, not a list of Quaternions.
					transform.rotation = rotQ;
					if (u > 0.5f && spriteRenderers[0].sortingOrder != eventualSortOrder)
					{
						setSortOrder(eventualSortOrder);
					}
					if (u > 0.75f && spriteRenderers[0].sortingLayerName != eventualSortLayer)
					{
						setSortingLayerName(eventualSortLayer);
					}
				}
				break;
		}
	}


	override public void OnMouseUpAsButton()
	{
		Blackjack.S.cardClicked (this);
		base.OnMouseUpAsButton ();
	}
}
