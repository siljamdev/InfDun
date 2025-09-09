using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class BefriendingAltar : Entity{
	static Sound altarSound;
	
	public static void initialize(){
		altarSound = Sound.monoFromAssembly("res.sounds.altar.ogg");
	}
	
	protected override int atlasColumn => 12;
	
	bool active;
	
	double time;
	double maxTime;
	
	public BefriendingAltar(Vector2i p) : base(p){
		active = true;
	}
	
	public override bool onClick(Scene sce){
		if(!active){
			return false;
		}
		
		if(distance(sce.p) > 1.5f){
			return false;
		}
		
		Entity[] possible = sce.entities.Where(n => n is Goblin).OrderBy(n => n.distance(this)).ToArray();
		if(possible.Length == 0){
			return false;
		}
		Goblin b = (Goblin) possible[0];
		
		b.befriend(sce);
		
		active = false; //Single use
		
		sce.sm.play(altarSound, new Vector3(position.X, position.Y, 0f), 0.9f, 0.6f + (float)sce.rand.NextDouble() * 0.8f);
		
		int c = sce.rand.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pr.add(new FriendParticle(position + new Vector2d(0d, 0.5d)));
		}
		
		return true;
	}
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		if(active){
			time += Dungeon.dh.deltaTime;
			if(time > maxTime){
				time = 0d;
				
				sce.pr.add(new FriendParticle(position + new Vector2d(0d, 0.5d)));
				
				maxTime = getRandom(sce, 0.8d, 4d);
			}
		}
	}
	
	public override int getAnimationFrame(Scene sce){
		return active ? 0 : 1;
	}
}