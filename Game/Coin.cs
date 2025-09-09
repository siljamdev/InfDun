using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Coin : Entity{
	static Sound coinSound;
	
	Animation idleAnimation = new Animation(8, 0, 0.1d);
	Animation dissapearAnimation;
	
	protected override int atlasColumn => 8;
	
	public static void initialize(){
		coinSound = Sound.monoFromAssembly("res.sounds.coin.ogg");
	}
	
	public Coin(Vector2i p) : base(p){
		doCollision = false;
	}
	
	public override bool onClick(Scene sce){
		if(dissapearAnimation != null){
			return false;
		}
		if(distance(sce.p) > 1.5f){
			return false;
		}
		sce.p.score += 5;
		
		sce.sm.play(coinSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.rand.NextDouble() * 0.7f);
		
		dissapearAnimation = new Animation(16, 0, 0.15d);
		
		return true;
	}
	
	public override float getAlpha() => (dissapearAnimation != null && dissapearAnimation.frame == 0) ? (float) (1d - dissapearAnimation.time/0.15d) : 1f;
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		idleAnimation.tick(Dungeon.dh.deltaTime);
		
		if(dissapearAnimation != null){
			dissapearAnimation.tick(Dungeon.dh.deltaTime);
			if(dissapearAnimation.frame > 0){
				sce.entitiesToRemove.Add(this);
			}
		}
		
		if(distance(sce.p) == 0f){
			onClick(sce);
		}
	}
	
	public override int getAnimationFrame(Scene sce){
		return idleAnimation.frame;
	}
}