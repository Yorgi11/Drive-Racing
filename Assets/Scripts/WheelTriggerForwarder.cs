using UnityEngine;
public class WheelTriggerForwarder : MonoBehaviour
{
    private Vehicle m_vehicle;
    private void Awake()
    {
        m_vehicle = GetComponentInParent<Vehicle>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_vehicle != null)
        {
            m_vehicle.HandleTrigger(collision);
        }
    }
}