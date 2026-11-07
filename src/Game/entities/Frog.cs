using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Frog : Living{
	static new Sound hurtSound;
	static new Sound deathSound;
	static SoundPool ambientSound;
	
	public static new void initialize(){
		hurtSound = Sound.monoFromAssembly("res.sounds.froghurt.ogg");
		deathSound = Sound.monoFromAssembly("res.sounds.frogdeath.ogg");
		ambientSound = new SoundPool(Sound.monoFromAssembly("res.sounds.frogsnd0.ogg"), Sound.monoFromAssembly("res.sounds.frogsnd1.ogg"));
	}
	
	const int keyDistSq = 48*48;
	const int wanderDistSq = 16*16;
	const int fleeDistSq = 8*8;
	
	protected override int atlasRow => 14;
	
	double time;
	double maxTime;
	
	EntityGoal goal;
	
	public bool hasKey = false;
	
	#if DEBUG_TIME
		public override int cat => 9;
	#endif
	
	public Frog(Vector2i p, Random rand) : base(p){
		facingLeft = rand.Next(2) == 0;
		
		health = 2;
		
		maxTime = getRandom(rand, 1d, 25d);
	}
	
	public override void newLevel(Scene sce){
		setGoal(sce);
	}
	
	public override bool doTurn(Scene sce){
		if(goal != null && goal.isFinished(sce)){
			goal = null;
		}
		
		setGoal(sce);
		
		bool f = goal?.act(sce) ?? false;
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(cat);
		#endif
		return f;
	}
	
	void setGoal(Scene sce){
		if(!hasKey && !(goal is FleeGoal)){
			EntityGoal key = StealKeyDiagonalLazyGoal.randomKey(sce, this);
			if(key != null){
				goal = key;
			}
		}
		
		if(goal != null){
			return;
		}
		
		if(hasKey){
			Entity[] possible = sce.entitiesInanimate.Where(n => n is Door).ToArray();
			
			if(possible.Length == 0){
				goal = new HopLazyGoal(this);
				return;
			}
			
			Entity d = possible[sce.randSerious.Next(possible.Length)];
			goal = WanderDiagonalLazyGoal.aroundArea(sce, this, d, wanderDistSq);
		}else{
			goal = new HopLazyGoal(this);
		}
	}
	
	protected override void onDeath(Scene sce, Living s){
		sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 0.4f, 0.6f + (float)sce.randCasual.NextDouble() * 0.6f);
		
		if(s == sce.p){
			sce.p.score -= 30;
		}
		
		if(hasKey){
			hasKey = false;
			sce.entitiesToAdd.Add(new Key(position));
			
			sce.sm.play(Key.keySound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
		}
	}
	
	protected override void onMove(Scene sce){ //No step sound
		
	}
	
	protected override void onHurt(Scene sce, Living s){
		sce.sm.play(hurtSound, new Vector3(position.X, position.Y, 0f), 0.8f, 0.7f + (float)sce.randCasual.NextDouble() * 0.5f);
		
		if(hasKey){
			hasKey = false;
			sce.entitiesToAdd.Add(new Key(position));
			
			sce.sm.play(Key.keySound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
		}
	}
	
	protected override void onAttack(Scene sce, Living s){
		int c = sce.randCasual.Next(5) + 2;
		for(int i = 0; i < c; i++){
			sce.pd.add(new FrogBloodParticle(sce, this));
		}
		
		goal = new FleeDiagonalGoal(this, s, fleeDistSq);
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
		
		time += Engine.instance.dh.deltaTime;
		
		if(time > maxTime){
			time = 0d;
			
			if(hasKey && sce.randCasual.Next(3) != 0){
				sce.sm.play(Key.keySound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 0.5f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
				sce.pd.add(new KeyParticle(position + new Vector2(0f, 0.25f)));
				sce.pd.add(new KeyParticle(position + new Vector2(0f, 0.25f)));
				sce.pd.add(new KeyParticle(position + new Vector2(0f, 0.25f)));
				sce.pd.add(new KeyParticle(position + new Vector2(0f, 0.25f)));
				sce.pd.add(new KeyParticle(position + new Vector2(0f, 0.25f)));
			}else{
				sce.sm.play(ambientSound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 0.4f, 0.8f + (float)sce.randCasual.NextDouble() * 0.5f, 6f);
			}
			
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
	
	#if DEBUG_ENTITIES
		protected override void drawDebug(Scene sce){
			if(goal != null){
				Engine.instance.fd.draw(sce, goal.ToString(), position.X - Renderer.sceneTextSize.X / 2f, position.Y + 0.5f, Renderer.sceneTextSize, Renderer.textColor, 0.65f);
			}
			goal?.drawDebug(sce);
		}
	#endif
}