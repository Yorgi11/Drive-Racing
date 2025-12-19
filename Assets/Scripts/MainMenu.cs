using UnityEngine;
using QF_Tools.QF_Utilities;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
public class MainMenu : MonoBehaviour
{
    [SerializeField] private float m_bottomButtonLerpTime = 0.125f;
    [SerializeField] private Vector2 m_bottomButtonOffset;
    [SerializeField] private Color m_bottomButtonActiveColor = Color.blue;
    [SerializeField] private Color m_bottomButtonUnactiveColor = Color.gray;
    [SerializeField] private Button m_startActiveButton;
    private readonly Dictionary<Image, (Coroutine move, Coroutine color)> m_movedButtons = new();
    private IEnumerator Start()
    {
        yield return null;
        if (m_startActiveButton != null) m_startActiveButton.onClick.Invoke();
    }
    public void OnBottomButtonClicked(Image button)
    {
        if (button == null) return;
        if (m_movedButtons.Count > 0)
        {
            // Copy keys to avoid modifying dictionary while iterating
            var keys = new List<Image>(m_movedButtons.Keys);
            foreach (var img in keys)
            {
                if (img == null) continue;
                var (move, color) = m_movedButtons[img];
                if (move != null) StopCoroutine(move);
                if (color != null) StopCoroutine(color);
                // Animate back to "inactive"
                var moveBack = StartCoroutine(QF_Coroutines.LerpOverTime<Vector3>(m_bottomButtonOffset, Vector3.zero, m_bottomButtonLerpTime,
                    QF_Coroutines.LerpVector3, v => img.transform.localPosition = v));
                var colorBack = StartCoroutine(QF_Coroutines.LerpOverTime<Color>(m_bottomButtonActiveColor, m_bottomButtonUnactiveColor, m_bottomButtonLerpTime,
                    QF_Coroutines.LerpColor, v => img.color = v));
                m_movedButtons[img] = (moveBack, colorBack);
            }
            m_movedButtons.Clear();
        }
        // If the clicked button was already being animated, stop it
        if (m_movedButtons.TryGetValue(button, out var existing))
        {
            if (existing.move != null) StopCoroutine(existing.move);
            if (existing.color != null) StopCoroutine(existing.color);
            m_movedButtons.Remove(button);
        }
        // Animate clicked button to "active"
        var moveRoutine = StartCoroutine(QF_Coroutines.LerpOverTime<Vector3>(Vector3.zero, m_bottomButtonOffset, m_bottomButtonLerpTime,
            QF_Coroutines.LerpVector3, v => button.transform.localPosition = v));
        var colorRoutine = StartCoroutine(QF_Coroutines.LerpOverTime<Color>(m_bottomButtonUnactiveColor, m_bottomButtonActiveColor, m_bottomButtonLerpTime,
            QF_Coroutines.LerpColor, v => button.color = v));
        m_movedButtons.Add(button, (moveRoutine, colorRoutine));
    }
}