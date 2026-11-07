using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Key : Entity{
	public static SoundPool keySound;
	
	Animation dissapearAnimation;
	
	protected override int atlasRow => 15;
	
	double time;
	double maxTime;
	
	public static new void initialize(){
		keySound = new SoundPool(Sound.monoFromAssembly("res.sounds.key0.ogg"), Sound.monoFromAssembly("res.sounds.key1.ogg"));
	}
	
	public Key(Vector2i p) : base(p){
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
		sce.p.hasKey = true;
		
		sce.sm.play(keySound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 1f, 0.8f + (float)sce.randCasual.NextDouble() * 0.7f);
		
		dissapearAnimation = new Animation(16, 0, 0.15d);
		
		return true;
	}
	
	public bool onFrogSteal(Scene sce, Frog fro){
		if(dissapearAnimation != null){
			return false;
		}
		if(distance(fro) > 0.5f){
			return false;
		}
		if(fro.isDying){
			return false;
		}
		
		sce.sm.play(keySound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 1f, 0.8f + (float)sce.randCasual.NextDouble() * 0.7f);
		
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
		}else{
			time += Engine.instance.dh.deltaTime;
			if(time > maxTime){
				time = 0d;
				
				sce.pd.add(new KeyParticle(position + new Vector2(0f, 0.5f)));
				
				maxTime = getRandomCasual(sce, 1.2d, 6d);
			}
			
			if(distance(sce.p) == 0f){
				playerInteract(sce);
			}
		}
	}
	
	public override int getAnimationFrame(Scene sce){
		return sce.coinAnimation.frame;
	}
}