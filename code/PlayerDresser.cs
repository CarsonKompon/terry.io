using Sandbox;

public class PlayerDresser : Component, Component.INetworkSpawn
{
    [Property] public SkinnedModelRenderer BodyRenderer { get; set; }

    public void OnNetworkSpawn( Connection owner )
    {
        var material = BodyRenderer.Model.Materials.FirstOrDefault();
        if ( material != null )
        {
            material = material.CreateCopy();
            material.Set( "color", Texture.LoadAvatar( (long)owner.SteamId ) );
            BodyRenderer.MaterialOverride = material;
        }
    }
}