using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class Bullet : MonoBehaviour
{
    [SerializeField] private float _speed = 14; // —корость
    public int _damage = 3; // урон

    //создадим событи€ putme на которое будет подписыватьс€ spawnmanager, дела€ рассылку putme spawn manager помймет что надо выключить нашку пулю и поместить в pool
    private Subject<MonoBehaviour> _putMe = new Subject<MonoBehaviour>(); 
    public IObservable<MonoBehaviour> PutMe => _putMe; // ссылка на событи€ putme
    private float _goTo; // записываетьс€ положение в которое нужно прилететь
    public bool _isEnemy; // понимает вражеска€ это пул€ или наша


    private void OnEnable() // когда объект включаетс€
    {
        var controller = Controller.Instance;
        _goTo = controller.LeftUpPoint.y + 2; // левый верхний край, добавим 2 чтобы наше пул€ точно улетела за границу экрана
        StartCoroutine(Move()); // —тарт корутины, передаем в нее Move, название корутины.
        //  орутины в Unity - простой и удобный способ запускать функции, которые должны работать параллельно в течении некоторого времени.
    }

    private IEnumerator Move() //  орутина
    {
        if(_isEnemy) //если это вражеска€ пул€, она будет лететь вниз, если пул€ игрока, то будет лететь вверх
        {
            while(transform.position.y > -_goTo)
            {
                transform.position -= new Vector3(0, Time.deltaTime * _speed, 0);
                yield return null;
            }
        }
        else
        {
            while (transform.position.y < _goTo) // пока наша пул€ меньше goTo будем двигать пулю вверх
            {
                transform.position += new Vector3(0, Time.deltaTime * _speed, 0);
                yield return null; // пропускаем кадр
            }
        }
        
        // как только пул€ улетит до верхней точки камеры мы будем рассылать событи€, мы будем рассылать событи€ putme и параметр будем рассылать себ€(this) 
        _putMe.OnNext(this); 
    }


    public void HitMe() // при взаимодействии вражеского корабл€ с пулей она исчезает
    {
        _putMe.OnNext(this);
    }

    private void OnDisable() // —оздаем функцию ondisable, когда объект выключаетьс€
    {
        StopAllCoroutines(); // останавливаем корутины
    }
}
