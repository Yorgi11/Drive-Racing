using TMPro;
using UnityEngine;
public class UIDial : MonoBehaviour
{
    [SerializeField] private float m_minValue;
    [SerializeField] private float m_maxValue;
    [SerializeField] private float m_minAngle;
    [SerializeField] private float m_maxAngle;
    [SerializeField] private Transform m_dial;
    [SerializeField] private TextMeshProUGUI m_text;
    public void UpdateDial(float val, string suffix)
    {
        float t = Mathf.InverseLerp(m_minValue, m_maxValue, val);
        float angle = Mathf.Lerp(m_minAngle, m_maxAngle, t);
        m_dial.rotation = Quaternion.Euler(0f, 0f, angle);
        m_text.text = $"{(int)val} {suffix}";
    }
}