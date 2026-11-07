using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Necromancer : Living{
	static new Sound hurtSound;
	static new Sound deathSound;
	static Sound magicSound;
	static Sound whisperSound;
	static Sound healSound;
	
	//Possible skeletons relative positions
	static readonly Vector2i[] offsets = new Vector2i[]{
		new Vector2i(-2, -2), new Vector2i(-1, -2), new Vector2i(0, -2), new Vector2i(1, -2), new Vector2i(2, -2),
		new Vector2i(-2, -1),                                                                 new Vector2i(2, -1),
		new Vector2i(-2,  0),                                                                 new Vector2i(2,  0),
		new Vector2i(-2,  1),                                                                 new Vector2i(2,  1),
		new Vector2i(-2,  2), new Vector2i(-1,  2), new Vector2i(0,  2), new Vector2i(1,  2), new Vector2i(2,  2)
	};
	
	public static int minionLimit = 1;
	
	public static new void initialize(){
		hurtSound = Sound.monoFromAssembly("res.sounds.necrohurt.ogg");
		deathSound = Sound.monoFromAssembly("res.sounds.necrodeath.ogg");
		magicSound = Sound.monoFromAssembly("res.sounds.magic.ogg");
		whisperSound = Sound.monoFromAssembly("res.sounds.necrosnd.ogg");
		healSound = Sound.monoFromAssembly("res.sounds.necroheal.ogg");
	}
	
	const int maxDistSq = 16*16;
	const int wanderDistSq = 48*48;
	const int fleeDistSq = 2*2;
	const int maxFleeDistSq = 7*7;
	const int attackDistSq = 6*6;
	
	//const int minionCounterMax = 7;
	
	protected override int atlasRow => 5;
	
	//Turn counter to spawn minion
	//int minionCounter;
	List<Skeleton> minions = new();
	
	//Whisper sounds
	double time;
	double maxTime;
	
	//Healing
	int healCounter = 0;
	const int healEveryXTurns = 12;
	
	EntityGoal goal;
	
	#if DEBUG_TIME
		public override int cat => 3;
	#endif
	
	public Necromancer(Vector2i p, Random rand) : base(p){
		facingLeft = rand.Next(2) == 0;
		
		maxHealth = 4;
		health = maxHealth;
		
		time = 0d;
		maxTime = getRandom(rand, 1d, 30d);
	}
	
	public override void newLevel(Scene sce){
		setGoal(sce);
	}
	
	public override bool doTurn(Scene sce){
		//Remove all dead minions
		minions.RemoveAll(n => n.isDying);
		
		if(goal != null && goal.isFinished(sce)){
			goal = null;
		}
		
		setGoal(sce);
		
		//Wait a tick after doing something
		if(state == EntityState.action){
			state = EntityState.idle;
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return true;
		}
		
		if(heal(sce)){
			return true;
		}
		
		if(canCure(1)){
			healCounter--;
		}
		
		bool f = goal?.act(sce) ?? false;
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(cat);
		#endif
		return f;
		
		//Invoke new minion
		//if(minionCounter > minionCounterMax && minions.Count < minionLimit){
		//	minionCounter = 0;
		//	
		//	state = EntityState.action;
		//	
		//	addSkeleton(sce, findPosition(sce));
		//	
		//	#if DEBUG_TIME
		//		Scene.tt.CategoryEnd(cat);
		//	#endif
		//	return true;
		//}else if(minions.Count < minionLimit){
		//	minionCounter++;
		//}else{
		//	minionCounter = 0;
		//}
	}
	
	void setGoal(Scene sce){
		if(goal is FleeGoal){
			return;
		}
		
		if(!(goal is MagicAttackGoal)){
			Living a = chooseEntity(sce);
				if(a != null){
				goal = new MagicAttackGoal(this, a, maxDistSq);
				
				foreach(Skeleton f in minions){ //Skellys also attack same guy
					f.masterAttacked(a);
				}
				return;
			}
		}
		
		goal ??= StealOrbGoal.randomOrb(sce, this);
		goal ??= WanderGoal.aroundEntity(sce, this, wanderDistSq);
	}
	
	Living chooseEntity(Scene sce){
		Entity[] near = sce.entitiesLiving.Where(n => n.distanceSquared(position) < maxDistSq).ToArray();
		
		Entity[] players = near.Where(n => n is Player).ToArray();
		Entity[] goblins = near.Where(n => n is Goblin || n is FastGoblin || n is GoblinFriend).ToArray();
		
		Entity[] possible = players.Length > 0 ? players : goblins;
		
		if(possible.Length == 0){
			return null;
		}
		
		Living x2 = possible[sce.randSerious.Next(possible.Length)] as Living;
		return x2;
	}
	
	bool heal(Scene sce){
		if(healCounter < 1 && !(goal is FleeGoal) && canCure(1)){
			healCounter = healEveryXTurns;
			cure(1);
			
			state = EntityState.action;
			
			int c = sce.randCasual.Next(4) + 3;
			for(int i = 0; i < c; i++){
				sce.pd.add(new MagicParticle(getParticlePos()));
			}
			
			sce.sm.play(healSound, new Vector3(position.X, position.Y, 0f), 0.5f, 0.5f + (float)sce.randCasual.NextDouble() * 0.7f, 1.5f);
			
			return true;
		}
		return false;
	}
	
	public bool attackMagic(Scene sce, Living target){
		if(target.distanceSquared(this) < attackDistSq){
			if(target.position.X > position.X){
				facingLeft = false;
			}else if(target.position.X > position.X){
				facingLeft = true;
			}
			
			state = EntityState.action;
			sce.entitiesToAdd.Add(new Magic(position, target));
			
			target.damage(1, this, sce);
			sce.sm.play(magicSound, new Vector3(target.position.X, target.position.Y, 0f), 1f, 0.5f + (float)sce.randCasual.NextDouble() * 0.8f);
			
			int c = sce.randCasual.Next(3) + 1;
			for(int i = 0; i < c; i++){
				sce.pd.add(new MagicParticle(getParticlePos()));
			}
			
			c = sce.randCasual.Next(3) + 1;
			for(int i = 0; i < c; i++){
				sce.pd.add(new MagicParticle(target.position + new Vector2(0f, 0.5f)));
			}
			
			return true;
		}
		return false;
	}
	
	public void addSkeleton(Scene sce, Vector2i pos, bool? lef = null){
		if(minions.Count >= minionLimit){
			return;
		}
		
		Skeleton s = new Skeleton(pos, this, lef, sce);
		minions.Add(s);
		sce.entitiesToAdd.Add(s);
		
		int c = sce.randCasual.Next(3) + 1;
		for(int i = 0; i < c; i++){
			sce.pd.add(new MagicParticle(getParticlePos()));
		}
		
		c = sce.randCasual.Next(3) + 1;
		for(int i = 0; i < c; i++){
			sce.pd.add(new MagicParticle(s.position + new Vector2(0f, 0.5f)));
		}
	}
	
	//Slightly better positioning
	Vector2 getParticlePos(){
		if(facingLeft){
			return position + new Vector2(-0.4f, 0.5f);
		}else{
			return position + new Vector2(0.4f, 0.5f);
		}
	}
	
	protected override void onDeath(Scene sce, Living s){
		sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
		if(s == sce.p){
			sce.p.score += 50;
		}
		
		int c = sce.randCasual.Next(8) + 4;
		for(int i = 0; i < c; i++){
			sce.pd.add(new MagicParticle(position + new Vector2(0f, 0.7f)));
		}
	}
	
	protected override void onMove(Scene sce){ //No step sound
		
	}
	
	protected override void onHurt(Scene sce, Living s){
		sce.sm.play(hurtSound, new Vector3(position.X, position.Y, 0f), 1f, 0.5f + (float)sce.randCasual.NextDouble() * 0.8f);
	}
	
	protected override void onAttack(Scene sce, Living s){
		goal = new FleeGoal(this, s, maxFleeDistSq);
		
		foreach(Skeleton f in minions){
			f.masterAttacked(s);
		}
	}
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		time += Engine.instance.dh.deltaTime;
		
		if(time > maxTime){
			time = 0d;
			
			sce.sm.play(whisperSound, new Vector3(position.X, position.Y, 0f), 0.08f, 0.8f + (float)sce.randCasual.NextDouble() * 0.5f, 6f);
			
			maxTime = getRandomCasual(sce, 7d, 30d);
		}
	}
	
	public override int getAnimationFrame(Scene sce){
		if(isDying){
			return dyingAnimation?.frame ?? 4;
		}
		
		if(state == EntityState.idle){
			return sce.idleAnimation.frame;
		}else if(state == EntityState.moving){
			return sce.runningAnimation.frame;
		}else if(state == EntityState.action){
			return sce.idleAnimation.frame + 6;
		}
		
		return 0;
	}
	
	//Finding a suitable position for skellys to spawn
	//public Vector2i findPosition(Scene sce){
	//	Vector2i[] possible = offsets.Select(n => n + position).Where(n => sce.isAvailableToMove(n)).ToArray();
	//	
	//	if(possible.Length == 0){
	//		return position;
	//	}
	//	
	//	return possible[sce.rand.Next(possible.Length)];
	//}
	
	#if DEBUG_ENTITIES
		protected override void drawDebug(Scene sce){
			if(goal != null){
				Engine.instance.fd.draw(sce, goal.ToString(), position.X - Renderer.sceneTextSize.X / 2f, position.Y + 0.5f, Renderer.sceneTextSize, Renderer.textColor, 0.65f);
			}
			Engine.instance.fd.draw(sce, health.ToString() + "/" + maxHealth.ToString(), position.X - Renderer.sceneTextSize.X / 2f, position.Y - 0.5f, Renderer.sceneTextSize, Renderer.textColor, 0.65f);
			goal?.drawDebug(sce);
		}
	#endif
}