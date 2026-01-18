using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class FollowMarker : MonoBehaviour
{
    [SerializeField] 
    private GameObject markerObject;

    // Update is called once per frame
    void Update()
    {
        transform.position = markerObject.transform.position;
        
        //transform.rotation = Quaternion.Euler(0,GetUnityHeading(Input.compass.trueHeading),0);
    }

    float GetUnityHeading(float heading)
    {
        return (heading > 180) ? heading - 360 : heading;
    }
}
