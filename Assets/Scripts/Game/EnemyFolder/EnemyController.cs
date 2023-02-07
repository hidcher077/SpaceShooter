using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float _minDelay = 2; // ¬ эту переменную будем записывать мнимальное значение между по€влением врагов в секундах
    [SerializeField] private float _maxDelay = 4;
    [SerializeField] private int _maxCountOneSpawn = 5; // максимальное значение по€вление кораблей за один спавн
    private float _timerDelay; // ¬ переменную будем записывать текущее врем€ таймера
    private int _countOnePull; // ¬ эту переменную мы будем определ€ть текущее колчиство кораблей котора€ будет созданно
    private SpawnManager _spawnManager; // ¬ нее записываем ссылку на spawn manager
    private CompositeDisposable _disposablesEnemy = new CompositeDisposable(); // —оздаем экземпл€р класса

    private Coroutine _coroutine; // переменна€ корутины

    private void Awake()
    {
        _spawnManager = GetComponent<SpawnManager>();
        _timerDelay = Random.Range(_minDelay, _maxDelay); // —лучайно выбирает секунды между по€влением группы кораблей
        //StartCoroutine(SpawnEnemy());
        
    }
    private void OnEnable()
    {
        _disposablesEnemy = new CompositeDisposable();
        _coroutine = StartCoroutine(SpawnEnemy()); // включение корутины
    }

    private IEnumerator SpawnEnemy() // ¬ корутине лежит бесконечный цикл, который перестанет работать если выключитьс€ корутина
    {
        while (true)
        {
            _timerDelay -= Time.deltaTime;
            

            if (_timerDelay < 0) //  огда в переменной кол-во секунд станет меньше 0, а это и есть подсчет времени, создадутьс€ корабли-враги
            {
                _countOnePull = Random.Range(1, _maxCountOneSpawn); // записываем в пул, сколько врагов должно создастьс€
                _timerDelay = Random.Range(_minDelay, _maxDelay);
                for (int i=0; i<_countOnePull; i++)
                {
                    var hunter = _spawnManager.SpawnEnemy(); // ¬ скрипт SpawnManager в функцию SpawnEnemy отправл€етьс€ количество созданных врагов из пула, на количество отработанных циклов
                   
                   // if (ship = null) continue;
                   // var hunter = ship as Hunter;
                    if(hunter != null)
                    {
                        hunter.Fire.Subscribe((param) => Fire(param.Item1, param.Item2)).AddTo(_disposablesEnemy);
                    }
                    yield return null;
                }
                _countOnePull = Random.Range(1, _maxCountOneSpawn);
            }
            yield return null; // если таймер _timerDelay больше нул€ мы пропускаем кадры в корутине
        }
    }

    private void Fire(Transform tr, Bullet bullet)
    {
        _spawnManager.SpawnBullet(tr, bullet);
    }

    private void OnDisable()
    {
        if(_coroutine != null)
        {
            StopCoroutine(_coroutine);
            _coroutine = null;
        }
        _disposablesEnemy.Dispose();
        _disposablesEnemy = null;
    } 

}
