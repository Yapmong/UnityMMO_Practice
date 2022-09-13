using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PlayerController : MonoBehaviour
{
    PlayerStat _stat;
    Vector3 _destPos;       // 마우스 클릭 목적지 정보

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

        // 입력 관리 대리자 이벤트 추가, 키보드는 사용 안함.
        // Managers.Input.KeyAction -= OnKeyboard;     
        // Managers.Input.KeyAction += OnKeyboard;
        Managers.Input.MouseAction -= OnMouseEvent;
        Managers.Input.MouseAction += OnMouseEvent;
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
        // 애니메이션
        Animator anim = GetComponent<Animator>();
        // 현재 게임 상태에 대한 정보를 넘겨줌
        anim.SetFloat("speed", 0);
    }

    void UpdateMoving()
    {
        Vector3 dir = _destPos - transform.position;        // 가려는 목적지로의 방향벡터(크기가 1이진 않음.)
        if (dir.magnitude < 0.1f)        // 목적지에 도달한 경우
        {
            _state = PlayerState.Idle;
        }
        else
        {
            NavMeshAgent nma = gameObject.GetOrAddComponent<NavMeshAgent>();
            float moveDist = Mathf.Clamp(_stat.MoveSpeed * Time.deltaTime, 0, dir.magnitude);
            nma.Move(dir.normalized * moveDist);

            Debug.DrawRay(transform.position + Vector3.up * 0.5f, dir, Color.green);
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(dir), 15.0f * Time.deltaTime);

            if (Physics.Raycast(transform.position, dir, 1.0f, LayerMask.GetMask("Block")))
            {
                if (Input.GetMouseButton(1))
                    return;
                _state = PlayerState.Idle;
                return;
            }
        }

        // 애니메이션
        Animator anim = GetComponent<Animator>();
        // 현재 게임 상태에 대한 정보를 넘겨줌
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
        if (Input.GetMouseButton(1))
            return;

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

    /* 키보드 입력 사용하던 부분
    void OnKeyboard()
    {
        // 절대 회전값
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

    int _mask = (1 << (int)Define.Layer.Ground) | (1 << (int)Define.Layer.Monster);     // 비트연산자 쓰는 이유 => 유니티에서 레이어를 구분할 때 32비트를 사용하지만
                                                                                        // 이를 모두 사용하는 것이 아니라 오직 하나의 비트만 사용해서 해당 비트의 자리수로 레이어를 구분함.
                                                                                        // 레이어의 총 갯수가 0~31까지 총 32개인 이유. 레이어의 index사용에 유의할 것.
    GameObject _lockTarget;

    void OnMouseEvent(Define.MouseEvent evt)
    {
        if (_state == PlayerState.Die)
            return;

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
                    _state = PlayerState.Moving;

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
                if(_lockTarget != null)
                {
                    _destPos = _lockTarget.transform.position;
                }
                else if (raycastHit)
                        _destPos = hit.point;
                break;

            case Define.MouseEvent.PointerUp:
                _lockTarget = null;
                break;
        }
    }
}
