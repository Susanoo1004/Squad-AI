using UnityEngine;
using static Movement;

public interface IBoid
{
    Vector3 getVelocity();
    float getMaxVelocity();
    Vector3 getPosition();
    float getMass();
}

public class Movement : MonoBehaviour, IBoid
{
    [SerializeField]
    private float maxSpeed = 20f;
    [SerializeField]
    private float maxForce = 8f;
    [SerializeField]
    private float slowingRadius = 2f;
    [SerializeField]
    private float safeRadius = 20f;
    [SerializeField]
    private bool bReplaceOutOfBorders = false;

    private Vector3 velocity = Vector3.zero;
    private Vector3 desiredVelocity = Vector3.zero;

    float wanderAngle = 0f;


    private float mass = 1f;

    private float orientation = 0f;

    private float posOffsetY = 0.5f;
    private Vector3 targetPos;
    public Vector3 TargetPos
    {
        get { return targetPos; }
        set { targetPos = value; targetPos.y += posOffsetY; }
    }

    private void Start()
    {
        targetPos = transform.position;
    }

    public void Stop()
    {
        velocity = Vector3.zero;
    }

    protected float GetOrientationFromDirection(Vector3 direction)
    {
        if (direction.magnitude > 0)
        {
            return Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
        }
        return orientation;
    }
    private Vector3 Truncate(Vector3 vec, float max)
    {
        if (vec.sqrMagnitude > maxForce * maxForce)
            return vec.normalized * maxForce;
        return vec;
    }
    // Update is called once per frame
    private void Update()
    {
        //Seek
        if (targetPos != Vector3.zero)
            desiredVelocity = Seek(targetPos);
        else
            desiredVelocity = Wander();
        //desiredVelocity += Flee(Vector3.zero);
    }
    void LateUpdate()
    {
        #region Steering
        //Steering
        Vector3 steering = desiredVelocity - velocity;
        Truncate(steering, maxForce);
        steering /= mass;

        //orientation += angularVelocity * Time.deltaTime;

        velocity += steering * Time.deltaTime;
        //orientation += angularVelocity * Time.deltaTime;

        // truncate to max speed
        Truncate(velocity, maxSpeed);
        #endregion //Steering
        // update position and orientation
        transform.position += velocity * Time.deltaTime;
        orientation = GetOrientationFromDirection(velocity);
        transform.eulerAngles = Vector3.up * orientation;

        // keep position above the floor
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(transform.position + Vector3.up * 10, Vector3.down, out hitInfo, 100, 1 << LayerMask.NameToLayer("Floor")))
        {
            transform.position = hitInfo.point + Vector3.up * posOffsetY;
        }
        if (bReplaceOutOfBorders)
            transform.position = ReplaceOutOfBorders(transform.position);
    }
    private Vector3 ReplaceOutOfBorders(Vector3 position)
    {
        //Warning HARD CODED, could be based on the terrain
        float xBorder = 55f;
        const float screenRatio = 9f / 16f;
        float yBorder = xBorder * screenRatio;

        if (position.x > xBorder)
            position = new Vector3(-xBorder, position.y, position.z);
        else if (position.x < -xBorder)
            position = new Vector3(xBorder, position.y, position.z);
        if (position.z > yBorder)
            position = new Vector3(position.x, position.y, -yBorder);
        else if (position.z < -yBorder)
            position = new Vector3(position.x, position.y, yBorder);
        return position;

    }

    private Vector3 Seek(Vector3 target)
    {
        //Seek
        Vector3 tempDesiredVelocity = target - transform.position;

        // Arrival
        float distance = tempDesiredVelocity.magnitude;
        if (distance < slowingRadius)
            tempDesiredVelocity = tempDesiredVelocity.normalized * maxSpeed * (distance / slowingRadius);
        else
            tempDesiredVelocity = tempDesiredVelocity.normalized * maxSpeed;

        return tempDesiredVelocity;
    }


    private Vector3 Flee(Vector3 target)
    {
        //Seek
        Vector3 tempDesiredVelocity = transform.position - target;

        // Arrival
        float distance = tempDesiredVelocity.magnitude;
        if (distance > safeRadius)
            tempDesiredVelocity = Vector3.zero;
        else if (distance == 0f)
            tempDesiredVelocity = new Vector3(Random.value, 0f, Random.value).normalized * maxSpeed;
        else
            tempDesiredVelocity = tempDesiredVelocity.normalized * maxSpeed;
        return tempDesiredVelocity;
    }
    private Vector3 Pursuit(IBoid target)
    {
        float timeStep = .3f;

        Vector3 futurePosition = target.getPosition() + target.getVelocity() * timeStep;

        return Seek(futurePosition);

    }
    public void DoPursuit(IBoid target)
    {
        desiredVelocity += Pursuit(target);
    }
    private Vector3 Avoid(IBoid target)
    {
        float timeStep = .3f;

        Vector3 futurePosition = target.getPosition() + target.getVelocity() * timeStep;

        return Flee(futurePosition);
    }
    public void DoAvoid(IBoid target)
    {
        desiredVelocity += Avoid(target);
    }
    private Vector3 Wander()
    {
        float CIRCLE_DISTANCE = 10f;
        float ANGLE_CHANGE = 5f * Mathf.Deg2Rad;
        if (velocity == Vector3.zero)
            velocity = new Vector3(Random.value, 0f, Random.value).normalized;

        Vector3 circleCenter = velocity.normalized * CIRCLE_DISTANCE;

        wanderAngle += (Random.value * ANGLE_CHANGE) - (ANGLE_CHANGE * .5f);
        //Simple rotation
        Vector3 displacement = new Vector3(CIRCLE_DISTANCE * Mathf.Cos(wanderAngle), 0f, CIRCLE_DISTANCE * Mathf.Sin(wanderAngle));

        Vector3 wanderForce = circleCenter + displacement;
        return wanderForce;
    }
    public void SetTargetWithWander(Vector3 target)
    {
        TargetPos = target + Wander(); /*Target Pos for the Y offset*/
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawRay(transform.position, velocity);
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, desiredVelocity);
    }

    Vector3 IBoid.getVelocity()
    {
        return velocity;
    }

    float IBoid.getMaxVelocity()
    {
        return maxSpeed;
    }

    Vector3 IBoid.getPosition()
    {
        return transform.position;
    }

    float IBoid.getMass()
    {
        return mass;
    }
}
