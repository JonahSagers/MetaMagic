using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeckManager : MonoBehaviour
{
    public GameObject cardPre;
    public Vector3 cardOffset;
    public GameObject card;
    public string gesture;
    public int heldCards;
    public int airCards;

    [Header("Left Hand")]
    public Collider thumb;
    [Header("Right Hand")]
    public Collider index;
    public Collider middle;

    void Start()
    {
        Draw();
    }

    public void Draw()
    {
        card = Instantiate(cardPre, transform.position, transform.rotation);
        card.transform.parent = transform;
        card.transform.localPosition = cardOffset;
    }

    public void TwoPoint()
    {
        gesture = "twopoint";
    }

    public void TwoPointEnd()
    {
        gesture = "none";
    }
}
