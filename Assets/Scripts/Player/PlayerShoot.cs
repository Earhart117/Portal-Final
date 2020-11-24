using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerShoot : MonoBehaviour
{
    [SerializeField] Camera cameraController;
    [SerializeField] Transform rayOrigin;
    [SerializeField] float shootDistance = 60f;
    [SerializeField] LayerMask hitLayers;
    [SerializeField] GameObject wallHitEffect;
    [SerializeField] GameObject orangePortal;
    [SerializeField] GameObject bluePortal;
 

    RaycastHit objectHit; // stores raycast hit info

    void Update()
    {
        
        if (Input.GetKeyDown(KeyCode.Mouse0)) 
        {
            Debug.Log("Fire 1 pressed");
            ShootBlue();
        }

        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Debug.Log("Fire 2 pressed");
            ShootOrange();
        }
    }
    //Shoot gun using raycast
    void ShootBlue()
    {
        //calc direction to shoot
        Vector3 rayDirection = cameraController.transform.forward;
        //cast debug ray
        Debug.DrawRay(rayOrigin.position, rayDirection * shootDistance, Color.blue, 1f);
        //do raycast
        if (Physics.Raycast(rayOrigin.position, rayDirection, out objectHit, shootDistance))
        {

            Debug.Log("You Hit the  " + objectHit.transform.name);

           
            if (objectHit.transform.tag == "Wall")
            {
                float offset = .1f;   
                bluePortal.transform.position = objectHit.point;
                Quaternion normalToQuat = Quaternion.LookRotation(objectHit.normal *180);
                bluePortal.transform.SetPositionAndRotation(objectHit.point - rayDirection.normalized * offset, normalToQuat);;

                
                Debug.Log("Hit wall");
            }





           
        }
        else
        {
            Debug.Log("Miss");
        }
    }
    void ShootOrange()
    {
        //calc direction to shoot
        Vector3 rayDirection = cameraController.transform.forward;
        //cast debug ray
        Debug.DrawRay(rayOrigin.position, rayDirection * shootDistance, Color.blue, 1f);
        //do raycast
        if (Physics.Raycast(rayOrigin.position, rayDirection, out objectHit, shootDistance))
        {

            Debug.Log("You Hit the  " + objectHit.transform.name);


            if (objectHit.transform.tag == "Wall")
            {
                float offset = .1f;
                orangePortal.transform.position = objectHit.point;
                Quaternion normalToQuat = Quaternion.LookRotation(objectHit.normal * -180);
                orangePortal.transform.SetPositionAndRotation(objectHit.point - rayDirection.normalized*offset, normalToQuat);
                Debug.Log("Hit wall");
            }






        }
        else
        {
            Debug.Log("Miss");
        }
    }
    public void PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        //this.wallCollider = wallCollider;
        transform.position = pos;
        transform.rotation = rot;
        transform.position -= transform.forward * 0.001f;
    }
}
