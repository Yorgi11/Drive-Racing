using System.Collections.Generic;
using UnityEngine;
[ExecuteAlways]
public class Bezier2D : MonoBehaviour
{
    [System.Serializable]
    private class Segment
    {
        public Transform startPoint;
        public Transform startHandle;
        public Transform endPoint;
        public Transform endHandle;
        public Vector3 GetPoint(float t)
        {
            if (startPoint == null || startHandle == null || endHandle == null || endPoint == null) return Vector3.zero;

            Vector3 a = startPoint.position;
            Vector3 b = startHandle.position;

            // Mirror end handle around the end point
            Vector3 endCtrl = endHandle.position;
            Vector3 endPos = endPoint.position;
            Vector3 c = endPos * 2f - endCtrl; // incoming handle
            Vector3 d = endPos;

            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;

            return uuu * a + 3f * uu * t * b + 3f * u * tt * c + ttt * d;
        }
    }
    [Header("Scene View Drawing")]
    [Range(2, 64)]
    public int segmentCount = 32;
    public Color curveColor = Color.yellow;
    public Color controlLineColor = Color.gray;
    public float handleGizmoSize = 0.3f;
    public float pointGizmoSize = 0.4f;

    [SerializeField, HideInInspector] private List<Segment> m_segments = new();

    /// <summary>
    /// Rebuilds the internal segment list from child transforms
    /// </summary>
    public void RebuildFromChildren()
    {
        m_segments.Clear();

        List<Transform> anchors = new();
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            Transform anchor = transform.GetChild(i);
            anchors.Add(anchor);
        }
        if (anchors.Count < 2) return;
        for (int i = 0; i < anchors.Count - 1; i++)
        {
            Transform a = anchors[i];
            Transform b = anchors[i + 1];
            if (a.childCount == 0 || b.childCount == 0) continue;
            Segment seg = new()
            {
                startPoint = a,
                startHandle = a.GetChild(0),
                endPoint = b,
                endHandle = b.GetChild(0)
            };
            m_segments.Add(seg);
        }
    }
    /// <summary>
    /// Evaluate the path at t in [0,1] across all segments
    /// </summary>
    public Vector3 GetPointOnCurves(float t)
    {
        if (m_segments == null || m_segments.Count == 0) return transform.position;

        t = Mathf.Clamp01(t);
        int segmentCountTotal = m_segments.Count;
        float scaled = t * segmentCountTotal;
        int index = Mathf.Min(segmentCountTotal - 1, Mathf.FloorToInt(scaled));
        float localT = Mathf.Clamp01(scaled - index);
        Segment seg = m_segments[index];
        return seg.GetPoint(localT);
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        RebuildFromChildren();
    }
    private void OnTransformChildrenChanged()
    {
        RebuildFromChildren();
    }
    private void OnDrawGizmos()
    {
        if (m_segments == null || m_segments.Count == 0) RebuildFromChildren();

        Gizmos.matrix = Matrix4x4.identity;
        foreach (var seg in m_segments)
        {
            if (seg.startPoint == null || seg.startHandle == null || seg.endPoint == null || seg.endHandle == null) continue;
            // Control lines
            Gizmos.color = controlLineColor;
            // Start handle
            Gizmos.DrawLine(seg.startPoint.position, seg.startHandle.position);
            // End handle
            Vector3 endCtrl = seg.endHandle.position;
            Vector3 endPos = seg.endPoint.position;
            Vector3 mirroredEnd = endPos * 2f - endCtrl;

            Gizmos.DrawLine(endPos, mirroredEnd);

            // Handles
            Gizmos.DrawSphere(seg.startHandle.position, handleGizmoSize);
            Gizmos.DrawSphere(seg.endHandle.position, handleGizmoSize);

            Gizmos.color = new Color(controlLineColor.r, controlLineColor.g, controlLineColor.b, 0.5f);
            Gizmos.DrawSphere(mirroredEnd, handleGizmoSize * 0.7f);

            // Anchor points
            Gizmos.color = curveColor;
            Gizmos.DrawSphere(seg.startPoint.position, pointGizmoSize);
            Gizmos.DrawSphere(seg.endPoint.position, pointGizmoSize);
        }
        Gizmos.color = curveColor;
        foreach (var seg in m_segments)
        {
            if (seg.startPoint == null || seg.startHandle == null || seg.endPoint == null || seg.endHandle == null) continue;
            Vector3 prev = seg.startPoint.position;
            for (int i = 1; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                Vector3 pt = seg.GetPoint(t);
                Gizmos.DrawLine(prev, pt);
                prev = pt;
            }
        }
    }
#endif
}