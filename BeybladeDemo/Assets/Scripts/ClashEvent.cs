using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClashEvent : MonoBehaviour
{
    //Players
    public ClashEventModule m_attacker;
    public ClashEventModule m_defender;

    //Attacker Target [L, R, U, D]
    List<Vector3> m_attackTargets;

    public float attackDuration = 1f;
    public float attackAmplitude = 10f;
    bool isAttacking = false;
    int groundLayer;
    
    public float maxDistanceThresh = 70;
    public float maxDistanceThreshOffset = 10f;
    public float topDownOffset = 10f;

    public void SetPlayers(ClashEventModule attacker, ClashEventModule defender)
    {
        m_attacker = attacker;
        m_defender = defender;
    }

    // Use this for initialization
    void Start()
    {
        groundLayer = LayerMask.GetMask("floor");
        //Init
        m_attackTargets = new List<Vector3>();

        //Compute Target points
        Vector3 collisionDir = m_defender.transform.position - m_attacker.transform.position;
        m_attackTargets.Add(m_defender.transform.position - collisionDir / 2);
        m_attackTargets.Add(m_defender.transform.position + Quaternion.Euler(0, -90, 0) * collisionDir / 2);
        m_attackTargets.Add(m_defender.transform.position + collisionDir / 2);
        m_attackTargets.Add(m_defender.transform.position - Quaternion.Euler(0, -90, 0) * collisionDir / 2);

        //Enable ClashEventModule
        m_attacker.enabled = true;
        m_defender.enabled = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (!isAttacking)
        {
            Vector3 dir = (m_defender.transform.position - m_attacker.transform.position).normalized;
            Vector3 cross = Vector3.Cross(dir, m_attacker.transform.TransformDirection(Vector3.up));
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                this.triggerAnimation(m_defender.transform.position, Vector3.up, dir, cross);
            }
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                this.triggerAnimation(m_defender.transform.position, Vector3.down, dir, cross);
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                this.triggerAnimation(m_defender.transform.position, Vector3.back, dir, cross);
            }

            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                this.triggerAnimation(m_defender.transform.position, Vector3.forward, dir, cross);
            }
        }
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        point = dir + pivot;
        return point;
    }

    void triggerAnimation(Vector3 end, Vector3 relativeDirection, Vector3 dir, Vector3 cross)
    {
        isAttacking = true;
        m_attacker.GetComponent<Rigidbody>().detectCollisions = false;
        m_attacker.GetComponent<Rigidbody>().isKinematic = true;
        m_attacker.GetComponent<Rigidbody>().useGravity = false;
        List<Vector3> list = new List<Vector3>();
        list.Add(m_attacker.transform.position);


        this.getRelativeDirection(list, relativeDirection, end, dir, cross);

        list.Add(end);
		float duration = attackDuration;
		if (relativeDirection == Vector3.forward) {
			duration *= 1.5f;
		} else if (relativeDirection == Vector3.back) {
			duration *= 0.5f;
		}
        LTDescr tween = LeanTween.move(m_attacker.gameObject, list.ToArray(), duration);
		//tween.setEaseInCubic();
        tween.setOnComplete(() =>
        {
            if (relativeDirection != Vector3.forward) {
                list.Reverse();
            }

            Vector3 relDir = Vector3.zero;

            if (relativeDirection == Vector3.up)
            {
                relDir = new Vector3(0, 90, 0);
            }
            else if (relativeDirection == Vector3.down)
            {
                relDir = new Vector3(0, -90, 0);
            }
            else if (relativeDirection == Vector3.forward)
            {
                Vector3 diff =  (m_defender.transform.position - m_attacker.transform.position);
                list = new List<Vector3>();
                list.Add(getTopDownPos(end));
                list.Add(getTopDownPos(end + dir * attackAmplitude * 0.5f));
                list.Add(getTopDownPos(end + dir * attackAmplitude* 0.75f));
                list.Add(getTopDownPos(end + dir * attackAmplitude* 1.0f));

            }
            if (relativeDirection != Vector3.zero)
            {
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = getTopDownPos(RotatePointAroundPivot(list[i], m_defender.transform.position, relDir));
                }
            }

            LTDescr tween2 = LeanTween.move(m_attacker.gameObject, list.ToArray(), duration);
			//tween2.setEaseOutCubic();
            tween2.setOnComplete(() =>
            {
                isAttacking = false;
                m_attacker.GetComponent<Rigidbody>().detectCollisions = true;
                m_attacker.GetComponent<Rigidbody>().isKinematic = false;
                m_attacker.GetComponent<Rigidbody>().useGravity = true;
            });
        });
    }

    void getRelativeDirection(List<Vector3> list, Vector3 relativeDirection, Vector3 end, Vector3 dir, Vector3 cross)
    {
        if (relativeDirection == Vector3.up || relativeDirection == Vector3.down)
        {
            // left/right
            Vector3 v2 = m_attacker.transform.position + 0.33f * dir + cross * attackAmplitude;
            Vector3 v3 = m_attacker.transform.position + 0.66f * dir + cross * attackAmplitude;
            list.Add(getTopDownPos(v2));
            list.Add(getTopDownPos(v3));
        }
        else if (relativeDirection == Vector3.back)
        {
            // down
            Vector3 v2 = m_attacker.transform.position + 0.33f * dir;
            Vector3 v3 = m_attacker.transform.position + 0.66f * dir;
            list.Add(getTopDownPos(v2));
            list.Add(getTopDownPos(v3));
        }
        else if (relativeDirection == Vector3.forward)
        {
            // up
            Vector3 relativeLeft = Vector3.up;
            Vector3 relativeRight = Vector3.down;
            float randFloat = Random.Range(0.0f, 1.0f);
            Vector3 relDir = randFloat >= 0.5 ? relativeRight : relativeLeft;
            Vector3 v2 = m_attacker.transform.position + 0.5f * dir + cross * attackAmplitude * 0.5f;
            list.Add(getTopDownPos(v2));
            Vector3 v3 = v2 + dir;
            list.Add(getTopDownPos(v3));
            Vector3 v4 = m_defender.transform.position + cross * attackAmplitude* 0.5f;
            list.Add(getTopDownPos(v4));

            Vector3 end2 = m_defender.transform.position + dir * attackAmplitude / 2;
            list.Add(getTopDownPos(v4));
            list.Add(getTopDownPos(m_defender.transform.position + dir * attackAmplitude));
            list.Add(getTopDownPos(end2));
        }
    }


    private Vector3 getTopDownPos(Vector3 orig) { 
        RaycastHit hit;
        if (Vector3.Distance(m_attacker.transform.position, Vector3.zero) > maxDistanceThresh) {
            orig = (orig - Vector3.zero).normalized * (maxDistanceThresh - maxDistanceThreshOffset);
        }

        if (Physics.Raycast(new Vector3( orig.x, 999999f, orig.z), Vector3.down, out hit, 99999999f, groundLayer)) {
            orig = new Vector3(orig.x, hit.point.y + topDownOffset, orig.z);
        }

   
        return orig;
    }
}