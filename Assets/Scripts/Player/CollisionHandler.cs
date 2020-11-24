using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionHandler : MonoBehaviour
{
    Collider player;
    GameObject wall;
    private void Start()
    {
        player = GetComponent<CapsuleCollider>();
        wall = GameObject.FindGameObjectWithTag("Wall");
    }
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Wall")
        {
            Debug.Log("colliding with wall");
        }
        if (collision.gameObject.tag == "Portal")
        {
            Debug.Log("colliding with portal");
        }
        if (collision.gameObject.tag == "Portal" && collision.gameObject.tag == "Wall")
        {
            Debug.Log("colliding with portal and wall");
        }
    }
}
