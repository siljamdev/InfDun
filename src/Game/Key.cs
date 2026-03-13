using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Key : Entity{
	static SoundPool keySound;
	
	Animation dissapearAnimation;
	
	protected override int atlasRow => 15;
	
	public static void initialize(){
		keySound = new SoundPool(Sound.monoFromAssembly("res.sounds.key0.ogg"), Sound.monoFromAssembly("res.sounds.key1.ogg"));
	}
	
	public Key(Vector2i p) : base(p){
		doCollision = false;
	}
	
	public override bool onClick(Scene sce){
		if(dissapearAnimation != null){
			return false;
		}
		if(distance(sce.p) > 1.5f){
			return false;
		}
		if(sce.p.isDying){
			return false;
		}
		sce.p.hasKey = true;
		
		sce.sm.play(keySound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 1f, 0.8f + (float)sce.randCasual.NextDouble() * 0.7f);
		
		dissapearAnimation = new Animation(16, 0, 0.15d);
		
		return true;
	}
	
	public override float getAlpha() => (dissapearAnimation != null && dissapearAnimation.frame == 0) ? (float) (1d - dissapearAnimation.time/0.15d) : 1f;
	
	public override void draw(Scene sce){
		base.draw(sce);
		
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
		return sce.coinAnimation.frame;
	}
}