using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.InputSystem;

// Ajan (yeşil küre) zemindeki hedefe (turuncu küp) ulaşmayı öğrenir.
public class RollerBallAgent : Agent
{
    public Transform target;
    public float forceMultiplier = 10f;

    [Header("Zorluk")]
    public float spawnRange = 3.5f;
    public bool randomAgentStart = false;
    public float minStartDistance = 2f;
    public float successDistance = 1.4f;

    Rigidbody body;
    bool resultRecorded;

    public override void Initialize()
    {
        body = GetComponent<Rigidbody>();
    }

    public override void OnEpisodeBegin()
    {
        // Süre dolarak biten bölüm başarısız sayılır.
        if (CompletedEpisodes > 0 && !resultRecorded)
            Academy.Instance.StatsRecorder.Add("RollerBall/Success", 0f);
        resultRecorded = false;

        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;
        transform.localPosition = randomAgentStart
            ? RandomPoint() : new Vector3(0f, 0.5f, 0f);

        Vector3 p;
        do
        {
            p = RandomPoint();
        } while (Vector3.Distance(p, transform.localPosition) < minStartDistance);
        target.localPosition = p;
    }

    Vector3 RandomPoint()
    {
        float x = Random.Range(-spawnRange, spawnRange);
        float z = Random.Range(-spawnRange, spawnRange);
        return new Vector3(x, 0.5f, z);
    }

    // Hedef konumu (3) + ajan konumu (3) + x/z hızı (2) = 8 gözlem
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(target.localPosition);
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(body.linearVelocity.x);
        sensor.AddObservation(body.linearVelocity.z);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        var a = actions.ContinuousActions;
        Vector3 force = new Vector3(a[0], 0f, a[1]) * forceMultiplier;
        body.AddForce(force, ForceMode.Force);

        if (Vector3.Distance(transform.localPosition, target.localPosition) < successDistance)
        {
            SetReward(1f);
            RecordResult(1f);
            EndEpisode();
        }
        else if (transform.localPosition.y < -1f)
        {
            RecordResult(0f);
            EndEpisode();
        }
    }

    void RecordResult(float success)
    {
        Academy.Instance.StatsRecorder.Add("RollerBall/Success", success);
        resultRecorded = true;
    }

    // Ortamı WASD ile elle test etmek için.
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        Vector2 move = InputSystem.actions.FindAction("Move").ReadValue<Vector2>();
        var continuous = actionsOut.ContinuousActions;
        continuous[0] = move.x;
        continuous[1] = move.y;
    }
}
