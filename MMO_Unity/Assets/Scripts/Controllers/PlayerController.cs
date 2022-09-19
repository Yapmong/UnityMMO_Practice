using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class PlayerController : BaseController
{
    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);     // 비트연산자 쓰는 이유 => 유니티에서 레이어를 구분할 때 32비트를 사용하지만.
                                                                                        // 이를 모두 사용하는 것이 아니라 오직 하나의 비트만 사용해서 해당 비트의 자리수로 레이어를 구분함.
                                                                                        // 레이어의 총 갯수가 0~31까지 총 32개인 이유. 레이어의 index사용에 유의할 것.
    PlayerStat _stat;

    bool _stopSkill = false;

    public override void Init()
    {
        _stat = gameObject.GetComponent<PlayerStat>();
        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;

        if (gameObject.GetComponentInChildren<UI_HPBar>() == null)
            Managers.UI.MakeWorldSpaceUI<UI_HPBar>(transform);
    }

    protected override void UpdateMoving()
    {
        if (_lockTarget != null)
        {
            _destPos = _lockTarget.transform.position;
            float distance = (_destPos - transform.position).magnitude;
            if (distance <= 1.0f)
            {
                State = Define.State.Skill;
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
            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir, Color.green);

            if (Physics.Raycast(transform.position, dir, 1.0f, LayerMask.GetMask("Block")))
            {
                if (Input.GetMouseButton(1))
                    return;
                State = Define.State.Idle;
                return;
            }
            float moveDist = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
            transform.position += dir.normalized * moveDist;
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
        if (_lockTarget != null)
        {
            Stat targetStat = _lockTarget.GetComponent<Stat>();
            PlayerStat myStat = gameObject.GetComponent<PlayerStat>();
            int damage = Mathf.Max(0, myStat.Attack - targetStat.Defense);
            targetStat.Hp -= damage;
        }
    }

    void EndHitAnim()
    {
        if (_stopSkill)
        {
            State = Define.State.Idle;
        }
        else
        {
            State = Define.State.Skill;
        }
    }

    void OnMouseEvent(Define.MouseEvent evt)
    {
        switch (State)
        {
            case Define.State.Idle:
                OnMouseEvent_IdleRun(evt);
                break;
            case Define.State.Moving:
                OnMouseEvent_IdleRun(evt);
                break;
            case Define.State.Skill:
                if (evt == Define.MouseEvent.PointerUp)
                    _stopSkill = true;
                break;
        }
    }

    void OnMouseEvent_IdleRun(Define.MouseEvent evt)
    {
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        bool raycastHit = Physics.Raycast(ray, out hit, 100.0f, _mask);
        // Debug.DrawRay(Camera.main.transform.position, ray.direction * 100.0f, Color.red, 1.0f);

        switch (evt)
        {
            case Define.MouseEvent.PointerDown:
                if (raycastHit)
                {
                    _destPos = hit.point;
                    State = Define.State.Moving;
                    _stopSkill = false;

                    if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
                    {
                        _lockTarget = hit.collider.gameObject;
                        Debug.Log("Monster Clicked");
                    }

                    else if (hit.collider.gameObject.layer == (int)Define.Layer.Ground)
                    {
                        _lockTarget = null;
                        Debug.Log("Ground Clicked");
                    }
                }
                break;

            case Define.MouseEvent.Press:
                if (_lockTarget == null && raycastHit)
                    _destPos = hit.point;
                break;

            case Define.MouseEvent.PointerUp:
                _stopSkill = true;
                break;
        }
    }
}