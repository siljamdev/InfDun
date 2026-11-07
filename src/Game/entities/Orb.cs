using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Orb : Entity{
	static Sound orbSound;
	
	Animation dissapearAnimation;
	
	public static new void initialize(){
		orbSound = Sound.monoFromAssembly("res.sounds.orb.ogg");
	}
	
	protected override int atlasRow => 10;
	
	public Orb(Vector2i p) : base(p){
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
		sce.p.maxHealth += 1;
		
		sce.sm.play(orbSound, new Vector3(position.X, position.Y, 0f), 3f, 0.8f + (float)sce.randCasual.NextDouble() * 0.5f);
		
		int c = sce.randCasual.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pd.add(new AltarParticle(sce.p.position + new Vector2(0f, 0.5f)));
		}
		
		dissapearAnimation = new Animation(16, 0, 0.15d);
		
		return true;
	}
	
	public bool onNecromancerSteal(Scene sce, Necromancer nec){
		if(dissapearAnimation != null){
			return false;
		}
		if(distance(nec) > 1.5f){
			return false;
		}
		if(nec.isDying){
			return false;
		}
		
		sce.sm.play(orbSound, new Vector3(position.X, position.Y, 0f), 3f, 0.8f + (float)sce.randCasual.NextDouble() * 0.5f);
		
		int c = sce.randCasual.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pd.add(new AltarParticle(sce.p.position + new Vector2(0f, 0.5f)));
		}
		
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
}