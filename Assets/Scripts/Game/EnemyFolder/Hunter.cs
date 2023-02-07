using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

public class Hunter : BaseEnemyShip
{
    [SerializeField] private Bullet _bulletPref;
    [SerializeField] private float _coolDown = 1.3f;
    private float _coolDownCurrent = 10;
    private StageShip _currentShip;

    private Subject<(Transform, Bullet)> _fire = new Subject<(Transform, Bullet)>();
    public IObservable<(Transform, Bullet)> Fire => _fire;

    private IEnumerator LocalUpdate() // ѕока StageShip находитс€ в стадии Wait мы будем стрел€ть по кораблю
    {
        while (_currentShip == StageShip.Wait)
        {
            if(_coolDownCurrent < _coolDown)
            {
                _coolDownCurrent += Time.deltaTime;
            }
            else
            {
                _coolDownCurrent = 0;
                _fire.OnNext((transform, _bulletPref));
            }
            yield return null;
        }
    }

    protected override void UpdateStage(StageShip stage)
    {
        _currentShip = stage;
        if(stage == StageShip.Wait)
        {
            StartCoroutine(LocalUpdate());
        }
    }

}
