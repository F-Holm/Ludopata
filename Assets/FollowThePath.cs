using System.Collections;
using System.ComponentModel.Design;
using System.Diagnostics;
using UnityEngine;

public class FollowThePath : MonoBehaviour
{

    public Transform[] waypoints; 

    [SerializeField]
    private float moveSpeed = 1f;

    [HideInInspector]
    public int waypointIndex = 0;

    public bool moveAllowed = false;

    public Animator animator;

    public bool movingToStart = false;

    private Collider2D collider;
    public bool colliderActivo = false;

    public bool inmortal = false;

    public bool movible;

    public bool moveBack = false;

    // Use this for initialization
    private void Start()
    {
        collider = GetComponent<Collider2D>();
        transform.position = waypoints[waypointIndex].transform.position;
    }

    // Update is called once per frame
    private void Update()
    {
        if (moveAllowed) Move();
        if (moveBack) StartCoroutine(MoveBack());
    }

    private void OnMouseDown()
    {
        colliderActivo = true;
    }

    public bool moviendo()
    {
        return (moveAllowed || movingToStart);
    }

    public void index_1()
    {
        waypointIndex -= 1;
    }

    public bool comible()
    {
        return !(waypointIndex == 0 || waypointIndex == 1 || waypointIndex == 9 || waypointIndex == 22 || waypointIndex == 35 || waypointIndex == 48 || waypointIndex > 51 || inmortal);
    }

    public void MoveStart()
    {
        movingToStart = true;
        StartCoroutine(moveStart());
    }

    public IEnumerator moveStart()
    {
        while (transform.position != waypoints[0].transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                waypoints[0].transform.position,
                moveSpeed * Time.deltaTime * 1.5f);
            animator.SetBool("Moving", true);
            yield return null;
        }
        animator.SetBool("Moving", false);
        waypointIndex = 0;
        movingToStart = false;
    }

    private void Move()
    {
        if (waypointIndex < waypoints.Length)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                waypoints[waypointIndex].transform.position,
                moveSpeed * Time.deltaTime);
            animator.SetBool("Moving", true);

            if (transform.position == waypoints[waypointIndex].transform.position)
            {
                waypointIndex += 1;
                animator.SetBool("Moving", false);
            }
        }
    }

    private IEnumerator MoveBack()
    {
        while (transform.position != waypoints[waypointIndex - 5].transform.position)
        {
            transform.position = Vector3.MoveTowards(transform.position,
                waypoints[0].transform.position,
                moveSpeed * Time.deltaTime);
            animator.SetBool("Moving", true);
            yield return null;
        }
        animator.SetBool("Moving", false);
        waypointIndex = waypointIndex - 5;
        moveBack = false;
    }
}