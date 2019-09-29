using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClashEvent : MonoBehaviour
{
    public GameSystem m_gameSystem;

    //Players
    public ClashEventModule m_attacker;
    public ClashEventModule m_defender;

    public float attackDuration = 1f;
    public float attackAmplitude = 10f;
    bool isAttacking = false;
    int groundLayer;
    int maxCombo;
    int current_combo;
    
    public float maxDistanceThresh = 70;
    public float maxDistanceThreshOffset = 10f;
    public float topDownOffset = 10f;

    public int GetMaxCombo() {
        return maxCombo;
    }

    public void SetMaxCombo(int count) {
        maxCombo = count;
    }

    public void IncrementCombo() {
        current_combo++;
    }

    public int GetComboCount() {
        return current_combo;
    }


    public void SetPlayers(ClashEventModule attacker, ClashEventModule defender)
    {
        m_attacker = attacker;
        m_defender = defender;
        current_combo = 0;
    }

    // Use this for initialization
    void Start()
    {
        groundLayer = LayerMask.GetMask("floor");
    }

    // up -> LEFT, down -> RIGHT, forward -> UP, back -> DOWN
    // Update is called once per frame
    void Update()
    {
        if (!isAttacking)
        {
            if (m_attacker.GetCommand() == 0)
            {
                this.triggerAnimation(m_defender.transform.position, Vector3.up);
            }
            else if (m_attacker.GetCommand() == 1)
            {
                this.triggerAnimation(m_defender.transform.position, Vector3.forward);
            }
            else if (m_attacker.GetCommand() == 2)
            {
                this.triggerAnimation(m_defender.transform.position, Vector3.down);
            }
            else if (m_attacker.GetCommand() == 3)
            {
                this.triggerAnimation(m_defender.transform.position, Vector3.back);
            }
        }

        if(current_combo == maxCombo) {
            m_gameSystem.Initiate3D(
                m_attacker.gameObject.GetComponent<Beyblade>(), 
                m_defender.gameObject.GetComponent<Beyblade>());
            this.enabled = false;
        }
    }

    Vector3 RotatePointAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        Vector3 dir = point - pivot;
        dir = Quaternion.Euler(angles) * dir;
        point = dir + pivot;
        return point;
    }

    void triggerAnimation(Vector3 end, Vector3 relativeDirection)
    {
        isAttacking = true;
        m_attacker.GetComponent<Rigidbody>().detectCollisions = false;
        m_attacker.GetComponent<Rigidbody>().isKinematic = true;
        m_attacker.GetComponent<Rigidbody>().useGravity = false;
        List<Vector3> list = new List<Vector3>();
        list.Add(m_attacker.transform.position);


        Vector3 diff = end - m_attacker.transform.position;
        this.getRelativeDirection(list, relativeDirection, end);

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
                for (int i = 0; i < list.Count; i++)
                {
                    list[i] = getTopDownPos(list[i] + diff);
                }
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

            //Perform Damage Check
            IncrementCombo();
        });
    }

    void getRelativeDirection(List<Vector3> list, Vector3 relativeDirection, Vector3 end)
    {
        Vector3 diff = end - m_attacker.transform.position;
        if (relativeDirection == Vector3.up || relativeDirection == Vector3.down)
        {
            // left/right
            Vector3 cross = Vector3.Cross(diff.normalized, m_attacker.transform.TransformDirection(relativeDirection));
            Vector3 v2 = m_attacker.transform.position + 0.33f * diff + cross * attackAmplitude;
            Vector3 v3 = m_attacker.transform.position + 0.66f * diff + cross * attackAmplitude;
            list.Add(getTopDownPos(v2));
            list.Add(getTopDownPos(v3));
        }
        else if (relativeDirection == Vector3.back)
        {
            // down
            Vector3 v2 = m_attacker.transform.position + 0.33f * diff;
            Vector3 v3 = m_attacker.transform.position + 0.66f * diff;
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
            Vector3 cross = Vector3.Cross(diff.normalized, m_attacker.transform.TransformDirection(relDir));
            Vector3 v2 = m_attacker.transform.position + 0.5f * diff + cross * attackAmplitude * 0.5f;
            list.Add(getTopDownPos(v2));
            Vector3 v3 = v2 + diff;
            list.Add(getTopDownPos(v3));
            Vector3 v4 = m_defender.transform.position + cross * attackAmplitude* 0.5f;
            list.Add(getTopDownPos(v4));

            Vector3 end2 = m_defender.transform.position + diff.normalized * attackAmplitude / 2;
            list.Add(getTopDownPos(v4));
            list.Add(getTopDownPos(m_defender.transform.position + diff.normalized * attackAmplitude));
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
