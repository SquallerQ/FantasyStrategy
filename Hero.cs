
using Godot;

public partial class Hero : Node2D
{
  private TileMapLayer _ground;
  private Vector2I _currentCell;
  private AdventureMap _map;

  private bool _isMoving;

  public override void _Ready()
  {
    _ground = GetParent().GetNode<TileMapLayer>("Ground");

    Vector2 localPosition = _ground.ToLocal(GlobalPosition);
    _currentCell = _ground.LocalToMap(localPosition);

    MoveToCell(_currentCell);

    _map = GetParent().GetNode<AdventureMap>("AdventureMap");
  }


  private async void MoveAlongPath(Vector2I[] path)
  {
    if (_isMoving || path.Length < 2)
      return;

    _isMoving = true;

    for (int i = 1; i < path.Length; i++)
    {
      Vector2I cell = path[i];

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
    }

    _isMoving = false;
  }


  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is InputEventMouseButton mouseEvent)
    {
      if (mouseEvent.ButtonIndex == MouseButton.Left && mouseEvent.Pressed)
      {
        if (_isMoving)
          return;

        Vector2 mousePosition = GetGlobalMousePosition();
        Vector2 localPosition = _ground.ToLocal(mousePosition);
        Vector2I targetCell = _ground.LocalToMap(localPosition);

        Vector2I[] path = _map.FindPath(_currentCell, targetCell);

        if (path.Length == 0)
        {
          GD.Print("No path found");
          return;
        }

        MoveAlongPath(path);
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
      TryMoveToCell(_currentCell + direction);
  }

  private void TryMoveToCell(Vector2I targetCell)
  {
    Vector2I difference = targetCell - _currentCell;

    int distance = Mathf.Abs(difference.X) + Mathf.Abs(difference.Y);

    if (distance != 1)
    {
      GD.Print("Select an adjacent cell");
      return;
    }

    if (!_map.IsWalkable(targetCell))
    {
      GD.Print("Cannot move: terrain is blocked");
      return;
    }

    _currentCell = targetCell;
    MoveToCell(_currentCell);

    GD.Print($"Hero moved to cell: {_currentCell}");
  }

  private void MoveToCell(Vector2I cell)
  {
    Vector2 localPosition = _ground.MapToLocal(cell);
    GlobalPosition = _ground.ToGlobal(localPosition);
  }

  public override void _Draw()
  {
    DrawCircle(Vector2.Zero, 24, Colors.Gold);
    DrawCircle(Vector2.Zero, 20, Colors.RoyalBlue);
  }
}
