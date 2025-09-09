using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class DripGenerator : Entity{
	static SoundPool dripSound;
	
	public static void initialize(){
		dripSound = new SoundPool(Sound.monoFromAssembly("res.sounds.drip0.ogg"), Sound.monoFromAssembly("res.sounds.drip1.ogg"), Sound.monoFromAssembly("res.sounds.drip2.ogg"), Sound.monoFromAssembly("res.sounds.drip3.ogg"));
	}
	
	double time;
	double maxTime;
	
	bool gen;
	
	public DripGenerator(Vector2i p) : base(p){
		doCollision = false;
	}
	
	public override void draw(Scene sce){
		time += Dungeon.dh.deltaTime;
		if(time > maxTime + 0.5d){
			time = 0d;
			gen = false;
			
			sce.sm.play(dripSound.get(sce.rand), new Vector3(position.X, position.Y, 0f), 1f, 0.8f + (float)sce.rand.NextDouble() * 0.4f);
			
			int c = sce.rand.Next(8) + 4;
			for(int i = 0; i < c; i++){
				sce.pr.add(new SplashParticle(position));
			}
			
			maxTime = getRandom(sce, 3d, 10d);
		}else if(!gen && time > maxTime){
			gen = true;
			sce.pr.add(new DripParticle(position));
		}
		
		/* AABB b = new AABB(position.Y + 1d, position.Y, position.X - 0.5d, position.X + 0.5d);
		
		b.drawWorld(); */
	}
}