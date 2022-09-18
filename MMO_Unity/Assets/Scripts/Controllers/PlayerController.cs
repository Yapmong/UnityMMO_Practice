using System.Collections;
using System.Collections.Generic;
using UnityEditor.SceneTemplate;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    public enum PlayerState
    {
        Idle,
        Moving,
        Die,
        Skill,
    }

    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);     // ��Ʈ������ ���� ���� => ����Ƽ���� ���̾ ������ �� 32��Ʈ�� ���������.
                                                                                        // �̸� ��� ����ϴ� ���� �ƴ϶� ���� �ϳ��� ��Ʈ�� ����ؼ� �ش� ��Ʈ�� �ڸ����� ���̾ ������.
                                                                                        // ���̾��� �� ������ 0~31���� �� 32���� ����. ���̾��� index��뿡 ������ ��.
    [SerializeField]
    float _rotationSpeed = 20.0f;

    PlayerStat _stat;
    Vector3 _destPos;       // ���콺 Ŭ�� ������ ����

    [SerializeField]
    PlayerState _state = PlayerState.Idle;

    GameObject _lockTarget;

    void Start()
    {
        _stat = gameObject.GetComponent<PlayerStat>();

        // �Է� ���� �븮�� �̺�Ʈ �߰�, Ű����� ��� ����.
        // Managers.Input.KeyAction -= OnKeyboard;     
        // Managers.Input.KeyAction += OnKeyboard;
        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;

        Managers.UI.MakeWorldSpaceUI<UI_HPBar>(transform);
    }

    // ���� _state�� ���� ������Ƽ, �ִϸ��̼� ���� ����� ���� ����
    public PlayerState State
    {
        get { return _state; }
        set
        {
            _state = value;
            Animator anim = GetComponent<Animator>();
            switch (_state)
            {
                case PlayerState.Idle:
                    anim.CrossFade("WAIT", 0.1f);
                    break;
                case PlayerState.Moving:
                    anim.CrossFade("RUN", 0.03f);
                    break;
                case PlayerState.Die:
                    break;
                case PlayerState.Skill:
                    anim.CrossFade("ATTACK", 0.1f, -1, 0);
                    break;
            }
        }
    }

    void UpdateIdle()
    {

    }

    void UpdateMoving()
    {
        if (_lockTarget != null)
        {
            _destPos = _lockTarget.transform.position;
            float distance = (_destPos - transform.position).magnitude;
            if (distance <= 1.0f)
            {
                State = PlayerState.Skill;
                return;
            }
        }

        // �̵�
        Vector3 dir = _destPos - transform.position;        // ������ ���������� ���⺤��(ũ�Ⱑ 1���� ����.)
        if (dir.magnitude < 0.1f)        // �������� ������ ���
        {
            State = PlayerState.Idle;
        }
        else
        {
            NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();
            float moveDist = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
            nma.Move(dir.normalized * moveDist);

            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir, Color.green);

            if (Physics.Raycast(transform.position, dir, 1.0f, LayerMask.GetMask("Block")))
            {
                if (Input.GetMouseButton(1))
                    return;
                State = PlayerState.Idle;
                return;
            }
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), _rotationSpeed * Time.deltaTime);
        }

    }
    void UpdateDie()
    {

    }

    void UpdateSkill()
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
        if (_stopSkill)
        {
            State = PlayerState.Idle;
        } 
        else
        {
            State = PlayerState.Skill;
        }
    }

    void Update()
    {
        switch (_state)
        {
            case PlayerState.Idle:
                UpdateIdle();
                break;
            case PlayerState.Moving:
                UpdateMoving();
                break;
            case PlayerState.Die:
                UpdateDie();
                break;
            case PlayerState.Skill:
                UpdateSkill();
                break;
        }
    }

    bool _stopSkill = false;

    void OnMouseEvent(Define.MouseEvent evt)
    {
        switch (State)
        {
            case PlayerState.Idle:
                OnMouseEvent_IdleRun(evt);
                break;
            case PlayerState.Moving:
                OnMouseEvent_IdleRun(evt);
                break;
            case PlayerState.Skill:
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
                    State = PlayerState.Moving;
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