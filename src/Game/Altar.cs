using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Altar : Entity{
	static Sound altarSound;
	
	public static void initialize(){
		altarSound = Sound.monoFromAssembly("res.sounds.altar.ogg");
	}
	
	protected override int atlasColumn => 11;
	
	double time;
	double maxTime;
	
	public Altar(Vector2i p) : base(p){
		
	}
	
	public override bool onClick(Scene sce){
		if(distance(sce.p) > 1.5f){
			return false;
		}
		if(sce.p.health < 2){
			return false;
		}
		sce.p.maxHealth += 1;
		sce.p.damage(1, null, sce);
		
		sce.sm.play(altarSound, new Vector3(position.X, position.Y, 0f), 0.9f, 0.6f + (float)sce.rand.NextDouble() * 0.8f);
		
		int c = sce.rand.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pr.add(new AltarParticle(position + new Vector2d(0d, 0.5d)));
		}
		
		c = sce.rand.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pr.add(new AltarParticle(sce.p.position + new Vector2d(0d, 0.5d)));
		}
		
		return true;
	}
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		time += Dungeon.dh.deltaTime;
		if(time > maxTime){
			time = 0d;
			
			sce.pr.add(new AltarParticle(position + new Vector2d(0d, 0.5d)));
			
			maxTime = getRandom(sce, 0.8d, 4d);
		}
	}
}