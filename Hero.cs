using Godot;

public partial class Hero : Node2D
{
  [Export]
  private float _speed = 200f;

  public override void _Process(double delta)
  {
    Vector2 direction = Input.GetVector(
        "move_left",
        "move_right",
        "move_up",
        "move_down"
    );

    Position += direction * _speed * (float)delta;
  }

  public override void _Draw()
  {
    DrawCircle(Vector2.Zero, 24, Colors.Gold);
    DrawCircle(Vector2.Zero, 20, Colors.RoyalBlue);
  }
}
