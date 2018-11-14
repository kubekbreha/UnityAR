using System.Collections.Generic;
using GoogleARCore;
using UnityEngine;


public class PlaneVisualizer : MonoBehaviour
{
    private static int planeCount = 0;

    private readonly Color[] planeColors = new Color[]{
        new Color(1.0f, 1.0f, 1.0f),
        new Color(0.956f, 0.262f, 0.211f),
        new Color(0.913f, 0.117f, 0.388f),
        new Color(0.611f, 0.152f, 0.654f),
        new Color(0.403f, 0.227f, 0.717f),
        new Color(0.247f, 0.317f, 0.709f),
        new Color(0.129f, 0.588f, 0.952f),
        new Color(0.011f, 0.662f, 0.956f),
        new Color(0f, 0.737f, 0.831f),
        new Color(0f, 0.588f, 0.533f),
        new Color(0.298f, 0.686f, 0.313f),
        new Color(0.545f, 0.764f, 0.290f),
        new Color(0.803f, 0.862f, 0.223f),
        new Color(1.0f, 0.921f, 0.231f),
        new Color(1.0f, 0.756f, 0.027f)
    };

    private DetectedPlane detectedPlane;

    // Keep previous frame's mesh polygon to avoid mesh update every frame.
    private List<Vector3> previousFrameMeshVertices = new List<Vector3>();
    private List<Vector3> meshVertices = new List<Vector3>();
    private Vector3 planeCenter = new Vector3();

    private List<Color> meshColors = new List<Color>();
    
    private List<int> meshIndices = new List<int>();

    private Mesh mesh;

    private MeshRenderer meshRenderer;

    
    public void Awake(){
        mesh = GetComponent<MeshFilter>().mesh;
        meshRenderer = GetComponent<MeshRenderer>();
    }


    public void Update(){
        if (detectedPlane == null){
            return;
        }else if (detectedPlane.SubsumedBy != null){
            Destroy(gameObject);
            return;
        }else if (detectedPlane.TrackingState != TrackingState.Tracking){
            meshRenderer.enabled = false;
            return;
        }

        meshRenderer.enabled = true;

        _UpdateMeshIfNeeded();
    }


    public void Initialize(DetectedPlane plane){
        detectedPlane = plane;
        meshRenderer.material.SetColor("_GridColor", planeColors[planeCount++ % planeColors.Length]);
        meshRenderer.material.SetFloat("_UvRotation", Random.Range(0.0f, 360.0f));

        Update();
    }


    private void _UpdateMeshIfNeeded(){
        detectedPlane.GetBoundaryPolygon(meshVertices);

        if (_AreVerticesListsEqual(previousFrameMeshVertices, meshVertices)){
            return;
        }

        previousFrameMeshVertices.Clear();
        previousFrameMeshVertices.AddRange(meshVertices);

        planeCenter = detectedPlane.CenterPose.position;

        Vector3 planeNormal = detectedPlane.CenterPose.rotation * Vector3.up;

        meshRenderer.material.SetVector("_PlaneNormal", planeNormal);

        int planePolygonCount = meshVertices.Count;

        // The following code converts a polygon to a mesh with two polygons, inner
        // polygon renders with 100% opacity and fade out to outter polygon with opacity 0%, as shown below.
        // The indices shown in the diagram are used in comments below.
        // _______________     0_______________1
        // |             |      |4___________5|
        // |             |      | |         | |
        // |             | =>   | |         | |
        // |             |      | |         | |
        // |             |      |7-----------6|
        // ---------------     3---------------2
        meshColors.Clear();

        // Fill transparent color to vertices 0 to 3.
        for (int i = 0; i < planePolygonCount; ++i)
        {
            meshColors.Add(Color.clear);
        }

        // Feather distance 0.2 meters.
        const float featherLength = 0.2f;

        // Feather scale over the distance between plane center and vertices.
        const float featherScale = 0.2f;

        // Add vertex 4 to 7.
        for (int i = 0; i < planePolygonCount; ++i)
        {
            Vector3 v = meshVertices[i];

            // Vector from plane center to current point
            Vector3 d = v - planeCenter;

            float scale = 1.0f - Mathf.Min(featherLength / d.magnitude, featherScale);
            meshVertices.Add((scale * d) + planeCenter);

            meshColors.Add(Color.white);
        }

        meshIndices.Clear();
        int firstOuterVertex = 0;
        int firstInnerVertex = planePolygonCount;

        // Generate triangle (4, 5, 6) and (4, 6, 7).
        for (int i = 0; i < planePolygonCount - 2; ++i)
        {
            meshIndices.Add(firstInnerVertex);
            meshIndices.Add(firstInnerVertex + i + 1);
            meshIndices.Add(firstInnerVertex + i + 2);
        }

        // Generate triangle (0, 1, 4), (4, 1, 5), (5, 1, 2), (5, 2, 6), (6, 2, 3), (6, 3, 7)
        // (7, 3, 0), (7, 0, 4)
        for (int i = 0; i < planePolygonCount; ++i)
        {
            int outerVertex1 = firstOuterVertex + i;
            int outerVertex2 = firstOuterVertex + ((i + 1) % planePolygonCount);
            int innerVertex1 = firstInnerVertex + i;
            int innerVertex2 = firstInnerVertex + ((i + 1) % planePolygonCount);

            meshIndices.Add(outerVertex1);
            meshIndices.Add(outerVertex2);
            meshIndices.Add(innerVertex1);

            meshIndices.Add(innerVertex1);
            meshIndices.Add(outerVertex2);
            meshIndices.Add(innerVertex2);
        }

        mesh.Clear();
        mesh.SetVertices(meshVertices);
        mesh.SetIndices(meshIndices.ToArray(), MeshTopology.Triangles, 0);
        mesh.SetColors(meshColors);
    }


    private bool _AreVerticesListsEqual(List<Vector3> firstList, List<Vector3> secondList){
        if (firstList.Count != secondList.Count){
            return false;
        }

        for (int i = 0; i < firstList.Count; i++){
            if (firstList[i] != secondList[i]){
                return false;
            }
        }

        return true;
    }
}

