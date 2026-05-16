using Godot;
using System;

public enum GameState { Exploration, BunkerMode, GameOver }

public partial class GameManager : Node
{
	public static GameManager Instance { get; private set; }

	[Export] public Player Player;
	[Export] public Bunker Bunker;

	private GameState _currentState = GameState.Exploration;
	public GameState CurrentState => _currentState;

	private float _waveTimer = 0.0f;
	private float _spawnInterval = 3.0f;

	public override void _Ready()
	{
		Instance = this;
	}

	public override void _Process(double delta)
	{
		if (_currentState == GameState.GameOver) return;

		// Solo spawnear drones si el jugador está en el búnker (o siempre, según prefieras)
		// Vamos a activarlo siempre para dar dinamismo
		_waveTimer += (float)delta;
		if (_waveTimer >= _spawnInterval)
		{
			_waveTimer = 0.0f;
			SpawnDrone();
		}
	}

	private void SpawnDrone()
	{
		// Lados: 0=Arriba, 1=Abajo, 2=Izquierda, 3=Derecha
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

		GD.Print($"Spawning drone from side {side} at position {spawnPos}");
		// TODO: Instanciar drone y dirigirlo al búnker
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
				// Lógica de fin de juego
				GD.Print("GAME OVER");
				break;
		}
	}
}
