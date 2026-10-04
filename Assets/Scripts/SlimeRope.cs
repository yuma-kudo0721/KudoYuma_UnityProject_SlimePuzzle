using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SlimeRope : MonoBehaviour
{
    [SerializeField] GameObject ropeSegmentPrefab;
    [SerializeField] int segmentCount = 12;
    [SerializeField] float breakDistance = 10f;
    [SerializeField] float pullSpeed = 12f;
    [SerializeField] float ropePullSpeed = 4f;
    [SerializeField] float stopDistance = 0.5f;
    [SerializeField] float shortenDistance = 0.25f;
    [SerializeField] KeyCode pullKey = KeyCode.V;

    [Header("Stability")]
    [SerializeField] float segmentMass = 0.08f;
    [SerializeField] float segmentGravityScale = 0.6f;
    [SerializeField] float segmentDrag = 2f;
    [SerializeField] float segmentAngularDrag = 2f;
    [SerializeField] float maxSegmentSpeed = 8f;
    [SerializeField] float anchorFollowSpeed = 25f;

    Transform player;
    Transform clone;

    Rigidbody2D playerRb;
    Rigidbody2D cloneRb;
    Collider2D playerCol;
    Collider2D cloneCol;

    LineRenderer line;
    readonly List<GameObject> segments = new List<GameObject>();

    float defaultCloneGravity;
    Rigidbody2D playerAnchorRb;
    HingeJoint2D cloneJoint;

    public void Setup(Transform playerTransform, Transform cloneTransform)
    {
        player = playerTransform;
        clone = cloneTransform;

        playerRb = player.GetComponent<Rigidbody2D>();
        cloneRb = clone.GetComponent<Rigidbody2D>();
        playerCol = player.GetComponent<Collider2D>();
        cloneCol = clone.GetComponent<Collider2D>();

        if (cloneRb != null)
        {
            defaultCloneGravity = cloneRb.gravityScale;
        }

        CreateRope();
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.useWorldSpace = true;
        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
    }

    void Update()
    {
        if (player == null || clone == null)
        {
            Destroy(gameObject);
            return;
        }

        if (Vector2.Distance(player.position, clone.position) > breakDistance)
        {
            Destroy(gameObject);
            return;
        }

        UpdateLine();

        if (Input.GetKey(pullKey))
        {
            PullClone();
        }
        else if (cloneRb != null)
        {
            cloneRb.gravityScale = defaultCloneGravity;
        }
    }

    void FixedUpdate()
    {
        if (playerAnchorRb != null && player != null)
        {
            Vector2 anchorPosition = Vector2.Lerp(
                playerAnchorRb.position,
                player.position,
                anchorFollowSpeed * Time.fixedDeltaTime
            );
            playerAnchorRb.MovePosition(anchorPosition);
        }

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;

            Rigidbody2D rb = segments[i].GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            rb.velocity = Vector2.ClampMagnitude(rb.velocity, maxSegmentSpeed);
        }
    }

    void CreateRope()
    {
        if (ropeSegmentPrefab == null) return;
        if (playerRb == null || cloneRb == null) return;

        playerAnchorRb = CreatePlayerAnchor();
        Rigidbody2D previousRb = playerAnchorRb;

        for (int i = 0; i < segmentCount; i++)
        {
            float t = (i + 1f) / (segmentCount + 1f);
            Vector2 pos = Vector2.Lerp(player.position, clone.position, t);

            GameObject segment = Instantiate(ropeSegmentPrefab, pos, Quaternion.identity, transform);
            segments.Add(segment);

            Rigidbody2D rb = segment.GetComponent<Rigidbody2D>();
            HingeJoint2D joint = segment.GetComponent<HingeJoint2D>();

            if (rb == null || joint == null)
            {
                Debug.LogError("RopeSegment prefab needs Rigidbody2D and HingeJoint2D.");
                Destroy(gameObject);
                return;
            }

            ConfigureSegment(rb, joint);
            IgnoreSegmentCollisions(segment);

            joint.connectedBody = previousRb;

            IgnoreOwnerCollision(segment);

            previousRb = rb;
        }

        cloneJoint = clone.gameObject.AddComponent<HingeJoint2D>();
        cloneJoint.connectedBody = previousRb;
        cloneJoint.autoConfigureConnectedAnchor = true;
        cloneJoint.enableCollision = false;
    }

    void ConfigureSegment(Rigidbody2D rb, HingeJoint2D joint)
    {
        rb.mass = segmentMass;
        rb.gravityScale = segmentGravityScale;
        rb.drag = segmentDrag;
        rb.angularDrag = segmentAngularDrag;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        joint.autoConfigureConnectedAnchor = true;
        joint.enableCollision = false;
    }

    void IgnoreSegmentCollisions(GameObject segment)
    {
        Collider2D segmentCol = segment.GetComponent<Collider2D>();
        if (segmentCol == null) return;

        for (int i = 0; i < segments.Count; i++)
        {
            GameObject other = segments[i];
            if (other == null || other == segment) continue;

            Collider2D otherCol = other.GetComponent<Collider2D>();
            if (otherCol != null)
            {
                Physics2D.IgnoreCollision(segmentCol, otherCol, true);
            }
        }
    }

    Rigidbody2D CreatePlayerAnchor()
    {
        GameObject anchor = new GameObject("SlimeRopePlayerAnchor");
        anchor.transform.SetParent(transform);
        anchor.transform.position = player.position;

        Rigidbody2D anchorRb = anchor.AddComponent<Rigidbody2D>();
        anchorRb.bodyType = RigidbodyType2D.Kinematic;
        anchorRb.gravityScale = 0f;
        anchorRb.interpolation = RigidbodyInterpolation2D.Interpolate;

        return anchorRb;
    }

    void UpdateLine()
    {
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            if (segments[i] == null)
            {
                Destroy(gameObject);
                return;
            }
        }

        line.positionCount = segments.Count + 2;

        line.SetPosition(0, player.position);

        for (int i = 0; i < segments.Count; i++)
        {
            line.SetPosition(i + 1, segments[i].transform.position);
        }

        line.SetPosition(segments.Count + 1, clone.position);
    }

    void PullClone()
    {
        if (cloneRb == null) return;

        if (segments.Count == 0)
        {
            PullCloneToPlayer();
            return;
        }

        PullSegmentsAlongRope();
        ShortenRopeIfCloneReachedLastSegment();

        if (segments.Count == 0)
        {
            PullCloneToPlayer();
            return;
        }

        Rigidbody2D lastSegmentRb = segments[segments.Count - 1].GetComponent<Rigidbody2D>();
        if (lastSegmentRb == null) return;

        Vector2 diff = lastSegmentRb.position - cloneRb.position;

        if (diff.magnitude <= stopDistance)
        {
            cloneRb.velocity = Vector2.zero;
            cloneRb.gravityScale = defaultCloneGravity;
            return;
        }

        cloneRb.velocity = diff.normalized * pullSpeed;
    }

    void PullCloneToPlayer()
    {
        Vector2 diff = (Vector2)player.position - cloneRb.position;

        if (diff.magnitude <= stopDistance)
        {
            cloneRb.velocity = Vector2.zero;
            cloneRb.gravityScale = defaultCloneGravity;
            return;
        }

        cloneRb.velocity = diff.normalized * pullSpeed;
    }

    void ShortenRopeIfCloneReachedLastSegment()
    {
        if (segments.Count == 0 || cloneRb == null) return;

        GameObject lastSegment = segments[segments.Count - 1];
        if (lastSegment == null)
        {
            Destroy(gameObject);
            return;
        }

        Rigidbody2D lastSegmentRb = lastSegment.GetComponent<Rigidbody2D>();
        if (lastSegmentRb == null) return;

        float distance = Vector2.Distance(cloneRb.position, lastSegmentRb.position);
        if (distance > shortenDistance) return;

        segments.RemoveAt(segments.Count - 1);
        Destroy(lastSegment);
        ReconnectCloneJointToRopeEnd();
    }

    void ReconnectCloneJointToRopeEnd()
    {
        if (cloneJoint == null) return;

        if (segments.Count > 0)
        {
            Rigidbody2D newEndRb = segments[segments.Count - 1].GetComponent<Rigidbody2D>();
            cloneJoint.connectedBody = newEndRb;
            return;
        }

        cloneJoint.connectedBody = playerAnchorRb;
    }

    void PullSegmentsAlongRope()
    {
        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] == null) continue;

            Rigidbody2D rb = segments[i].GetComponent<Rigidbody2D>();
            if (rb == null) continue;

            Vector2 targetPosition = i == 0
                ? (Vector2)player.position
                : (Vector2)segments[i - 1].transform.position;

            Vector2 diff = targetPosition - rb.position;
            if (diff.magnitude <= 0.05f) continue;

            rb.velocity = diff.normalized * ropePullSpeed;
        }
    }

    void IgnoreOwnerCollision(GameObject segment)
    {
        Collider2D segmentCol = segment.GetComponent<Collider2D>();
        if (segmentCol == null) return;

        if (playerCol != null)
        {
            Physics2D.IgnoreCollision(segmentCol, playerCol, true);
        }

        if (cloneCol != null)
        {
            Physics2D.IgnoreCollision(segmentCol, cloneCol, true);
        }
    }

    void OnDestroy()
    {
        if (cloneJoint != null)
        {
            Destroy(cloneJoint);
        }

        for (int i = 0; i < segments.Count; i++)
        {
            if (segments[i] != null)
            {
                Destroy(segments[i]);
            }
        }

        if (playerAnchorRb != null)
        {
            Destroy(playerAnchorRb.gameObject);
        }
    }
}
