using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Random = UnityEngine.Random;
public enum SpawnedObjectType
{ 
    SnakeBody,
    SnakeHead,
    Edibles,
}

public class GridObject
{
    public SpawnedObjectType ObjectType;
    public GameObject Object;
    public GridObject(SpawnedObjectType type,GameObject gameObject)
    {
        ObjectType = type;
        Object = gameObject;
    }
}

public class GameController : MonoBehaviour
{
    [SerializeField] private GameParams _gameParams;
    [SerializeField] private Edibles _ediblesData;

    [Header("Visuals")]
    [SerializeField] private Transform _gridDisplay;
    [SerializeField] private GameObject _snakeHeadPrefab;
    [SerializeField] private GameObject _snakeBodyPrefab;


    private float _timeForNextUpdate = 0.1f;
    private float _timeForNextEdible = 1;
    private float _timeForSpeedUp = 0;
    private float _timeForSpeedDown = 0;

    private Vector3 _headPosition;
    private Vector3 _taileLastPosition;

    private Vector2 _currentMovementDirection = new Vector2(0, 1);
    private Vector2 _movementDirection = new Vector2(0, 1);
    private float _snakeSpeed;

    private Vector3 _right = new Vector3(1, 0, 0);
    private Vector3 _up = new Vector3(0, 1, 0);

    private List<GameObject> _snakeParts = new List<GameObject>();
    private Dictionary<Vector2, EdibleEnum> _spawnedEdiblesType = new Dictionary<Vector2, EdibleEnum>();
    private Dictionary<Vector2, GridObject> _spawnedObject = new Dictionary<Vector2, GridObject>();

    private int _boundriesX;
    private int _boundriesY;

    private int _score = 0;
    private bool _isFinished=true;
    
    public Action<int> OnScoreChanged;
    public Action OnGameOver;
    public Action OnGameStart;

    private void StartGame()
    {
        CleanUp();
        _isFinished = false;
        _score = 0;
        OnScoreChanged?.Invoke(_score);

        _boundriesX = Mathf.CeilToInt(_gameParams.GridSize.x / 2);
        _boundriesY = Mathf.CeilToInt(_gameParams.GridSize.y / 2);
        _gridDisplay.localScale = new Vector3(_boundriesX * 2 + 1, _boundriesY * 2 + 1, 1);
        _movementDirection = new Vector2(0, 1);
        var cameraSize = (_boundriesX > _boundriesY ? _boundriesX : _boundriesY) * _gameParams.CameraToGridSizeRatio * 2;
        Camera.main.orthographicSize = cameraSize;

        GameObject head = Instantiate(_snakeHeadPrefab, Vector3.zero, Quaternion.identity);
        _snakeParts.Add(head);
        _spawnedObject.Add(Vector2.zero,new GridObject(SpawnedObjectType.SnakeHead,head));

        _taileLastPosition = new Vector3(0, -1, 0);

        _headPosition = head.transform.position;
        _snakeSpeed = _gameParams.SnakeSpeed;
        _timeForNextUpdate = _snakeSpeed;
    }

    private void CleanUp()
    {
        foreach (var item in _spawnedObject)
        {
            Destroy(item.Value.Object);
        }
        if (_snakeParts.Count > 0)
        {
            if (_snakeParts[0] != null)
                Destroy(_snakeParts[0]);
        }
        _spawnedObject= new Dictionary<Vector2, GridObject>();
        _spawnedEdiblesType = new Dictionary<Vector2, EdibleEnum>();
        _snakeParts = new List<GameObject>();
    }

    private void FixedUpdate()
    {
        if (_isFinished) return;
        if (_timeForNextEdible > 0)
        {
                _timeForNextEdible -= Time.deltaTime;
        }
        else
        {
            GenerateEdible();
            _timeForNextEdible = Random.Range(_gameParams.TimeForNextEdibleMin, _gameParams.TimeForNextEdibleMax);
        }

        OnGameStart?.Invoke();

        if (_timeForNextUpdate > 0)
        {
            if (_timeForSpeedDown > 0)
            {
                _timeForSpeedDown -= Time.deltaTime;
                _timeForNextUpdate -= Time.deltaTime * _gameParams.SnakeSpeedDownMultiplayer;
            }
            else if (_timeForSpeedUp > 0)
            {
                _timeForSpeedUp -= Time.deltaTime;
                _timeForNextUpdate -= Time.deltaTime * _gameParams.SnakeSpeedUpMultiplayer;
            }
            else
            {
                _timeForNextUpdate -= Time.deltaTime;
            }
            return;
        }

        MoveSnake();
        CheckCollision();
        _timeForNextUpdate = _snakeSpeed;
    }

    private void MoveSnake()
    {
        _taileLastPosition = _snakeParts[_snakeParts.Count - 1].transform.position;

        _spawnedObject.Remove(new Vector2(_taileLastPosition.x, _taileLastPosition.y));

        //Moving body
        for (int i = _snakeParts.Count - 1; i > 0; i--)
        {
            int index = i;
            _snakeParts[index].transform.position = _snakeParts[index - 1].transform.position;
            var key = new Vector2(_snakeParts[index].transform.position.x, _snakeParts[index].transform.position.y);
            if (!_spawnedObject.ContainsKey(key))
            {
                _spawnedObject.Add(key, new GridObject(SpawnedObjectType.SnakeBody, _snakeParts[index]));
            }
            else
            {
                _spawnedObject[key].Object = _snakeParts[index];
                _spawnedObject[key].ObjectType = SpawnedObjectType.SnakeBody;
            }
        }
        _currentMovementDirection = _movementDirection;
        //Moving Head
        if (_currentMovementDirection.x > 0)
        {
            _headPosition += _right;
            if (_headPosition.x > _boundriesX)
            {
                _headPosition.x = -_boundriesX;
            }
        }
        else if (_currentMovementDirection.x < 0)
        {
            _headPosition -= _right;
            if (_headPosition.x < -_boundriesX)
            {
                _headPosition.x = _boundriesX;
            }
        }
        else if (_currentMovementDirection.y > 0)
        {
            _headPosition += _up;
            if (_headPosition.y > _boundriesY)
            {
                _headPosition.y = -_boundriesY;
            }
        }
        else if (_currentMovementDirection.y < 0)
        {
            _headPosition -= _up;
            if (_headPosition.y < -_boundriesY)
            {
                _headPosition.y = _boundriesY;
            }
        }

        _snakeParts[0].transform.position = _headPosition;
    }

    private void CheckCollision()
    {
        var headKey = new Vector2(_snakeParts[0].transform.position.x, _snakeParts[0].transform.position.y);
        if (_spawnedObject.ContainsKey(headKey))
        {
            if (_spawnedObject[headKey].ObjectType == SpawnedObjectType.SnakeBody)
            {
                GameOver();
                return;
            }
            else
            {
                IncreseScore();
                var typeToCheck = _spawnedEdiblesType[headKey];
                switch (typeToCheck)
                {
                    case EdibleEnum.SlowDown:
                        {
                            SlowDown();
                            break;
                        }
                    case EdibleEnum.IncreseSize:
                        {
                            IncreseSize();
                            break;
                        }
                    case EdibleEnum.DecreseSize:
                        {
                            if (_snakeParts.Count > 1)
                            {
                                DecreseSize();
                            }
                            else
                            {
                                GameOver();
                                return;
                            }
                            break;
                        }
                    case EdibleEnum.SpeedUP:
                        {
                            SpeedUp();
                            break;
                        }
                    case EdibleEnum.TurnAround:
                        {
                            TurnAround();
                            break;
                        }
                }
                Destroy(_spawnedObject[headKey].Object);
                _spawnedEdiblesType.Remove(headKey);
            }
            _spawnedObject[headKey].Object = _snakeParts[0].gameObject;
            _spawnedObject[headKey].ObjectType = SpawnedObjectType.SnakeHead;
        }
        else
        {
          
            _spawnedObject.Add(headKey, new GridObject(SpawnedObjectType.SnakeHead, _snakeParts[0].gameObject));
        }

    }

    private void GameOver()
    {
        _isFinished = true;
        OnGameOver?.Invoke();
        _timeForSpeedDown = 0;
        _timeForSpeedUp = 0;
    }

    private void IncreseScore()
    {
        _score++;
        OnScoreChanged?.Invoke(_score);
    }

    private void GenerateEdible()
    {
        var edible = _ediblesData.GetRandomEdible();

        Vector2 newPosition = GetNewPositionForEdibles();
        if (newPosition.x == int.MaxValue) return;

        var newObject = Instantiate(edible.EdiblePrefab, newPosition, Quaternion.identity);
        _spawnedObject.Add(newPosition, new GridObject(SpawnedObjectType.Edibles, newObject));
        _spawnedEdiblesType.Add(newPosition, edible.EdibleType);
    }

    private Vector2 GetNewPositionForEdibles(int counter = 0)
    {
        Vector2 newPosition = Vector2.zero;
        newPosition.x = Random.Range(-_boundriesX, _boundriesX);
        newPosition.y = Random.Range(-_boundriesY, _boundriesY);

        if (counter > 1000) return new Vector2(-int.MaxValue,-int.MaxValue);

        if (_spawnedObject.ContainsKey(newPosition)) return GetNewPositionForEdibles(counter++);

        return newPosition;
    }

    private void TurnAround()
    {
        var newLastTailPosition = _snakeParts[0].transform.position;

        _snakeParts[0].transform.position = _taileLastPosition;
        _headPosition = _taileLastPosition;

        var lastBodyPartIndex = _snakeParts.Count - 1;

        if (_taileLastPosition.x > _snakeParts[lastBodyPartIndex].transform.position.x)
        {
            _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, -90);
            _currentMovementDirection.x = 1;
            _currentMovementDirection.y = 0;
            if (_taileLastPosition.x + _currentMovementDirection.x > _boundriesX)
            {
                _currentMovementDirection.x = -1;
                _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
        else if (_taileLastPosition.x < _snakeParts[lastBodyPartIndex].transform.position.x)
        {
            _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 90);
            _currentMovementDirection.x = -1;
            _currentMovementDirection.y = 0;
            if (_taileLastPosition.x + _currentMovementDirection.x < -_boundriesY)
            {
                _currentMovementDirection.x = 1;
                _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, -90);
            }
        }
        else if (_taileLastPosition.y > _snakeParts[lastBodyPartIndex].transform.position.y)
        {
            _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 0);
            _currentMovementDirection.x = 0;
            _currentMovementDirection.y = 1;
            if (_taileLastPosition.y + _currentMovementDirection.y > _boundriesY)
            {
                _currentMovementDirection.y = -1;
                _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
        else if (_taileLastPosition.y < _snakeParts[lastBodyPartIndex].transform.position.y)
        {
            _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 180);
            _currentMovementDirection.x = 0;
            _currentMovementDirection.y = -1;
            if (_taileLastPosition.y + _currentMovementDirection.y < -_boundriesY)
            {
                _currentMovementDirection.y = 1;
                _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 0);
            }
        }

        _movementDirection = _currentMovementDirection;

        List<GameObject> newBodyPartsOrder = new List<GameObject>();

        for (int i = _snakeParts.Count - 1; i > 0; i--)
        {
            newBodyPartsOrder.Add(_snakeParts[i]);
        }

        for (int i = 1; i < _snakeParts.Count; i++)
        {
            _snakeParts[i] = newBodyPartsOrder[i - 1];
        }

        _taileLastPosition = newLastTailPosition;
    }

    private void IncreseSize()
    {
        GameObject bodyPart = Instantiate(_snakeBodyPrefab, _taileLastPosition, Quaternion.identity);
        _snakeParts.Add(bodyPart);
        _spawnedObject.Add(_taileLastPosition, new GridObject(SpawnedObjectType.Edibles, bodyPart));
    }


    private void DecreseSize()
    {
        _taileLastPosition = _snakeParts[_snakeParts.Count - 1].transform.position;
        var objectKey = new Vector2(_taileLastPosition.x, _taileLastPosition.y);
        _spawnedObject.Remove(objectKey);
        Destroy(_snakeParts[_snakeParts.Count - 1]);
        _snakeParts.RemoveAt(_snakeParts.Count - 1);
    }

    private void SpeedUp()
    {
        if (_timeForSpeedDown > 0)
        {
            _timeForSpeedDown = 0;
        }
        else
        {
            _timeForSpeedUp += _gameParams.DurationForSpeedUp;
        }
    }

    private void SlowDown()
    {
        if (_timeForSpeedUp > 0)
        {
            _timeForSpeedUp = 0;
        }
        else
        {
            _timeForSpeedDown += _gameParams.DurationForSpeedDown;
        }
    }

    //Invoke from InputSystem
    public void OnMove(InputValue value)
    {
        var newDirection = value.Get<Vector2>();
        if (newDirection.x > 0)
        {
            if (_currentMovementDirection.x >= 0)
            {
                _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, -90);
                _movementDirection.x = 1;
                _movementDirection.y = 0;
            }
        }
        else if (newDirection.x < 0)
        {
            if (_currentMovementDirection.x <= 0)
            {
                _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 90);
                _movementDirection.x = -1;
                _movementDirection.y = 0;
            }
        }
        else if (newDirection.y > 0)
        {
            if (_currentMovementDirection.y >= 0)
            {
                _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 0);
                _movementDirection.x = 0;
                _movementDirection.y = 1;
            }
        }
        else if (newDirection.y < 0)
        {
            if (_currentMovementDirection.y <= 0)
            {
                _snakeParts[0].transform.rotation = Quaternion.Euler(0, 0, 180);
                _movementDirection.x = 0;
                _movementDirection.y = -1;
            }
        }
    }

    public void OnStart(InputValue value)
    {
        if (!_isFinished) return;
        StartGame();
    }
}
