using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using GoogleARCore;

public class SurfaceScript: MonoBehaviour {
    
    public DetectedPlane trackedPlane;

    public MeshRenderer meshRenderer;
    public Mesh mesh;

	void Start () {
        meshRenderer = GetComponent<MeshRenderer>();
        mesh = new Mesh();
        GetComponent<MeshFilter>().mesh = mesh;
	}
	
	void Update () {
        if (trackedPlane == null) return;

        if(trackedPlane.SubsumedBy != null){
            Destroy(gameObject);
            return;
        }

        if(trackedPlane.TrackingState != TrackingState.Tracking){
            meshRenderer.enabled = false;
            return;
        }

        meshRenderer.enabled = true;

        UpdateVerticles();
    
    }


    void UpdateVerticles(){
        var vertices = new List<Vector3>();
        trackedPlane.GetBoundaryPolygon(vertices);

        vertices.Add(trackedPlane.CenterPose.position);
        var triangles = new List<int>();
       
        for (int i = 0; i < vertices.Count -1 ; i++){
            triangles.Add(vertices.Count - 1);
            triangles.Add(i);
            triangles.Add(i == vertices.Count - 2 ? 0 : i + 1);
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles.ToArray(), 0);
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
    }
}
