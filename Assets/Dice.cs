using System;
using System.Collections;
//using System.Runtime.Remoting.Metadata.W3cXsd2001;
using UnityEngine;

public class Dice : MonoBehaviour
{

    private Sprite[] diceSides;
    private SpriteRenderer rend;
    public static int whosTurn = 0;
    private bool coroutineAllowed = true;

    // Use this for initialization
    private void Start()
    {
        rend = GetComponent<SpriteRenderer>();
        diceSides = Resources.LoadAll<Sprite>("DiceSides/");
        rend.sprite = diceSides[5];
        whosTurn = UnityEngine.Random.Range(0, 4);
    }

    private void OnMouseDown()
    {
        //StartCoroutine(testComer()); return;// Para testear
        if (!GameControl.gameStarted) GameObject.Find("MapButton").gameObject.SetActive(false);
        GameControl.gameStarted = true;
        if (!GameControl.jugadorEnMovimiento() && !GameControl.gameOver && coroutineAllowed && !GameControl.seleccionandoJugador) StartCoroutine("RollTheDice");
    }

    private IEnumerator RollTheDice()
    {
        coroutineAllowed = false;
        int randomDiceSide = 0;
        for (int i = 0; i <= 20; i++)
        {
            randomDiceSide = UnityEngine.Random.Range(0, 6);
            rend.sprite = diceSides[randomDiceSide];
            yield return new WaitForSeconds(0.05f);
        }

        GameControl.diceSideThrown = randomDiceSide + 1;
        GameControl.MovePlayer(whosTurn);
        int numero = GameControl.diceSideThrown;
        while (GameControl.seleccionandoPieza) yield return null;
        if (!(GameControl.comio || numero == 6 || GameControl.tiroExtra_)) whosTurn++;
        if (whosTurn == 4) whosTurn = 0;
        if (GameControl.tiroExtra_) GameControl.tiroExtra_ = false;
        bool continuar = true;
        while (continuar)
        {
            continuar = false;
            for (int i = 0; i < GameControl.turnosPerdidos.Count; i++)
            {
                if (GameControl.turnosPerdidos[i] == whosTurn)
                {
                    i = GameControl.turnosPerdidos.Count;
                    GameControl.turnosPerdidos.RemoveAt(i);
                    whosTurn++;
                    if (whosTurn == 4) whosTurn = 0;
                    continuar = true;
                }
            }
        }
        coroutineAllowed = true;
    }

    public IEnumerator esperarTerminarMovimiento()
    {
        while (GameControl.jugadorEnMovimiento()) yield return null;
    }

    /*private IEnumerator testComer()
    {
        //Salen los 2 primeros personajes
        GameControl.diceSideThrown = 1;
        GameControl.MovePlayer(1);
        while (GameControl.jugadorEnMovimiento()) yield return null;
        GameControl.diceSideThrown = 13;
        GameControl.MovePlayer(1);
        while (GameControl.jugadorEnMovimiento()) yield return null;
        GameControl.diceSideThrown = 1;
        GameControl.MovePlayer(2);
        //Sale el jugador 1
        while (GameControl.jugadorEnMovimiento()) yield return null;
        GameControl.MovePlayer(1);

        //El personaje 1 va a un escudo y el 2 lo intenta comer
        while (GameControl.jugadorEnMovimiento()) yield return null;
        GameControl.diceSideThrown = 21;
        GameControl.MovePlayer(1);
        while (GameControl.jugadorEnMovimiento()) yield return null;
        GameControl.diceSideThrown = 8;
        GameControl.MovePlayer(2);
        //El personaje 1 sale del escudo y el personaje 2 lo come
        while (GameControl.jugadorEnMovimiento()) yield return null;
        GameControl.diceSideThrown = 1;
        GameControl.MovePlayer(1);
        while (GameControl.jugadorEnMovimiento()) yield return null;
        GameControl.diceSideThrown = 9;
        GameControl.MovePlayer(2);
    }*/
}