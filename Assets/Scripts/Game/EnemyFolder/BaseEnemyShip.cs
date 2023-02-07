using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;

// вражеские корабли будут иметь три стадии, In когда летит сверху к точке остановки, wait когда стоит в точке остановки и Out когда летит вниз
public enum StageShip // создадим перечислитель
{
    In,
    Wait,
    Out
}

public abstract class BaseEnemyShip : MonoBehaviour // класс то же делаем абстрактным
{
    [Header("Base Fields")] //этот тег отображаеться в редакторе в виде вол-текста
    [SerializeField] private float _normalSpeed = 8; // скорость вражеского коробля
    [SerializeField] private float _delayTurbo = 2; // время в секундах после которого вражеский корабль летит либо в наш корабль либо летит вниз 
    [SerializeField] private float _turboSpeed = 5; // скорость коробля когда он будет покидать видимую зону камеры
    [SerializeField] private float _speedRotation = 0.01f; // скорость поворота вражеского коробля
    [SerializeField] private int _collisionDamage = 10; // урон который наносит корабль при столкновении с кораблем игрока
    [SerializeField] private int _maxHealth = 2; // переменная максимально жизни вражеского корабля
    [SerializeField] private int _costPointesScore = 5; // очки при уничтожении вражеского корабля

    public int CostPointersScore => _costPointesScore; // ссылка на очки за подбитый корабль


    [HideInInspector] public PlayerShip _player;
    [HideInInspector] public Transform _myRoot;
    [HideInInspector] public Vector3 _playerLastPos = Vector3.up; // последняя позиция корабля игрока

    private Subject<MonoBehaviour> _putMe = new Subject<MonoBehaviour>();
    public IObservable<MonoBehaviour> PutMe => _putMe;

    private Vector3 DirectionToPlayer => transform.position - new Vector3(_playerLastPos.x, _playerLastPos.y,0); // рассчитывает позицию до нашего корабля
    private int _health = 100; // текущее значение жизней корабля
    private float _goTo; // окончательная точка движение нашего корабля
    private float _goToPointTurbo; // точка на которой вражеский корабль должен остановиться и следить за кораблем игрока
    private float _timerDelay; // будет записывать текущее показание таймера


    private IEnumerator Core() // создаем корутину
    {
        UpdateStage(StageShip.In);   // задаеться точка к которой движеться вражеский корабль и останавливается там
        while (transform.position.y > _goToPointTurbo)  // пока позиция игрока по y больше чем _goToPointTurbo, мы будем двигать корабль
        {
            transform.position -= new Vector3(0, Time.deltaTime * _normalSpeed, 0);
            Look(new Vector3(0, _goToPointTurbo, 0)); // повернем в направление движение вражеский корабль 
            yield return null;
        }

        UpdateStage(StageShip.Wait); // после поворота на положении корабля игрока, вражеский корабль начинает ждать
        while (_timerDelay < _delayTurbo) // пока timerDelay меньше delayTurbo корабль будет ждать, когда он станет больше корабль начнет движение
        {
            _timerDelay += Time.deltaTime;
            yield return null;

        }

        UpdateStage(StageShip.Out); // корабль начинает лететь в сторону корабля после расчета или по прямой если нерасчитал позицию нашего корабля
        if (_playerLastPos != Vector3.up) // если вектор _playerLastPos неравняеться Vector3.up то он был изменен, а значит это корабль типа kamikadze
        {
            var dir = DirectionToPlayer / DirectionToPlayer.magnitude;
            while (transform.position.y > _goTo && transform.position.y < -_goTo)
            {
                Look(dir); // повернем в направление движение 
                transform.position -= dir * (Time.deltaTime * _turboSpeed); // двигаем наш корабль в направление предыдущей позиции корабля игрока
                yield return null;

            }

        }
        else // другой тип корабля летит вниз по экрану
        {
            while (transform.position.y > _goTo)
            {
                transform.position -= new Vector3(0, Time.deltaTime * _turboSpeed, 0);
                yield return null;
            }
        }

        _putMe.OnNext(this); // вызываем событие putMe которая переместит корабль обратно в пул, после окончание движения
    }

    private void OnEnable()
    {
        _timerDelay = 0; // назначаем время начала ожидание вражеского корабля
        var controller = Controller.Instance;
        _goTo = controller.RightDownPoint.y - 2; // точка ниже экрана куда должен улететь наш корабль, конечная позиция
        _goToPointTurbo = UnityEngine.Random.Range((controller.CenterCam.y + 1), (controller.LeftUpPoint.y - 1)); // создаеться случайная точка, где корабль отсанавливаеться и поварчиваться в направлении корабля игрока
        _health = _maxHealth;
        StartCoroutine(Core());
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }



    protected void Look(Vector3 dir, bool lerp = false, bool invertion = false)
    {
        float signedAngle = Vector2.SignedAngle(Vector2.down, dir); // в перпеменную signedAngle записываем угол напрваления
        if (invertion == true) signedAngle += 180; //если invertion равно true то мы переворачиваем корабль на 180 градусов
        if (MathF.Abs(signedAngle) >= 1e-3f) // проверяем что модуль abs больше или равен 1e-3f
        {
            var agles = transform.eulerAngles; // назначаем диапозон поворота
            agles.z = signedAngle; // вращенние вражеского корабля идет по z
            if (lerp)
            {
                transform.eulerAngles = Vector3.Lerp(transform.eulerAngles, agles, _speedRotation); // корабль будет поворачиваться на скорость указанную speedrotation
            }
            else
            {
                transform.eulerAngles = agles; //если lerp нестоит то мы будем присваивать текущему agles нашы расчеты
            }

        }
    }

    private void OnTriggerEnter2D(Collider2D collision) // Создаем колизии косание пули и вражеского коробля
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
            obj.GetComponent<PlayerShip>().DamageMe(_collisionDamage); // урон который наносит вражеский корабль, кораблю игрока
            Controller.Instance.Score.Value += (_costPointesScore/2); // если мы задели корабль, то получаем половину очков
            _putMe.OnNext(this);
        }
    }

    private void DamageMe(int damage, BaseEnemyShip baseEnemy) // Когда жизни уменьшелись меньше или равно нулю, то корабль исчезает
    {
        _health -= damage;
        if(_health <= 0)
        {
            _health = _maxHealth;
            Controller.Instance.Score.Value += _costPointesScore; // если задели пулей вражеский корабль то получаем полные очки
            _putMe.OnNext(this);
        }
    }


    protected abstract void UpdateStage(StageShip stage); // тип видимости protected могут видеть классы которые наследуются от BaseShip, а также сам класс BaseShip

}

