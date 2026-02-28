using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Magic : Entity{
	static Sound magicSound;
	
	Animation idleAnimation = new Animation(2, 0, 0.1d);
	
	protected override int atlasColumn => 13;
	
	public Magic(Vector2i p, Living t2) : base(p){
		doCollision = false;
		
		oldPosition = position;
		position = t2.position;
		state = EntityState.moving;
	}
	
	public override void endSmooth(Scene sce){
		sce.entitiesToRemove.Add(this);
		base.endSmooth(sce);
	}
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		idleAnimation.tick(Dungeon.dh.deltaTime);
	}
	
	public override int getAnimationFrame(Scene sce){
		return idleAnimation.frame;
	}
}