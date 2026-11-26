using Godot;

public partial class TurnManager : Node
{
  [Signal]
  public delegate void DayChangedEventHandler(int day);

  public int CurrentDay { get; private set; } = 1;

  public void EndTurn()
  {
    CurrentDay++;

    EmitSignal(SignalName.DayChanged, CurrentDay);

    GD.Print($"Day {CurrentDay} started");
  }
}