using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class SmokeGenerator : Inanimate{	
	double time;
	double maxTime;
	
	public SmokeGenerator(Vector2i p, Random rand) : base(p){
		doCollision = false;
		
		maxTime = getRandom(rand, 0d, 1.6d);
	}
	
	public override void draw(Scene sce){
		time += Dungeon.dh.deltaTime;
		if(time > maxTime){
			time = 0d;
			
			int c = sce.randCasual.Next(3) + 1;
			for(int i = 0; i < c; i++){
				sce.pr.add(new SmokeParticle(position));
			}
			
			maxTime = getRandomCasual(sce, 0.3f, 1.8d);
		}
	}
}