using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UniRx;

public class GameUIController : MonoBehaviour
{
    [SerializeField] Text _countHealth;
    [SerializeField] Text _countScore;
    [SerializeField] Slider _barHealth;
    [SerializeField] Text _countScoreWindowGameOver;
    [SerializeField] GameObject _windowGameOver;

    private CompositeDisposable _disposables;

    private void Start()
    {
        _disposables = new CompositeDisposable();
        var controller = Controller.Instance;

        controller.OnGameOver.Subscribe((_) => ShowWindowGameOver()).AddTo(_disposables);
        controller._myShip._health.Subscribe(UpdateBar).AddTo(_disposables);
        controller.Score.Subscribe(UpdateScore).AddTo(_disposables);

    }

    private void UpdateBar(int value)
    {
        _barHealth.value = ((float)value)/100; // изменение слайдера жизней, конвертируем value во float
        _countHealth.text = value.ToString(); // изменение текста жизней
    }

    private void UpdateScore(int score)
    {
        if (!_windowGameOver.activeSelf) //если открыта окно Game Over, то Score необновл€етьс€
        {
            _countScore.text = score.ToString(); // добавление очков за вражеские корабли
        }
        
    }

    public void ShowWindowGameOver() // результаты очков в окне gameover
    {
        _countScoreWindowGameOver.text = Controller.Instance.Score.Value.ToString();
        _windowGameOver.SetActive(true);
    }

    public void ClickToMainMenu() // ѕри нажатии меню будет произведен переход на сцену меню
    {
        LevelManager.PlayScene(Scenes.MainMenu); 
        gameObject.SetActive(false);
    }

    public void ClickRestart() // ѕри нажатии меню будет произведен рестарт игры
    {
        LevelManager.PlayScene(Scenes.Game);
        gameObject.SetActive(false);
    }


    private void OnDestroy()
    {
        if(_disposables != null)
        {
            _disposables.Dispose(); // отписываемс€ от подписок когда сцена игры закрываетьс€, после если игрок откроет игровую сцену мы снова подписываемс€
            
        }
        _disposables = null; // обнуление переменной делаетьс€ дл€ того чтобы в дальнейшем ненакапливалась пам€ть
    }
}
