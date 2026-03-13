using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Frog : Living{
	static Sound hurtSound;
	static Sound deathSound;
	static SoundPool ambientSound;
	
	public static void initialize(){
		hurtSound = Sound.monoFromAssembly("res.sounds.froghurt.ogg");
		deathSound = Sound.monoFromAssembly("res.sounds.frogdeath.ogg");
		ambientSound = new SoundPool(Sound.monoFromAssembly("res.sounds.frogsnd0.ogg"), Sound.monoFromAssembly("res.sounds.frogsnd1.ogg"));
	}
	
	protected override int atlasRow => 14;
	
	double time;
	double maxTime;
	
	#if DEBUG_TIME
		const int cat = 9;
	#endif
	
	public Frog(Vector2i p, Random rand) : base(p){
		facingLeft = rand.Next(2) == 0;
		
		health = 1;
		
		maxTime = getRandom(rand, 1d, 25d);
	}
	
	public override bool doTurn(Scene sce){
		//Do nothing
		if(sce.randSerious.Next(2) == 0){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return false;
		}
		
		//Move
		Vector2i jumpTarget = position + new Vector2i(sce.randSerious.Next(-1, 2), sce.randSerious.Next(-1, 2));
		if(tryMove(jumpTarget, sce)){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return true;
		}
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(cat);
		#endif
		return false;
	}
	
	protected override void onDeath(Scene sce, Living s){
		sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 0.4f, 0.6f + (float)sce.randCasual.NextDouble() * 0.6f);
		
		if(s == sce.p){
			sce.p.score -= 30;
		}
	}
	
	protected override void onMove(Scene sce){ //No step sound
		
	}
	
	protected override void onHurt(Scene sce, Living s){
		sce.sm.play(hurtSound, new Vector3(position.X, position.Y, 0f), 0.8f, 0.7f + (float)sce.randCasual.NextDouble() * 0.5f);
	}
	
	protected override void onAttack(Scene sce, Living s){
		int c = sce.randCasual.Next(5) + 2;
		for(int i = 0; i < c; i++){
			sce.pr.add(new FrogBloodParticle(sce, this));
		}
	}
	
	//public override void endSmooth(Scene sce){	
	//	if(state == EntityState.moving){
	//		int c = sce.randCasual.Next(4) + 3;
	//		for(int i = 0; i < c; i++){
	//			sce.pr.add(new SplashParticle(position - new Vector2(0f, 0.0f)));
	//		}
	//	}
	//	
	//	base.endSmooth(sce);
	//}
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		time += Dungeon.dh.deltaTime;
		
		if(time > maxTime){
			time = 0d;
			
			sce.sm.play(ambientSound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 0.4f, 0.8f + (float)sce.randCasual.NextDouble() * 0.5f, 6f);
			
			maxTime = getRandomCasual(sce, 6d, 25d);
		}
	}
	
	public override int getAnimationFrame(Scene sce){
		if(isDying){
			return 4;
		}
		
		if(state == EntityState.idle){
			return sce.idleAnimation.frame;
		}else if(state == EntityState.moving){
			return sce.frogAnimationFrame;
		}
		
		return 0;
	}
}