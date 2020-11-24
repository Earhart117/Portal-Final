using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Portal : MonoBehaviour {
 //LINK THE PORTAL
    public Portal linkedPortal;
    public MeshRenderer screen;
    public int recursionLimit = 5;

    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    [SerializeField] private Portal otherPortal;
    private List<PortalTraveller> portalObjects = new List<PortalTraveller>();
    RenderTexture viewTexture;
    Camera portalCam;
    Camera playerCam;
    Material firstRecursionMat;
    List<PortalTraveller> trackedTravellers;
    MeshFilter screenMeshFilter;
    Collider wallCollider;
    Collider player;
    PortalTraveller traveller;
    private void Start()
    {
        PlacePortal(wallCollider, transform.position, transform.rotation);
        player = GetComponent<CapsuleCollider>();
    }
    void Awake () {
        playerCam = Camera.main;
        portalCam = GetComponentInChildren<Camera> ();
        portalCam.enabled = false;
        trackedTravellers = new List<PortalTraveller> ();
        screenMeshFilter = screen.GetComponent<MeshFilter> ();
        screen.material.SetInt ("displayMask", 1);
    }

    void LateUpdate () {
        HandleTravellers ();
    }

    void HandleTravellers () {

        for (int i = 0; i < trackedTravellers.Count; i++) {
            PortalTraveller traveller = trackedTravellers[i];
            Transform travellerT = traveller.transform;
            var m = linkedPortal.transform.localToWorldMatrix * transform.worldToLocalMatrix * travellerT.localToWorldMatrix;

            Vector3 offsetFromPortal = travellerT.position - transform.position;
            int portalSide = System.Math.Sign (Vector3.Dot (offsetFromPortal, transform.forward));
            int portalSideOld = System.Math.Sign (Vector3.Dot (traveller.previousOffsetFromPortal, transform.forward));
            // Teleport if you cross through to other side
            if (portalSide != portalSideOld) {
                var positionOld = travellerT.position;
                var rotOld = travellerT.rotation;
                traveller.Teleport (transform, linkedPortal.transform, m.GetColumn (3), m.rotation);
                traveller.graphicsClone.transform.SetPositionAndRotation (positionOld, rotOld);
                Debug.Log("Player passed through portal");
                // trigger enter/exit wont call reliably since running on fixxed update
                linkedPortal.OnTravellerEnterPortal (traveller);
                trackedTravellers.RemoveAt (i);
                i--;

            } else {
                traveller.graphicsClone.transform.SetPositionAndRotation (m.GetColumn (3), m.rotation);
                // UpdateSliceParams 
                traveller.previousOffsetFromPortal = offsetFromPortal;
            }
        }
    }

    // update slice params before portal cams render=
    public void PrePortalRender () {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams (traveller);
        }
    }

    // manually renderportal camera 
    public void Render () {

        

        CreateViewTexture ();
        //allow for portal recursion
        var localToWorldMatrix = playerCam.transform.localToWorldMatrix;
        var renderPositions = new Vector3[recursionLimit];
        var renderRotations = new Quaternion[recursionLimit];
        Debug.Log("Portal view rendered");
        int startIndex = 0;
        portalCam.projectionMatrix = playerCam.projectionMatrix;
        for (int i = 0; i < recursionLimit; i++) {
            if (i > 0) {
                //dont render recursion if linked portal isnt visble
                if (!CameraUtility.BoundsOverlap (screenMeshFilter, linkedPortal.screenMeshFilter, portalCam)) {
                    Debug.Log("Portal not visible");
                    break;
                }
            }
            localToWorldMatrix = transform.localToWorldMatrix * linkedPortal.transform.worldToLocalMatrix * localToWorldMatrix;
            int renderOrderIndex = recursionLimit - i - 1;
            renderPositions[renderOrderIndex] = localToWorldMatrix.GetColumn (3);
            renderRotations[renderOrderIndex] = localToWorldMatrix.rotation;

            portalCam.transform.SetPositionAndRotation (renderPositions[renderOrderIndex], renderRotations[renderOrderIndex]);
            startIndex = renderOrderIndex;
        }

        // hide portal screen so that camera can see through portal
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;
        linkedPortal.screen.material.SetInt ("displayMask", 0);
        //limit amount of recursions
        for (int i = startIndex; i < recursionLimit; i++) {
            portalCam.transform.SetPositionAndRotation (renderPositions[i], renderRotations[i]);
            SetNearClipPlane ();
            HandleClipping ();
            portalCam.Render ();

            if (i == startIndex) {
                linkedPortal.screen.material.SetInt ("displayMask", 1);
            }
        }

        // show hidden objects at render
        screen.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.On;
    }

    void HandleClipping () {
        //handle clipping while player is sliced in portal
        const float hideDst = -1000;
        const float showDst = 1000;
        float screenThickness = linkedPortal.ProtectScreenFromClipping (portalCam.transform.position);

        foreach (var traveller in trackedTravellers) {
            if (SameSideOfPortal (traveller.transform.position, portalCamPos)) {
                // hide mesh on back of portal
                traveller.SetSliceOffsetDst (hideDst, false);
            } else {
                //hide sliced mesh seam while in portal
                traveller.SetSliceOffsetDst (showDst, false);
            }

            // Ensure clone is properly sliced, in case it's visible through this portal:
            int cloneSideOfLinkedPortal = -SideOfPortal (traveller.transform.position);
            bool camSameSideAsClone = linkedPortal.SideOfPortal (portalCamPos) == cloneSideOfLinkedPortal;
            if (camSameSideAsClone) {
                traveller.SetSliceOffsetDst (screenThickness, true);
            } else {
                traveller.SetSliceOffsetDst (-screenThickness, true);
            }
        }
        //handle portal camera position relative to player
        var offsetFromPortalToCam = portalCamPos - transform.position;
        foreach (var linkedTraveller in linkedPortal.trackedTravellers) {
            var travellerPos = linkedTraveller.graphicsObject.transform.position;
            var clonePos = linkedTraveller.graphicsClone.transform.position;
            // handle clone of linked portal coming through this portal
            bool cloneOnSameSideAsCam = linkedPortal.SideOfPortal (travellerPos) != SideOfPortal (portalCamPos);
            if (cloneOnSameSideAsCam) {
                // hide mesh on back of portal
                linkedTraveller.SetSliceOffsetDst (hideDst, true);
            } else {
                //hide sliced mesh seam while in portal
                linkedTraveller.SetSliceOffsetDst (showDst, true);
            }

            // make traveller from linked portal properly slice so it looks right if visible from other portal
            bool camSameSideAsTraveller = linkedPortal.SameSideOfPortal (linkedTraveller.transform.position, portalCamPos);
            if (camSameSideAsTraveller) {
                linkedTraveller.SetSliceOffsetDst (screenThickness, false);
            } else {
                linkedTraveller.SetSliceOffsetDst (-screenThickness, false);
            }
        }
    }//place portal where clicked
    public void PlacePortal(Collider wallCollider, Vector3 pos, Quaternion rot)
    {
        //sticks portal to wall with upright and flat postition and rotation relative to surface cast to
        this.wallCollider = wallCollider;
        transform.position = pos;
        transform.rotation = rot;
        transform.position -= transform.forward * 0.001f;
        Debug.Log("Portal placed");
    }

        // used when all portal cams are render 
    public void PostPortalRender () {
        foreach (var traveller in trackedTravellers) {
            UpdateSliceParams (traveller);
        }
        ProtectScreenFromClipping (playerCam.transform.position);
    }
    // apply camerato portal screen
    void CreateViewTexture () {
        if (viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height) {
            if (viewTexture != null) {
                viewTexture.Release ();
            }
            viewTexture = new RenderTexture (Screen.width, Screen.height, 0);
            
            portalCam.targetTexture = viewTexture;
            // show camera view on the screen of the linked portal
            linkedPortal.screen.material.SetTexture ("_MainTex", viewTexture);
        }
    }

    // Set the thickness of the portal screen so it doesnt clip with camera when player goes through
    float ProtectScreenFromClipping (Vector3 viewPoint) {
        float halfHeight = playerCam.nearClipPlane * Mathf.Tan (playerCam.fieldOfView * 0.5f * Mathf.Deg2Rad);
        float halfWidth = halfHeight * playerCam.aspect;
        float dstToNearClipPlaneCorner = new Vector3 (halfWidth, halfHeight, playerCam.nearClipPlane).magnitude;
        float screenThickness = dstToNearClipPlaneCorner;

        Transform screenT = screen.transform;
        bool camFacingSameDirAsPortal = Vector3.Dot (transform.forward, transform.position - viewPoint) > 0;
        screenT.localScale = new Vector3 (screenT.localScale.x, screenT.localScale.y, screenThickness);
        screenT.localPosition = Vector3.forward * screenThickness * ((camFacingSameDirAsPortal) ? 0.5f : -0.5f);
        return screenThickness;
    }

    void UpdateSliceParams (PortalTraveller traveller) {
        // calculate slice normal
        int side = SideOfPortal (traveller.transform.position);
        Vector3 sliceNormal = transform.forward * -side;
        Vector3 cloneSliceNormal = linkedPortal.transform.forward * side;

        // calculate slice position
        Vector3 slicePos = transform.position;
        Vector3 cloneSlicePos = linkedPortal.transform.position;

        // Adjust slice offset so when player is standing on other side of portal to an object thatisslicing, the slice doesn't clip through
        float sliceOffsetDst = 0;
        float cloneSliceOffsetDst = 0;
        float screenThickness = screen.transform.localScale.z;

        bool playerSameSideAsTraveller = SameSideOfPortal (playerCam.transform.position, traveller.transform.position);
        if (!playerSameSideAsTraveller) {
            sliceOffsetDst = -screenThickness;
        }
        bool playerSameSideAsCloneAppearing = side != linkedPortal.SideOfPortal (playerCam.transform.position);
        if (!playerSameSideAsCloneAppearing) {
            cloneSliceOffsetDst = -screenThickness;
        }

        // apply slice parameters
        for (int i = 0; i < traveller.originalMaterials.Length; i++) {
            traveller.originalMaterials[i].SetVector ("sliceCentre", slicePos);
            traveller.originalMaterials[i].SetVector ("sliceNormal", sliceNormal);
            traveller.originalMaterials[i].SetFloat ("sliceOffsetDst", sliceOffsetDst);

            traveller.cloneMaterials[i].SetVector ("sliceCentre", cloneSlicePos);
            traveller.cloneMaterials[i].SetVector ("sliceNormal", cloneSliceNormal);
            traveller.cloneMaterials[i].SetFloat ("sliceOffsetDst", cloneSliceOffsetDst);

        }

    }

    //projection matrix to align portal camera's near clip plane with the surface of the portal
    void SetNearClipPlane () {
        
        Transform clipPlane = transform;
        int dot = System.Math.Sign (Vector3.Dot (clipPlane.forward, transform.position - portalCam.transform.position));

        Vector3 camSpacePos = portalCam.worldToCameraMatrix.MultiplyPoint (clipPlane.position);
        Vector3 camSpaceNormal = portalCam.worldToCameraMatrix.MultiplyVector (clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot (camSpacePos, camSpaceNormal) + nearClipOffset;

        // prevent use of oblique clip plane if very close to portal to prevent visual artifacts
        if (Mathf.Abs (camSpaceDst) > nearClipLimit) {
            Vector4 clipPlaneCameraSpace = new Vector4 (camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            // Update projection based on new clip plane
            // calculate matrix with player cam so  playercamera settings are used
            portalCam.projectionMatrix = playerCam.CalculateObliqueMatrix (clipPlaneCameraSpace);
        } else {
            portalCam.projectionMatrix = playerCam.projectionMatrix;
        }
    }
   // instantiate model clone for slicingv
    void OnTravellerEnterPortal (PortalTraveller traveller) {
        if (!trackedTravellers.Contains (traveller)) {
            Debug.Log("player entered portal");
            //player.enabled = false;
            traveller.EnterPortalThreshold ();
            traveller.previousOffsetFromPortal = traveller.transform.position - transform.position;
            trackedTravellers.Add (traveller);
        }
    }
    //When you enter portal, track traveller and transform to other portal, disable wall collisions
    void OnTriggerEnter (Collider other) {
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller != null) {
            OnTravellerEnterPortal (traveller);
            portalObjects.Add(traveller);
            traveller.SetIsInPortal(this, otherPortal, wallCollider);
        }
    }
    //when exit other portal, stop tracking and denable collisions 
    void OnTriggerExit (Collider other) {
        var traveller = other.GetComponent<PortalTraveller> ();
        if (traveller && trackedTravellers.Contains (traveller)) {
            Debug.Log("player exit portal");
            //player.enabled = true;
            traveller.ExitPortalThreshold ();
            trackedTravellers.Remove (traveller);
            portalObjects.Remove(traveller);
            traveller.ExitPortal(wallCollider);
        }
    }

    

    int SideOfPortal (Vector3 pos) {
        return System.Math.Sign (Vector3.Dot (pos - transform.position, transform.forward));
    }

    bool SameSideOfPortal (Vector3 posA, Vector3 posB) {
        return SideOfPortal (posA) == SideOfPortal (posB);
    }
    //get camera postion
    Vector3 portalCamPos {
        get {
            return portalCam.transform.position;
        }
    }
    // check  that portals are linked
    void OnValidate () {
        if (linkedPortal != null) {
            linkedPortal.linkedPortal = this;
        }
    }
}