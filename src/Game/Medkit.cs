using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Medkit : Entity{
	static Sound medkitSound;
	
	Animation dissapearAnimation;
	
	protected override int atlasColumn => 9;
	
	public static void initialize(){
		medkitSound = Sound.monoFromAssembly("res.sounds.medkit.ogg");
	}
	
	bool isBlue;
	
	public Medkit(Vector2i p, Random rand) : base(p){
		doCollision = false;
		
		isBlue = rand.Next(5) == 0;
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
		if(!sce.p.cure(isBlue ? 2 : 1)){
			return false;
		}
		
		sce.sm.play(medkitSound, new Vector3(position.X, position.Y, 0f), 1f, 0.8f + (float)sce.rand.NextDouble() * 0.5f);
		
		int c = sce.rand.Next(5) + 2;
		for(int i = 0; i < c; i++){
			sce.pr.add(new MedkitParticle(position + new Vector2d(0d, 0.5d), isBlue));
		}
		
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
		return isBlue ? 1 : 0;
	}
}