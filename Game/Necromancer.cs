using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Necromancer : Living{
	static Sound hurtSound;
	static Sound deathSound;
	static Sound magicSound;
	
	static readonly Vector2i[] offsets = new Vector2i[]{
		new Vector2i(-2, -2), new Vector2i(-1, -2), new Vector2i(0, -2), new Vector2i(1, -2), new Vector2i(2, -2),
		new Vector2i(-2, -1), new Vector2i(-1, -1), new Vector2i(0, -1), new Vector2i(1, -1), new Vector2i(2, -1),
		new Vector2i(-2,  0), new Vector2i(-1,  0),                      new Vector2i(1,  0), new Vector2i(2,  0),
		new Vector2i(-2,  1), new Vector2i(-1,  1), new Vector2i(0,  1), new Vector2i(1,  1), new Vector2i(2,  1),
		new Vector2i(-2,  2), new Vector2i(-1,  2), new Vector2i(0,  2), new Vector2i(1,  2), new Vector2i(2,  2)
	};
	
	public static int minionLimit = 1;
	
	public static void initialize(){
		hurtSound = Sound.monoFromAssembly("res.sounds.necrohurt.ogg");
		deathSound = Sound.monoFromAssembly("res.sounds.necrodeath.ogg");
		magicSound = Sound.monoFromAssembly("res.sounds.magic.ogg");
	}
	
	const int maxDistSq = 16*16;
	const int wanderDistSq = 48*48;
	
	const int minionCounterMax = 7;
	
	protected override int atlasColumn => 5;
	
	int minionCounter;
	List<Living> minions = new();
	
	public Living target {get; private set;} = null;
	public Vector2i? posTar => posTarget;
	
	#if DEBUG_TIME
		const int cat = 3;
	#endif
	
	public Necromancer(Vector2i p, Random rand) : base(p){
		facingLeft = rand.Next(2) == 0;
		
		health = 4;
	}
	
	public override void newLevel(Scene sce){
		//Set target
		setTar(sce);
		if(target == null){
			setPosTar(sce);
		}
	}
	
	public override bool doTurn(Scene sce){
		//Remove all dead minions
		minions.RemoveAll(n => n.isDying);
		
		//Reset
		if(target != null){
			if(target.isDying || target.distanceSquared(position) > maxDistSq){
				target = null;
			}
		}else if(posTarget == position){
			setPosTarget(sce, null);
		}
		
		//Set target
		if(target == null){
			setTar(sce);
		}
		
		//Wait a tick after doing something
		if(state == EntityState.action){
			state = EntityState.idle;
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return true;
		}
		
		//Atack
		if(target != null && target.distanceSquared(position) < 25f){ //Dist 5
			if(target.position.X > position.X){
				facingLeft = false;
			}else if(target.position.X > position.X){
				facingLeft = true;
			}
			
			state = EntityState.action;
			sce.entitiesToAdd.Add(new Magic(position, target));
			
			target.damage(1, this, sce);
			sce.sm.play(magicSound, new Vector3(target.position.X, target.position.Y, 0f), 1f, 0.5f + (float)sce.rand.NextDouble() * 0.8f);
			
			int c = sce.rand.Next(3) + 1;
			for(int i = 0; i < c; i++){
				sce.pr.add(new MagicParticle(getParticlePos()));
			}
			
			c = sce.rand.Next(3) + 1;
			for(int i = 0; i < c; i++){
				sce.pr.add(new MagicParticle(target.position + new Vector2d(0d, 0.5d)));
			}
			
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return true;
		}
		
		//Invoke new minion
		if(minionCounter > minionCounterMax && minions.Count < minionLimit){
			minionCounter = 0;
			
			state = EntityState.action;
			
			addSkeleton(sce, findPosition(sce));
			
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return true;
		}else if(minions.Count < minionLimit){
			minionCounter++;
		}else{
			minionCounter = 0;
		}
		
		//Pathfind
		if(target != null){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			Vector2i? x2 = Living.findFirstMove(position, target.position, sce);
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(6);
			#endif
			
			if(tryMove(x2, sce)){
				#if DEBUG_TIME
					Scene.tt.CategoryEnd(cat);
				#endif
				return true;
			}
		}else if(posTarget != null){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			Vector2i? x2 = getNextMove(sce);
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(7);
			#endif
			
			if(tryMove(x2, sce)){
				#if DEBUG_TIME
					Scene.tt.CategoryEnd(cat);
				#endif
				return true;
			}
		}
		
		//Set pos target
		setPosTar(sce);
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(cat);
		#endif
		return false;
	}
	
	void setTar(Scene sce){
		Entity[] near = sce.entities.Where(n => n.distanceSquared(position) < maxDistSq).ToArray();
		
		Entity[] players = near.Where(n => n is Player).ToArray();
		Entity[] goblins = near.Where(n => n is Goblin || n is FastGoblin || n is GoblinFriend).ToArray();
		
		Entity[] possible = players.Length > 0 ? players : goblins;
		
		if(possible.Length == 0){
			target = null;
			return;
		}
		
		Living x2 = possible[sce.rand.Next(possible.Length)] as Living;
		
		target = x2;
		setPosTarget(sce, null);
	}
	
	void setPosTar(Scene sce){
		int[] posibleTiles = sce.transitable.Select((v, i) => (v, i)).Where(n => n.Item1).Select(n => n.Item2).
		Where(n => this.distanceSquared(new Vector2i(n % sce.mapXsize, n / sce.mapXsize)) < wanderDistSq).ToArray();
		
		int x2 = posibleTiles[sce.rand.Next(posibleTiles.Length)];
		
		setPosTarget(sce, new Vector2i(x2 % sce.mapXsize, x2 / sce.mapXsize));
		target = null;
	}
	
	public void addSkeleton(Scene sce, Vector2i pos, bool? lef = null){
		if(minions.Count > minionLimit){
			return;
		}
		
		Living s = new Skeleton(pos, this, lef, sce);
		minions.Add(s);
		sce.entitiesToAdd.Add(s);
		
		int c = sce.rand.Next(3) + 1;
		for(int i = 0; i < c; i++){
			sce.pr.add(new MagicParticle(getParticlePos()));
		}
		
		c = sce.rand.Next(3) + 1;
		for(int i = 0; i < c; i++){
			sce.pr.add(new MagicParticle(s.position + new Vector2d(0d, 0.5d)));
		}
	}
	
	//Slightly better positioning
	Vector2d getParticlePos(){
		if(facingLeft){
			return position + new Vector2d(-0.4d, 0.5d);
		}else{
			return position + new Vector2d(0.4d, 0.5d);
		}
	}
	
	protected override void onDeath(Scene sce, Living s){
		sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.rand.NextDouble() * 0.7f);
		if(s == sce.p){
			sce.p.score += 50;
		}
		
		int c = sce.rand.Next(8) + 4;
		for(int i = 0; i < c; i++){
			sce.pr.add(new MagicParticle(position + new Vector2d(0d, 0.7d)));
		}
	}
	
	protected override void onMove(Scene sce){ //No step sound
		
	}
	
	protected override void onHurt(Scene sce, Living s){
		sce.sm.play(hurtSound, new Vector3(position.X, position.Y, 0f), 1f, 0.5f + (float)sce.rand.NextDouble() * 0.8f);
	}
	
	protected override void onAttack(Scene sce, Living s){
		target = s;
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
	public Vector2i findPosition(Scene sce){
		Vector2i[] possible = offsets.Select(n => n + position).Where(n => sce.isAvailableToMove(n)).ToArray();
		
		if(possible.Length == 0){
			return position;
		}
		
		return possible[sce.rand.Next(possible.Length)];
	}
}