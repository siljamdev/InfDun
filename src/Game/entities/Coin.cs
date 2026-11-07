using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Coin : Entity{
	public static Sound coinSound {get; private set;}
	
	Animation dissapearAnimation;
	
	protected override int atlasRow => 8;
	
	public static new void initialize(){
		coinSound = Sound.monoFromAssembly("res.sounds.coin.ogg");
	}
	
	public Coin(Vector2i p) : base(p){
		doCollision = false;
	}
	
	public override bool canPlayerInteract(Scene sce){
		if(dissapearAnimation != null){
			return false;
		}
		if(distance(sce.p) > 1.5f){
			return false;
		}
		if(sce.p.isDying){
			return false;
		}
		return true;
	}
	
	protected override bool doPlayerInteract(Scene sce){
		sce.p.score += 5;
		
		sce.sm.play(coinSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
		
		dissapearAnimation = new Animation(16, 0, 0.15d);
		
		return true;
	}
	
	public bool onGoblinSteal(Scene sce, Goblin gob){
		if(dissapearAnimation != null){
			return false;
		}
		if(distance(gob) > 1.5f){
			return false;
		}
		if(gob.isDying){
			return false;
		}
		
		sce.sm.play(coinSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
		
		dissapearAnimation = new Animation(16, 0, 0.15d);
		
		return true;
	}
	
	public override float getAlpha() => (dissapearAnimation != null && dissapearAnimation.frame == 0) ? (float) (1d - dissapearAnimation.time/0.15d) : 1f;
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		if(dissapearAnimation != null){
			dissapearAnimation.tick(Engine.instance.dh.deltaTime);
			if(dissapearAnimation.frame > 0){
				sce.entitiesToRemove.Add(this);
			}
		}
		
		if(distance(sce.p) == 0f){
			playerInteract(sce);
		}
	}
	
	public override int getAnimationFrame(Scene sce){
		return sce.coinAnimation.frame;
	}
}