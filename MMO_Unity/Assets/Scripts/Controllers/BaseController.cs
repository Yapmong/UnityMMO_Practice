using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public abstract class BaseController : MonoBehaviour
{
    [SerializeField]
    protected float _rotationSpeed = 20.0f;

    [SerializeField]
    protected Vector3 _destPos;       // 마우스 클릭 목적지 정보

    [SerializeField]
    protected Define.State _state = Define.State.Idle;

    [SerializeField]
    protected GameObject _lockTarget;

    public Define.WorldObject WorldObjectType { get; protected set; } = Define.WorldObject.Unknown;

    // 변수 _state에 대한 프로퍼티, 애니메이션 변경 기능을 묶기 위함
    public virtual Define.State State
    {
        get { return _state; }
        set
        {
            _state = value;
            Animator anim = GetComponent<Animator>();
            switch (_state)
            {
                case Define.State.Idle:
                    anim.CrossFade("WAIT", 0.1f);
                    break;
                case Define.State.Moving:
                    anim.CrossFade("RUN", 0.03f);
                    break;
                case Define.State.Die:
                    break;
                case Define.State.Skill:
                    anim.CrossFade("ATTACK", 0.1f, -1, 0);
                    break;
            }
        }
    }

    private void Start()
    {
        Init();
    }

    void Update()
    {
        switch (_state)
        {
            case Define.State.Idle:
                UpdateIdle();
                break;
            case Define.State.Moving:
                UpdateMoving();
                break;
            case Define.State.Die:
                UpdateDie();
                break;
            case Define.State.Skill:
                UpdateSkill();
                break;
        }
    }
    public abstract void Init();

    protected virtual void UpdateIdle() { }
    protected virtual void UpdateMoving() { }
    protected virtual void UpdateDie() { }
    protected virtual void UpdateSkill() { }
}