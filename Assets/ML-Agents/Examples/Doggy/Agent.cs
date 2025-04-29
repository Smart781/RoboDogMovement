using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System.Threading.Tasks;
using System.Security.Cryptography;

public class Player : Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer PlaneMeshRenderer;

    private float start_dist = 0f;
    private HashSet<float> visitedPositions;


    public override void OnEpisodeBegin()
    {
        Debug.Log("Start");
        // transform.localPosition = new Vector3(Random.Range(-10f, +10f), 0f, Random.Range(-10f, 10f));
        // targetTransform.localPosition = new Vector3(Random.Range(-10f, 10f), 0f, Random.Range(-10f, 10f));

        transform.localPosition = new Vector3(-71.72f, 2.91f, 4.51f);
        targetTransform.localPosition = new Vector3(-44.53f, 4.14f, -5.02f);

        start_dist = Vector3.Distance(transform.localPosition, targetTransform.localPosition);

        visitedPositions = new HashSet<float>();
        // visitedPositions.Add(transform.localPosition);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];

        float speed = 10f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * speed;

        float distanceToTarget = Vector3.Distance(transform.localPosition, targetTransform.localPosition);

        SetReward((start_dist - distanceToTarget) * 0.01f);

        if (visitedPositions.Contains(distanceToTarget))
        {
            // Debug.Log("REPEAT");
            SetReward(-1000f);
        }
        else
        {
            visitedPositions.Add(distanceToTarget);
            // Debug.Log(distanceToTarget);
            // Debug.Log(transform.localPosition);
        }

        // Debug.Log(distanceToTarget);
        //Debug.Log("player " + transform.localPosition);
        //Debug.Log("target " + targetTransform.localPosition);
        
        if (distanceToTarget < 1.42f)
        {
            // Debug.Log("YES");
            SetReward(100.0f);
            EndEpisode();
        }

        if (distanceToTarget > 35.0f)
        {
            SetReward(-100.0f);
            EndEpisode();
        }

        else if (transform.localPosition.y < 0)
        {
            SetReward(-100f);
            EndEpisode();
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
    }

    private void OnTriggerEnter(Collider other) 
    {
        Debug.Log("Hello, Unity!");
        if (other.TryGetComponent<Wall>(out Wall wall)) {
            Debug.Log("Wall");
            SetReward(-1f);
            PlaneMeshRenderer.material = loseMaterial;
            EndEpisode();
        }

        if (other.TryGetComponent<Obstacle>(out Obstacle obstacle)) {
            Debug.Log("Obstacle");
            SetReward(-1f);
            PlaneMeshRenderer.material = loseMaterial;
            EndEpisode();
        }

        if (other.TryGetComponent<Goal>(out Goal goal)) {
            Debug.Log("Target");
            SetReward(+1f);
            PlaneMeshRenderer.material = winMaterial;
            EndEpisode();
        }
    }

}