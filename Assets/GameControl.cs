using Microsoft.Win32.SafeHandles;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Reflection;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class GameControl : MonoBehaviour
{
    private static GameObject[] playerIcons = new GameObject[4] { null, null, null, null };
    private static GameObject[] playerTexts = new GameObject[4] { null, null, null, null };
    private static GameObject[] playerPowerTexts = new GameObject[4] { null, null, null, null };

    private static GameObject[,] players = new GameObject[4, 4] { { null, null, null, null }, { null, null, null, null }, { null, null, null, null }, { null, null, null, null } };// players[ JUGADOR, PIEZA ]

    public static int diceSideThrown = 0;
    public static int[,] startWaypoints = new int[4, 4] { { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 }, { 0, 0, 0, 0 } };

    public static bool gameOver = false;
    public static bool gameStarted = false;

    public static bool seleccionandoPieza = false;
    public static bool piezaYaSeleccionada = false;
    private static int piezaElegida = 4;
    private static int playerElegido = 4;
    public static bool comio = false;
    public static bool seleccionandoJugador = false;
    public static bool poderesHabilitados = false;
    //power[ PLAYER, POWER ]
    public static GameObject[,] powers = new GameObject[4, 6] { { null, null, null, null, null, null }, { null, null, null, null, null, null }, { null, null, null, null, null, null }, { null, null, null, null, null, null } };
    private static int power;

    public static List<int> turnosPerdidos = new List<int>();

    private static HashSet<Transform> kapkans = new HashSet<Transform>();
    private static HashSet<Transform> gridlocks = new HashSet<Transform>();
    private static bool tramp = false;

    public static bool tiroExtra_ = false;
    // Use this for initialization
    void Start()
    {
        for (int i = 0;i < 4; i++)
        {
            playerIcons[i] = GameObject.Find("Player" + (i + 1) + "Icon");
            playerTexts[i] = GameObject.Find("Player" + (i + 1) + "MoveText");
            playerPowerTexts[i] = GameObject.Find("Player" + (i + 1) + "PowerText");
            playerPowerTexts[i].gameObject.SetActive(false);
            playerTexts[i].GetComponent<UnityEngine.UI.Text>().text = "Tu Turno";
            setIconsTexts(i, false);

            for (int j = 0; j < 4; j++)
            {
                players[i, j] = GameObject.Find("Player" + (i + 1).ToString() + (j + 1).ToString());
                players[i, j].GetComponent<FollowThePath>().moveAllowed = false;
            }

            for (int j = 0; j < 6; j++)
            {
                powers[i, j] = GameObject.Find("Power" + (j + 1).ToString() + (i + 1).ToString());
                powers[i, j].GetComponent<Power>().setPlayer(i);
                powers[i, j].gameObject.SetActive(false);
            }
        }
    }

    private static void setIconsTexts(int player, bool activo)
    {
        playerIcons[player].gameObject.SetActive(activo);
        playerTexts[player].gameObject.SetActive(activo);
    }

    private static void setPowersOff()
    {
        for (int i = 0;i < 4; i++) for (int j = 0;j < 6; j++) if (powers[i, j].activeSelf) powers[i, j].gameObject.SetActive(false);
        for (int i = 0;i < 4; i++) if (playerPowerTexts[i].activeSelf) playerPowerTexts[i].SetActive(false);
    }

    private static void setPowersPlayer(int player, bool activo)
    {
        for (int j = 0; j < 6; j++) powers[player, j].gameObject.SetActive(activo);
        playerPowerTexts[player].gameObject.SetActive(activo);
    }

    private static void verificarTurno ()
    {
        int player;
        if (gameOver)
        {
            player = ganador();
            setPowersOff();
        }
        else
        {
            player = Dice.whosTurn;
            if (player == -1) player = 3;
        }
        if (!playerIcons[player].activeSelf)
        {
            setIconsTexts(player, true);
            for (int i = 0; i < 4; i++) if (i != player && playerIcons[player].activeSelf) setIconsTexts(i, false);
            setPowersPlayer(player, true);
            //for (int i = 0; i < 4; i++) if (player != i) setPowersPlayer(player, false);
            for (int i = 0; i < 4; i++) for (int j = 0; j < 6; j++) if (player != i && powers[i, j].activeSelf) powers[i, j].gameObject.SetActive(false);
            for (int i = 0; i < 4; i++) if (player != i && playerPowerTexts[i].activeSelf) playerPowerTexts[i].gameObject.SetActive(false);
            //for (int i = 0; i < 4; i++) if (i != player && playerPowerTexts[player].activeSelf) playerPowerTexts[player].gameObject.SetActive(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        verificarTurno();
        for (int i = 0;i < 4; i++)
        {
            bool termino = true;
            for (int j = 0; j < 4; j++)
            {
                if (players[i, j].GetComponent<FollowThePath>().waypointIndex > startWaypoints[i, j] + diceSideThrown)
                {
                    players[i, j].GetComponent<FollowThePath>().moveAllowed = false;
                    players[i, j].GetComponent<FollowThePath>().index_1();
                    startWaypoints[i, j] = players[i, j].GetComponent<FollowThePath>().waypointIndex;
                }
                if (players[i, j].GetComponent<FollowThePath>().waypointIndex < players[i, j].GetComponent<FollowThePath>().waypoints.Length - 1) termino = false;
            }
            gameOver = termino;
        }

        if (gameOver)
        {
            UnityEngine.Debug.Log("GAME OVER");
        }

        if (seleccionandoPieza)
        {
            if (piezaElegida != 4)
            {
                seleccionandoPieza = false;
                piezaYaSeleccionada = true;
            }
            else
            {
                for (int i = 0; i < 4; i++)
                {
                    if (players[playerElegido, i].GetComponent<FollowThePath>().colliderActivo && movible(playerElegido, i)) piezaElegida = i;
                }
            }
        }
        if (piezaYaSeleccionada)
        {
            while (powerActive())
            {
                UnityEngine.Debug.Log("Poder " + power + " activo");
                powers[playerElegido, power].GetComponent<Power>().disponible = false;
                switch (power)
                {
                    case 0:
                        saltarTurno();
                        break;
                    case 1:
                        tornado();
                        break;
                    case 2:
                        inmortalidad();
                        break;
                    case 3:
                        kapkan();
                        break;
                    case 4:
                        gridlock();
                        break;
                    case 5:
                        tiroExtra();
                        break;
                }
            }
        }
        if (piezaYaSeleccionada)
        {
            piezaYaSeleccionada = false;
            mover();
        }
        if (tramp && !jugadorEnMovimiento())
        {
            tramp = false;
            trampas();
            playerElegido = 4;
            piezaElegida = 4;
        }
    }

    private void saltarTurno()   //PODER 1 -- Un jugador random pierde un turno. El que lo activó no puede perder el turno
    {
        int player;
        do
        {
            player = UnityEngine.Random.Range(0, 4);
        } while (player == playerElegido);
        turnosPerdidos.Add(player);
    }

    private void tornado()       //PODER 2 -- Una pieza random rival retrocede 5 posiciones.
    {
        ArrayList jugadoresMovibles = new ArrayList();
        ArrayList piezasMovibles = new ArrayList();
        for (int i = 0; i < 4; i++) if (i != playerElegido) for (int j = 0; j < 4; j++) if (movible(i, j) && players[i, j].GetComponent<FollowThePath>().waypointIndex > 4)
                    {
                        jugadoresMovibles.Add(i);
                        piezasMovibles.Add(j);
                    }
        switch (piezasMovibles.Count)
        {
            case 0:
                return;
                //break;
            case 1:
                tornado_((int) jugadoresMovibles[0], (int) piezasMovibles[0]);
                break;
            default:
                int ran = UnityEngine.Random.Range(0, jugadoresMovibles.Count);
                tornado_((int) jugadoresMovibles[ran], (int) piezasMovibles[ran]);
                break;
        }
    }

    private void tornado_(int player, int pieza)
    {
        players[player, pieza].GetComponent<FollowThePath>().moveBack = true;
    }

    private void inmortalidad()  //PODER 3 -- Tenés inmortalidad en todas tus piezas hasta tu próximo turno.
    {
        for (int i = 0; i < 4; i++) players[playerElegido, i].GetComponent<FollowThePath>().inmortal = true;
    }

    private static void mortalidad()
    {
        if (players[playerElegido, 0].GetComponent<FollowThePath>().inmortal) for (int i = 0; i < 4; i++) players[playerElegido, i].GetComponent<FollowThePath>().inmortal = false;
    }

    private void kapkan()        //PODER 4 -- Deja una trampa en la posición actual de una pieza random de sus piezas. Si alguien cae ahí, su pieza muere (vuelve al inicio). Tiene fuego amigo.
    {
        int pieza;
        ArrayList piezas = new ArrayList();
        for (int i = 0; i < 4; i++) if (!salida(playerElegido, i)) piezas.Add(i);
        switch (piezas.Count)
        {
            case 0:
                return;
                //break;
            case 1:
                pieza = (int)piezas[0];
                break;
            default:
                pieza = (int)piezas[UnityEngine.Random.Range(0, piezas.Count)];
                break;

        }
        kapkans.Add(players[playerElegido, pieza].GetComponent<FollowThePath>().waypoints[players[playerElegido, pieza].GetComponent<FollowThePath>().waypointIndex]);
    }

    private void gridlock()      //PODER 5 -- Deja una trampa en la posición actual de una pieza random de sus piezas. Si alguien cae ahí, su pieza no se podrá mover hasta el próximo turno. Tiene fuego amigo.
    {
        int pieza;
        ArrayList piezas = new ArrayList();
        for (int i = 0; i < 4; i++) if (!salida(playerElegido, i)) piezas.Add(i);
        switch (piezas.Count)
        {
            case 0:
                return;
                //break;
            case 1:
                pieza = (int)piezas[0];
                break;
            default:
                pieza = (int)piezas[UnityEngine.Random.Range(0, piezas.Count)];
                break;

        }
        gridlocks.Add(players[playerElegido, pieza].GetComponent<FollowThePath>().waypoints[players[playerElegido, pieza].GetComponent<FollowThePath>().waypointIndex]);
    }

    private bool trampas()
    {
        if (kapkans.Contains(players[playerElegido, piezaElegida].GetComponent<FollowThePath>().waypoints[players[playerElegido, piezaElegida].GetComponent<FollowThePath>().waypointIndex]))
        {
            players[playerElegido, piezaElegida].GetComponent<FollowThePath>().MoveStart();
            return true;
        }
        if (gridlocks.Contains(players[playerElegido, piezaElegida].GetComponent<FollowThePath>().waypoints[players[playerElegido, piezaElegida].GetComponent<FollowThePath>().waypointIndex]))
        {
            players[playerElegido, piezaElegida].GetComponent<FollowThePath>().movible = false;
            return true;
        }
        return false;
    }

    private void tiroExtra()     //PODER 6 -- Tenés un tiro extra.
    {
        tiroExtra_ = true;
    }

    private static void givePower()
    {
        List<int> poderesDisponibles = new List<int>();
        for (int i = 0;i < 6;i++) if (!powers[playerElegido, i].GetComponent<Power>().disponible) poderesDisponibles.Add(i);
        UnityEngine.Debug.Log(poderesDisponibles.Count);
        switch (poderesDisponibles.Count)
        {
            case 0:
                UnityEngine.Debug.Log("0");
                return;
            //break;
            case 1:
                powers[playerElegido, poderesDisponibles[0]].GetComponent<Power>().disponible = true;
                UnityEngine.Debug.Log("1");
                break;
            default:
                powers[playerElegido, poderesDisponibles[UnityEngine.Random.Range(0, poderesDisponibles.Count)]].GetComponent<Power>().disponible = true;
                UnityEngine.Debug.Log("MUCHOS");
                break;
        }
    }

    private static bool powerActive()
    {
        for (int i = 0;i < 6;i++) if (powers[Dice.whosTurn, i].GetComponent<Power>().activo)
            {
                UnityEngine.Debug.Log("powerActive");
                power = i;
                return true;
            }
        return false;
    }
    
    private static int ganador()
    {
        for (int i = 0;i < 4; i++)
        {
            int contadorPiezasFinales = 0;
            for (int j = 0;j < 4; j++) if (players[i, j].GetComponent<FollowThePath>().waypointIndex == players[i, j].GetComponent<FollowThePath>().waypoints.Length - 1) contadorPiezasFinales++;
            if (contadorPiezasFinales == 4)
            {
                playerTexts[i].GetComponent<UnityEngine.UI.Text>().text = "GANASTE";
                return i;
            }
        }
        return 4;
    }

    private static bool sePaso(int player, int pieza)
    {
        return (players[player, pieza].GetComponent<FollowThePath>().waypointIndex + diceSideThrown >= players[player, pieza].GetComponent<FollowThePath>().waypoints.Length);
    }

    private static bool movible(int player, int pieza)
    {
        if (players[player, pieza].GetComponent<FollowThePath>().waypointIndex + diceSideThrown == 57) return true;
        if (sePaso(player, pieza)) return false;
        if (salida(player, pieza) && (diceSideThrown == 1 || diceSideThrown == 6))
        {
            for (int j = 0; j < 4; j++)
            {
                if (j != pieza)
                {
                    if (players[player, pieza].GetComponent<FollowThePath>().waypoints[players[player, pieza].GetComponent<FollowThePath>().waypointIndex + 1] == players[player, j].GetComponent<FollowThePath>().waypoints[players[player, j].GetComponent<FollowThePath>().waypointIndex]) return false;
                }
            }
            return true;
        }
        else if (salida(player, pieza)) return false;
        for (int i = 0; i < 4; i++)
        {
            if (i != player)
            {
                for (int j = 0;j < 4; j++)
                {
                    if (players[player, pieza].GetComponent<FollowThePath>().waypoints[players[player, pieza].GetComponent<FollowThePath>().waypointIndex + diceSideThrown] == players[i, j].GetComponent<FollowThePath>().waypoints[players[i, j].GetComponent<FollowThePath>().waypointIndex] && !(players[i, j].GetComponent<FollowThePath>().comible())) return false;
                }
            }
            else
            {
                for (int j = 0; j < 4; j++)
                {
                    if (j != pieza)
                    {
                        if (players[player, pieza].GetComponent<FollowThePath>().waypoints[players[player, pieza].GetComponent<FollowThePath>().waypointIndex + diceSideThrown] == players[i, j].GetComponent<FollowThePath>().waypoints[players[i, j].GetComponent<FollowThePath>().waypointIndex]) return false;
                    }
                }
            }
        }
        return true;
    }

    private static bool come(int player, int pieza)
    {
        if (!movible(player, pieza)) return false;
        if (salida(player, pieza)) diceSideThrown = 1;
        bool comio_ = false;
        for (int i = 0;i < 4; i++)
        {
            if (i != player)
            {
                for (int j = 0;j < 4; j++)
                {
                    if (players[player, pieza].GetComponent<FollowThePath>().waypoints[players[player, pieza].GetComponent<FollowThePath>().waypointIndex + diceSideThrown] == players[i, j].GetComponent<FollowThePath>().waypoints[players[i, j].GetComponent<FollowThePath>().waypointIndex] && players[i, j].GetComponent<FollowThePath>().comible())
                    {
                        players[i, j].GetComponent<FollowThePath>().MoveStart();
                        comio_ = true;
                    }
                }
            }
        }
        return comio_;
    }

    private static bool salida(int player, int píeza)
    {
        if (players[player, píeza].GetComponent<FollowThePath>().waypointIndex == 0)
        {
            startWaypoints[player, píeza] = 0;
            return true;
        }
        return false;
    }

    private static int cantPiezasMovibles_(int player)
    {
        int cant = 0;
        for (int i = 0; i < 4; i++) if (movible(player, i)) cant++;
        return cant;
    }

    private static int unicaPiezaMovible(int player)
    {
        for (int i = 0; i < 4; i++) if (movible(player, i)) return i;
        return 0;
    }

    public static void MovePlayer (int player)
    {
        comio = false;
        playerElegido = player;
        mortalidad();
        for (int i = 0;i < 6;i++) if (powers[player, i].GetComponent<Power>().activo) powers[player, i].GetComponent<Power>().activo = false;
        int cantPiezasMovibles = cantPiezasMovibles_(player);
        if (cantPiezasMovibles == 0) return;
        if (cantPiezasMovibles == 1)
        {
            piezaElegida = unicaPiezaMovible(player);
            piezaYaSeleccionada = true;
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                players[player, i].GetComponent<FollowThePath>().colliderActivo = false;
            }
            seleccionandoPieza = true;
        }
        seleccionandoJugador = true;
    }

    private static void mover()
    {
        comio = come(playerElegido, piezaElegida);
        int waypoint = players[playerElegido, piezaElegida].GetComponent<FollowThePath>().waypointIndex + diceSideThrown;
        if (waypoint == 9 || waypoint == 22 || waypoint == 35 || waypoint == 48) givePower();
        players[playerElegido, piezaElegida].GetComponent<FollowThePath>().moveAllowed = true;
        seleccionandoJugador = false;
        tramp = true;
    }

    public static bool jugadorEnMovimiento()
    {
        for (int i = 0;i < 4; i++)
        {
            for (int j = 0;j < 4; j++)
            {
                if (players[i, j].GetComponent<FollowThePath>().moviendo()) return true;
            }
        }
        return false;
    }
}
