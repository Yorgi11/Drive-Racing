using TMPro;
using UnityEngine;
using QF_Tools.QF_Utilities;
public class Player : MonoBehaviour
{
    [SerializeField] private float m_camFollowSpeed;
    [SerializeField] private Vector3 m_camOffset;
    [SerializeField] private Transform m_cam;
    [SerializeField] private Vehicle m_vehicle;
    [SerializeField] private UIDial m_speedDial;
    [SerializeField] private UIDial m_gasGuage;
    [SerializeField] private TextMeshProUGUI m_pointsText;
    [SerializeField] private TextMeshProUGUI m_flipText;

    [Header("Points etc")]
    [SerializeField] private int m_frontFlipPoints = 100;
    [SerializeField] private int m_backFlipPoints = 150;
    [SerializeField] private int m_pointsPerUnitDistance = 1;
    [SerializeField] private int m_bonusPointsPerUnitDistance = 10;
    [SerializeField] private int m_pointsPerCoin = 25;
    [SerializeField] private int m_gasPerCan = 25;
    [Header("Layers")]
    public int m_coinLayer;
    public int m_gasLayer;

    private int m_points = 0;
    private float m_timeSinceLastPos;
    private float m_lastPosition;
    private float m_oldRecord;
    private float m_horzInput;
    private InputSystem_Actions m_inputActions;
    public enum ValueType { Coin, Gas }
    public void AddValue(ValueType type)
    {
        switch (type)
        {
            case ValueType.Coin:
                m_points += m_pointsPerCoin;
                break;
            case ValueType.Gas:
                m_vehicle.CurrentGas += m_gasPerCan;
                break;
        }
    }
    private void OnEnable() { m_inputActions.Enable(); }
    private void OnDisable() { m_inputActions.Disable(); }
    private void Awake()
    {
        m_inputActions = new();
    }
    private void Start()
    {
        m_vehicle.RunStart(this);
        m_lastPosition = m_vehicle.transform.position.x;
    }
    private void Update()
    {
        m_horzInput = m_inputActions.Player.Move.ReadValue<Vector2>().x;
        if (m_vehicle.CurrentGas > 0f) m_vehicle.RunUpdate();
        m_speedDial.UpdateDial(m_vehicle.Kph, "Km/h");
        m_gasGuage.UpdateDial(100f * (m_vehicle.CurrentGas / m_vehicle.MaxGas), "%");
        HandleFlipPoints();
        if (m_vehicle.transform.position.x - m_lastPosition > 1f)
        {
            m_points += m_vehicle.transform.position.x > m_oldRecord ? m_pointsPerUnitDistance : m_bonusPointsPerUnitDistance;
            m_lastPosition = m_vehicle.transform.position.x;
            m_timeSinceLastPos = 0f;
        }
        m_timeSinceLastPos += Time.deltaTime;
        if (m_timeSinceLastPos > 5f) Debug.Log("GameOver");
    }
    private void LateUpdate()
    {
        m_pointsText.text = $"Points: {m_points}";
    }
    private void FixedUpdate()
    {
        m_vehicle.RunFixedUpdate(m_horzInput);
        m_cam.position = Vector3.Lerp(m_cam.position, m_vehicle.transform.position + m_camOffset, m_camFollowSpeed * Time.fixedDeltaTime);
    }
    private void HandleFlipPoints()
    {
        if (!m_vehicle.IsGrounded) return;
        int frontFlips = m_vehicle.FrontFlips;
        if (frontFlips > 0)
        {
            if (frontFlips == 1) m_flipText.text = "Front Flip";
            else m_flipText.text = $"Front Flip x{frontFlips}";
        }
        while (m_vehicle.FrontFlips > 0)
        {
            m_points += m_frontFlipPoints;
            m_vehicle.FrontFlips--;
        }
        int backFlips = m_vehicle.BackFlips;
        if (backFlips > 0)
        {
            if (backFlips == 1) m_flipText.text = "Back Flip";
            else m_flipText.text = $"Back Flip x{backFlips}";
        }
        while (m_vehicle.BackFlips > 0)
        {
            m_points += m_backFlipPoints;
            m_vehicle.BackFlips--;
        }
        if (frontFlips > 0 || backFlips > 0)
        {
            StartCoroutine(QF_Coroutines.LerpPingPongOverTimeRepeat<Vector3>(new Vector3(0f, 0f, -20f), new Vector3(0f, 0f, 20f),
                5, 1.5f, QF_Coroutines.LerpVector3, v => m_flipText.transform.rotation = Quaternion.Euler(v)));
            StartCoroutine(QF_Coroutines.LerpPingHangPongOverTime<Vector3>(Vector3.zero, Vector3.one, 0.25f, 1f, 0.25f,
            QF_Coroutines.LerpVector3, v => m_flipText.transform.localScale = v));
        }
    }
}