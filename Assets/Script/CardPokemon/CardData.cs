using System;
using UnityEngine;

/// <summary>
/// Dữ liệu thẻ bài - CHỈ 5 THUỘC TÍNH
/// </summary>
[Serializable]
public class CardData
{
    public Sprite cardImage;
    public string id;
    public string name;
    public int gemPrice;
    public string type;
    
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
