﻿using Sandbox;
using System;
using System.Linq;

partial class Inventory : BaseInventory
{
	public Inventory( Player player ) : base( player )
	{
	}

	public override bool CanAdd( Entity entity )
	{
		if ( !entity.IsValid() )
			return false;

		if ( !base.CanAdd( entity ) )
			return false;

		if ( entity is Weapon weapon )
		{
			if ( weapon.Bucket >= 0 && weapon.Bucket <= 2 )
			{
				foreach ( var wep in List )
				{
					if ( wep is Weapon w )
					{
						if ( w.Bucket == weapon.Bucket ) return false;
					}
				}
			}
		}

		return !IsCarryingType( entity.GetType() );
	}

	public virtual bool CanReplace( Entity entity )
	{
		if ( !entity.IsValid() )
			return false;

		if ( !base.CanAdd( entity ) )
			return false;

		if ( entity is Weapon weapon )
		{
			if ( weapon.Bucket >= 0 && weapon.Bucket <= 2 )
			{
				foreach ( var wep in List )
				{
					if ( wep is Weapon w )
					{
						if ( w.Bucket == weapon.Bucket ) return true;
					}
				}
			}
		}

		return false;
	}

	public Entity GetReplaceEntity( Entity entity )
	{
		if ( entity is Weapon weapon )
		{
			if ( weapon.Bucket >= 0 && weapon.Bucket <= 2 )
			{
				foreach ( var wep in List )
				{
					if ( wep is Weapon w )
					{
						if ( w.Bucket == weapon.Bucket ) return w;
					}
				}
			}
		}

		return null;
	}

	public virtual Entity Replace( Entity entity )
	{
		if ( !Host.IsServer ) return null;

		var repent = GetReplaceEntity( entity );

		if ( repent != null && repent.IsValid )
		{
			var ply = Owner as Player;
			var ac = ply.ActiveChild;

			if ( Drop( repent ) )
			{
				if ( ac != null && ac == repent )
				{
					ply.ActiveChild = null;
				}

				Owner.StartTouch( entity );

				return repent;
			}
		}

		return null;
	}

	public override bool Add( Entity entity, bool makeActive = false )
	{
		if ( !entity.IsValid() )
			return false;

		if ( IsCarryingType( entity.GetType() ) ) return false;
		if ( !CanAdd( entity ) ) return false;

		return base.Add( entity, makeActive );
	}

	public bool IsCarryingType( Type t )
	{
		return List.Any( x => x?.GetType() == t );
	}

	public override Entity DropActive()
	{
		if ( !Host.IsServer ) return null;

		var ply = Owner as Player;
		var ac = ply.ActiveChild;
		if ( ac == null ) return null;
		if ( Drop( ac ) )
		{
			ply.ActiveChild = null;
			return ac;
		}

		return null;
	}

	public virtual bool DropAll()
	{
		if ( !Host.IsServer ) return false;

		var ply = Owner as Player;

		ply.ActiveChild = null;

		for ( int i = 0; i < List.Count; i++ )
		{
			var wep = List[i];

			Drop( wep );
		}

		return true;
	}

	public override bool Drop( Entity ent )
	{
		if ( !Host.IsServer )
			return false;

		if ( !Contains( ent ) )
			return false;

		if ( ent is BaseCarriable bc )
		{
			bc.OnCarryDrop( Owner );
		}

		return ent.Parent == null;
	}
}
