using Godot;
using System;

public partial class InputManager : Node
{
	public static InputManager Instance { get; private set; }

	public override void _Ready()
	{
		Instance = this;
	}

	public Vector2 GetMovementDirection()
	{
		return Input.GetVector("move_left", "move_right", "move_up", "move_down");
	}
}
