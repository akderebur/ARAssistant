using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using static UnityEngine.GraphicsBuffer;

public class ARPlacement : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public Animator charAnim;
    public Camera arCam;

    public ManomotionManager handManager;
    public BoxCollider boxCollider;
    public BoxCollider pickTarget;
    public GameObject appleObj;
    public GameObject cushionObj;
    public Transform cushionPos;

    GameObject curObj;


    bool placed = false;
    Vector3 placePos = Vector3.zero;

    bool waitPickUp;
    bool picked;

    bool autoPlace = true;

    public void SetSelectedObj(int selId)
    {
        if (selId == 0)
            curObj = appleObj;
        else
            curObj = cushionObj;
    }

    public void SetAutoPlacement(bool isAuto)
    {
        autoPlace = isAuto;
    }

    // Start is called before the first frame update
    void Start()
    {
        curObj = appleObj;
    }

    // Update is called once per frame
    void Update()
    {
        //appleObj.SetActive(true);
        //appleObj.transform.position = arCam.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 1f));

        var lookPos = arCam.transform.position - transform.position;
        lookPos.y = 0;
        var rotation = Quaternion.LookRotation(lookPos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * 2f);

        if (!placed)
        {
            if (autoPlace)
                CheckPlacement();
            else
                ManualPlacement();

            return;
        }

        transform.position = placePos;
        CheckHand();
    }

    void EnablePickUp()
    {
        picked = false;
        waitPickUp = false;
    }


    void PickObj()
    {
        picked = true;
        appleObj.SetActive(false);

        Invoke("EnablePickUp", 5f);
    }

    void Sit()
    {
        picked = true;
        curObj.transform.position = cushionPos.position;
        curObj = null;
        charAnim.SetTrigger("Sit");
    }

    void CheckHand()
    {
        if (handManager.Hand_infos != null && handManager.Hand_infos.Length > 0)
        {
            HandInfoUnity handInfoUnity = handManager.Hand_infos[0];
            //if (handInfoUnity.hand_info.gesture_info.mano_class == ManoClass.POINTER_GESTURE)
            //{
            //    if (handInfoUnity.hand_info.gesture_info.state >= 9)
            //    {
            //        Vector3 top = Camera.main.WorldToViewportPoint(transform.position + Vector3.up * boxCollider.center.y + Vector3.up * boxCollider.bounds.extents.y);
            //        Vector3 bottom = Camera.main.WorldToViewportPoint(transform.position + Vector3.up * boxCollider.center.y + Vector3.down * boxCollider.bounds.extents.y);

            //        float handY = handInfoUnity.hand_info.tracking_info.bounding_box.top_left.y;
            //        if (handY < top.y && handY > bottom.y)
            //            charAnim.SetTrigger("Wave");
            //    }
            //}
            if (handInfoUnity.hand_info.gesture_info.mano_class == ManoClass.PINCH_GESTURE)
            {
                if (picked)
                    return;

                if (curObj == appleObj)
                {
                    Vector3 poiViewPoint = handInfoUnity.hand_info.tracking_info.poi;
                    poiViewPoint.z = transform.position.z + (arCam.transform.position - transform.position).normalized.z * 0.2f;
                    Vector3 pinchWorldPos = arCam.ViewportToWorldPoint(poiViewPoint);

                    float dist = Vector3.Distance(transform.position, arCam.transform.position);
                    curObj.transform.position = pinchWorldPos;
                    curObj.transform.localScale = Vector3.one * 1f / dist * transform.localScale.x;
                    curObj.SetActive(true);
                }
                else
                {
                    Vector3 poiViewPoint = handInfoUnity.hand_info.tracking_info.poi;
                    poiViewPoint.z = transform.position.z + (arCam.transform.position + transform.position).normalized.z * 0.5f;
                    Vector3 pinchWorldPos = arCam.ViewportToWorldPoint(poiViewPoint);

                    float dist = Vector3.Distance(transform.position, arCam.transform.position);
                    curObj.transform.position = pinchWorldPos;
                    curObj.transform.localScale = Vector3.one * 1f / dist * 0.1f * transform.localScale.x;
                    curObj.SetActive(true);
                }

                if (!waitPickUp)
                {
                    Vector3 left = Camera.main.WorldToViewportPoint(transform.position + Vector3.left * boxCollider.bounds.extents.x * transform.lossyScale.x);
                    Vector3 right = Camera.main.WorldToViewportPoint(transform.position + Vector3.right * boxCollider.bounds.extents.x * transform.lossyScale.x);

                    Vector3 top = Camera.main.WorldToViewportPoint(transform.position + Vector3.up * boxCollider.center.y + Vector3.up * boxCollider.bounds.extents.y * transform.lossyScale.y);
                    Vector3 bottom = Camera.main.WorldToViewportPoint(transform.position + Vector3.up * boxCollider.center.y + Vector3.down * boxCollider.bounds.extents.y * transform.lossyScale.y);

                    //Vector3 left = Camera.main.WorldToViewportPoint(transform.position + pickTarget.transform.localPosition + Vector3.left * pickTarget.bounds.extents.x);
                    //Vector3 right = Camera.main.WorldToViewportPoint(transform.position + pickTarget.transform.localPosition + Vector3.right * boxCollider.bounds.extents.x);

                    //Vector3 top = Camera.main.WorldToViewportPoint(transform.position + pickTarget.transform.localPosition + Vector3.up * boxCollider.bounds.extents.y);
                    //Vector3 bottom = Camera.main.WorldToViewportPoint(transform.position + pickTarget.transform.localPosition + Vector3.down * boxCollider.bounds.extents.y);

                    float handX = handInfoUnity.hand_info.tracking_info.poi.x;
                    float handY = handInfoUnity.hand_info.tracking_info.poi.y;
                    if (handY < top.y && handY > bottom.y && handX < right.x && handX > left.x)
                    {
                        if (curObj == appleObj)
                        {
                            charAnim.SetTrigger("Pick");
                            waitPickUp = true;
                            Invoke("PickObj", 1f);
                        }
                        else
                        {
                            Sit();
                        }
                    }
                }

            }
            else
            {
                //appleObj.SetActive(false);
                if(curObj != null)
                    curObj.SetActive(false);
            }

        }
    }

    void CheckPlacement()
    {
        Vector2 screenPosition = Camera.main.ViewportToScreenPoint(new Vector2(0.5f, 0.5f));
        List<ARRaycastHit> hits = new List<ARRaycastHit>();
        raycastManager.Raycast(screenPosition, hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes);

        if (hits.Count > 0)
        {
            transform.position = hits[0].pose.position;
            //transform.rotation = hits[0].pose.rotation;
            placePos = transform.position;
        }

        //if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        //{
        //    transform.GetChild(0).parent = null;
        //    charAnim.SetTrigger("Wave");
        //    placed = true;
        //}
    }

    private float initialDistance;
    private Vector3 initialScale;
    void ManualPlacement()
    {
        if (Input.touchCount == 1)
        {
            var touch = Input.GetTouch(0);
            Vector3 pos = transform.position;
            pos += new Vector3(touch.deltaPosition.x, 0f, touch.deltaPosition.y) / 1000f;
            transform.position = pos;
            placePos = pos;
        }
        else if (Input.touchCount == 2) // Scale
        {
            var touchZero = Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            // if one of the touches Ended or Canceled do nothing
            if (touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled
               || touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled)
            {
                return;
            }

            // It is enough to check whether one of them began since we
            // already excluded the Ended and Canceled phase in the line before
            if (touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began)
            {
                // track the initial values
                initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                initialScale = transform.localScale;
            }
            // else now is any other case where touchZero and/or touchOne are in one of the states
            // of Stationary or Moved
            else
            {
                // otherwise get the current distance
                var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);

                // A little emergency brake ;)
                if (Mathf.Approximately(initialDistance, 0)) return;

                // get the scale factor of the current distance relative to the inital one
                var factor = currentDistance / initialDistance;

                // apply the scale
                // instead of a continuous addition rather always base the 
                // calculation on the initial and current value only
                transform.localScale = initialScale * factor;
            }
        }
    }

    public void FinalizePlacement()
    {
        charAnim.SetTrigger("Wave");
        placed = true;
    }


}
