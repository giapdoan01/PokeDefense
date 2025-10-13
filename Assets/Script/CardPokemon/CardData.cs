using System;
using UnityEngine;

[Serializable]
public class CardData
{
    public Sprite cardImage;
    public string id;
    public string name;
    public int gemPrice;
    public string type;
    public int coinInGame;
    public GameObject pokemonGhostPrefab;
    public GameObject pokemonPrefab; 
    
    public CardData() { }
    
    public CardData(Sprite cardImage,string id, string name, int gemPrice, string type)
    {
        this.cardImage = cardImage;
        this.id = id;
        this.name = name;
        this.gemPrice = gemPrice;
        this.type = type;
    }
}
