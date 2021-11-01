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

		Style.BackgroundColor = new Color( 0, 0, 0, pawn.LifeState != LifeState.Alive ? 1 : 0 );
		Style.Dirty();
	}
}
