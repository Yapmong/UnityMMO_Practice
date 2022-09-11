using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    PlayerStat _stat;
    Vector3 _destPos;       // ���콺 Ŭ�� ������ ����

    Texture2D _attackIcon;
    Texture2D _handIcon;

    enum CursorType
    {
        None,
        Attack,
        Hand,
    }

    CursorType _cursorType = CursorType.None;

    void Start()
    {
        _attackIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Attack");
        _handIcon = Managers.Resource.Load<Texture2D>("Textures/Cursor/Hand");

        _stat = gameObject.GetComponent<PlayerStat>();

        // �Է� ���� �븮�� �̺�Ʈ �߰�, Ű����� ��� ����.
        // Managers.Input.KeyAction -= OnKeyboard;     
        // Managers.Input.KeyAction += OnKeyboard;
        Managers.Input.MouseAction -= OnMouseClicked;
        Managers.Input.MouseAction += OnMouseClicked;
    }

    public enum PlayerState
    {
        Idle,
        Moving,
        Die,
        Skill,
    }

    PlayerState _state = PlayerState.Idle;

    void UpdateIdle()
    {
        // �ִϸ��̼�
        Animator anim = GetComponent<Animator>();
        // ���� ���� ���¿� ���� ������ �Ѱ���
        anim.SetFloat("speed", 0);
    }

    void UpdateMoving()
    {
        Vector3 dir = _destPos - transform.position;        // ������ ���������� ���⺤��(ũ�Ⱑ 1���� ����.)
        if (dir.magnitude < 0.1f)        // �������� ������ ���
        {
            _state = PlayerState.Idle;
        }
        else
        {
            NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();
            float moveDist = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
            nma.Move(dir.normalized * moveDist);

            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir, Color.green);
            if (Physics.Raycast(transform.position, dir, 1.0f, LayerMask.GetMask("Block")))
            {
                _state = PlayerState.Idle;
                return;
            }

            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 15.0f * Time.deltaTime);
        }

        // �ִϸ��̼�
        Animator anim = GetComponent<Animator>();
        // ���� ���� ���¿� ���� ������ �Ѱ���
        anim.SetFloat("speed", _stat.MoveSpeed);
    }
    void UpdateDie()
    {

    }

    void Update()
    {
        UpdateMouseCursor();

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
        }
    }

    void UpdateMouseCursor()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100.0f, _mask))
        {
            if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
            {
                if (_cursorType != CursorType.Attack)
                {
                    Cursor.SetCursor(_attackIcon, new Vector2(_attackIcon.width / 5, 0), CursorMode.Auto);
                    _cursorType = CursorType.Attack;
                }
            }

            else if (hit.collider.gameObject.layer == (int)Define.Layer.Ground)
            {
                if (_cursorType != CursorType.Hand)
                {
                    Cursor.SetCursor(_handIcon, new Vector2(_handIcon.width / 3, 0), CursorMode.Auto);
                    _cursorType = CursorType.Hand;
                }
            }
        }
    }

    /* Ű���� �Է� ����ϴ� �κ�
    void OnKeyboard()
    {
        // ���� ȸ����
        //_yAngle += Time.deltaTime * 100;
        // transform.eulerAngles = new Vector3(0.0f, _yAngle, 0.0f);

        // + - delta
        // transform.Rotate(new Vector3(0.0f, Time.deltaTime * 100.0f, 0.0f));

        // Local -> World : TransformDirection
        // World -> Local : InverseTransformDirection

        if (Input.GetKey(KeyCode.W))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.fwd), 0.3f);
            transform.position += Vector3.fwd * Time.deltaTime * _speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.back), 0.3f);
            transform.position += Vector3.back * Time.deltaTime * _speed;
        }
        if (Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.left), 0.3f);
            transform.position += Vector3.left * Time.deltaTime * _speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(Vector3.right), 0.3f);
            transform.position += Vector3.right * Time.deltaTime * _speed;
        }

        _moveToDest = false;
    }
    */

    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);     // ��Ʈ������ ���� ���� => ����Ƽ���� ���̾ ������ �� 32��Ʈ�� ���������
                                                                                        // �̸� ��� ����ϴ� ���� �ƴ϶� ���� �ϳ��� ��Ʈ�� ����ؼ� �ش� ��Ʈ�� �ڸ����� ���̾ ������.
                                                                                        // ���̾��� �� ������ 0~31���� �� 32���� ����. ���̾��� index��뿡 ������ ��.
    void OnMouseClicked(Define.MouseEvent evt)
    {
        if (_state == PlayerState.Die)
            return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        Debug.DrawRay(Camera.main.transform.position, ray.direction * 100.0f, Color.red, 1.0f);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100.0f, _mask))
        {
            _destPos = hit.point;
            _state = PlayerState.Moving;

            if (hit.collider.gameObject.layer == (int)Define.Layer.Monster)
            {
                Debug.Log("Monster Clicked");
            }

            else if (hit.collider.gameObject.layer == (int)Define.Layer.Ground)
            {
                Debug.Log("Ground Clicked");
            }
        }
    }
}
