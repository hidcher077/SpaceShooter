using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using System;
using UnityEngine.SceneManagement; //Добавим для загрузочной сцены

public class PlayerShip : MonoBehaviour
{
    [SerializeField] private float _speed = 15;
    [SerializeField] private float _coolDown = 0.1f;
    public int _maxHealth = 100;
    [SerializeField] private float _shipRollEuler = 45; // назначаем вращение коробля при движении 45 гардусов
    [SerializeField] private float _shipRollSpeed = 80; // скорость поворота
    [SerializeField] private float _smoothness = 1.2f; // плавное движение коробля

    private Subject<Unit> _fireclick = new Subject<Unit>(); // рассылает подписчикам уведомление, создадим события рассылать она будет пустые данные, создадим экземпляр объекта
    public IObservable<Unit> FireClick => _fireclick; // будем ссылаться на собятия _fireclick,

    private Rigidbody2D _rigidbody; // запишем компонент rigidbody2d который был создан в корабле
    private float _coolDownCurrent = 10; // будет записываться время между выстрелами
    private MeshRenderer _mR; // meshrender отвечает за отображение трехмерных объектов в сцене нашей игры. В данном случае коробля
    private Vector3 _sizeWorldship; // в эту переменную запишем размер нашего коробля по трем осям
    private Controller _controller; // создадим эту переменную для удобства чтобы обращаться в него в не controller instance

    [HideInInspector] public ReactiveProperty<int> _health = new ReactiveProperty<int>();
    // Создадим переменную и экземпляр объекта для реактивного программирования, 
    //для взаимодействия прогресс бара с жизнью коробля и нанесение урона кораблю. Напишем hideininspector, чтобы спрятать его в редакторе

    // Так как скрипт будет лежать в том же объекте что и rigidbody2d, напишем GetComponent<RigitBody2D>();, то же самое и для MeshRender
    private void Awake()
    {
        if(Controller.Instance == null) // Если контроллер будет null, то это означает что загрузачная сцена еще незагруженна
        {
            SceneManager.LoadScene(0); //Загрузачная сцена является нулевой
            return;
        }

        _rigidbody = GetComponent<Rigidbody2D>();
        _mR = GetComponent<MeshRenderer>();
        _controller = Controller.Instance; // присваеваем переменной метод класса Controller другого скрипта
        _controller._myShip = this; // Укажим ссылку со скрипта Controller
        _sizeWorldship = _mR.bounds.extents; // получаем в переменную размер коробля
    }

    private void Start()
    {
        _controller.UpdateCameraSettings(); // Вызываем Controller и его функцию
        _health.Value = _maxHealth; // присваем кол-во жизней корабля для health бара
    }

    private void Update()
    {
        UpdateKey();
        FireButtonClick();
    }

    private void FireButtonClick()
    {
        if(Input.GetMouseButton(0)) // возьмом нажатие левой кнопки мыши (0 - левая, 1 - правая)
        {
            if(_coolDownCurrent >= _coolDown) 
            {
                _coolDownCurrent = 0;
                _fireclick.OnNext(Unit.Default); // делаем рассылку пустых данных
            }
        }
        if (_coolDownCurrent < _coolDown)
        {
            _coolDownCurrent += Time.deltaTime; // добавляем значение
        }
    }

    private void UpdateKey()
    {
        float moveHor = Input.GetAxis("Horizontal");
        float moveVert = Input.GetAxis("Vertical");

        // в функции .Lerp() первым параметром передаем текущую velocity, второй параметр какой она должна стать, третий параметр как быстро она становиться такой
        _rigidbody.velocity = Vector2.Lerp(_rigidbody.velocity, new Vector2(moveHor * _speed * 1.2f, moveVert * _speed), _smoothness);


        transform.position = CheckBoardWorld();

        // Меняем вращение корабля, вращение в 3-х мерном пространстве описываеться с помощью Quaternion, для этого используем класс Quaternion и функцию Eulel
        var targetRotation = Quaternion.Euler(0, 180 + (-moveHor * _shipRollEuler), 0); // задаем углы поворота по осях X, Y, Z, вданном случае какое мы значение указываем _shipRollEuller, такой и будет угол наклона корабля
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _shipRollSpeed * Time.deltaTime); //Slerp отвечает за плавное вращение. Задаем текущее вращение,вращение куда он должен стремиться и скорость
        
    }

    // Функция для ограничение нашего коробля в зоне перемещение камеры
    private Vector3 CheckBoardWorld()
    {
        var pos = transform.position; // текущее местоположение объекта
        var x = pos.x;
        var y = pos.y;

        x = Mathf.Clamp(x,_controller.LeftDownPoint.x + _sizeWorldship.x, _controller.RightDownPoint.x - _sizeWorldship.x); // Ограничивает переменную от максимального до минимального значение
        y = Mathf.Clamp(y, _controller.LeftDownPoint.y + _sizeWorldship.y, _controller.LeftUpPoint.y - _sizeWorldship.y);

        return new Vector3(x,y,0); // Так как корабль летает по осям X и Y, Z будет равен 0
    }
    public void DamageMe(int damage) // Если у нас уменьшаться жизни до нуля мы проигрываем игру
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
        if(obj.CompareTag("EnemyBullet")) //проверяет по навзанию тега пули
        {
            /*var bull = obj.GetComponent<Bullet>();
            DamageMe(bull._damage);
            bull.HitMe(); // удаляем пулю*/
            var bull = obj.GetComponent<Bullet>();
            bull.HitMe();
            DamageMe(bull._damage);
            return;
        }
    }
}

