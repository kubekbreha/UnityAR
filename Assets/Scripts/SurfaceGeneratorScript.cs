using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleARCore;

public class SurfaceGeneratorScript : MonoBehaviour {
    
    public GameObject surfacePrefab;

    private List<DetectedPlane> newPlanes = new List<DetectedPlane>();

    void Update(){
        if (Session.Status != SessionStatus.Tracking){
            return;
        }

        Session.GetTrackables<DetectedPlane>(newPlanes, TrackableQueryFilter.New);
        foreach (var plane in newPlanes){
            GameObject planeObject = Instantiate(surfacePrefab, Vector3.zero, Quaternion.identity, transform);
            //planeObject.GetComponent<Surface>().Initialize(plane);
        }


    }
}
