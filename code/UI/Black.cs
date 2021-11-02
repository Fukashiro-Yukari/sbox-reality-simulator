using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;
using System;

public partial class Black : Panel
{
	Panel black;

	public Black()
	{
		black = Add.Panel();
	}

	public override void Tick()
	{
		var pawn = Local.Pawn;

		if ( pawn == null ) return;

		var aliceColor = new Color( 1, 0, 0, (1f - (pawn.Health / 100f)) * 0.95f );
		var deathColor = new Color( 0, 0, 0 );

		if ( pawn.LifeState == LifeState.Alive )
			Style.BackgroundColor = aliceColor;
		else
			Style.BackgroundColor = deathColor;

		Style.Dirty();
	}
}
