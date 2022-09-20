using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : BaseController
{
    Stat _stat;

    [SerializeField]
    float _scanRange = 10.0f;

    [SerializeField]
    float _attackRange = 2.0f;

    public override void Init()
    {
        _stat = gameObject.GetComponent<Stat>();

        if (gameObject.GetComponentInChildren<UI_HPBar>() == null)
            Managers.UI.MakeWorldSpaceUI<UI_HPBar>(transform);

        WorldObjectType = Define.WorldObject.Monster;
    }

    protected override void UpdateIdle()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
            return;

        float distance = (player.transform.position - transform.position).magnitude;
        if (distance <= _scanRange)
        {
            _lockTarget = player;
            State = Define.State.Moving;
            return;
        }
    }

    protected override void UpdateMoving()
    {
        if (_lockTarget != null)
        {
            _destPos = _lockTarget.transform.position;
            float distance = (_destPos - transform.position).magnitude;
            if (distance <= _attackRange)
            {
                State = Define.State.Skill;
                NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();
                nma.SetDestination(transform.position);
                return;
            }
        }

        // 이동
        Vector3 dir = _destPos - transform.position;        // 가려는 목적지로의 방향벡터(크기가 1이진 않음.)
        if (dir.magnitude < 0.1f)        // 목적지에 도달한 경우
        {
            State = Define.State.Idle;
        }
        else
        {
            NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();
            nma.SetDestination(_destPos);
            nma.speed = _stat.MoveSpeed;

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), _rotationSpeed * Time.deltaTime);
        }
    }

    protected override void UpdateSkill()
    {
        if (_lockTarget != null)
        {
            Vector3 dir = _lockTarget.transform.position - transform.position;
            Quaternion quat = Quaternion.LookRotation(dir);
            transform.rotation = Quaternion.Lerp(transform.rotation, quat, _rotationSpeed * Time.deltaTime);
        }
    }

    void OnHitEvent()
    {
        Stat targetStat = _lockTarget.GetComponent<Stat>();
        int damage = Mathf.Max(0, _stat.Attack - targetStat.Defense);
        targetStat.Hp -= damage;
    }

    void EndHitAnim()
    {
        if (_lockTarget != null)
        {
            if (_lockTarget != null)
            {
                Stat targetStat = _lockTarget.GetComponent<Stat>();

                if (targetStat.Hp <= 0)
                {
                    Managers.Game.Despawn(targetStat.gameObject);
                    State = Define.State.Idle;
                }

                if (targetStat.Hp > 0)
                {
                    float distance = (_lockTarget.transform.position - transform.position).magnitude;
                    if (distance <= _attackRange)
                        State = Define.State.Skill;
                    else
                        State = Define.State.Idle;
                }
                else
                {

                }
            }

        }

        else
        {
            State = Define.State.Idle;
        }
    }
}
