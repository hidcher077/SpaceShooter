using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx; // добавим using для скрипта GameUIController
using System; // добавим using System

public class Controller : MonoBehaviour
{
    
    [SerializeField] private AudioSource _musicSource;
    [SerializeField] private AudioSource _effectSource;

    public ReactiveProperty<int> Score = new ReactiveProperty<int>(); // создадим переменную score для скрипта GameUIController

    public AudioSource MusicSource => _musicSource;
    public AudioSource EffectSource => _effectSource;

    public static Controller Instance;
    public PlayerShip _myShip; // Чтобы в дальнейшем наши скрипты имели ссылку в скрипт PlayerShip, добавим PlayerShip в Controller

    // Создадим переменные по значением точек камеры, по которым будем ограничивать наш корабль
    private Vector3 _leftDownPoint;
    private Vector3 _rightDownPoint;
    private Vector3 _leftUpPoint;
    private Vector3 _rightUpPoint;
    private Vector2 _centerCam;

    public Vector3 LeftDownPoint => _leftDownPoint;
    public Vector3 RightDownPoint => _rightDownPoint;
    public Vector3 LeftUpPoint => _leftUpPoint;
    public Vector3 RightUpPoint => _rightUpPoint;

    public Vector2 CenterCam => _centerCam;



    private Subject<Unit> _gameOver = new Subject<Unit>();
    public IObservable<Unit> OnGameOver => _gameOver; // ссылка на окно game over

   
    
    private void Awake()
    {
        Instance = this;
    }

    public void UpdateCameraSettings()
    {
        var cameraMain = Camera.main; // присвоим камеру
        if (cameraMain != null) // проверим пришла ли полноценная камера, если да, то мы можем ей оперировать
        {
            _centerCam = cameraMain.transform.position; // позиция камеры

            float distance = cameraMain.farClipPlane; // узнаем дистанцию от камеры до дальней точки
            _leftDownPoint = cameraMain.ScreenToWorldPoint(new Vector3(0,0,distance));
            _leftUpPoint = cameraMain.ScreenToWorldPoint(new Vector3(0, cameraMain.pixelHeight, distance)); // количество пикселей в высоту
            _rightUpPoint = cameraMain.ScreenToWorldPoint(new Vector3(cameraMain.pixelWidth, cameraMain.pixelHeight, distance)); 
            _rightDownPoint = cameraMain.ScreenToWorldPoint(new Vector3(cameraMain.pixelWidth, 0, distance)); // количество пикселей в ширину
        }
    }

    public void GameOver()
    {
        _gameOver.OnNext(Unit.Default); // создадим событие оконачние игры, на это событие подпишеться GAmeUIController
    }
}
