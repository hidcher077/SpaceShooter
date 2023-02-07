using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Kamikadze : BaseEnemyShip
{
    private StageShip _currentStage;
    private IEnumerator LookAtPlayer()
    {
        if (_player == null) yield break; // возварщает break если player неактивен
        while (_currentStage == StageShip.Wait)
        {
            Look(_player.transform.position - transform.position, true, true); // вызываем функцию Look дл€ слежки за нашим кораблем
            yield return null;
        }
    }
    protected override void UpdateStage(StageShip stage) // ключевое слово override означает что мы переписываем функцию котора€ объ€влена в BaseEnemyShip
    {
        _currentStage = stage;
        
        switch (_currentStage)
        {
            case StageShip.In:
                break;
            case StageShip.Wait:
                StartCoroutine(LookAtPlayer());
                break;
            case StageShip.Out:
                _playerLastPos = _player.transform.position; // присваеваем позицию корабл€
                break;
        }
    }
}
