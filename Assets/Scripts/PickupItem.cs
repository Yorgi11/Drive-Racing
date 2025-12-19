using UnityEngine;
public class PickupItem : MonoBehaviour
{
    [SerializeField] private float m_rotateSpeed;
    [SerializeField] private Transform m_sprite;
    private float m_yRot = 0f;
    private void Start()
    {
        m_sprite = transform;
    }
    void Update()
    {
        m_sprite.rotation = Quaternion.Euler(0f, m_yRot, 0f);
        m_yRot += Time.deltaTime * m_rotateSpeed;
        if (m_yRot > 359.99f) m_yRot = 0f;
    }
}