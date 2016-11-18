using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Card : MonoBehaviour {
	public string suit;
	public int rank;
	public Color color = Color.black;
	public string colS = "Black";
	public List<GameObject> decoGOs = new List<GameObject>();
	public List<GameObject> pipGOs = new List<GameObject>();
	public GameObject back;
	public CardDefinition def;
	public SpriteRenderer[] spriteRenderers;


	void Start()
	{
		setSortOrder(0);
	}


	public bool faceUp
	{
		get
		{
			return (!back.activeSelf);
		}
		set
		{
			back.SetActive(!value);
		}
	}

	public void populateSpriteRenderers()
	{
		if (spriteRenderers == null || spriteRenderers.Length == 0)
		{
			spriteRenderers = GetComponentsInChildren<SpriteRenderer>();
		}
	}

	public void setSortingLayerName(string tSLN)
	{
		populateSpriteRenderers();
		foreach(SpriteRenderer tSR in spriteRenderers)
		{
			tSR.sortingLayerName = tSLN;
		}
	}
	public void setSortOrder (int sOrd)
	{
		populateSpriteRenderers();
		foreach(SpriteRenderer tSR in spriteRenderers)
		{
			if (tSR.gameObject == this.gameObject)
			{
				tSR.sortingOrder = sOrd;
				continue;
			}switch (tSR.gameObject.name)
			{
				case "back":
					tSR.sortingOrder = sOrd+2;
					break;
				case "face": //Nothing, apparently, just fall through.
					default:
					tSR.sortingOrder = sOrd+1;
					break;
			}
		}
	}
		
	virtual public void OnMouseUpAsButton()
	{
		//print (name); //Just a stand in, to be overridden by classes for specific games. - Commented out because it's noisy.
	}

}







[System.Serializable]
public class Decorator
{
	public string type;
	public Vector3 loc;
	public bool flip = false;
	public float scale = 1f;
}




[System.Serializable]
public class CardDefinition
{
	public string face;
	public int rank;
	public List<Decorator> pips = new List<Decorator>();
}