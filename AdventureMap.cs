using Godot;

public partial class AdventureMap : Node
{
  private TileMapLayer _ground;

  public override void _Ready()
  {
    _ground = GetParent().GetNode<TileMapLayer>("Ground");
  }

  public bool IsWalkable(Vector2I cell)
  {
    TileData tileData = _ground.GetCellTileData(cell);

    if (tileData == null)
      return false;

    return tileData.GetCustomData("walkable").AsBool();
  }
}
