using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterStateMachine : MonoBehaviour
{
    public GameObject Monster;
    public Animator animator;
    public Animator animator_lod;
    public Transform Target;
    public AudioSource StepSound;
    public AudioSource SniffSound;
    public AudioSource RoarSound;

    public float SearchRange;
    public float Speed;
    public float CatchRange;
    public int ScaredHeartrate;

    private Vector3 DirectionToTarget;
    private Vector3 RandomDirection;
    private int heartrate;

    enum State {
        Idle,
        Stalk,
        Search,
        Detect,
        Roar,
        Chase,
        Catch
    }

    State currentState = State.Idle;
    float stateDuration = 0.0f;
    bool setStateDuration = true;
    float elapsedStateTime = 0.0f;

    // Start is called before the first frame update
    void Start()
    {
        animator.Play("Idle");
        animator_lod.Play("Idle");
        HeartrateEventManager.OnHeartrateUpdate += SetHeartrate;
    }

    private void SetRandomDirection() {
        Vector3 direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f));
        RandomDirection = direction.normalized;
    }

    private void SetHeartrate(object sender, HeartrateEventArgs e) {
        heartrate = e.heartrate;
    }

    // Update is called once per frame
    void Update()
    {
        DirectionToTarget = Target.position - Monster.transform.position;
        // Run State Machine
        switch (currentState) {
            case State.Idle:
                currentState = IdleState();
                break;
            case State.Stalk:
                currentState = StalkState();
                break;
            case State.Search:
                currentState = SearchState();
                break;
            case State.Detect:
                currentState = DetectState();
                break;
            case State.Chase:
                currentState = ChaseState();
                break;
            case State.Roar:
                currentState = RoarState();
                break;
            default:
                break;
        }
    }

    private State IdleState() {
        Debug.Log("Idle");

        if (setStateDuration) {
            stateDuration = Random.Range(2f, 3f);
            setStateDuration = false;
        }

        elapsedStateTime += Time.deltaTime;

        if (elapsedStateTime >= stateDuration) {
            setStateDuration = true;
            elapsedStateTime = 0f;

            if (DirectionToTarget.magnitude > SearchRange) {
                animator.CrossFade("Stalk", 0.1f);
                animator_lod.CrossFade("Stalk", 0.1f);
                return State.Stalk;
            } else {
                animator.CrossFade("Search", 0.1f);
                animator_lod.CrossFade("Search", 0.1f);
                return State.Search;
            }
        }
        return State.Idle;
    }

    private State StalkState() {
        Debug.Log("Stalk");

        if (setStateDuration) {
            stateDuration = Random.Range(5f, 7f);
            setStateDuration = false;
            RandomDirection = DirectionToTarget + new Vector3(Random.Range(-8f, 8f), 0f, 0f);
            StepSound.Play();
        }

        elapsedStateTime += Time.deltaTime;

        // animator.Play("Stalk");

        // move towards the player
        Quaternion targetRotation = Quaternion.LookRotation(RandomDirection);
        Monster.transform.rotation = Quaternion.Slerp(Monster.transform.rotation, targetRotation, elapsedStateTime / 2);
        Monster.transform.position += RandomDirection.normalized * Time.deltaTime * Speed * 2f;

        if (elapsedStateTime >= stateDuration) {
            setStateDuration = true;
            elapsedStateTime = 0f;
            StepSound.Stop();

            if (DirectionToTarget.magnitude > SearchRange) {
                animator.CrossFade("Idle", 0.1f);
                animator_lod.CrossFade("Idle", 0.1f);
                return State.Idle;
            } else {
                animator.CrossFade("Detect", 0.1f);
                animator_lod.CrossFade("Detect", 0.1f);
                return State.Detect;
            }
        }

        return State.Stalk;
    }

    private State SearchState() {
        Debug.Log("Search");

        if (setStateDuration) {
            stateDuration = Random.Range(2f, 4f);
            setStateDuration = false;
            StepSound.Play();
            SetRandomDirection();
        }

        elapsedStateTime += Time.deltaTime;

        // move in a random direction
        Quaternion targetRotation = Quaternion.LookRotation(RandomDirection);
        Monster.transform.rotation = Quaternion.Slerp(Monster.transform.rotation, targetRotation, elapsedStateTime / 2);
        Monster.transform.position += RandomDirection * Time.deltaTime * Speed;

        if (elapsedStateTime >= stateDuration) {
            setStateDuration = true;
            elapsedStateTime = 0f;
            StepSound.Stop();

            if (DirectionToTarget.magnitude > SearchRange) {
                animator.CrossFade("Idle", 0.1f);
                animator_lod.CrossFade("Idle", 0.1f);
                return State.Idle;
            } else {
                animator.CrossFade("Detect", 0.1f);
                animator_lod.CrossFade("Detect", 0.1f);
                return State.Detect;
            }
        }

        return State.Search;
    }

    private State DetectState() {
        Debug.Log("Detect");

        if (setStateDuration) {
            stateDuration = 4.84f;
            setStateDuration = false;
            SniffSound.Play();
        }

        elapsedStateTime += Time.deltaTime;

        if (elapsedStateTime >= stateDuration) {
            setStateDuration = true;
            elapsedStateTime = 0f;

            if (heartrate > ScaredHeartrate) {
                animator.CrossFade("Roar", 0.2f);
                animator_lod.CrossFade("Roar", 0.2f);
                return State.Roar;
            } else {
                if (DirectionToTarget.magnitude > SearchRange) {
                    animator.CrossFade("Stalk", 0.1f);
                    animator_lod.CrossFade("Stalk", 0.1f);
                    return State.Stalk;
                } else {
                    animator.CrossFade("Search", 0.1f);
                    animator_lod.CrossFade("Search", 0.1f);
                    return State.Search;
                }
            }
        }

        return State.Detect;
    }

    private State RoarState() {
        Debug.Log("Roar");

        if (setStateDuration) {
            stateDuration = 5f;
            RoarSound.Play();
            setStateDuration = false;
        }

        elapsedStateTime += Time.deltaTime;

        Quaternion targetRotation = Quaternion.LookRotation(DirectionToTarget);
        Monster.transform.rotation = Quaternion.Slerp(Monster.transform.rotation, targetRotation, elapsedStateTime);

        if (elapsedStateTime >= stateDuration) {
            setStateDuration = true;
            elapsedStateTime = 0;

            animator.CrossFade("Chase", 0.1f);
            animator_lod.CrossFade("Chase", 0.1f);

            return State.Chase;
        }

        return State.Roar;
    }

    private State ChaseState() {
        Debug.Log("Chase");

        Quaternion targetRotation = Quaternion.LookRotation(DirectionToTarget);
        Monster.transform.rotation = Quaternion.Slerp(Monster.transform.rotation, targetRotation, elapsedStateTime);
        Monster.transform.position += DirectionToTarget.normalized * Time.deltaTime * Speed * 2f;

        if (heartrate < ScaredHeartrate) {
            setStateDuration = true;
            elapsedStateTime = 0f;
            animator.CrossFade("Detect", 0.1f);
            animator_lod.CrossFade("Detect", 0.1f);
            return State.Detect;
        }

        if (DirectionToTarget.magnitude < CatchRange) {
            Debug.Log("Boo!");
            return State.Catch;
        }
        return State.Chase;
    }
}
