using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;
using System.Collections.Generic;

[Library]
public partial class SpawnMenu : Panel
{
	public static SpawnMenu Instance;

	public SpawnMenu()
	{
		Instance = this;

		StyleSheet.Load( "/UI/SpawnMenu.scss" );

		var left = Add.Panel( "left" );
		{
			var tabs = left.AddChild<ButtonGroup>();
			tabs.AddClass( "tabs" );

			var body = left.Add.Panel( "body" );
			{
				var ents = body.AddChild<EntityList>();
				tabs.SelectedButton = tabs.AddButtonActive( "Entities", ( b ) => ents.SetClass( "active", b ) );

				var weps = body.AddChild<WeaponList>();
				tabs.AddButtonActive( "Weapons", ( b ) => weps.SetClass( "active", b ) );
			}
		}
	}

	public override void Tick()
	{
		base.Tick();

		Parent.SetClass( "spawnmenuopen", Input.Down( InputButton.Menu ) );
	}
}
