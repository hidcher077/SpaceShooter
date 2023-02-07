using System.Collections; // добавим using для корутины
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // добавим using для Image

public enum Scenes // перечеслитель (тип перечислений), перечислем в ней сцены
{
    MainMenu,
    Game
}

public class LevelManager : MonoBehaviour
{
    private static float FadeSpeed = 0.02f; // скорость появление экрана
    private static Color FadeTransparancy = new Color(0, 0, 0, 0.04f); // затемняющий экран
    private static AsyncOperation _async; // асинхронная переменная

    //создаем  instance этого класса как статичную переменную
    public static LevelManager Instance;

    public GameObject _faderObj;
    public Image _faderImg;

    
    
    // Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        Instance = this;
        SceneManager.sceneLoaded += OnLevelFinishedLoading; // Подпись на корутину
        PlayScene(Scenes.MainMenu);
    }

    public static void PlayScene(Scenes sceneEnum)   //статическая функция невозрващающая значение
    {
        Instance.LoadScene(sceneEnum.ToString());
        //SceneManager.LoadScene(sceneEnum.ToString()); // Загрузку сцены можно осуществлять нетолько по индексу, но и по имени сцены
    }    

    // Нужно написать функцию которая асинхронна красиво будет загружать сцены, при эттом анимировать Fade, то есть затемнять картинку при переходе
    // и когда сцена загрузиться делать картинку прозрачной, для этого напишем функции

    private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        Instance.StartCoroutine(FadeIn(Instance._faderObj, Instance._faderImg));
    }
    
    private void LoadScene(string sceneName)
    {
        Instance.StartCoroutine(Load(sceneName));
        Instance.StartCoroutine(FadeOut(Instance._faderObj, Instance._faderImg));
    }

    private static IEnumerator FadeOut(GameObject faderObject, Image fader)
    {
        faderObject.SetActive(true); // включили занавес
        while (fader.color.a < 1) // цикл работает пока занавес станет прозрачным
        {
            fader.color += FadeTransparancy; // делаем занавес прозрачным
            yield return new WaitForSeconds(FadeSpeed); // пропускаем непросто кадр, а ожидаем некторое количество в секундах
        }
        ActivateScene();
    }

    private static IEnumerator FadeIn(GameObject faderObject, Image fader)
    {
        faderObject.SetActive(true); 
        while (fader.color.a > 0) // цикл работает пока занавес станет непрозрачным
        {
            fader.color -= FadeTransparancy; // делаем занавес непрозрачным
            yield return new WaitForSeconds(FadeSpeed); // пропускаем непросто кадр, а ожидаем некторое количество в секундах
        }
        faderObject.SetActive(false); // отключаем занавес
    }


    private static IEnumerator Load(string sceneName) 
    {
        // будет загружать сцену, пока активна текущаяя сцена, это неповлияет на торможение сцены и небудет резкого прихода
        _async = SceneManager.LoadSceneAsync(sceneName);
        _async.allowSceneActivation = false;
        yield return _async;
    }

    private static void ActivateScene() // включение сцены
    {
        _async.allowSceneActivation = true;
    }

}
