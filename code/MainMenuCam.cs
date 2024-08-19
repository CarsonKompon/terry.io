using Sandbox;

public sealed class MainMenuCam : Component
{
	protected override void OnUpdate()
	{
		Scene.Camera.Transform.Position += new Vector3( 20, 20, 0 ) * Time.Delta;
		if ( Scene.Camera.Transform.Position.x > 256 )
		{
			Scene.Camera.Transform.Position = new Vector3( 0, 0, Scene.Camera.Transform.Position.z );
		}
	}
}
