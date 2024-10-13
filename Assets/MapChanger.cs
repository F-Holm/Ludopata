using System.Collections;
using UnityEngine;

public class MapChanger : MonoBehaviour
{

    private Sprite[] maps;
    private SpriteRenderer rend;
    private int contador = 0;

    // Use this for initialization
    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        maps = Resources.LoadAll<Sprite>("Maps/");
        rend.sprite = maps[contador];
    }

    private void OnMouseDown()
    {
        if (!GameControl.gameStarted)
        {
            contador++;
            if (contador >= 3)
            {
                contador = 0;
            }
            rend.sprite = maps[contador];
        }
    }

}
