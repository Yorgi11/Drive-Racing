using UnityEngine;
/// <summary>
/// Simple 2D cubic Bezier curve defined by 4 control points.
/// Attach this to an empty GameObject and assign 4 point Transforms.
/// </summary>
[ExecuteAlways]
public class Bezier2D : MonoBehaviour
{
    [System.Serializable]
    public class BezCurve
    {
        [Header("Control Points")]
        public Transform _startPoint;
        public Transform _StartHandle;
        public Transform _endPoint;
        public Transform _endHandle;
        /// <summary>
        /// Evaluate the cubic Bezier at t in [0,1].
        /// </summary>
        public Vector3 GetPoint(float t)
        {
            if (_startPoint == null || _StartHandle == null || _endHandle == null || _endPoint == null) return Vector3.zero;
            Vector3 a = _startPoint.position;
            Vector3 b = _StartHandle.position;
            Vector3 c = _endHandle.position;
            Vector3 d = _endPoint.position;
            float u = 1f - t;
            float tt = t * t;
            float uu = u * u;
            float uuu = uu * u;
            float ttt = tt * t;
            Vector3 point = uuu * a + 3f * uu * t * b + 3f * u * tt * c + ttt * d;
            return point;
        }
    }
    [Header("Curves")]
    [SerializeField] private BezCurve[] m_curves;
    [Header("Scene View Drawing")]
    [Range(2, 64)]
    public int segmentCount = 32;
    public Color curveColor = Color.yellow;
    public Color controlLineColor = Color.gray;
    public Vector3 GetPointOnCurves(float t)
    {
        if (m_curves == null || m_curves.Length == 0) return transform.position;
        t = Mathf.Clamp01(t);
        int curveCount = m_curves.Length;
        float scaled = t * curveCount;
        int index = Mathf.Min(curveCount - 1, Mathf.FloorToInt(scaled));
        float localT = Mathf.Clamp01(scaled - index);
        BezCurve curve = m_curves[index];
        return curve.GetPoint(localT);
    }
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        foreach (var curve in m_curves)
        {
            if (curve._startPoint == null || curve._StartHandle == null || curve._endPoint == null || curve._endHandle == null) return;

            Gizmos.color = controlLineColor;
            Gizmos.DrawLine(curve._startPoint.position, curve._StartHandle.position);
            Gizmos.DrawLine(curve._endHandle.position, curve._endPoint.position);
            Gizmos.color = curveColor;
            Vector3 prev = curve._startPoint.position;
            for (int i = 1; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                Vector3 pt = curve.GetPoint(t);
                Gizmos.DrawLine(prev, pt);
                prev = pt;
            }
        }
    }
#endif
}