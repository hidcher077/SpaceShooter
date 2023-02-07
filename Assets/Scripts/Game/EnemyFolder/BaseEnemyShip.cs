using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

// ��������� ������� ����� ����� ��� ������, In ����� ����� ������ � ����� ���������, wait ����� ����� � ����� ��������� � Out ����� ����� ����
public enum StageShip // �������� �������������
{
    In,
    Wait,
    Out
}

public abstract class BaseEnemyShip : MonoBehaviour // ����� �� �� ������ �����������
{
    [Header("Base Fields")] //���� ��� ������������� � ��������� � ���� ���-������
    [SerializeField] private float _normalSpeed = 8; // �������� ���������� �������
    [SerializeField] private float _delayTurbo = 2; // ����� � �������� ����� �������� ��������� ������� ����� ���� � ��� ������� ���� ����� ���� 
    [SerializeField] private float _turboSpeed = 5; // �������� ������� ����� �� ����� �������� ������� ���� ������
    [SerializeField] private float _speedRotation = 0.01f; // �������� �������� ���������� �������
    [SerializeField] private int _collisionDamage = 10; // ���� ������� ������� ������� ��� ������������ � �������� ������
    [SerializeField] private int _maxHealth = 2; // ���������� ����������� ����� ���������� �������
    [SerializeField] private int _costPointesScore = 5; // ���� ��� ����������� ���������� �������

    public int CostPointersScore => _costPointesScore; // ������ �� ���� �� �������� �������


    [HideInInspector] public PlayerShip _player;
    [HideInInspector] public Transform _myRoot;
    [HideInInspector] public Vector3 _playerLastPos = Vector3.up; // ��������� ������� ������� ������

    private Subject<MonoBehaviour> _putMe = new Subject<MonoBehaviour>();
    public IObservable<MonoBehaviour> PutMe => _putMe;

    private Vector3 DirectionToPlayer => transform.position - new Vector3(_playerLastPos.x, _playerLastPos.y,0); // ������������ ������� �� ������ �������
    private int _health = 100; // ������� �������� ������ �������
    private float _goTo; // ������������� ����� �������� ������ �������
    private float _goToPointTurbo; // ����� �� ������� ��������� ������� ������ ������������ � ������� �� �������� ������
    private float _timerDelay; // ����� ���������� ������� ��������� �������


    private IEnumerator Core() // ������� ��������
    {
        UpdateStage(StageShip.In);   // ��������� ����� � ������� ��������� ��������� ������� � ��������������� ���
        while (transform.position.y > _goToPointTurbo)  // ���� ������� ������ �� y ������ ��� _goToPointTurbo, �� ����� ������� �������
        {
            transform.position -= new Vector3(0, Time.deltaTime * _normalSpeed, 0);
            Look(new Vector3(0, _goToPointTurbo, 0)); // �������� � ����������� �������� ��������� ������� 
            yield return null;
        }

        UpdateStage(StageShip.Wait); // ����� �������� �� ��������� ������� ������, ��������� ������� �������� �����
        while (_timerDelay < _delayTurbo) // ���� timerDelay ������ delayTurbo ������� ����� �����, ����� �� ������ ������ ������� ������ ��������
        {
            _timerDelay += Time.deltaTime;
            yield return null;

        }

        UpdateStage(StageShip.Out); // ������� �������� ������ � ������� ������� ����� ������� ��� �� ������ ���� ���������� ������� ������ �������
        if (_playerLastPos != Vector3.up) // ���� ������ _playerLastPos ������������ Vector3.up �� �� ��� �������, � ������ ��� ������� ���� kamikadze
        {
            var dir = DirectionToPlayer / DirectionToPlayer.magnitude;
            while (transform.position.y > _goTo && transform.position.y < -_goTo)
            {
                Look(dir); // �������� � ����������� �������� 
                transform.position -= dir * (Time.deltaTime * _turboSpeed); // ������� ��� ������� � ����������� ���������� ������� ������� ������
                yield return null;

            }

        }
        else // ������ ��� ������� ����� ���� �� ������
        {
            while (transform.position.y > _goTo)
            {
                transform.position -= new Vector3(0, Time.deltaTime * _turboSpeed, 0);
                yield return null;
            }
        }

        _putMe.OnNext(this); // �������� ������� putMe ������� ���������� ������� ������� � ���, ����� ��������� ��������
    }

    private void OnEnable()
    {
        _timerDelay = 0; // ��������� ����� ������ �������� ���������� �������
        var controller = Controller.Instance;
        _goTo = controller.RightDownPoint.y - 2; // ����� ���� ������ ���� ������ ������� ��� �������, �������� �������
        _goToPointTurbo = UnityEngine.Random.Range((controller.CenterCam.y + 1), (controller.LeftUpPoint.y - 1)); // ���������� ��������� �����, ��� ������� ���������������� � ������������� � ����������� ������� ������
        _health = _maxHealth;
        StartCoroutine(Core());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }



    protected void Look(Vector3 dir, bool lerp = false, bool invertion = false)
    {
        float signedAngle = Vector2.SignedAngle(Vector2.down, dir); // � ����������� signedAngle ���������� ���� �����������
        if (invertion == true) signedAngle += 180; //���� invertion ����� true �� �� �������������� ������� �� 180 ��������
        if (MathF.Abs(signedAngle) >= 1e-3f) // ��������� ��� ������ abs ������ ��� ����� 1e-3f
        {
            var agles = transform.eulerAngles; // ��������� �������� ��������
            agles.z = signedAngle; // ��������� ���������� ������� ���� �� z
            if (lerp)
            {
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, agles, _speedRotation); // ������� ����� �������������� �� �������� ��������� speedrotation
            }
            else
            {
                transform.eulerAngles = agles; //���� lerp ������� �� �� ����� ����������� �������� agles ���� �������
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // ������� ������� ������� ���� � ���������� �������
    {
        var obj = collision.gameObject;
        if (obj.CompareTag("Bullet"))
        {
            var bull = obj.GetComponent<Bullet>();
            bull.HitMe();
            DamageMe(bull._damage, this);
            return;

        }
        if (obj.CompareTag("Player"))
        {
            obj.GetComponent<PlayerShip>().DamageMe(_collisionDamage); // ���� ������� ������� ��������� �������, ������� ������
            Controller.Instance.Score.Value += (_costPointesScore/2); // ���� �� ������ �������, �� �������� �������� �����
            _putMe.OnNext(this);
        }
    }

    private void DamageMe(int damage, BaseEnemyShip baseEnemy) // ����� ����� ����������� ������ ��� ����� ����, �� ������� ��������
    {
        _health -= damage;
        if(_health <= 0)
        {
            _health = _maxHealth;
            Controller.Instance.Score.Value += _costPointesScore; // ���� ������ ����� ��������� ������� �� �������� ������ ����
            _putMe.OnNext(this);
        }
    }


    protected abstract void UpdateStage(StageShip stage); // ��� ��������� protected ����� ������ ������ ������� ����������� �� BaseShip, � ����� ��� ����� BaseShip

}

