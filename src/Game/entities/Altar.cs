using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Altar : Inanimate{
	static Sound altarSound;
	
	public static new void initialize(){
		altarSound = Sound.monoFromAssembly("res.sounds.altar.ogg");
		BefriendingAltar.initialize(altarSound); //Avoid double sound
	}
	
	protected override int atlasRow => 11;
	
	double time;
	double maxTime;
	
	public Altar(Vector2i p) : base(p){
		
	}
	
	public override bool canPlayerInteract(Scene sce){
		if(distance(sce.p) > 1.5f){
			return false;
		}
		if(sce.p.health < 2){
			return false;
		}
		return true;
	}
	
	protected override bool doPlayerInteract(Scene sce){
		sce.p.maxHealth += 1;
		sce.p.damage(1, null, sce);
		
		sce.sm.play(altarSound, new Vector3(position.X, position.Y, 0f), 0.9f, 0.6f + (float)sce.randCasual.NextDouble() * 0.8f);
		
		int c = sce.randCasual.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pd.add(new AltarParticle(position + new Vector2(0f, 0.5f)));
		}
		
		c = sce.randCasual.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pd.add(new AltarParticle(sce.p.position + new Vector2(0f, 0.5f)));
		}
		
		return true;
	}
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		time += Engine.instance.dh.deltaTime;
		if(time > maxTime){
			time = 0d;
			
			sce.pd.add(new AltarParticle(position + new Vector2(0f, 0.5f)));
			
			maxTime = getRandomCasual(sce, 0.8d, 4d);
		}
	}
}