using Assets.Scripts;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine;
using Unity.MLAgents.Actuators; 
public class DriverController : Agent
{
    private const float POSITION_CHANGE_WATCHDOG = 20;

    [SerializeField]
    private List<AxleInfo> _axleInfos = null;
    [SerializeField]
    private float _maxMotorTorque = 800;
    [SerializeField]
    private float _maxSteeringAngle = 30;

    // [SerializeField] private List<Checkpoint> _checkpoints;
    // [SerializeField] private Checkpoint _finalCheckpoint;
    // [SerializeField] private TrackType _type;

    public float LocalVelocity;
    public float Horizontal;
    public float Vertical;

    private Rigidbody _rb;
    private Vector3 _initialPosition;
    private Quaternion _initialRotation;
    private List<GameObject> _deactivatedRewards;
    private int _numRewardCollected;

    private CalculateTime _calculateTime;
    // private Manager _manager;

    public override void Initialize()
    {
        _rb = transform.GetComponent<Rigidbody>();
        _deactivatedRewards = new List<GameObject>();
        this.MaxStep = 10000;
    }    

    private void Start()
    {
        _initialPosition = transform.position;
        _initialRotation = transform.rotation;
        _calculateTime = FindObjectOfType<CalculateTime>();
    }

    public override void OnEpisodeBegin()
    {
        StartCoroutine(PositionChangeWatchdog());
        _rb.velocity = Vector3.zero;
        transform.position = _initialPosition;
        transform.rotation = _initialRotation;
        ResetCheckpoints();
        _deactivatedRewards.Clear();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        //Calculate and return the velocity vector of the Rigidbody relative to the vehicle's local coordinates.
        // This is used to obtain the local speed of the vehicle, typically for determining whether the vehicle is moving forward or backward.
        var localVelocity = transform.InverseTransformDirection(_rb.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);
        sensor.AddObservation(_rb.velocity.magnitude);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Boundary"))
        {
            SetReward(-10f);
            EndEpisode();
        }
    }

    private void Update()
    {
        //Manually End Episode
        if (Input.GetKeyDown(KeyCode.R))
        {
            EndEpisode();
        }

        LocalVelocity = transform.InverseTransformDirection(_rb.velocity).z;

        //If its going backwards
        if (LocalVelocity < 0)
        {
            SetReward(-1f);
        }

        //If its colliding with the ground
        foreach(AxleInfo wheel in _axleInfos)
        {
            if(wheel.leftWheel.GetGroundHit(out WheelHit hit))
            {               
                if (hit.collider.gameObject.CompareTag("Ground"))
                {
                    SetReward(-1f);
                }
            }

        }
    }

    private void ResetCheckpoints()
    {
        foreach (GameObject reward in _deactivatedRewards)
        {
            reward.SetActive(true);
        }
    }

    private IEnumerator PositionChangeWatchdog()
    {
        Vector3 positionBefore = transform.position;

        yield return new WaitForSeconds(POSITION_CHANGE_WATCHDOG);

        float distance = Vector3.Distance(positionBefore, transform.position);
        if (distance < 0.5f)
        {
            EndEpisode();
        }
        else
        {
            StartCoroutine(PositionChangeWatchdog());
        }
        
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxis("Vertical");
        continuousActions[1] = Input.GetAxis("Horizontal");
    }


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Reward"))
        {
            _deactivatedRewards.Add(other.gameObject);
            other.gameObject.SetActive(false);
            SetReward(10f);
            _numRewardCollected++;
        }

        else if (other.gameObject.CompareTag("Start"))
        {
            _calculateTime.StartAttempt();
        }
        
        else if (other.gameObject.CompareTag("End"))
        {
            _calculateTime.Finish();
            this.SetReward(10f);
            EndEpisode();
        }

        if((_numRewardCollected % 5) == 0)
        {
            ResetCheckpoints();
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        Move(actionBuffers.ContinuousActions);
    }   

    private void Move(in ActionSegment<float> actions)
    {
        float motor = _maxMotorTorque * actions[0];
        float steering = _maxSteeringAngle * actions[1];

        Vertical = motor;
        Horizontal = steering;

        foreach (AxleInfo axleInfo in _axleInfos)
        {
            if (axleInfo.steering)
            {
                axleInfo.leftWheel.steerAngle = steering;
                axleInfo.rightWheel.steerAngle = steering;
            }
            if (axleInfo.motor)
            {
                axleInfo.leftWheel.motorTorque = motor;
                axleInfo.rightWheel.motorTorque = motor;
            }

            ApplyLocalPositionToVisuals(axleInfo.leftWheel);
            ApplyLocalPositionToVisuals(axleInfo.rightWheel);
        }
    }

    // finds the corresponding visual wheel
    // correctly applies the transform
    public void ApplyLocalPositionToVisuals(WheelCollider collider)
    {
        if (collider.transform.childCount == 0)
        {
            return;
        }

        Transform visualWheel = collider.transform.GetChild(0);

        Vector3 position;
        Quaternion rotation;
        collider.GetWorldPose(out position, out rotation);

        visualWheel.transform.position = position;
        visualWheel.transform.rotation = rotation;
    }    
}
