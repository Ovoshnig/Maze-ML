using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(RayPerceptionSensorComponent3D))]
public class AgentMover : Agent
{
    private const string HorizontalAxisName = "Horizontal";
    private const string VerticalAxisName = "Vertical";

    [SerializeField, Min(0f)] private float _speed;
    [SerializeField] private Transform _targetTransform;
    [SerializeField] private MeshRenderer _planeMeshRenderer;
    [SerializeField] private Transform _checkpointsTransform;

    private Rigidbody _rigidbody;
    private RayPerceptionSensorComponent3D _raySensor;
    private Vector3 _startPosition;
    private Vector3 _targetStartPosition;

    public override void Initialize()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _raySensor = GetComponent<RayPerceptionSensorComponent3D>();
        _startPosition = transform.localPosition;
        _targetStartPosition = _targetTransform.localPosition;
    }

    public override void OnEpisodeBegin()
    {
        transform.localPosition = _startPosition;
        _targetTransform.localPosition = _targetStartPosition;

        foreach (Transform checkpoint in _checkpointsTransform)
            checkpoint.transform.gameObject.SetActive(true);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition.x);
        sensor.AddObservation(transform.localPosition.z);
        sensor.AddObservation(_rigidbody.linearVelocity.x);
        sensor.AddObservation(_rigidbody.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.DiscreteActions[0] - 1;
        float moveZ = actions.DiscreteActions[1] - 1;
        _rigidbody.AddRelativeForce(_speed * new Vector3(moveX, 0f, moveZ));

        SetReward(-0.005f);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        discreteActions[0] = (int)Input.GetAxisRaw(HorizontalAxisName) + 1;
        discreteActions[1] = (int)Input.GetAxisRaw(VerticalAxisName) + 1;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.TryGetComponent(out Wall _))
        {
            SetReward(-1f);
            EndEpisode();
        }
        else if (collision.gameObject.TryGetComponent(out Obstacle _))
        {
            SetReward(-1f);
            EndEpisode();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Checkpoint _))
        {
            SetReward(2f);
            other.gameObject.SetActive(false);
        }
        else if (other.TryGetComponent(out Target _))
        {
            SetReward(4f);
            EndEpisode();
        }
    }
}
