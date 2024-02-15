using System;

public sealed class CameraFollower : Component
{
	protected override void OnUpdate()
	{
		var pos = Vector3.Zero;
		int am = 0;
		float scl = 0f;
		foreach ( var player in Scene.GetAllComponents<Player>() )
		{
			if ( player.Network.OwnerId != Connection.Local.Id ) continue;
			pos += player.Transform.Position;
			am++;
			scl += player.Scale;
		}
		if ( am > 0 )
		{
			pos /= am;
			pos.z = 200;
			Transform.Position = pos;
		}

		Scene.Camera.OrthographicHeight = 1204 + MathF.Max( (scl - 4f) * 128f, 0f );
	}
}