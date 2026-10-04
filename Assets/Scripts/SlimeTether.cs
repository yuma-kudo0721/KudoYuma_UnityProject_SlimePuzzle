using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
[RequireComponent(typeof(EdgeCollider2D))]
public class SlimeTether : MonoBehaviour
{
    [SerializeField] float breakDistance = 8f;
    [SerializeField] float pullSpeed = 12f;
    [SerializeField] float stopDistance = 0.5f;
    [SerializeField] KeyCode pullKey = KeyCode.V;

    [Header("Catch Objects")]
    [SerializeField] LayerMask catchLayer;
    [SerializeField] float catchPullSpeed = 6f;
    [SerializeField] float releaseDistance = 0.4f;

    Transform player;
    Transform clone;

    Rigidbody2D cloneRb;
    Collider2D playerCol;
    Collider2D cloneCol;
    LineRenderer line;
    EdgeCollider2D edgeCollider;

    float defaultGravity;
    bool isBroken;
    bool isIgnoringPlayerCloneCollision;

    Vector2[] points = new Vector2[2];
    List<Rigidbody2D> caughtBodies = new List<Rigidbody2D>();

    public void Setup(Transform playerTransform, Transform cloneTransform)
    {
        player = playerTransform;
        clone = cloneTransform;

        if (clone != null)
        {
            cloneRb = clone.GetComponent<Rigidbody2D>();
            cloneCol = clone.GetComponent<Collider2D>();

            if (cloneRb != null)
            {
                defaultGravity = cloneRb.gravityScale;
            }
        }

        playerCol = player.GetComponent<Collider2D>();

        if (edgeCollider != null && playerCol != null)
        {
            Physics2D.IgnoreCollision(edgeCollider, playerCol, true);
        }

        if (edgeCollider != null && cloneCol != null)
        {
            Physics2D.IgnoreCollision(edgeCollider, cloneCol, true);
        }
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = 0.08f;
        line.endWidth = 0.08f;

        edgeCollider = GetComponent<EdgeCollider2D>();
        edgeCollider.isTrigger = false;
        edgeCollider.edgeRadius = 0.08f;
    }

    void OnDestroy()
    {
        SetCloneColliderEnabled(true);
        SetPlayerCloneCollisionIgnored(false);

        for (int i = 0; i < caughtBodies.Count; i++)
        {
            ReleaseCaughtBody(caughtBodies[i]);
        }
    }

    void Update()
    {
        if (isBroken || player == null || clone == null)
        {
            Destroy(gameObject);
            return;
        }

        line.SetPosition(0, player.position);
        line.SetPosition(1, clone.position);

        points[0] = transform.InverseTransformPoint(player.position);
        points[1] = transform.InverseTransformPoint(clone.position);
        edgeCollider.points = points;

        float distance = Vector2.Distance(player.position, clone.position);
        if (distance > breakDistance)
        {
            BreakTether();
            return;
        }

        if (Input.GetKey(pullKey))
        {
            PullCloneStraight();
        }
        else
        {
            StopPulling();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) & catchLayer) == 0) return;

        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        if (!caughtBodies.Contains(rb))
        {
            caughtBodies.Add(rb);
            Debug.Log( collision.gameObject.name + " caught by tether. Total caught: " + caughtBodies.Count);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        Rigidbody2D rb = collision.rigidbody;
        if (rb == null) return;

        ReleaseCaughtBody(rb);
        caughtBodies.Remove(rb);
        Debug.Log( collision.gameObject.name + " released from tether. Total caught: " + caughtBodies.Count);
    }

    void PullCloneStraight()
    {
        if (cloneRb == null) return;

        Vector2 diff = (Vector2)player.position - cloneRb.position;

        if (diff.magnitude <= stopDistance)
        {
            cloneRb.velocity = Vector2.zero;
            cloneRb.gravityScale = defaultGravity;
            SetCloneColliderEnabled(true);
            SetPlayerCloneCollisionIgnored(false);

            return;
        }

        SetCloneColliderEnabled(false);
        SetPlayerCloneCollisionIgnored(true);

        Vector2 pullDirection = diff.normalized;

        cloneRb.velocity = pullDirection * pullSpeed;
        PullCaughtBodies(pullDirection);
    }

    void PullCaughtBodies(Vector2 pullDirection)
    {
        for (int i = caughtBodies.Count - 1; i >= 0; i--)
        {
            Rigidbody2D rb = caughtBodies[i];

            if (rb == null)
            {
                caughtBodies.RemoveAt(i);
                continue;
            }

            float distanceToTether = DistancePointToSegment(
                rb.position,
                player.position,
                clone.position
            );

            if (distanceToTether > releaseDistance)
            {
                ReleaseCaughtBody(rb);
                caughtBodies.RemoveAt(i);
                continue;
            }

            PushableRock rock = rb.GetComponent<PushableRock>();
            if (rock != null)
            {
                rock.SetPulledByTether(true);
            }

            rb.velocity = pullDirection * catchPullSpeed;
        }
    }

    void ReleaseCaughtBody(Rigidbody2D rb)
    {
        if (rb == null) return;

        PushableRock rock = rb.GetComponent<PushableRock>();
        if (rock != null)
        {
            rock.SetPulledByTether(false);
        }
    }

    float DistancePointToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 lineVector = end - start;
        float lineLength = lineVector.sqrMagnitude;

        if (lineLength == 0f)
        {
            return Vector2.Distance(point, start);
        }

        float t = Vector2.Dot(point - start, lineVector) / lineLength;
        t = Mathf.Clamp01(t);

        Vector2 closestPoint = start + lineVector * t;
        return Vector2.Distance(point, closestPoint);
    }

    void StopPulling()
    {
        if (cloneRb != null)
        {
            cloneRb.gravityScale = defaultGravity;
        }

        SetPlayerCloneCollisionIgnored(false);
        SetCloneColliderEnabled(true);

        for (int i = 0; i < caughtBodies.Count; i++)
        {
            ReleaseCaughtBody(caughtBodies[i]);
        }
    }

    public void BreakTether()
    {
        isBroken = true;
        StopPulling();
        Destroy(gameObject);
    }

    public bool IsConnectedTo(GameObject target)
    {
        return clone != null && clone.gameObject == target;
    }

    void SetPlayerCloneCollisionIgnored(bool ignore)
    {
        if (isIgnoringPlayerCloneCollision == ignore) return;
        if (playerCol == null || cloneCol == null) return;

        Physics2D.IgnoreCollision(playerCol, cloneCol, ignore);
        isIgnoringPlayerCloneCollision = ignore;
    }

    void SetCloneColliderEnabled(bool enabled)
    {
        if (cloneCol != null)
        {
            cloneCol.enabled = enabled;
        }
    }
}
