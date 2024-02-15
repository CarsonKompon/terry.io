using System;
using Sandbox;

public sealed class BlobManager : Component
{
	public static BlobManager Instance { get; private set; }

	[Property] GameObject PlayerPrefab { get; set; }
	[Property] GameObject PelletPrefab { get; set; }

	TimeSince lastPelletSpawn = 0f;

	protected override void OnAwake()
	{
		Instance = this;
		Pellet.ResetCount();
	}

	protected override void OnStart()
	{
		if ( Networking.IsHost )
		{
			for ( var i = 0; i < 200; i++ )
			{
				SpawnPellet();
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
	}

	public void SpawnPellet()
	{
		var pellet = PelletPrefab.Clone();
		pellet.Transform.Position = new Vector3( Random.Shared.Float( -5000, 5000 ), Random.Shared.Float( -5000, 5000 ), 0 );
		pellet.NetworkSpawn( null ); // No owner
	}

	public void RespawnLocalPlayer()
	{
		if ( Player.Local.IsValid() ) return;
		var player = PlayerPrefab.Clone();
		player.Transform.Position = new Vector3( Random.Shared.Float( -5000, 5000 ), Random.Shared.Float( -5000, 5000 ), 0 );
		player.NetworkSpawn( null ); // No owner
	}
}