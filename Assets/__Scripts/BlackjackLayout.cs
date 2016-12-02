﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class SlotDefBl
{
	public float x;
	public float y;
	public bool faceUp=false;
	public string layerName="Default";
	public int layerID=0;
	public int id;
	public float rot;
	public string type="slot";
	public Vector2 stagger;
	public int player;
	public Vector3 pos;
}

public class BlackjackLayout : MonoBehaviour {
	public PT_XMLReader xmlr;
	public PT_XMLHashtable xml;
	public Vector2 multiplier;

	public List<SlotDefBl> slotDefs;
	public List<SlotDefBl> showDefs;
	public SlotDefBl drawPile;
	public SlotDefBl discardPile;

	public void readLayout(string xmlText)
	{
		xmlr = new PT_XMLReader ();
		xmlr.Parse (xmlText);
		xml = xmlr.xml ["xml"] [0];

		multiplier.x = float.Parse (xml ["multiplier"] [0].att ("x"));
		multiplier.y = float.Parse (xml ["multiplier"] [0].att ("y"));

		SlotDefBl tSD;
		PT_XMLHashList slotsX = xml ["slot"];
		for (int i = 0; i < slotsX.Count; i++)
		{
			tSD = new SlotDefBl ();
			if (slotsX [i].HasAtt ("type"))
			{
				tSD.type = slotsX [i].att ("type");
			}
			else
			{
				tSD.type = "slot";
			}

			tSD.x = float.Parse (slotsX [i].att ("x"));
			tSD.y = float.Parse (slotsX [i].att ("y"));
			tSD.pos = new Vector3 (tSD.x * multiplier.x, tSD.y * multiplier.y, 0);

			tSD.layerID = int.Parse (slotsX [i].att ("layer"));
			tSD.layerName = tSD.layerID.ToString ();
			switch (tSD.type)
			{
				case "slot":
					break;
				case "drawpile":
					drawPile = tSD;
					break;
				case "discardpile":
					discardPile = tSD;
					break;
				case "hand":
					tSD.player = int.Parse (slotsX [i].att ("player"));
					tSD.rot = float.Parse (slotsX [i].att ("rot"));
					slotDefs.Add (tSD);
					break;
				case "show":
					showDefs.Add (tSD);
					break;
			}
		}
	}

}
