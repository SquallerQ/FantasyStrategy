using Godot;

public partial class PathPreview : Line2D
{
  private TileMapLayer _ground;

  public override void _Ready()
  {
    _ground = GetParent().GetNode<TileMapLayer>("Ground");
  }

  public void ShowPath(Vector2I[] path)
  {
    ClearPoints();

    foreach (Vector2I cell in path)
    {
      Vector2 localPosition = _ground.MapToLocal(cell);
      Vector2 globalPosition = _ground.ToGlobal(localPosition);

      AddPoint(ToLocal(globalPosition));
    }
  }

  public void ClearPath()
  {
    ClearPoints();
  }
}
