using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PathLine : MonoBehaviour{
    public Vector3[] nodes;
    public float[] distances;
    private float maxDist;


    private Vector3 currentPostion;
    private Vector3 currentRotation;
    
    private Vector3 DifPosition;
    private Vector3 DifRotation;
    private Vector3 DifScale;
    #region Iadded

    private void Start()
    {
        currentPostion = transform.position;
        currentRotation = transform.rotation.eulerAngles;
    }

    private void Update()
    {
        if (PauseManager.paused)
        {
            return;
        }
        
        if (transform.position != currentPostion)
        {
            DifPosition = currentPostion - transform.position;

            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i] -= DifPosition;
            }

            currentPostion = transform.position;
        }
        
        if (transform.rotation.eulerAngles != currentRotation)
        {
            DifRotation = currentRotation - transform.rotation.eulerAngles;
            
            for (var i = 0; i < nodes.Length; i++)
            {
                nodes[i] = RotatePointAroundPivot(nodes[i], transform.position, DifRotation);
            }

            currentRotation = transform.rotation.eulerAngles;
        }
    }

    #endregion
    
    private Vector3 RotatePointAroundPivot(Vector3 point, Vector3  pivot, Vector3 angles) {
        var dir = point - pivot; // get point direction relative to pivot
        dir = Quaternion.Euler(angles) * dir; // rotate it
        point = dir + pivot; // calculate rotated point
        return point; // return it
    }
    private void OnDrawGizmos()
    {
        Draw();
    }

    private void CalcDistances()
    {
        distances = new float[nodes.Length];
        distances[0] = 0;

        for (var i = 0; i < nodes.Length - 1; i++)
        {
            distances[i + 1] = distances[i] + Vector3.Distance(nodes[i], nodes[i + 1]);
        }

        maxDist = distances[^1];
    }
    
    public void Draw()
    {
        for (var i = 0; i < nodes.Length - 1; i++)
        {
            Debug.DrawLine(nodes[i], nodes[i + 1], Color.green, 0.0f, false);
        }
    }

    public float GetParam(Vector3 position)
    {
        int closestSegment = GetClosestSegment(position);

        if (distances.Length == 0)
            CalcDistances();

        float param = this.distances[closestSegment] + GetParamForSegment(position, nodes[closestSegment], nodes[closestSegment + 1]);

        return param;
    }

    public int GetClosestSegment(Vector3 position)
    {
        /* Find the first point in the closest line segment to the path */
        float closestDist = DistToSegment(position, nodes[0], nodes[1]);
        int closestSegment = 0;

        for (int i = 1; i < nodes.Length - 1; i++)
        {
            float dist = DistToSegment(position, nodes[i], nodes[i + 1]);

            if (dist <= closestDist)
            {
                closestDist = dist;
                closestSegment = i;
            }
        }

        return closestSegment;
    }

    private static float DistToSegment(Vector3 p, Vector3 v, Vector3 w)
    {
        Vector3 vw = w - v;

        float l2 = Vector3.Dot(vw, vw);

        if (l2 == 0)
        {
            return Vector3.Distance(p, v);
        }

        float t = Vector3.Dot(p - v, vw) / l2;

        if (t < 0)
        {
            return Vector3.Distance(p, v);
        }

        if (t > 1)
        {
            return Vector3.Distance(p, w);
        }

        Vector3 closestPoint = Vector3.Lerp(v, w, t);

        return Vector3.Distance(p, closestPoint);
    }

    public Vector3 GetPosition(float param, bool pathLoop = false)
    {
        /* Make sure the param is not past the beginning or end of the path */
        if (param < 0)
        {
            param = (pathLoop) ? param + maxDist : 0;
        }
        else if (param > maxDist)
        {
            param = (pathLoop) ? param - maxDist : maxDist;
        }

        /* Find the first node that is farther than given param */
        int i = 0;
        for (; i < distances.Length; i++)
        {
            if (distances[i] > param)
            {
                break;
            }
        }

        /* Convert it to the first node of the line segment that the param is in */
        if (i > distances.Length - 2)
        {
            i = distances.Length - 2;
        }
        else
        {
            i -= 1;
        }

        /* Get how far along the line segment the param is */
        float t = (param - distances[i]) / Vector3.Distance(nodes[i], nodes[i + 1]);

        /* Get the position of the param */
        return Vector3.Lerp(nodes[i], nodes[i + 1], t);
    }

    private static float GetParamForSegment(Vector3 p, Vector3 v, Vector3 w)
    {
        Vector3 vw = w - v;

        float l2 = Vector3.Dot(vw, vw);

        if (l2 == 0)
        {
            return 0;
        }

        float t = Vector3.Dot(p - v, vw) / l2;

        if (t < 0)
        {
            t = 0;
        }
        else if (t > 1)
        {
            t = 1;
        }

        return t * Mathf.Sqrt(l2);
    }
}
