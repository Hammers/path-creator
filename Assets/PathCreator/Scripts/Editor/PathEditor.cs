using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.Analytics;

[CustomEditor(typeof(PathCreator))]
public class PathEditor : Editor
{
    private PathCreator creator;
    private Path path;

    private const float SEGMENT_SELECT_DISTANCE_THRESHOLD = .1f;
    private int selectedSegmentIndex = -1;
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUI.BeginChangeCheck();
        if (GUILayout.Button("Create New"))
        {
            Undo.RecordObject(creator,"Create New");
            creator.CreatePath();
            path = creator.path;
        }

        bool isClosed = GUILayout.Toggle(path.IsClosed, "Closed");
        if (isClosed != path.IsClosed)
        {
            Undo.RecordObject(creator,"Toggle closed");
            path.IsClosed = isClosed;
        }

        bool autoSetControlPoints = GUILayout.Toggle(path.AutoSetControlPoints, "Auto Set Control Points");
        if (autoSetControlPoints != path.AutoSetControlPoints)
        {
            Undo.RecordObject(creator,"Toggle auto set controls");
            path.AutoSetControlPoints = autoSetControlPoints;
        }

        if (EditorGUI.EndChangeCheck())
        {
            SceneView.RepaintAll();
        }
    }

    void OnSceneGUI()
    {
        Input();
        Draw();
    }

    void Input()
    {
        Event guiEvent = Event.current;
        Vector2 mousePos = HandleUtility.GUIPointToWorldRay(guiEvent.mousePosition).origin;

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 0 && guiEvent.shift)
        {
            if (selectedSegmentIndex != -1)
            {
                Undo.RecordObject(creator,"Split segment");
                path.SplitSegment(mousePos,selectedSegmentIndex);
            }
            else if (!path.IsClosed)
            {
                Undo.RecordObject(creator, "Add segment");
                path.AddSegment(mousePos);
            }
        }

        if (guiEvent.type == EventType.MouseDown && guiEvent.button == 1)
        {
            float minDistToAnchor = .05f;
            int closetAnchorIndex = -1;

            for (int i = 0; i < path.NumPoints; i+=3)
            {
                float dist = Vector2.Distance(mousePos, path[i]);
                if (dist < minDistToAnchor)
                {
                    minDistToAnchor = dist;
                    closetAnchorIndex = i;
                }

                if (closetAnchorIndex != -1)
                {
                    Undo.RecordObject(creator,"Delete segment");
                    path.DeleteSegment(closetAnchorIndex);
                }
            }
        }

        if (guiEvent.type == EventType.MouseMove)
        {
            float minDistToSegment = SEGMENT_SELECT_DISTANCE_THRESHOLD;
            int newSelectedSegmentIndex = -1;
            for (int i = 0; i < path.NumSegments; i++)
            {
                Vector2[] points = path.GetPointsInSegment(i);
                float distance =
                    HandleUtility.DistancePointBezier(mousePos, points[0], points[3], points[1], points[2]);
                if (distance < minDistToSegment)
                {
                    minDistToSegment = distance;
                    newSelectedSegmentIndex = i;
                }
            }

            if (newSelectedSegmentIndex != selectedSegmentIndex)
            {
                selectedSegmentIndex = newSelectedSegmentIndex;
                HandleUtility.Repaint();
            }
        }

        HandleUtility.AddDefaultControl(0);
    }
    
    private void Draw()
    {
        for (int i = 0; i < path.NumSegments; i++)
        {
            Vector2[] points = path.GetPointsInSegment(i);
            if (!path.AutoSetControlPoints)
            {
                Handles.DrawLine(points[0], points[1]);
                Handles.DrawLine(points[2], points[3]);
            }

            Color segmentCol = (i == selectedSegmentIndex && Event.current.shift) ? Color.cyan : Color.green;
            Handles.DrawBezier(points[0],points[3],points[1],points[2],segmentCol, null,2);
        }
        
        Handles.color = Color.red;
        for (int i = 0; i < path.NumPoints; i++)
        {
            bool isAnchorPoint = i % 3 == 0;
            Handles.color = isAnchorPoint ? Color.red : Color.yellow;
            if (!isAnchorPoint && path.AutoSetControlPoints)
                continue;
            
            Vector2 newPos = Handles.FreeMoveHandle(path[i], Quaternion.identity, .1f, Vector3.zero, Handles.CylinderHandleCap);
            if (path[i] != newPos)
            {
                Undo.RecordObject(creator,"Move point");
                path.MovePoint(i,newPos);
            }
        }
    }
    
    private void OnEnable()
    {
        creator = (PathCreator) target;
        if (creator.path == null)
        {
            creator.CreatePath();
        }

        path = creator.path;
    }
}
