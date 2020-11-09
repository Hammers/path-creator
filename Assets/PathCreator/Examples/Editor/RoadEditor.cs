using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadGenerator))]
public class RoadEditor : Editor
{
    private RoadGenerator generator;

    private void OnSceneGUI()
    {
        if (generator.autoUpdate && Event.current.type == EventType.Repaint)
        {
            generator.UpdateRoad();
        }
    }

    private void OnEnable()
    {
        generator = (RoadGenerator) target;
    }
}
