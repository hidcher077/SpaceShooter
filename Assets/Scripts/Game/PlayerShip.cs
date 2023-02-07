using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.SceneManagement; //������� ��� ����������� �����

public class PlayerShip : MonoBehaviour
{
    [SerializeField] private float _speed = 15;
    [SerializeField] private float _coolDown = 0.1f;
    public int _maxHealth = 100;
    [SerializeField] private float _shipRollEuler = 45; // ��������� �������� ������� ��� �������� 45 ��������
    [SerializeField] private float _shipRollSpeed = 80; // �������� ��������
    [SerializeField] private float _smoothness = 1.2f; // ������� �������� �������

    private Subject<Unit> _fireclick = new Subject<Unit>(); // ��������� ����������� �����������, �������� ������� ��������� ��� ����� ������ ������, �������� ��������� �������
    public IObservable<Unit> FireClick => _fireclick; // ����� ��������� �� ������� _fireclick,

    private Rigidbody2D _rigidbody; // ������� ��������� rigidbody2d ������� ��� ������ � �������
    private float _coolDownCurrent = 10; // ����� ������������ ����� ����� ����������
    private MeshRenderer _mR; // meshrender �������� �� ����������� ���������� �������� � ����� ����� ����. � ������ ������ �������
    private Vector3 _sizeWorldship; // � ��� ���������� ������� ������ ������ ������� �� ���� ����
    private Controller _controller; // �������� ��� ���������� ��� �������� ����� ���������� � ���� � �� controller instance

    [HideInInspector] public ReactiveProperty<int> _health = new ReactiveProperty<int>();
    // �������� ���������� � ��������� ������� ��� ����������� ����������������, 
    //��� �������������� �������� ���� � ������ ������� � ��������� ����� �������. ������� hideininspector, ����� �������� ��� � ���������

    // ��� ��� ������ ����� ������ � ��� �� ������� ��� � rigidbody2d, ������� GetComponent<RigitBody2D>();, �� �� ����� � ��� MeshRender
    private void Awake()
    {
        if(Controller.Instance == null) // ���� ���������� ����� null, �� ��� �������� ��� ����������� ����� ��� ������������
        {
            SceneManager.LoadScene(0); //����������� ����� �������� �������
            return;
        }

        _rigidbody = GetComponent<Rigidbody2D>();
        _mR = GetComponent<MeshRenderer>();
        _controller = Controller.Instance; // ����������� ���������� ����� ������ Controller ������� �������
        _controller._myShip = this; // ������ ������ �� ������� Controller
        _sizeWorldship = _mR.bounds.extents; // �������� � ���������� ������ �������
    }

    private void Start()
    {
        _controller.UpdateCameraSettings(); // �������� Controller � ��� �������
        _health.Value = _maxHealth; // �������� ���-�� ������ ������� ��� health ����
    }

    private void Update()
    {
        UpdateKey();
        FireButtonClick();
    }

    private void FireButtonClick()
    {
        if(Input.GetMouseButton(0)) // ������� ������� ����� ������ ���� (0 - �����, 1 - ������)
        {
            if(_coolDownCurrent >= _coolDown) 
            {
                _coolDownCurrent = 0;
                _fireclick.OnNext(Unit.Default); // ������ �������� ������ ������
            }
        }
        if (_coolDownCurrent < _coolDown)
        {
            _coolDownCurrent += Time.deltaTime; // ��������� ��������
        }
    }

    private void UpdateKey()
    {
        float moveHor = Input.GetAxis("Horizontal");
        float moveVert = Input.GetAxis("Vertical");

        // � ������� .Lerp() ������ ���������� �������� ������� velocity, ������ �������� ����� ��� ������ �����, ������ �������� ��� ������ ��� ����������� �����
        _rigidbody.velocity = Vector2.Lerp(_rigidbody.velocity, new Vector2(moveHor * _speed * 1.2f, moveVert * _speed), _smoothness);


        transform.position = CheckBoardWorld();

        // ������ �������� �������, �������� � 3-� ������ ������������ ������������ � ������� Quaternion, ��� ����� ���������� ����� Quaternion � ������� Eulel
        var targetRotation = Quaternion.Euler(0, 180 + (-moveHor * _shipRollEuler), 0); // ������ ���� �������� �� ���� X, Y, Z, ������� ������ ����� �� �������� ��������� _shipRollEuller, ����� � ����� ���� ������� �������
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _shipRollSpeed * Time.deltaTime); //Slerp �������� �� ������� ��������. ������ ������� ��������,�������� ���� �� ������ ���������� � ��������
        
    }

    // ������� ��� ����������� ������ ������� � ���� ����������� ������
    private Vector3 CheckBoardWorld()
    {
        var pos = transform.position; // ������� �������������� �������
        var x = pos.x;
        var y = pos.y;

        x = Mathf.Clamp(x,_controller.LeftDownPoint.x + _sizeWorldship.x, _controller.RightDownPoint.x - _sizeWorldship.x); // ������������ ���������� �� ������������� �� ������������ ��������
        y = Mathf.Clamp(y, _controller.LeftDownPoint.y + _sizeWorldship.y, _controller.LeftUpPoint.y - _sizeWorldship.y);

        return new Vector3(x,y,0); // ��� ��� ������� ������ �� ���� X � Y, Z ����� ����� 0
    }
    public void DamageMe(int damage) // ���� � ��� ����������� ����� �� ���� �� ����������� ����
    {
        _health.Value -= damage;
        if(_health.Value <= 0)
        {
            _health.Value = 0;
            var tr = transform;
            var position = tr.position;
            gameObject.SetActive(false);
            _controller.GameOver();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var obj = collision.gameObject;
        if(obj.CompareTag("EnemyBullet")) //��������� �� �������� ���� ����
        {
            /*var bull = obj.GetComponent<Bullet>();
            DamageMe(bull._damage);
            bull.HitMe(); // ������� ����*/
            var bull = obj.GetComponent<Bullet>();
            bull.HitMe();
            DamageMe(bull._damage);
            return;
        }
    }
}

