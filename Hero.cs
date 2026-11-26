
using Godot;

public partial class Hero : Node2D
{

  [Signal]
  public delegate void MovementPointsChangedEventHandler(
    int current,
    int maximum
  );

  public int MovementPoints => _movementPoints;

  public int MaxMovementPoints => _maxMovementPoints;

  public bool IsMoving => _isMoving;

  private TileMapLayer _ground;
  private Vector2I _currentCell;
  private AdventureMap _map;

  private bool _isMoving;

  private PathPreview _pathPreview;

  private Vector2I? _selectedCell;

  private Vector2I[] _selectedPath = System.Array.Empty<Vector2I>();

  [Export]
  private int _maxMovementPoints = 10;

  private int _movementPoints;

  private TurnManager _turnManager;

  public override void _Ready()
  {
    _movementPoints = _maxMovementPoints;

    _pathPreview = GetParent().GetNode<PathPreview>("PathPreview");

    _ground = GetParent().GetNode<TileMapLayer>("Ground");

    Vector2 localPosition = _ground.ToLocal(GlobalPosition);
    _currentCell = _ground.LocalToMap(localPosition);

    MoveToCell(_currentCell);

    _map = GetParent().GetNode<AdventureMap>("AdventureMap");

    _turnManager = GetParent().GetNode<TurnManager>("TurnManager");

    _turnManager.DayChanged += OnDayChanged;
  }



  private async void MoveAlongPath(Vector2I[] path)
  {
    if (_isMoving || path.Length < 2)
      return;

    _isMoving = true;

    for (int i = 1; i < path.Length; i++)
    {
      Vector2I cell = path[i];

      if (!_map.IsWalkable(cell))
        break;

      int cost = _map.GetMovementCost(cell);

      if (_movementPoints < cost)
      {
        GD.Print("Not enough movement points");
        break;
      }

      Vector2 localPosition = _ground.MapToLocal(cell);
      Vector2 targetPosition = _ground.ToGlobal(localPosition);

      Tween tween = CreateTween();

      tween.TweenProperty(
          this,
          "global_position",
          targetPosition,
          0.2
      );

      await ToSignal(tween, Tween.SignalName.Finished);

      _currentCell = cell;
      _movementPoints -= cost;
      NotifyMovementPointsChanged();

      GD.Print($"Movement points: {_movementPoints}/{_maxMovementPoints}");
    }

    _isMoving = false;
  }


  public override void _UnhandledInput(InputEvent @event)
  {

    if (@event.IsActionPressed("restore_movement"))
    {
      RestoreMovementPoints();
      return;
    }

    if (@event is InputEventMouseButton mouseEvent)
    {
      if (!mouseEvent.Pressed || _isMoving)
        return;

      if (mouseEvent.ButtonIndex == MouseButton.Left)
      {
        Vector2 mousePosition = GetGlobalMousePosition();
        Vector2 localPosition = _ground.ToLocal(mousePosition);
        Vector2I targetCell = _ground.LocalToMap(localPosition);

        SelectDestination(targetCell);
      }
      else if (mouseEvent.ButtonIndex == MouseButton.Right)
      {
        ClearSelectedPath();
      }

      return;
    }

    if (@event is not InputEventKey keyEvent)
      return;

    if (_isMoving)
      return;

    if (!keyEvent.Pressed || keyEvent.Echo)
      return;

    Vector2I direction = Vector2I.Zero;

    if (Input.IsActionJustPressed("move_up"))
      direction = Vector2I.Up;
    else if (Input.IsActionJustPressed("move_down"))
      direction = Vector2I.Down;
    else if (Input.IsActionJustPressed("move_left"))
      direction = Vector2I.Left;
    else if (Input.IsActionJustPressed("move_right"))
      direction = Vector2I.Right;

    if (direction != Vector2I.Zero)
    {
      ClearSelectedPath();
      TryMoveToCell(_currentCell + direction);
    }
  }

  private void TryMoveToCell(Vector2I targetCell)
  {
    Vector2I difference = targetCell - _currentCell;

    int distance = Mathf.Abs(difference.X)
                 + Mathf.Abs(difference.Y);

    if (distance != 1)
      return;

    if (!_map.IsWalkable(targetCell))
      return;

    MoveAlongPath(new Vector2I[]
    {
        _currentCell,
        targetCell
    });
  }

  private void MoveToCell(Vector2I cell)
  {
    Vector2 localPosition = _ground.MapToLocal(cell);
    GlobalPosition = _ground.ToGlobal(localPosition);
  }


  private void SelectDestination(Vector2I targetCell)
  {
    if (_isMoving)
      return;

    if (_selectedCell == targetCell)
    {
      Vector2I[] path = _selectedPath;

      ClearSelectedPath();
      MoveAlongPath(path);

      return;
    }

    Vector2I[] newPath = _map.FindPath(
        _currentCell,
        targetCell
    );

    if (newPath.Length < 2)
    {
      ClearSelectedPath();
      return;
    }

    _selectedCell = targetCell;
    _selectedPath = newPath;

    _pathPreview.ShowPath(newPath);
  }

  private void ClearSelectedPath()
  {
    _selectedCell = null;
    _selectedPath = System.Array.Empty<Vector2I>();

    _pathPreview.ClearPath();
  }

  public override void _Draw()
  {
    DrawCircle(Vector2.Zero, 24, Colors.Gold);
    DrawCircle(Vector2.Zero, 20, Colors.RoyalBlue);
  }

  public void RestoreMovementPoints()
  {
    if (_isMoving)
      return;

    _movementPoints = _maxMovementPoints;

    NotifyMovementPointsChanged();

    GD.Print($"Movement points restored: {_movementPoints}");
  }

  private void OnDayChanged(int day)
  {
    RestoreMovementPoints();
    ClearSelectedPath();
  }

  public override void _ExitTree()
  {
    _turnManager.DayChanged -= OnDayChanged;
  }

  private void NotifyMovementPointsChanged()
  {
    EmitSignal(
        SignalName.MovementPointsChanged,
        _movementPoints,
        _maxMovementPoints
    );
  }
}
