using UnityEngine;

public class MoveBackground : MonoBehaviour
{
    [SerializeField] private MeshRenderer _bgRenderer; //В нее будет помещаться рендер который отображает наши текстуры 
    [SerializeField] private float _speed = 0.01f; // Скорость движения фона

    private Vector2 _startOffset; //Переменная для смещение текстуры
    private int _mainTextureId = Shader.PropertyToID("_MainTex"); //Шейдер отображает наши текстуры через материал используя MeshRender
    private float _tempYOffset; // Стартовые значение оффсета


    void Start()
    {
        _startOffset = _bgRenderer.sharedMaterial.GetTextureOffset(_mainTextureId);
    }

    
    void Update()
    {
        _tempYOffset = Mathf.Repeat(_tempYOffset + Time.deltaTime * _speed, 1); // Функция ограничивает между 0 и 1, если значение больше 1 она снова возвращаеться в 0 и так по кругу
        Vector2 offset = new Vector2(_startOffset.x, _tempYOffset); //_tempYOffset новая позиция
        _bgRenderer.sharedMaterial.SetTextureOffset(_mainTextureId, offset); //передаем Id и значение
    }
}
