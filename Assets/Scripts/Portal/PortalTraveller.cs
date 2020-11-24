using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(Collider))]
public class PortalTraveller : MonoBehaviour {

    public GameObject graphicsObject;
    public GameObject graphicsClone { get; set; }
    public Vector3 previousOffsetFromPortal { get; set; }
    public Material[] originalMaterials { get; set; }
    public Material[] cloneMaterials { get; set; }
    new Collider collider;
    private new Rigidbody rigidbody;
    private Portal inPortal;
    private Portal outPortal;
    private int inPortalCount = 0;
    protected virtual void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        collider = GetComponent<Collider>();
    }
    public virtual void Teleport (Transform fromPortal, Transform toPortal, Vector3 pos, Quaternion rot) {
        transform.position = pos;
        transform.rotation = rot;
    }

    // called when first enter portal, loads clone for slicing
    public virtual void EnterPortalThreshold () {
        if (graphicsClone == null) {
            graphicsClone = Instantiate (graphicsObject);
            graphicsClone.transform.parent = graphicsObject.transform.parent;
            graphicsClone.transform.localScale = graphicsObject.transform.localScale;
            originalMaterials = GetMaterials (graphicsObject);
            cloneMaterials = GetMaterials (graphicsClone);
        } else {
            graphicsClone.SetActive (true);
        }
    }

    // called after leaving other teleporter
    public virtual void ExitPortalThreshold () {
        graphicsClone.SetActive (false);
        // Disable slicing
        for (int i = 0; i < originalMaterials.Length; i++) {
            originalMaterials[i].SetVector ("sliceNormal", Vector3.zero);
        }
    }

    public void SetSliceOffsetDst (float dst, bool clone) {
        for (int i = 0; i < originalMaterials.Length; i++) {
            if (clone) {
                cloneMaterials[i].SetFloat ("sliceOffsetDst", dst);
            } else {
                originalMaterials[i].SetFloat ("sliceOffsetDst", dst);
            }

        }
    }

    Material[] GetMaterials (GameObject g) {
        var renderers = g.GetComponentsInChildren<MeshRenderer> ();
        var matList = new List<Material> ();
        foreach (var renderer in renderers) {
            foreach (var mat in renderer.materials) {
                matList.Add (mat);
            }
        }
        return matList.ToArray ();
    }
    //set isin portal to disable wall collision allowing travel when against wall
    public void SetIsInPortal(Portal inPortal, Portal outPortal, Collider wallCollider)
    {
        this.inPortal = inPortal;
        this.outPortal = outPortal;

        Physics.IgnoreCollision(collider, wallCollider);

        //cloneObject.SetActive(true);

        ++inPortalCount;
    }
    //renabke wall collision when out of other portal
    public void ExitPortal(Collider wallCollider)
    {
        Physics.IgnoreCollision(collider, wallCollider, false);
        --inPortalCount;

        
    }
    //ignore collion of wall?
    public void EnableCollsion(Collider wallCollider)
    {
        Physics.IgnoreCollision(collider, wallCollider, false);
 
    }
}