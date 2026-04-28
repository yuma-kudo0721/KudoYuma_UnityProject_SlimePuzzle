using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SlimeTether : MonoBehaviour
{
    [SerializeField] float breakDistance = 8f;
    [SerializeField] float pullSpeed = 12f;
    [SerializeField] float stopDistance = 0.5f;
    [SerializeField] KeyCode pullKey = KeyCode.V;

    Transform player;
    Transform clone;

    Rigidbody2D cloneRb;
    Collider2D cloneCol;
    LineRenderer line;

    float defaultGravity;
    bool isBroken;

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
    }

    void Awake()
    {
        line = GetComponent<LineRenderer>();
        line.positionCount = 2;
        line.useWorldSpace = true;
        line.startWidth = 0.08f;
        line.endWidth = 0.08f;
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

    void PullCloneStraight()
    {
        if (cloneRb == null) return;

        Vector2 diff = (Vector2)player.position - cloneRb.position;

        if (diff.magnitude <= stopDistance)
        {
            cloneRb.velocity = Vector2.zero;
            cloneRb.gravityScale = defaultGravity;

            if (cloneCol != null)
            {
                cloneCol.enabled = true;
            }
            return;
        }

        if (cloneCol != null)
        {
            cloneCol.enabled = false;
        }

        cloneRb.velocity = diff.normalized * pullSpeed;
    }

    void StopPulling()
    {
        if (cloneRb != null)
        {

            cloneRb.gravityScale = defaultGravity;
        }

        if (cloneCol != null)
        {
            cloneCol.enabled = true;
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
}
