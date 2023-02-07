using UnityEngine;
using UniRx;
using System.Collections.Generic;

public class SpawnManager : MonoBehaviour
{
    private PlayerShip _playerShip; // объявим перпемнную player ship которая будет сохраянть наш корабль чтобы необращаться в его контроллер

    [SerializeField] private GameObject _bulletPref; // В нее мы будем помещать ссылку на префаб нашей пули
    [SerializeField] private Transform _poolBulletMy; // Создаим трансформер нашего pool, в который будет помещаться наши пули
    [SerializeField] private List<GameObject> _enemyPrefabs = new List<GameObject>(); // создаем список префабов
    [SerializeField] private Transform _poolEnemyRoot; // Созададим пул куда будут попадать префабы

    [SerializeField] private Transform _poolEnemyBullet; // Создадим пул для пуль вражеского корабля

    private List<Transform> _rootEnemyType = new List<Transform>(); // список трансформеров типов
    private CompositeDisposable _disposables = new CompositeDisposable(); // для отмены и збрасывание подписок, создадим экземпляр объекта
    
    private void Start()
    {
        Controller.Instance.Score.Value = 0; // вначале игры обнуляем очки
        _playerShip = Controller.Instance._myShip;
        _playerShip.FireClick.Subscribe((_)=> SpawnBullet()); // подпишимся на события fireClick в скрипте Playership


        foreach (var enemy in _enemyPrefabs)
        {
            GameObject root = new GameObject("root" + enemy.name);
            root.transform.parent = _poolEnemyRoot; // К трансформу пула приминяем родителя
            _rootEnemyType.Add(root.transform); // добавляем пулы в наш список
        }
    }

    // private void SpawnBullet(Transform enemyTransform = null, Bullet enemyBullet = null) - spawnbullet зделаем публичной
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
                bullet = Instantiate(enemyBullet).gameObject; // создадим объект для вражеской пули
                var bulletScript = bullet.GetComponent<Bullet>();
                bulletScript.PutMe.Subscribe(PutObject).AddTo(_disposables);
            }
            bullet.transform.parent = transform;           //меняем _poolBulletMy на transform незнаю для чего (
            var position = enemyTransform.transform.position;
            bullet.transform.position = new Vector3(position.x, position.y - 1.2f, 0); // устанавливаем позицию пули к кораблю hunter и смещаем чуть ниже
        }
        else
        {
            if (_poolBulletMy.childCount > 0) // Если в нашем pool дочерних элементов больше нуля, это означает что у нас больше нуля свободных пуль
            {
                bullet = _poolBulletMy.GetChild(0).gameObject; // назначаем bullet перввый дочерний элемент pool-a
            }
            else // Если в poola нету дочерних элементов  создадим новый экзепляр пули
            {
                bullet = Instantiate(_bulletPref); // Функции создающие объекты
                bullet.GetComponent<Bullet>().PutMe.Subscribe(PutObject).AddTo(_disposables); // при создание пули, будем подписываться на PutMe, получим компонент скрипта Bullet
            }
            bullet.transform.parent = transform; // меняем пули родительский компонтент на текущий
            var pos = _playerShip.transform.position; // получаем позицию корбля
            bullet.transform.position = new Vector3(pos.x, pos.y + 1.2f, 0); // перемещаем пулю в позицию нашего корабля, позицию по игрику смещаем вверх
        }

       
        bullet.gameObject.SetActive(true); // включаем объект
    }


    public Hunter SpawnEnemy() // Создадим публичную функцию которая будет возвращать базовый класс врагов
    {
        var controller = Controller.Instance;
        GameObject ship;
        int type = Random.Range(0, _enemyPrefabs.Count); // Здесь мы будем создавать рандомный префаб из списка, count узнает количетсов элементов
        var pool = _rootEnemyType[type]; // выберим префаб из списка пулов
        

        if (pool.childCount > 0) // елси в нашем пуле число дочерних элементов больше нуля мы берем первый попавшийся корабль
        {
            ship = pool.GetChild(0).gameObject; // берем первый попавшийся корабль
            
        }
        else
        {
            ship = Instantiate(_enemyPrefabs[type]); // если в пуле нету объектов мы создаем новый корабль
            var enemyShip = ship.GetComponent<BaseEnemyShip>(); //
            enemyShip.PutMe.Subscribe(PutObject).AddTo(_disposables); // подпишимся на событие putme и будем отправлять в putobject, подписку будем подписывать AddTo передовая в disposibles
            enemyShip._myRoot = pool; // создадим вражескому кораблю его пул
            enemyShip._player = _playerShip;
            
        }

        ship.transform.parent = _poolEnemyRoot; // пока вражеский корабль будет жить он попадает в пул enemy root
        var height = controller.RightUpPoint.y + 2; // повяление вражеского корабля сверху 

        Vector3 spawnPos = new Vector3(Random.Range(controller.LeftUpPoint.x + 0.5f, controller.RightUpPoint.x - 0.5f), height, 0); // рассчитаем положение нашего коробля

        ship.transform.position = spawnPos; // задаем позиции в игре
        ship.SetActive(true); // отображаем корабль в игре, наш корабль может создаваться и переноситься между пулами в объектах в игре

        return ship.GetComponent<Hunter>(); // отправляем данные в компонент BaseEnemyShip для движение кораблей в скрипте BaseEnemy


    }

    private void PutObject(MonoBehaviour mono)
    {
        var objBull = mono as Bullet; // проверяет соотвествует ли класс mono классу Bullet, если же придет null то мы пропускаем условия
        if(objBull != null)
        {
            if(objBull._isEnemy)
            {
                objBull.transform.parent = _poolEnemyBullet;
            }
            else
            {
                objBull.transform.parent = _poolBulletMy; // помещаем нашу пулю в pool пуль
            }
            
            objBull.gameObject.SetActive(false); // выключаем пулю
            return;
        }
        


        var objShip = mono as BaseEnemyShip; 
        if (objShip != null)
        {
            //Controller.Instance.Score.Value += objShip.CostPointersScore; // подсчет очков за Вражеский корабль
            objShip.transform.parent = objShip._myRoot; // после перемещения вниз возвращаем наши корабли в пул 
            objShip.gameObject.SetActive(false); // выключаем объекты
           
        }
        

    }


    private void OnEnable()
    {
        _disposables = new CompositeDisposable();        
    }

    private void OnDisable()
    {
        _disposables.Dispose(); // Функция Dispose отменяет все подписки находящиеся в disposables
        _disposables = null;
    }
}
