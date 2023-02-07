using UnityEngine;
using UniRx;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    private PlayerShip _playerShip; // ������� ���������� player ship ������� ����� ��������� ��� ������� ����� ������������ � ��� ����������

    [SerializeField] private GameObject _bulletPref; // � ��� �� ����� �������� ������ �� ������ ����� ����
    [SerializeField] private Transform _poolBulletMy; // ������� ����������� ������ pool, � ������� ����� ���������� ���� ����
    [SerializeField] private List<GameObject> _enemyPrefabs = new List<GameObject>(); // ������� ������ ��������
    [SerializeField] private Transform _poolEnemyRoot; // ��������� ��� ���� ����� �������� �������

    [SerializeField] private Transform _poolEnemyBullet; // �������� ��� ��� ���� ���������� �������

    private List<Transform> _rootEnemyType = new List<Transform>(); // ������ ������������� �����
    private CompositeDisposable _disposables = new CompositeDisposable(); // ��� ������ � ����������� ��������, �������� ��������� �������
    
    private void Start()
    {
        Controller.Instance.Score.Value = 0; // ������� ���� �������� ����
        _playerShip = Controller.Instance._myShip;
        _playerShip.FireClick.Subscribe((_)=> SpawnBullet()); // ���������� �� ������� fireClick � ������� Playership


        foreach (var enemy in _enemyPrefabs)
        {
            GameObject root = new GameObject("root" + enemy.name);
            root.transform.parent = _poolEnemyRoot; // � ���������� ���� ��������� ��������
            _rootEnemyType.Add(root.transform); // ��������� ���� � ��� ������
        }
    }

    // private void SpawnBullet(Transform enemyTransform = null, Bullet enemyBullet = null) - spawnbullet ������� ���������
    public void SpawnBullet(Transform enemyTransform = null, Bullet enemyBullet = null) 
    {
        GameObject bullet;

        if(enemyBullet != null && enemyTransform != null)
        {
            if(_poolEnemyBullet.childCount > 0)
            {
                bullet = _poolEnemyBullet.GetChild(0).gameObject;
            }
            else
            {
                bullet = Instantiate(enemyBullet).gameObject; // �������� ������ ��� ��������� ����
                var bulletScript = bullet.GetComponent<Bullet>();
                bulletScript.PutMe.Subscribe(PutObject).AddTo(_disposables);
            }
            bullet.transform.parent = transform;           //������ _poolBulletMy �� transform ������ ��� ���� (
            var position = enemyTransform.transform.position;
            bullet.transform.position = new Vector3(position.x, position.y - 1.2f, 0); // ������������� ������� ���� � ������� hunter � ������� ���� ����
        }
        else
        {
            if (_poolBulletMy.childCount > 0) // ���� � ����� pool �������� ��������� ������ ����, ��� �������� ��� � ��� ������ ���� ��������� ����
            {
                bullet = _poolBulletMy.GetChild(0).gameObject; // ��������� bullet ������� �������� ������� pool-a
            }
            else // ���� � poola ���� �������� ���������  �������� ����� �������� ����
            {
                bullet = Instantiate(_bulletPref); // ������� ��������� �������
                bullet.GetComponent<Bullet>().PutMe.Subscribe(PutObject).AddTo(_disposables); // ��� �������� ����, ����� ������������� �� PutMe, ������� ��������� ������� Bullet
            }
            bullet.transform.parent = transform; // ������ ���� ������������ ���������� �� �������
            var pos = _playerShip.transform.position; // �������� ������� ������
            bullet.transform.position = new Vector3(pos.x, pos.y + 1.2f, 0); // ���������� ���� � ������� ������ �������, ������� �� ������ ������� �����
        }

       
        bullet.gameObject.SetActive(true); // �������� ������
    }


    public Hunter SpawnEnemy() // �������� ��������� ������� ������� ����� ���������� ������� ����� ������
    {
        var controller = Controller.Instance;
        GameObject ship;
        int type = Random.Range(0, _enemyPrefabs.Count); // ����� �� ����� ��������� ��������� ������ �� ������, count ������ ���������� ���������
        var pool = _rootEnemyType[type]; // ������� ������ �� ������ �����
        

        if (pool.childCount > 0) // ���� � ����� ���� ����� �������� ��������� ������ ���� �� ����� ������ ���������� �������
        {
            ship = pool.GetChild(0).gameObject; // ����� ������ ���������� �������
            
        }
        else
        {
            ship = Instantiate(_enemyPrefabs[type]); // ���� � ���� ���� �������� �� ������� ����� �������
            var enemyShip = ship.GetComponent<BaseEnemyShip>(); //
            enemyShip.PutMe.Subscribe(PutObject).AddTo(_disposables); // ���������� �� ������� putme � ����� ���������� � putobject, �������� ����� ����������� AddTo ��������� � disposibles
            enemyShip._myRoot = pool; // �������� ���������� ������� ��� ���
            enemyShip._player = _playerShip;
            
        }

        ship.transform.parent = _poolEnemyRoot; // ���� ��������� ������� ����� ���� �� �������� � ��� enemy root
        var height = controller.RightUpPoint.y + 2; // ��������� ���������� ������� ������ 

        Vector3 spawnPos = new Vector3(Random.Range(controller.LeftUpPoint.x + 0.5f, controller.RightUpPoint.x - 0.5f), height, 0); // ���������� ��������� ������ �������

        ship.transform.position = spawnPos; // ������ ������� � ����
        ship.SetActive(true); // ���������� ������� � ����, ��� ������� ����� ����������� � ������������ ����� ������ � �������� � ����

        return ship.GetComponent<Hunter>(); // ���������� ������ � ��������� BaseEnemyShip ��� �������� �������� � ������� BaseEnemy


    }

    private void PutObject(MonoBehaviour mono)
    {
        var objBull = mono as Bullet; // ��������� ������������ �� ����� mono ������ Bullet, ���� �� ������ null �� �� ���������� �������
        if(objBull != null)
        {
            if(objBull._isEnemy)
            {
                objBull.transform.parent = _poolEnemyBullet;
            }
            else
            {
                objBull.transform.parent = _poolBulletMy; // �������� ���� ���� � pool ����
            }
            
            objBull.gameObject.SetActive(false); // ��������� ����
            return;
        }
        


        var objShip = mono as BaseEnemyShip; 
        if (objShip != null)
        {
            //Controller.Instance.Score.Value += objShip.CostPointersScore; // ������� ����� �� ��������� �������
            objShip.transform.parent = objShip._myRoot; // ����� ����������� ���� ���������� ���� ������� � ��� 
            objShip.gameObject.SetActive(false); // ��������� �������
           
        }
        

    }


    private void OnEnable()
    {
        _disposables = new CompositeDisposable();        
    }

    private void OnDisable()
    {
        _disposables.Dispose(); // ������� Dispose �������� ��� �������� ����������� � disposables
        _disposables = null;
    }
}