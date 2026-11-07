using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class DripGenerator : Inanimate{
	static SoundPool dripSound;
	
	public static new void initialize(){
		dripSound = new SoundPool(Sound.monoFromAssembly("res.sounds.drip0.ogg"), Sound.monoFromAssembly("res.sounds.drip1.ogg"), Sound.monoFromAssembly("res.sounds.drip2.ogg"), Sound.monoFromAssembly("res.sounds.drip3.ogg"));
	}
	
	double time;
	double maxTime;
	
	bool gen;
	
	public DripGenerator(Vector2i p, Random rand) : base(p){
		doCollision = false;
		
		maxTime = getRandom(rand, 0d, 13d);
	}
	
	public override void draw(Scene sce){
		time += Engine.instance.dh.deltaTime;
		if(time > maxTime + 0.5d){
			time = 0d;
			gen = false;
			
			sce.sm.play(dripSound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 1f, 0.8f + (float)sce.randCasual.NextDouble() * 0.4f, 2.5f);
			
			int c = sce.randCasual.Next(8) + 4;
			for(int i = 0; i < c; i++){
				sce.pd.add(new SplashParticle(position));
			}
			
			maxTime = getRandomCasual(sce, 3d, 13d);
		}else if(!gen && time > maxTime){
			gen = true;
			sce.pd.add(new DripParticle(position));
		}
		
		//if(time > maxTime){
		//	AABB b = new AABB(position.Y + 1d, position.Y, position.X - 0.5d, position.X + 0.5d);
		//	b.drawWorld();
		//}
	}
}