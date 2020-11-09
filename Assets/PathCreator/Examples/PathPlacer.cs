using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PathCreator))]
public class PathPlacer : MonoBehaviour
{
    public float spacing = .1f;
    public float resolution = 1;

    void Start()
    {
        Path path = GetComponent<PathCreator>().path;
        Vector2[] points = path.CalculateEvenlySpacedPoints(spacing, resolution);
        foreach (var point in points)
        {
            GameObject g = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            g.transform.position = point;
            g.transform.localScale = Vector3.one * spacing * .5f;
        }
    }
}
