using Godot;
using System;

public enum GameState { Exploration, BunkerMode, GameOver }

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Export] public Player Player;
	[Export] public Bunker Bunker;
	[Export] public PackedScene DroneScene;

	private GameState _currentState = GameState.Exploration;
	public GameState CurrentState => _currentState;

	private float _waveTimer = 0.0f;
	private float _spawnInterval = 4.0f; // Spawning lento como pediste

	public override void _Ready()
	{
		Instance = this;
	}

	public override void _Process(double delta)
	{
		if (_currentState == GameState.GameOver)
		{
			if (Input.IsActionJustPressed("interact")) // Reiniciar con E
			{
				GetTree().ReloadCurrentScene();
			}
			return;
		}

		_waveTimer += (float)delta;
		if (_waveTimer >= _spawnInterval)
		{
			_waveTimer = 0.0f;
			SpawnDrone();
		}
	}

	private void SpawnDrone()
	{
		if (DroneScene == null) return;

		Drone drone = DroneScene.Instantiate<Drone>();
		GetParent().AddChild(drone);

		int side = GD.RandRange(0, 3);
		Vector2 spawnPos = Vector2.Zero;
		Vector2 screenSize = GetViewport().GetVisibleRect().Size;

		switch (side)
		{
			case 0: spawnPos = new Vector2(GD.Randf() * screenSize.X, -50); break;
			case 1: spawnPos = new Vector2(GD.Randf() * screenSize.X, screenSize.Y + 50); break;
			case 2: spawnPos = new Vector2(-50, GD.Randf() * screenSize.Y); break;
			case 3: spawnPos = new Vector2(screenSize.X + 50, GD.Randf() * screenSize.Y); break;
		}

		drone.GlobalPosition = spawnPos;
	}

	public void ChangeState(GameState newState)
	{
		_currentState = newState;
		
		switch (_currentState)
		{
			case GameState.Exploration:
				Player.Visible = true;
				Player.SetPhysicsProcess(true);
				Bunker.SetBunkerActive(false);
				break;
				
			case GameState.BunkerMode:
				Player.Visible = false;
				Player.SetPhysicsProcess(false);
				Bunker.SetBunkerActive(true);
				break;
				
			case GameState.GameOver:
				GD.Print("¡COOKED! GAME OVER. Presiona E para reiniciar.");
				// Podríamos añadir una pausa o efectos aquí
				break;
		}
	}
}
