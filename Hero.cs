
using Godot;

public partial class Hero : Node2D
{
  private TileMapLayer _ground;
  private Vector2I _currentCell;
  private AdventureMap _map;

  public override void _Ready()
  {
    _ground = GetParent().GetNode<TileMapLayer>("Ground");

    Vector2 localPosition = _ground.ToLocal(GlobalPosition);
    _currentCell = _ground.LocalToMap(localPosition);

    MoveToCell(_currentCell);

    _map = GetParent().GetNode<AdventureMap>("AdventureMap");
  }

  public override void _UnhandledInput(InputEvent @event)
  {
    if (@event is not InputEventKey keyEvent)
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

    if (direction == Vector2I.Zero)
      return;

    Vector2I targetCell = _currentCell + direction;


    if (!_map.IsWalkable(targetCell))
    {
      GD.Print($"Cannot move to {targetCell}");
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
