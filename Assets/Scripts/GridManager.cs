using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class GridManager : MonoBehaviour {
    public int width = 10;
    public int height = 10;
    public Transform dotParent;
    
    public GameObject particle;
    public GameObject objectPrefab;
    public Transform shapeSpawnArea;
    public GameObject[] shapePrefabs;
    public Transform feedBack;
    
    private int _combo = 1;
    private bool _isFail = false;
    private DotProperties[,] _allDots;
    private List<int> _resetColumn = new();
    private List<int> _resetRow = new();
    private GameObject _selectedShape;
    private Vector3 _initialPosition;
    private int _rotationValue;
    private int _selectedShapeRow;
    private int _selectedShapeColumn;
    private Camera _mainCamera;
    private List<Transform> _allShape = new();
    private List<bool> _isShapeAtPosition = new();
    private Color _randomColor;
    
    public List<bool> _successPlatform = new();
    private Vector3 _offset;
    private bool _isDragging = false;
    private float SmoothSpeed { get; } = 15f;
    private List<Transform> _mainAnimDot = new();
    
    [Serializable]
    public class DotProperties {
        private GameObject _mainObject;
        private SpriteRenderer[] _edges = new SpriteRenderer[4]; //0 = left, 1 = top, 2 = right, 3 = bottom
        private bool[] _isEdgePainted = new bool[4]; //0 = left, 1 = top, 2 = right, 3 = bottom
        private SpriteRenderer[] _corners = new SpriteRenderer[4]; // 0 = left bottom, 1 = left top, 2 = right top, 3 = right bottom  
        private bool[] _isCornerPainted = new bool[4]; // 0 = left bottom, 1 = left top, 2 = right top, 3 = right bottom  
        private bool _isFullyPainted;
        private static readonly Color _initColor = new Color(63f / 255f, 57f / 255f, 153f / 255f, 1f);

        public DotProperties(GameObject dotObject) {
            _mainObject = dotObject;
            InitializeDotProperties(dotObject);
        }
        
        private void InitializeDotProperties(GameObject dotObject) {
            for (int i = 0; i < 4; i++) {
                _edges[i] = dotObject.transform.GetChild(0).GetChild(i).GetComponent<SpriteRenderer>();
                _corners[i] = dotObject.transform.GetChild(1).GetChild(i).GetComponent<SpriteRenderer>();
                
            }
        }

        public Transform DotTransform() => _mainObject.transform;
        public bool IsFullyPainted() => _isFullyPainted;

        public bool IsObjectAddedToMainList() =>_mainObject.transform.GetChild(2).localScale.x > 0.7f;
        
        
        //Paints the left edge and its adjacent corners.
        public void PaintLeft(Color color) {
            if(_mainObject == null) return;
            _isEdgePainted[0] = true;
            _edges[0].color = color;
            _isCornerPainted[0] = true;
            _isCornerPainted[1] = true;
            _corners[0].color = color;
            _corners[1].color = color;
            
            _isFullyPainted = AreAllEdgesAndCornersPainted();
        }

        public bool IsLeftPainted() => _mainObject == null ? true : _isEdgePainted[0];
        
        
        //Paints the top edge and its adjacent corners.
        public void PaintTop(Color color) {
            if(_mainObject == null) return;
            _isEdgePainted[1] = true;
            _edges[1].color = color;
            _isCornerPainted[1] = true;
            _isCornerPainted[2] = true;
            _corners[1].color = color;
            _corners[2].color = color;
            
            _isFullyPainted = AreAllEdgesAndCornersPainted();
        }
        
        public bool IsTopPainted() => _mainObject == null ? true : _isEdgePainted[1];
        
        //Paints the right edge and its adjacent corners.
        public void PaintRight(Color color) {
            if(_mainObject == null) return;
            _isEdgePainted[2] = true;
            _edges[2].color = color;
            _isCornerPainted[2] = true;
            _isCornerPainted[3] = true;
            _corners[2].color = color;
            _corners[3].color = color;
            
            _isFullyPainted = AreAllEdgesAndCornersPainted();
        }
        
        public bool IsRightPainted() => _mainObject == null ? true : _isEdgePainted[2];
        
        //Paints the bottom edge and its adjacent corners.
        public void PaintBottom(Color color) {
            if(_mainObject == null) return;
            _isEdgePainted[3] = true;
            _edges[3].color = color;
            _isCornerPainted[3] = true;
            _isCornerPainted[0] = true;
            _corners[3].color = color;
            _corners[0].color = color;
            
            _isFullyPainted = AreAllEdgesAndCornersPainted();
        }
        
        public bool IsBottomPainted() => _mainObject == null ? true : _isEdgePainted[3];

        public void PaintCorner(Color color, int index) {
            if (_mainObject == null) return;
            _isCornerPainted[index] = true;
            _corners[index].color = color;
            
            _isFullyPainted = AreAllEdgesAndCornersPainted();
        }

        public bool IsPaintCorner(int index) => _mainObject == null ? true : _isCornerPainted[index];

        
        //Checks if all edges and corners are painted.
        public bool AreAllEdgesAndCornersPainted() {
            //int count = 0;
            for (int i = 0; i < 4; i++) {
                // if (_isCornerPainted[i]) 
                //     count++;
                //
                // if (_isEdgePainted[i]) 
                //     count++;
                if (!_isCornerPainted[i] || !_isEdgePainted[i]) {
                    return false;
                }
            }

            return true;
        }
        public void ResetLeftEdge() {
            _edges[0].color = _initColor;
            _isEdgePainted[0] = false;
        }
        
        public void ResetTopEdge() {
            _edges[1].color = _initColor;
            _isEdgePainted[1] = false;
        }
        
        public void ResetRightEdge() {
            _edges[2].color = _initColor;
            _isEdgePainted[2] = false;
        }

        public void ResetBottomEdge() {
            _edges[3].color = _initColor;
            _isEdgePainted[3] = false;
        }

        public void ResetCorner(int index) {
            _corners[index].color = _initColor;
            _isCornerPainted[index] = false;
        }

        //Resets dot properties when a full column is cleared.
        public void ResetForColumnClear(bool isLeftEmpty, bool isTopLeftCornerEmpty, bool isBottomLeftCornerEmpty, bool isRightEmpty, bool isTopRightCornerEmpty, bool isBottomRightCornerEmpty) {
            Color initMainColor = new Color(1f, 1f, 1f, 0);
            _isFullyPainted = false;
            _mainObject.transform.GetChild(2).GetComponent<SpriteRenderer>().color = initMainColor;
            _mainObject.transform.GetChild(2).localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            //Always reset top and bottom edges.
            _edges[1].color = _initColor;
            _edges[3].color = _initColor;
            _isEdgePainted[1] = false;
            _isEdgePainted[3] = false;
            
            //Reset left edge and its adjacent corners if needed.
            if (isLeftEmpty) {
                _edges[0].color = _initColor;
                _isEdgePainted[0] = false;
                if (isTopLeftCornerEmpty) {
                    _isCornerPainted[1] = false;
                    _corners[1].color = _initColor;
                }
                if (isBottomLeftCornerEmpty) {
                    _isCornerPainted[0] = false;
                    _corners[0].color = _initColor;
                }
                
                
            }

            //Reset right edge and its adjacent corners if needed.
            if (isRightEmpty) {
                _edges[2].color = _initColor;
                _isEdgePainted[2] = false;
                if (isTopRightCornerEmpty) {
                    _isCornerPainted[2] = false;
                    _corners[2].color = _initColor;
                }

                if (isBottomRightCornerEmpty) {
                    _isCornerPainted[3] = false;
                    _corners[3].color = _initColor; 
                }
            }
        }
        
        //Resets square properties when a full row is cleared.
        public void ResetForRowClear(bool isTopEdgeEmpty, bool isTopLeftCornerEmpty, bool isTopRightCornerEmpty, bool isBottomEdgeEmpty, bool isBottomLeftCornerEmpty, bool isBottomRightCornerEmpty) {
            Color initMainColor = new Color(1f, 1f, 1f, 0);
            _isFullyPainted = false;
            _mainObject.transform.GetChild(2).GetComponent<SpriteRenderer>().color = initMainColor;
            _mainObject.transform.GetChild(2).localScale = new Vector3(0.1f, 0.1f, 0.1f);
            
            //Always reset right and left edges.
            _edges[0].color = _initColor;
            _edges[2].color = _initColor;
            _isEdgePainted[0] = false;
            _isEdgePainted[2] = false;
            //Reset top edge and its adjacent corners if needed.
            if (isTopEdgeEmpty) {
                _edges[1].color = _initColor;
                _isEdgePainted[1] = false;
                if (isTopLeftCornerEmpty) {
                    _isCornerPainted[1] = false;
                    _corners[1].color = _initColor; 
                }

                if (isTopRightCornerEmpty) {
                    _isCornerPainted[2] = false;
                    _corners[2].color = _initColor;
                }
            }
            
            //Reset bottom edge and its adjacent corners if needed.
            if (isBottomEdgeEmpty) {
                _edges[3].color = _initColor;
                _isEdgePainted[3] = false;
                if (isBottomLeftCornerEmpty) {
                    _isCornerPainted[0] = false;
                    _corners[0].color = _initColor;
                }
                if (isBottomRightCornerEmpty) {
                    _isCornerPainted[3] = false;
                    _corners[3].color = _initColor;
                }
            }
            
        }
    }

    private void Start() {
        //If the grid dimensions can be set via the UI, they are retrieved from the UIManager.
        if (UIManager.storedValueWidth > 0) width = UIManager.storedValueWidth;
        if (UIManager.storedValueHeigth > 0) height = UIManager.storedValueHeigth;
        _allDots = new DotProperties[width, height];
        _combo = 1;
        DetermineColor();
        AdjustCameraPos();
        PlaceGridObjects(); 
        SpawnInitialShapes();
    }
    
    //Sets a random color from a preset list.
    public void DetermineColor() {
        Color[] allColors = new[] {
            new Color(206f / 255f, 235f / 255f, 251f / 255f, 1),
            new Color(163f / 255f, 214f / 255f, 254f / 255f, 1f),
            new Color(102f / 255f, 167f / 255f, 197f / 255f, 1f),
            new Color(238f / 255f, 50f / 255f, 51f / 255f, 1f),
            new Color(240f / 255f, 236f / 255f, 235f / 255f, 1f),
            new Color(108f / 255f, 116f / 255f, 118f / 255f, 1f),
        };
        _randomColor = allColors[Random.Range(0, allColors.Length)];
    }
    //Adjusts the camera position and grid scale based on grid dimensions.
    private void AdjustCameraPos() {
        _mainCamera = Camera.main;
        Vector3 gridCenter = GetCellWorldPosition((width - 1) / 2f, (height - 1) / 2f);
        
        //Center the dotParent and adjust its scale.
        dotParent.position = gridCenter;
        dotParent.localScale = new Vector3(width + 1, height + 1);
        
        //Position the shape spawn area.
        shapeSpawnArea.position = new Vector3(gridCenter.x, -2.5f);
        
        // Calculate the required camera size.
        float verticalRequired = (height + 1) * 0.5f + height * 0.2f;
        float horizontalRequired = (width + 1) * 0.5f / _mainCamera.aspect;
        float sizeValue = Mathf.Max(verticalRequired + 2, horizontalRequired + 2);
        _mainCamera.orthographicSize = sizeValue;
        _mainCamera.transform.position = new Vector3(gridCenter.x, gridCenter.y -1f, -10f);
    }
    
    //Instantiates the grid squares and sets up their inactive edges/corners.
    private void PlaceGridObjects() {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++) {
                Vector3 cellPosition = GetCellWorldPosition(x, y);
                GameObject dot = Instantiate(objectPrefab, cellPosition, Quaternion.identity);
                dot.transform.SetParent(dotParent);
                if (x == 0) SetDotsEdgesAndCorners(y == 0 ? PossiblePos.FirstDot : PossiblePos.BottomEdge, dot.transform);

                
                else SetDotsEdgesAndCorners(y == 0 ? PossiblePos.LeftEdge : PossiblePos.BottomAndLeftEdge, dot.transform);
                _allDots[x,y] = new DotProperties(dot);
            }
        }
    }
    
    //Spawns three initial shapes in the spawn area with random rotations and random color.
    private void SpawnInitialShapes() {
        for (int i = 0; i < 3; i++) {
            Vector3 spawnPosition = shapeSpawnArea.GetChild(i).position;
            int[] rotation = {
                0,
                90,
                180,
                270
            };

            int randomValue = Random.Range(0, rotation.Length);
            int randomRot = rotation[randomValue];
            
            Transform shape = Instantiate(GetRandomShapePrefab(), spawnPosition+10f*Vector3.right, Quaternion.Euler(0f, 0f, randomRot)).transform;
            //Set the shape's color to the assigned random color.
            for (int j = 0; j < shape.childCount; j++) {
                shape.GetChild(j).GetComponent<SpriteRenderer>().color = _randomColor;
            }
            if (!_allShape.Contains(shape)) {
                _allShape.Add(shape);
                _isShapeAtPosition.Add(false);
                _successPlatform.Add(true);
            }
        }
    }
    private void Update() {
        if(UIManager.I.IsStopMovement()) return;
        HandleShapeDragging();
        MoveShapesToSpawnPosition();
        AnimateMainObjectColor();
        ControlFailState();
        CheckFailCondition();
        if (_isFail) {
            StartCoroutine(UIManager.I.ActiveFailUI());
            _isFail = false;
        }
    }
    
    private Vector3 GetCellWorldPosition(float x, float y) => new Vector3(x, y, 0);
    
    //Smoothly moves shapes to their designated spawn positions.
    private void MoveShapesToSpawnPosition() {
        if(_allShape.Count != 3) return;
        for (int i = 0; i < _allShape.Count; i++) {
            Vector3 spawnPos = shapeSpawnArea.GetChild(i).position;
            if (!_isShapeAtPosition[i]) {
                _allShape[i].position = Vector3.Lerp(_allShape[i].position, spawnPos, 10f * Time.deltaTime);
                if (Vector3.Distance(_allShape[i].position, spawnPos) < 0.1f) {
                    _isShapeAtPosition[i] = true;
                }
            }
        }
    }

    private GameObject GetRandomShapePrefab() => shapePrefabs[Random.Range(0, shapePrefabs.Length)];
    
    //Processes mouse input for dragging shapes.
    private void HandleShapeDragging() {
        
        if (Input.GetMouseButtonDown(0)) {
            OnDragStart();
        }

        if (_isDragging && Input.GetMouseButton(0) && _selectedShape != null) {
            OnDragging();
        }

        if (_selectedShape != null && Input.GetMouseButtonUp(0)) {
            OnDragEnd();
        }
    }

    //Called when the user starts dragging a shape.
    private void OnDragStart() {
        Vector3 worldPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition);
        Collider2D hitCollider = Physics2D.OverlapPoint(worldPosition);

        if (hitCollider != null)
        {
            _selectedShape = hitCollider.gameObject;
            _initialPosition = _selectedShape.transform.position;
            int rotZValue = Mathf.RoundToInt(_selectedShape.transform.eulerAngles.z);
            _rotationValue = rotZValue < 0 ? rotZValue + 360 : rotZValue;
            
            // Offset hesaplama
            _offset = _selectedShape.transform.position - worldPosition;
            _offset.z = 0;
            
            _isDragging = true;
        }
    }

    //Called while dragging to update the shape's position.
    private void OnDragging() {
        Vector3 targetPosition = _mainCamera.ScreenToWorldPoint(Input.mousePosition) + _offset;
        targetPosition.z = 0;

        
        _selectedShape.transform.position = Vector3.Lerp(_selectedShape.transform.position, targetPosition, SmoothSpeed * Time.deltaTime);
        
        //Provide feedback if the current grid cell is valid and not already painted.
        if (IsValidDropPosition() && !IsCurrentGridCellPainted())
        {
            int childIndex = 0;
            if (_selectedShape.CompareTag("IShape")) childIndex = 1;
            else if (_selectedShape.CompareTag("UShape")) childIndex = 2;
            
            Transform child = feedBack.GetChild(childIndex);
            child.gameObject.SetActive(true);
            child.eulerAngles = _selectedShape.transform.eulerAngles;
            child.position = RoundToGrid();
        }
        else
        {
            for (int i = 0; i < feedBack.childCount; i++)
            {
                if (feedBack.GetChild(i).gameObject.activeSelf)
                    feedBack.GetChild(i).gameObject.SetActive(false);
            }
        }
    }

    //Called when the user releases the mouse button.
    private void OnDragEnd() {
        _isDragging = false;
        if (IsValidDropPosition() && !IsCurrentGridCellPainted())
            PlaceShapeOnGrid();
        else {
            _selectedShape.transform.position = _initialPosition;
            GameManager.I.PlaySound(4);
        }
            
        _selectedShape = null;
        _rotationValue = 0;
        for (int i = 0; i < feedBack.childCount; i++) {
            if(feedBack.GetChild(i).gameObject.activeSelf) feedBack.GetChild(i).gameObject.SetActive(false);
        }
    }

    //Checks if the drop position is within grid bounds.
    private bool IsValidDropPosition() {
        Vector3 gridPos = RoundToGrid();
        return gridPos.x >= -0.5f && gridPos.x < width && gridPos.y >= -0.5f && gridPos.y < height;
    }

    //Checks if the current grid cell (where the shape is being dragged) is already painted.
    private bool IsCurrentGridCellPainted() {
        Vector3 gridPosition = RoundToGrid();
        int shapeColumn = Mathf.RoundToInt(gridPosition.x);
        int shapeRow = Mathf.RoundToInt(gridPosition.y);
        //A special condition for the "I"-shaped piece because it centers perfectly on the square.
        if (_selectedShape.CompareTag("IShape")) {
            shapeColumn = Mathf.RoundToInt(Mathf.Floor(gridPosition.x));
            shapeRow = Mathf.RoundToInt(Mathf.Floor(gridPosition.y));
            
            if (gridPosition.x < 0) shapeColumn = 0;

            if (gridPosition.y < 0) shapeRow = 0;
        }

        return IsCornerAndEdgePainted(shapeColumn, shapeRow);
    }

    //Places the dragged shape onto the grid and paints edges/corners.
    private void PlaceShapeOnGrid() {
        Vector3 gridPosition = RoundToGrid();
        _selectedShapeColumn = Mathf.RoundToInt(gridPosition.x);
        _selectedShapeRow = Mathf.RoundToInt(gridPosition.y);
        ////A special condition for the "I"-shaped piece because it centers perfectly on the square.
        if (_selectedShape.CompareTag("IShape")) {
            _selectedShapeColumn = Mathf.RoundToInt(Mathf.Floor(gridPosition.x));
            _selectedShapeRow = Mathf.RoundToInt(Mathf.Floor(gridPosition.y));
            
            if (gridPosition.x < 0) _selectedShapeColumn = 0;

            if (gridPosition.y < 0) _selectedShapeRow = 0;
        }
        _selectedShape.transform.position = gridPosition;
        DetermineAndPaintEdgesAndCorners();
        
        Destroy(_selectedShape);
        int index = _allShape.IndexOf(_selectedShape.transform);
        if (index >= 0) {
            _allShape.RemoveAt(index);
            _isShapeAtPosition.RemoveAt(index);
            _successPlatform.RemoveAt(index);
        }
        if (_allShape.Count == 0) {
            SpawnInitialShapes();
        }

    }

    //Rounds the shape position to the nearest grid cell.
    private Vector3 RoundToGrid() {
        Vector3 shapePos = _selectedShape.transform.position;
        float gridX = Mathf.RoundToInt(shapePos.x);
        float gridY = Mathf.RoundToInt(shapePos.y);
        //A special condition for the "I"-shaped piece because it centers perfectly on the square.
        if (_selectedShape.CompareTag("IShape")) {
            if (_rotationValue == 90 || _rotationValue == 270) {
                if (shapePos.y < 0 && shapePos.y >= -0.5f) {
                    gridY = -0.5f;
                }
                else {
                    gridY = Mathf.Floor(shapePos.y) + 0.5f;
                }
            }
            else {
                if (shapePos.x < 0 && shapePos.x >= -0.5f) {
                    gridX = -0.5f;
                }
                else {
                    gridX = Mathf.Floor(shapePos.x) + 0.5f;
                }
            }
        }

        return new Vector3(gridX,gridY,shapePos.z);
    }
    //Checks if a specific square edge/corner is already painted based on the shape type and its rotation.
    //True if at least one relevant edge/corner is painted
    public bool IsCornerAndEdgePainted(int column, int row) {
        Transform shape = _selectedShape.transform;
        string tagName = shape.tag;
        bool bTop = false;
        bool bLeft = false;
        bool bRight = false;
        bool bBottom = false;
        
        switch (tagName) {
            case "LShape":
                (bLeft, bBottom, bRight, bTop) = _rotationValue switch {
                    0 => (_allDots[column, row].IsLeftPainted(), _allDots[column, row].IsBottomPainted(), false, false),
                    90 => (false, _allDots[column, row].IsBottomPainted(), _allDots[column, row].IsRightPainted(),
                        false),
                    180 => (false, false, _allDots[column, row].IsRightPainted(), _allDots[column, row].IsTopPainted()),
                    270 => (_allDots[column, row].IsLeftPainted(), false, false, _allDots[column, row].IsTopPainted()),
                    _ => (false, false, false, false)
                };
                break;
            case "UShape":
                (bLeft, bBottom, bRight, bTop) = _rotationValue switch {
                    0 => (_allDots[column, row].IsLeftPainted(), _allDots[column, row].IsBottomPainted(),
                        _allDots[column, row].IsRightPainted(), false),
                    90 => (false, _allDots[column, row].IsBottomPainted(), _allDots[column, row].IsRightPainted(),
                        _allDots[column, row].IsTopPainted()),
                    180 => (_allDots[column, row].IsLeftPainted(), false, _allDots[column, row].IsRightPainted(),
                        _allDots[column, row].IsTopPainted()),
                    270 => (_allDots[column, row].IsLeftPainted(), _allDots[column, row].IsBottomPainted(), false,
                        _allDots[column, row].IsTopPainted()),
                    _ => (false, false, false, false)
                };
                break;
            case "IShape":
                (bLeft, bBottom, bRight, bTop) = _rotationValue switch {
                    0 or 180 => (shape.position.x < 0 ? _allDots[column, row].IsLeftPainted() : false, false,
                        shape.position.x >= 0 ? _allDots[column, row].IsRightPainted() : false, false),
                    90 or 270 => (false, shape.position.y < 0 ? _allDots[column, row].IsBottomPainted() : false, false,
                        shape.position.y >= 0 ? _allDots[column, row].IsTopPainted() : false),
                    _ => (false, false, false, false)
                };
                break;
        }

        return bTop || bRight || bLeft || bBottom;
    }

    
    //Determines which edges and corners of a specific shape will be painted in the grid.
    public void DetermineAndPaintEdgesAndCorners() {
        Transform shape = _selectedShape.transform;
        string tagName = shape.tag;
        Dictionary<(string, int), Action> paintActions = new()
        {
            { ("LShape", 0), () => { PaintLeftEdge(); PaintBottomEdge(); } },
            { ("LShape", 90), () => { PaintRightEdge(); PaintBottomEdge(); } },
            { ("LShape", 180), () => { PaintTopEdge(); PaintRightEdge(); } },
            { ("LShape", 270), () => { PaintLeftEdge(); PaintTopEdge(); } },

            { ("UShape", 0), () => { PaintLeftEdge(); PaintBottomEdge(); PaintRightEdge(); } },
            { ("UShape", 90), () => { PaintTopEdge(); PaintRightEdge(); PaintBottomEdge(); } },
            { ("UShape", 180), () => { PaintTopEdge(); PaintRightEdge(); PaintLeftEdge(); } },
            { ("UShape", 270), () => { PaintLeftEdge(); PaintTopEdge(); PaintBottomEdge(); } },

            { ("IShape", 0), () => { if (shape.position.x < 0) PaintLeftEdge(); else PaintRightEdge(); } },
            { ("IShape", 90), () => { if (shape.position.y < 0) PaintBottomEdge(); else PaintTopEdge(); } },
            { ("IShape", 180), () => { if (shape.position.x < 0) PaintLeftEdge(); else PaintRightEdge(); } },
            { ("IShape", 270), () => { if (shape.position.y < 0) PaintBottomEdge(); else PaintTopEdge(); } }
        };

        if (paintActions.TryGetValue((tagName, _rotationValue), out Action paintAction)) {
            paintAction.Invoke();
        }
        
        //After painting, check for full rows/columns and update animation.
        ControlAllDots();
        ResetRowOrColumnIfNeeded();
        AddSquareToAnimationList();
        
    }
    
    private void PaintLeftEdge() {
        int x = _selectedShapeColumn;
        int y = _selectedShapeRow;
        _allDots[x, y].PaintLeft(_randomColor);
        //Paint adjacent corners
        //The adjacent square on the above.
        if (y + 1 < height) {
            _allDots[x, y+1].PaintCorner(_randomColor,0); //Bottom-Left corner
        }
        //The adjacent square on the below.
        if (y - 1 >= 0) {
            _allDots[x, y-1].PaintCorner(_randomColor, 1); //Top-Left corner
        }
        //Paint the square on the left if it exists.
        if (x - 1 >= 0) {
            _allDots[x-1,y].PaintRight(_randomColor);
            //Paint adjacent corners
            //The square above the square on the left.
            if (y + 1 < height) {
                _allDots[x-1, y+1].PaintCorner(_randomColor,3); //Bottom-Right corner
            }
            //The square below the square on the left.
            if (y - 1 >= 0) {
                _allDots[x-1, y-1].PaintCorner(_randomColor, 2);//Top-Right corner
            }
        }
    }

    private void PaintTopEdge() {
        int x = _selectedShapeColumn;
        int y = _selectedShapeRow;
        _allDots[x, y].PaintTop(_randomColor);
        //Paint adjacent corners
        //The adjacent square on the right.
        if (x + 1 < width) {
            _allDots[x+1,y].PaintCorner(_randomColor, 1); //Top-Left corner
        }
        //The adjacent square on the right.
        if (x - 1 >= 0) {
            _allDots[x-1,y].PaintCorner(_randomColor, 2); //Top-RightCorner
        }
        //Paint the square above if it exists.
        if (y + 1 < height) {
            _allDots[x, y+1].PaintBottom(_randomColor);
            //Paint adjacent corners
            //The square above the square on the right.
            if (x + 1 < width) {
                _allDots[x+1,y+1].PaintCorner(_randomColor, 0); //Bottom-Left corner
            }
            //The square above the square on the left.
            if (x - 1 >= 0) {
                _allDots[x-1,y+1].PaintCorner(_randomColor, 3);//Bottom right corner
            }
        }
    }
    
    private void PaintRightEdge() {
        int x = _selectedShapeColumn;
        int y = _selectedShapeRow;
        _allDots[x, y].PaintRight(_randomColor);
        //Paint adjacent corners
        //The adjacent square on the above.
        if (y + 1 < height) 
            _allDots[x, y+1].PaintCorner(_randomColor, 3); //Bottom-Right corner
        //The adjacent square on the below.
        if (y - 1 >= 0) 
            _allDots[x,y-1].PaintCorner(_randomColor, 2); //Top-Right corner
        
        //Paint the square on the right if it exists.
        if (x + 1 < width) {
            _allDots[x+1, y].PaintLeft(_randomColor);
            
            //The square above the square on the right.
            if (y + 1 < height) {
                _allDots[x+1,y+1].PaintCorner(_randomColor, 0); //Bottom-Left corner
            }
            //The square below the square on the right.
            if (y - 1 >= 0) {
                _allDots[x+1, y-1].PaintCorner(_randomColor, 1); //Top-RightCorner
            }
        }
    }
    
    private void PaintBottomEdge() {
        int x = _selectedShapeColumn;
        int y = _selectedShapeRow;
        _allDots[x,y].PaintBottom(_randomColor);
        //Paint adjacent corners
        //The adjacent square on the right.
        if (x + 1 < width)
            _allDots[x+1,y].PaintCorner(_randomColor, 0); //Bottom-Left corner
        //The adjacent square on the left.
        if (x - 1 >= 0)
            _allDots[x-1,y].PaintCorner(_randomColor, 3); //Bottom-Right corner
        
        //Paint the square below if it exists.
        if (y - 1 >= 0) {
            _allDots[x, y-1].PaintTop(_randomColor);
            //Paint adjacent corners
            //The square below the square on the right.
            if (x + 1 < width) 
                _allDots[x+1,y-1].PaintCorner(_randomColor, 1); //Top-Left corner
            //The square below the square on the left.
            if (x - 1 >= 0) 
                _allDots[x-1, y-1].PaintCorner(_randomColor, 2); //Top-Right corner
            
        }
    }
    
    //Scans the grid for fully painted squares. If a whole row or column is fully painted, its index is added to the respective reset list.
    public void ControlAllDots() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (_allDots[i, j].IsFullyPainted()) {
                    if (IsFullColumn(i) && !_resetColumn.Contains(i)) _resetColumn.Add(i);
                    
                    if (IsFullRow(j) && !_resetRow.Contains(j)) _resetRow.Add(j);
                    
                }
            }
        }
    }
    
    //Check entire column
    private bool IsFullColumn(int column) {
        for (int k = 0; k < height; k++) {
            if (!_allDots[column, k].IsFullyPainted()) {
                return false;
            }
        }
        return true;
    }
    //Check entire row
    private bool IsFullRow(int row) {
        for (int k = 0; k < width; k++) {
            if (!_allDots[k, row].IsFullyPainted()) {
                return false;
            }
        }
        return true;
    }

    //After placing the shape, resets other squares in the grid if an entire column or row is filled and triggers spark particle effects.
    public void ResetRowOrColumnIfNeeded() {
        ProcessResetColumns();
        ProcessResetRows();
    }

    private void ProcessResetColumns() {
        if (_resetColumn.Count == 0) return;
        for (int i = 0; i < _resetColumn.Count; i++) {
            int column = _resetColumn[i];
            CreateSparkEffect(new Vector3(column, -2.5f, 0f), height, false);

            for (int j = 0; j < height; j++) {
                //Checks if the squares in the column to the left of the column to be reset are painted.
                bool bLeftEmpty = column - 1 < 0 || !_allDots[column - 1, j].IsFullyPainted();
                //Checks if the squares in the column to the right of the column to be reset are painted.
                bool bRightEmpty = (column + 1 >= width) || !_allDots[column + 1, j].IsFullyPainted();

                bool bTopLeftCornerEmpty = true;
                bool bBottomLeftCornerEmpty = true;

                bool bTopRightCornerEmpty = true;
                bool bBottomRightCornerEmpty = true;
                
                //If the square in the column to the left of the column to be reset is not painted.
                if (bLeftEmpty && column - 1 >= 0) {
                    //If the top-right corner of the square is painted.
                    if (_allDots[column - 1, j].IsPaintCorner(2)) {
                        //If the top edge of the square is painted.
                        if (_allDots[column - 1, j].IsTopPainted()) {
                            bTopLeftCornerEmpty = false;
                        }
                        //If the top edge of the square is not painted, remove the paint from the top-right corner.
                        else
                            _allDots[column - 1, j].ResetCorner(2);
                    }
                    //If the top-right corner of the square is not painted
                    else
                        _allDots[column - 1, j].ResetCorner(2);
                    
                    //If the bottom-right corner of the square is painted.
                    if (_allDots[column - 1, j].IsPaintCorner(3)) {
                        //If the bottom edge of the square is painted.
                        if (_allDots[column - 1, j].IsBottomPainted()) {
                            bBottomLeftCornerEmpty = false;
                        }
                        //If the bottom edge of the square is not painted, remove the paint from the bottom-right corner.
                        else {
                            _allDots[column - 1, j].ResetCorner(3);
                        }
                    }
                    //If the bottom-right corner of the square is not painted, remove the paint from the bottom-right corner.
                    else
                        _allDots[column - 1, j].ResetCorner(3);
                    //If the square is not painted.
                    if (!_allDots[column - 1, j].IsFullyPainted())
                        //Remove the paint from the right edge of the square.
                        _allDots[column - 1, j].ResetRightEdge();
                }
                
                //If the square in the column to the right of the column to be reset is not painted.
                if (bRightEmpty && column + 1 < width) {
                    //If the top-left corner of the square is painted.
                    if (_allDots[column + 1, j].IsPaintCorner(1)) {
                        //If the top edge of the square is painted.
                        if (_allDots[column + 1, j].IsTopPainted())
                            bTopRightCornerEmpty = false;
                        //If the top edge of the square is not painted, remove the paint from the top-left corner.
                        else
                            _allDots[column + 1, j].ResetCorner(1);
                    }
                    //If the top-left corner of the square is not painted
                    else
                        _allDots[column + 1, j].ResetCorner(1);
                    
                    //If the bottom-left corner of the square is painted.
                    if (_allDots[column + 1, j].IsPaintCorner(0)) {
                        //If the bottom edge of the square is painted.
                        if (_allDots[column + 1, j].IsBottomPainted())
                            bBottomRightCornerEmpty = false;
                        //If the bottom edge of the square is not painted, remove the paint from the bottom-left corner.
                        else
                            _allDots[column + 1, j].ResetCorner(0);
                    }
                    //If the bottom-left corner of the square is not painted, remove the paint from the bottom-left corner.
                    else
                        _allDots[column + 1, j].ResetCorner(0);
                    
                    //If the square is not painted.
                    if (!_allDots[column + 1, j].IsFullyPainted())
                        //Remove the paint from the left edge of the square.
                        _allDots[column + 1, j].ResetLeftEdge();
                }

                _allDots[column, j]
                    .ResetForColumnClear(bLeftEmpty, bTopLeftCornerEmpty, bBottomLeftCornerEmpty, bRightEmpty,
                        bTopRightCornerEmpty, bBottomRightCornerEmpty);
            }
        }
        GameManager.I.PlaySound(3);
        _resetColumn.Clear();
    }

    private void ProcessResetRows() {
        if (_resetRow.Count == 0) return;
        for (int i = 0; i < _resetRow.Count; i++) {
            int row = _resetRow[i];
            CreateSparkEffect(new Vector3(-2f, row, 0f), width, true);

            for (int j = 0; j < width; j++) {
                //Checks if the squares in the row to the below of the row to be reset are painted.
                bool bBottomEmpty = row - 1 < 0 || !_allDots[j, row - 1].IsFullyPainted();
                //Checks if the squares in the row to the above of the row to be reset are painted.
                bool bTopEmpty = row + 1 >= height || !_allDots[j, row + 1].IsFullyPainted();

                bool bBottomLeftBottomCorner = true;
                bool bBottomRightBottomCorner = true;

                bool bTopLeftTopCorner = true;
                bool bTopRightTopCorner = true;
                //If the square in the row to the below of the row to be reset is not painted.
                if (bBottomEmpty && row - 1 >= 0) {
                    //If the top-left corner of the square is painted.
                    if (_allDots[j, row - 1].IsPaintCorner(1)) {
                        //If the left edge of the square is painted.
                        if (_allDots[j, row - 1].IsLeftPainted()) {
                            bBottomLeftBottomCorner = false;
                        }
                        //If the left edge of the square is not painted, remove the paint from the top-left corner.
                        else {
                            _allDots[j, row - 1].ResetCorner(1);
                        }
                    }
                    //If the top-left corner of the square is not painted
                    else
                        _allDots[j, row - 1].ResetCorner(1);
                    
                    //If the top-right corner of the square is painted.
                    if (_allDots[j, row - 1].IsPaintCorner(2)) {
                        //If the right edge of the square is painted.
                        if (_allDots[j, row - 1].IsRightPainted()) {
                            bBottomRightBottomCorner = false;
                        }
                        //If the right edge of the square is not painted, remove the paint from the top-right corner.
                        else {
                            _allDots[j, row - 1].ResetCorner(2);
                        }
                    }
                    //If the top-right corner of the square is not painted
                    else
                        _allDots[j, row - 1].ResetCorner(2);
                    //If the square is not painted.
                    if (!_allDots[j, row - 1].IsFullyPainted()) {
                        //Remove the paint from the top edge of the square.
                        _allDots[j, row - 1].ResetTopEdge();
                    }
                }
                
                //If the square in the row to the right of the row to be reset is not painted.
                if (bTopEmpty && row + 1 < height) {
                    //If the bottom-left corner of the square is painted.
                    if (_allDots[j, row + 1].IsPaintCorner(0)) {
                        //If the left edge of the square is painted.
                        if (_allDots[j, row + 1].IsLeftPainted())
                            bTopLeftTopCorner = false;
                        //If the left edge of the square is not painted, remove the paint from the bottom-left corner
                        else
                            _allDots[j, row + 1].ResetCorner(0);
                    }
                    //If the bottom-left corner of the square is not painted, remove the paint from the bottom-left corner
                    else
                        _allDots[j, row + 1].ResetCorner(0);
                    
                    //If the bottom-right corner of the square is painted.
                    if (_allDots[j, row + 1].IsPaintCorner(3)) {
                        //If the right edge of the square is painted.
                        if (_allDots[j, row + 1].IsRightPainted())
                            bTopRightTopCorner = false;
                        //If the right edge of the square is not painted, remove the paint from the bottom-right corner.
                        else
                            _allDots[j, row + 1].ResetCorner(3);
                    }
                    //If the bottom-left right of the square is not painted, remove the paint from the bottom-right corner.
                    else
                        _allDots[j, row + 1].ResetCorner(3);
                    //If the square is not painted.
                    if (!_allDots[j, row + 1].IsFullyPainted())
                        //Remove the paint from the bottom edge of the square.
                        _allDots[j, row + 1].ResetBottomEdge();
                }

                _allDots[j, row]
                    .ResetForRowClear(bTopEmpty, bTopLeftTopCorner, bTopRightTopCorner, bBottomEmpty,
                        bBottomLeftBottomCorner, bBottomRightBottomCorner);
            }

            _resetRow.Clear();
            GameManager.I.PlaySound(3);
        }
    }

    private void CreateSparkEffect(Vector3 position, int scaleFactor, bool bRow) {
        GameObject sparkParticle = Instantiate(particle, position, Quaternion.identity);
        ParticleSystemRenderer ps = sparkParticle.GetComponent<ParticleSystemRenderer>();
        sparkParticle.transform.eulerAngles = new Vector3(bRow ? 0 : 90f, bRow ? -90f : 0f, 0);
        ps.lengthScale = (scaleFactor + 2) * -10f;
        Destroy(sparkParticle, 0.5f);
    }
    
    //Adds fully painted squares (that haven't been animated yet) to the animation list,and processes score/combos.
    void AddSquareToAnimationList() {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                Transform dotSquare = _allDots[i, j].DotTransform();
                if (_allDots[i, j].IsFullyPainted() && !_mainAnimDot.Contains(dotSquare) && !_allDots[i,j].IsObjectAddedToMainList()) {
                    _mainAnimDot.Add(dotSquare);
                }
            }
        }

        if (_mainAnimDot.Count == 0) {
            _combo = 1;
            GameManager.I.GainPoint(3*_combo);
            GameManager.I.PlaySound(1);
            return;
        }
        GameManager.I.PlaySound(2);
        int index = _mainAnimDot.Count;
        int totalValue = _combo + index - 1;
        UIManager.I.ComboUIControl(totalValue);
        GameManager.I.GainPoint(3 * totalValue);
        GameManager.I.GainPoint(10 * totalValue);
        StartCoroutine(UIManager.I.DelayPointAnim(10 * totalValue, _mainCamera.WorldToScreenPoint(_mainAnimDot[^1].transform.position)));
        _combo += index;
    }
    
    //Animates the feedback sprite on fully painted squares.
    void AnimateMainObjectColor() {
        if(_mainAnimDot.Count == 0) return;
        for (int i = _mainAnimDot.Count-1; i >= 0 ; i--) {
            Transform dotAnim = _mainAnimDot[i];
            Transform feedbackSprite = dotAnim.GetChild(2);
            feedbackSprite.localScale = Vector3.Lerp(feedbackSprite.localScale, Vector3.one, 10f * Time.deltaTime);
            SpriteRenderer sr = feedbackSprite.GetComponent<SpriteRenderer>();
            sr.color = Color.Lerp(sr.color, _randomColor, 10f*Time.deltaTime);
            if (Vector3.Distance(feedbackSprite.localScale, Vector3.one) < 0.1f && sr.color == _randomColor) {
                _mainAnimDot.RemoveAt(i);
            }
        }
    }
    
    //Checks if each remaining shape has a valid position in the grid.
    public void ControlFailState() {
        for (int i = 0; i < _allShape.Count; i++) {
            int rotZValue = Mathf.RoundToInt(_allShape[i].eulerAngles.z);
            int shapeRotation = rotZValue < 0 ?  rotZValue + 360 : rotZValue;
            string tagName = _allShape[i].tag;
            _successPlatform[i] = tagName switch {
                "LShape" => ControlDotsForShape(0, shapeRotation),
                "UShape" => ControlDotsForShape(1, shapeRotation),
                "IShape" => ControlDotsForShape(2, shapeRotation), 
                _ => false
            };
        }
    }

    //Iterates through each dot in the grid to check for edges/corners.
    //Returns true if there is a valid place in the grid based on the shape of the spawned object.
    public bool ControlDotsForShape(int shapeType, int rotationIndex) {
        for (int i = 0; i < width; i++) {
            for (int j = 0; j < height; j++) {
                if (shapeType == 0) { // LShape
                    bool isValid = rotationIndex switch {
                        0 => !_allDots[i, j].IsLeftPainted() && !_allDots[i, j].IsBottomPainted(), //Checks whether the left and bottom edges are painted.
                        90 => !_allDots[i, j].IsRightPainted() && !_allDots[i, j].IsBottomPainted(), //Checks whether the right and bottom edges are painted.
                        180 => !_allDots[i, j].IsTopPainted() && !_allDots[i, j].IsRightPainted(), //Checks whether the top and right edges are painted.
                        270 => !_allDots[i, j].IsLeftPainted() && !_allDots[i, j].IsTopPainted(), //Checks whether the left and top edges are painted.
                        _ => false
                    };
                    if (isValid) {
                        return true;
                    }
                }
                if (shapeType == 1) { // UShape
                    bool isValid = rotationIndex switch {
                        0 => !_allDots[i, j].IsLeftPainted() && !_allDots[i, j].IsBottomPainted() && !_allDots[i, j].IsRightPainted(), //Checks whether the left, bottom and right edges are painted.
                        90 => !_allDots[i, j].IsTopPainted() && !_allDots[i, j].IsBottomPainted() && !_allDots[i, j].IsRightPainted(), //Checks whether the top, bottom and right edges are painted.
                        180 => !_allDots[i, j].IsLeftPainted() && !_allDots[i, j].IsTopPainted() && !_allDots[i, j].IsRightPainted(), //Checks whether the left, top and right edges are painted.
                        270 => !_allDots[i, j].IsLeftPainted() && !_allDots[i, j].IsTopPainted() && !_allDots[i, j].IsBottomPainted(), //Checks whether the left, top and bottom edges are painted.
                        _ => false
                    };
                    if (isValid) {
                        return true;
                    }
                }
                if (shapeType == 2) { // IShape
                    bool isValid = rotationIndex switch {
                        0 or 180 => !_allDots[i, j].IsLeftPainted() || !_allDots[i, j].IsRightPainted(), //Checks whether the left or right edges are painted.
                        90 or 270 => !_allDots[i, j].IsTopPainted() || !_allDots[i, j].IsBottomPainted(), //Checks whether the top or bottom edges are painted
                        _ => false
                    };
                    if (isValid) {
                        return true;
                    }
                }
                
            }
        }
        return false;
    }

    //Checks if the overall fail condition is met (i.e., no shape has a valid position in the grid).
    public void CheckFailCondition() {
        if(_successPlatform.Count == 0 || _isShapeAtPosition.Count == 0) return;
        //If the shapes are at the designated position and there is no valid place for them in the grid.
        if (_successPlatform.TrueForAll(sp => !sp) && _isShapeAtPosition.TrueForAll(sp => sp)) {
            _isFail = true;
        }
        
    }

    public enum PossiblePos {
        BottomEdge, //Only the bottom edge/corner is deactivated.
        BottomAndLeftEdge, //Bottom and left edges/corners are deactivated.
        FirstDot, //First dot, no deactivation.
        LeftEdge //Only the left edge/corner is deactivated.
    }

    //Sets deactivated edges/corners for dots based on their grid position.
    public void SetDotsEdgesAndCorners(PossiblePos position, Transform dotTransform) {
        switch (position) {
            case PossiblePos.BottomEdge:
                DeactivateBottomEdgesAndCorners(dotTransform);
                break;
            case PossiblePos.BottomAndLeftEdge:
                DeactivateBottomEdgesAndCorners(dotTransform);
                DeactivateLeftEdgesAndCorners(dotTransform);
                break;
            case PossiblePos.FirstDot:
                // No deactivation needed.
                break;
            case PossiblePos.LeftEdge:
                DeactivateLeftEdgesAndCorners(dotTransform);
                break;
        }
    }

    public void DeactivateBottomEdgesAndCorners(Transform dotTransform) {
        dotTransform.GetChild(0).GetChild(3).gameObject.SetActive(false); //Bottom edge
        dotTransform.GetChild(1).GetChild(3).gameObject.SetActive(false); //Bottom-right corner
        dotTransform.GetChild(1).GetChild(0).gameObject.SetActive(false); //Bottom-left corner
        
    }
    
    public void DeactivateLeftEdgesAndCorners(Transform dotTransform) {
        dotTransform.GetChild(0).GetChild(0).gameObject.SetActive(false); //Left edge
        dotTransform.GetChild(1).GetChild(1).gameObject.SetActive(false); //Top-left corner
        dotTransform.GetChild(1).GetChild(0).gameObject.SetActive(false); //Bottom-left corner
        
    }
}
