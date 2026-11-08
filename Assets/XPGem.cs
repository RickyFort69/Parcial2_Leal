using UnityEngine;

public class XPGem : MonoBehaviour
{
    public int value = 1;
    public float flySpeed = 10f;
    Transform target;

    public void Attract(Transform t) => target = t;

    void Update()
    {
        if (target == null) return;
        transform.position = Vector3.MoveTowards(transform.position, target.position, flySpeed * Time.deltaTime);
        if ((transform.position - target.position).sqrMagnitude < 0.1f)
        {
            target.GetComponent<PlayerXP>().AddXP(value);
            Destroy(gameObject);
        }
    }
}