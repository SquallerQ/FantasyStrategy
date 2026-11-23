using Godot;
using System.Linq;

public partial class AdventureMap : Node
{
  private TileMapLayer _ground;
  private AStarGrid2D _pathfinder;

  public override void _Ready()
  {
    _ground = GetParent().GetNode<TileMapLayer>("Ground");

    SetupPathfinder();
  }

  private void SetupPathfinder()
  {
    _pathfinder = new AStarGrid2D();

    _pathfinder.Region = _ground.GetUsedRect();
    _pathfinder.CellSize = Vector2.One;
    _pathfinder.DiagonalMode = AStarGrid2D.DiagonalModeEnum.Never;

    _pathfinder.Update();

    foreach (Vector2I cell in _ground.GetUsedCells())
    {
      if (!IsWalkable(cell))
        _pathfinder.SetPointSolid(cell, true);
    }

    Rect2I region = _pathfinder.Region;

    for (int x = region.Position.X; x < region.End.X; x++)
    {
      for (int y = region.Position.Y; y < region.End.Y; y++)
      {
        Vector2I cell = new Vector2I(x, y);

        if (_ground.GetCellSourceId(cell) == -1)
          _pathfinder.SetPointSolid(cell, true);
      }
    }
  }

  public bool IsWalkable(Vector2I cell)
  {
    TileData tileData = _ground.GetCellTileData(cell);

    if (tileData == null)
      return false;

    return tileData.GetCustomData("walkable").AsBool();
  }

  public Vector2I[] FindPath(Vector2I startCell, Vector2I targetCell)
  {
    if (!IsWalkable(startCell) || !IsWalkable(targetCell))
      return System.Array.Empty<Vector2I>();

    return _pathfinder.GetIdPath(startCell, targetCell).ToArray();
  }
}
