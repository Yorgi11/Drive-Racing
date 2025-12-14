using UnityEngine;
public class Vehicle : MonoBehaviour
{
    [Header("Bodies")]
    [SerializeField] private Rigidbody2D m_chassis;

    [Header("Roll")]
    [SerializeField] private float m_groundDistance = 5f;
    [SerializeField] private float m_onGroundDistance = 1.5f;
    [SerializeField] private float m_maxBodyRPM = 30f;
    [SerializeField] private float m_rollTorque = 400f;

    [Header("Drive")]
    public float m_maxWheelRPM = 200f;
    public float m_driveTorque = 200f;

    [Header("Suspension")]
    [SerializeField] private LayerMask m_groundLayers;
    [SerializeField] private WheelAndSuspension[] m_wheelsAndSuspensions;

    [Header("Gas")]
    [SerializeField] private float m_maxGas;
    [SerializeField] private float m_gasConsumptionrate;

    private float m_distanceToGround;
    private float m_driveMulti;
    private float m_lastAngle;
    private float m_airRotationAccum; // accumulated rotation while airborne
    private Transform m_transform;
    private Player m_player;
    private readonly RaycastHit2D[] m_hits = new RaycastHit2D[1];
    public bool IsGrounded { get; set; }
    public int FrontFlips { get; set; }
    public int BackFlips { get; set; }
    public float Kph { get; private set; }
    public float CurrentGas { get; set; }
    public float MaxGas { get { return m_maxGas; } }
    [System.Serializable]
    public class WheelAndSuspension
    {
        [Header("Suspension Settings")]
        public float m_springRestLength = 1f;
        public float m_springStiffness = 800f;  // k
        public float m_springDamping = 80f;     // c

        [Tooltip("How strongly to keep wheels in line with the suspension axis (sideways constraint).")]
        public float m_lateralStiffness = 4000f;
        public float m_lateralDamping = 40f;

        [Header("Suspension Parts")]
        public Transform m_suspensionPoint;
        public Rigidbody2D m_wheel;

        [Header("Drive")]
        public bool m_drive = true;
    }
    public void RunStart(Player p)
    {
        m_transform = transform;
        int driveCount = 0;
        foreach (WheelAndSuspension ws in m_wheelsAndSuspensions)
        {
            if (ws.m_drive) driveCount++;
        }
        m_driveMulti = 1f / driveCount;
        CurrentGas = m_maxGas;
        m_lastAngle = m_chassis.rotation;
        m_player = p;
    }
    public void RunUpdate()
    {
        if (CurrentGas <= 0f) return;
        foreach (WheelAndSuspension ws in m_wheelsAndSuspensions)
        {
            LimitRPM(ws.m_wheel, m_maxWheelRPM);
        }
        LimitRPM(m_chassis, m_maxBodyRPM);
        Kph = m_chassis.linearVelocityX * 3.6f;
        CurrentGas -= m_gasConsumptionrate * Time.deltaTime;
    }
    public void RunFixedUpdate(float input)
    {
        if (CurrentGas <= 0f) return;
        CheckGround();
        foreach (WheelAndSuspension ws in m_wheelsAndSuspensions)
        {
            ApplySuspension(ws);
            if (ws.m_drive) ws.m_wheel.AddTorque(input * -m_driveTorque * m_driveMulti);
        }
        float t = Mathf.InverseLerp(0.1f, m_groundDistance, m_distanceToGround);
        float rollFactor = t * t * t;
        m_chassis.AddTorque(input * -m_rollTorque * rollFactor);
        TrackFlips();
    }
    private void ApplySuspension(WheelAndSuspension ws)
    {
        Vector2 anchorPos = ws.m_suspensionPoint.position;

        // ---- SUSPENSION ----
        // Suspension axis
        Vector2 axisUp = -ws.m_suspensionPoint.up;
        axisUp.Normalize();

        Vector2 wheelPos = ws.m_wheel.worldCenterOfMass;
        Vector2 anchorToWheel = wheelPos - anchorPos;

        // Position along suspension axis
        float distAlongAxis = Vector2.Dot(anchorToWheel, axisUp);
        Vector2 wheelOnAxis = anchorPos + axisUp * distAlongAxis;

        float extension = distAlongAxis - ws.m_springRestLength;

        Vector2 vAnchor = m_chassis.GetPointVelocity(anchorPos);
        Vector2 vWheel = ws.m_wheel.linearVelocity;
        Vector2 relVel = vWheel - vAnchor;

        float relVelAxis = Vector2.Dot(relVel, axisUp);

        float springForceMag = -ws.m_springStiffness * extension - ws.m_springDamping * relVelAxis;
        Vector2 springForce = axisUp * springForceMag;

        // ---- LATERAL CONTROL ----
        // Lateral axis
        Vector2 axisRight = new(-axisUp.y, axisUp.x);

        float relVelLateral = Vector2.Dot(relVel, axisRight);
        Vector2 lateralOffset = wheelPos - wheelOnAxis;

        Vector2 lateralSpringForce = -lateralOffset * ws.m_lateralStiffness;
        Vector2 lateralDampingForce = -relVelLateral * ws.m_lateralDamping * axisRight;

        // ---- Forces ----
        Vector2 totalWheelForce = springForce + lateralSpringForce + lateralDampingForce;
        ws.m_wheel.AddForce(totalWheelForce, ForceMode2D.Force);
        m_chassis.AddForceAtPosition(-totalWheelForce, anchorPos, ForceMode2D.Force);
    }
    void LimitRPM(Rigidbody2D wheel, float maxRPM)
    {
        float maxAngularVel = (maxRPM / 60f) * 360f;
        wheel.angularVelocity = Mathf.Clamp(wheel.angularVelocity, -maxAngularVel, maxAngularVel);
    }
    private void CheckGround()
    {
        int wheelCount = m_wheelsAndSuspensions.Length;
        int groundedWheels = 0;
        float closestDist = float.MaxValue;
        foreach (WheelAndSuspension ws in m_wheelsAndSuspensions)
        {
            int hitCount = Physics2D.RaycastNonAlloc(ws.m_suspensionPoint.position, -ws.m_suspensionPoint.up, m_hits, m_groundDistance, m_groundLayers);
            if (hitCount > 0)
            {
                groundedWheels++;
                float d = m_hits[0].distance;
                if (d < closestDist) closestDist = d;
            }
        }
        if (closestDist == float.MaxValue) m_distanceToGround = m_groundDistance;
        else m_distanceToGround = closestDist;
        // Grounded if >= 50% of wheels detect ground
        IsGrounded = groundedWheels >= (wheelCount * 0.5f);
    }
    private void TrackFlips()
    {
        float currentAngle = m_chassis.rotation;
        float delta = Mathf.DeltaAngle(m_lastAngle, currentAngle);
        m_lastAngle = currentAngle;

        if (!IsGrounded)
        {
            m_airRotationAccum += delta;
            while (m_airRotationAccum >= 300f)
            {
                BackFlips++;
                m_airRotationAccum -= 360f;
                OnBackFlip();
            }
            while (m_airRotationAccum <= -300f)
            {
                FrontFlips++;
                m_airRotationAccum += 360f;
                OnFrontFlip();
            }
        }
        else m_airRotationAccum = 0f;
    }
    private void OnFrontFlip()
    {
        // play vfx, sfx
    }
    private void OnBackFlip()
    {
        // play vfx, sfx
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null)
        {
            GameObject obj = collision.gameObject;
            if (obj.layer == m_player.m_coinLayer)
            {
                m_player.AddValue(Player.ValueType.Coin);
                Destroy(obj);
            }
            if (obj.layer == m_player.m_gasLayer)
            {
                m_player.AddValue(Player.ValueType.Gas);
                Destroy(obj);
            }
        }
    }
}