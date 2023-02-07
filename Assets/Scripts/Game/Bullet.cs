using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 14; // ��������
    public int _damage = 3; // ����

    //�������� ������� putme �� ������� ����� ������������� spawnmanager, ����� �������� putme spawn manager ������� ��� ���� ��������� ����� ���� � ��������� � pool
    private Subject<MonoBehaviour> _putMe = new Subject<MonoBehaviour>(); 
    public IObservable<MonoBehaviour> PutMe => _putMe; // ������ �� ������� putme
    private float _goTo; // ������������� ��������� � ������� ����� ���������
    public bool _isEnemy; // �������� ��������� ��� ���� ��� ����


    private void OnEnable() // ����� ������ ����������
    {
        var controller = Controller.Instance;
        _goTo = controller.LeftUpPoint.y + 2; // ����� ������� ����, ������� 2 ����� ���� ���� ����� ������� �� ������� ������
        StartCoroutine(Move()); // ����� ��������, �������� � ��� Move, �������� ��������.
        // �������� � Unity - ������� � ������� ������ ��������� �������, ������� ������ �������� ����������� � ������� ���������� �������.
    }

    private IEnumerator Move() // ��������
    {
        if(_isEnemy) //���� ��� ��������� ����, ��� ����� ������ ����, ���� ���� ������, �� ����� ������ �����
        {
            while(transform.position.y > -_goTo)
            {
                transform.position -= new Vector3(0, Time.deltaTime * _speed, 0);
                yield return null;
            }
        }
        else
        {
            while (transform.position.y < _goTo) // ���� ���� ���� ������ goTo ����� ������� ���� �����
            {
                transform.position += new Vector3(0, Time.deltaTime * _speed, 0);
                yield return null; // ���������� ����
            }
        }
        
        // ��� ������ ���� ������ �� ������� ����� ������ �� ����� ��������� �������, �� ����� ��������� ������� putme � �������� ����� ��������� ����(this) 
        _putMe.OnNext(this); 
    }


    public void HitMe() // ��� �������������� ���������� ������� � ����� ��� ��������
    {
        _putMe.OnNext(this);
    }

    private void OnDisable() // ������� ������� ondisable, ����� ������ ������������
    {
        StopAllCoroutines(); // ������������� ��������
    }
}
