using Sandbox;

partial class RealityPlayer
{
	PawnController lastController;

	public void BecomePlayer()
	{
		if ( !IsServer || Ragdoll == null ) return;

		Position = Ragdoll.Position;
		Velocity = default;

		var forward_reference = Ragdoll.GetAttachment( "forward_reference" );

		if ( forward_reference != null )
		{
			var ang = forward_reference.Value.Rotation.Angles();

			Rotation = Rotation.From( new Angles( 0, ang.yaw, 0 ) );
		}

		Controller = lastController;
		EnableAllCollisions = true;
		EnableDrawing = true;

		Ragdoll.Delete();
	}

	public void BecomeRagdoll()
	{
		BecomeRagdoll( Velocity );
	}

	public void BecomeRagdoll( Vector3 velocity, bool playsound )
	{
		BecomeRagdoll( velocity, default, default, default, -1, playsound );
	}

	public void BecomeRagdoll( Vector3 velocity, DamageFlags damageFlags = default, Vector3 forcePos = default, Vector3 force = default, int bone = -1, bool playsound = false )
	{
		if ( !IsServer || Ragdoll != null ) return;

		lastController = Controller;
		Controller = null;
		EnableAllCollisions = false;
		EnableDrawing = false;

		if ( Vehicle.IsValid() )
			velocity += Vehicle.Velocity * Vector3.Up * 5000;

		if ( forcePos.IsNaN )
			forcePos = Position;

		if ( force.IsNaN )
			force = velocity;

		var ent = new Ragdoll();
		ent.Position = Position;
		ent.Rotation = Rotation;
		ent.Scale = Scale;
		ent.MoveType = MoveType.Physics;
		ent.UsePhysicsCollision = true;
		ent.EnableAllCollisions = true;
		ent.CollisionGroup = CollisionGroup.Player;
		ent.SetModel( GetModelName() );
		ent.SetPlayer( this );
		ent.CopyBonesFrom( this );
		ent.CopyBodyGroups( this );
		ent.CopyMaterialGroup( this );
		ent.TakeDecalsFrom( this );
		ent.EnableHitboxes = true;
		ent.EnableAllCollisions = true;
		ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
		ent.RenderColor = RenderColor;
		ent.PhysicsGroup.Velocity = velocity;

		foreach ( var child in Children )
		{
			if ( !child.Tags.Has( "clothes" ) ) continue;
			if ( child is not ModelEntity e ) continue;

			var model = e.GetModelName();

			var clothing = new ModelEntity();
			clothing.SetModel( model );
			clothing.SetParent( ent, true );
			clothing.RenderColor = e.RenderColor;
			clothing.CopyBodyGroups( e );
			clothing.CopyMaterialGroup( e );
		}

		if ( damageFlags.HasFlag( DamageFlags.Bullet ) ||
			 damageFlags.HasFlag( DamageFlags.PhysicsImpact ) )
		{
			PhysicsBody body = bone > 0 ? ent.GetBonePhysicsBody( bone ) : null;

			if ( body != null )
			{
				body.ApplyImpulseAt( forcePos, force * body.Mass );
			}
			else
			{
				ent.PhysicsGroup.ApplyImpulse( force );
			}
		}

		if ( damageFlags.HasFlag( DamageFlags.Blast ) )
		{
			if ( ent.PhysicsGroup != null )
			{
				ent.PhysicsGroup.AddVelocity( (Position - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f) );
				var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
				ent.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
			}
		}

		Ragdoll = ent;

		if ( playsound )
			Ragdoll.PainSound();
	}
}
