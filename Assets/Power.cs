using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using UnityEngine;

public class Power : MonoBehaviour
{
    public Sprite spriteSi;
    public Sprite spriteNo;
    public SpriteRenderer spriteRenderer;
    private int player = 4;
    public bool activo = false;
    public bool disponible = false;
    public bool si = false;
    private Collider2D collider;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        collider = GetComponent<Collider2D>();
        setNo();
    }

    // Update is called once per frame
    void Update()
    {
        if (!GameControl.poderesHabilitados || GameControl.gameOver || !GameControl.gameStarted || Dice.whosTurn != player || !disponible) activo = false;
        if (si && !disponible) setNo();
        else if (!si && disponible) setSi();
    }

    private void OnMouseDown()
    {
        if (GameControl.poderesHabilitados && !GameControl.gameOver && GameControl.gameStarted && Dice.whosTurn == player && disponible) activo = true;
    }

    public void setPlayer(int player)
    {
        this.player = player;
    }

    private void setNo()
    {
        UnityEngine.Debug.Log("NO");
        si = false;
        spriteRenderer.sprite = spriteNo;
    }

    private void setSi()
    {
        UnityEngine.Debug.Log("SI");
        si = true;
        spriteRenderer.sprite = spriteSi;
    }
}
