using System;
using Sandbox;

public sealed class BlobManager : Component
{
	public static BlobManager Instance { get; private set; }

	[Property] GameObject PlayerPrefab { get; set; }
	[Property] GameObject PelletPrefab { get; set; }
	[Property] GameObject VirusPrefab { get; set; }

	TimeSince lastPelletSpawn = 0f;
	TimeSince lastVirusSpawn = 0f;

	protected override void OnAwake()
	{
		Instance = this;
		Pellet.ResetCount();
		Virus.ResetCount();
	}

	protected override void OnStart()
	{
		if ( Networking.IsHost )
		{
			for ( var i = 0; i < 200; i++ )
			{
				SpawnPellet();
			}

			for ( var i = 0; i < 3; i++ )
			{
				SpawnVirus();
			}
		}
	}

	protected override void OnUpdate()
	{
		if ( !Networking.IsHost ) return;

		if ( lastPelletSpawn > 0.25f && Pellet.TotalCount < 500 )
		{
			SpawnPellet();

			lastPelletSpawn = 0f;
		}

		if ( lastVirusSpawn > 10f && Virus.TotalCount < 5 )
		{
			SpawnVirus();

			lastVirusSpawn = 0f;
		}
	}

	public void SpawnPellet()
	{
		var pellet = PelletPrefab.Clone();
		pellet.Transform.Position = new Vector3( Random.Shared.Float( -5000, 5000 ), Random.Shared.Float( -5000, 5000 ), 0 );
		pellet.NetworkSpawn( null ); // No owner
	}

	public void SpawnVirus()
	{
		var virus = VirusPrefab.Clone();
		virus.Transform.Position = new Vector3( Random.Shared.Float( -5000, 5000 ), Random.Shared.Float( -5000, 5000 ), 0 );
		virus.NetworkSpawn( null ); // No owner
	}

	public void RespawnLocalPlayer()
	{
		foreach ( var ply in Scene.GetAllComponents<Player>() )
		{
			if ( ply.Network.OwnerId == Connection.Local.Id ) return;
		}

		var player = PlayerPrefab.Clone();
		player.Transform.Position = new Vector3( Random.Shared.Float( -5000, 5000 ), Random.Shared.Float( -5000, 5000 ), 0 );
		player.NetworkSpawn();

		GameHud.Instance.HideGameOver();
	}
}