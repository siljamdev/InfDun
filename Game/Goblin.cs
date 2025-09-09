using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Goblin : Living{
	const int maxDistSq = 16*16;
	const int wanderDistSq = 48*48;
	
	int _c;
	protected override int atlasColumn => _c;
	
	//Only one of these will be non null at a time
	Living target = null;
	//Vector2i? posTarget = null;
	
	#if DEBUG_TIME
		const int cat = 0;
	#endif
	
	public Goblin(Vector2i p, Random rand) : base(p){
		_c = 1 + rand.Next(2);
		
		facingLeft = rand.Next(2) == 0;
	}
	
	public override void newLevel(Scene sce){
		//Set target
		if(sce.rand.Next(10) == 0){
			setTar(sce);
		}else if(target == null){
			setPosTar(sce);
		}
	}
	
	public override bool doTurn(Scene sce){
		//Reset
		if(target != null){
			if(target.isDying || target.distanceSquared(position) > maxDistSq){
				target = null;
			}
		}else if(posTarget == position){
			setPosTarget(sce, null);
		}
		
		//Attack
		if(attack(1, target, sce)){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return true;
		}
		
		//Do nothing
		if(sce.rand.Next(2) == 0){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return false;
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
		
		//Set target
		if(sce.rand.Next(10) == 0){
			setTar(sce);
		}else if(target == null){
			setPosTar(sce);
		}
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(cat);
		#endif
		return false;
	}
	
	void setTar(Scene sce){
		Entity[] possible = sce.entities.Where(n => (n != this && n is Living)).Where(n => n.distanceSquared(position) < maxDistSq).ToArray();
		
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
	
	public void befriend(Scene sce){
		isDying = true; //If someone has it as target, delete it
		sce.entitiesToRemove.Add(this);
		sce.entitiesToAdd.Add(new GoblinFriend(position, _c, facingLeft, health, sce));
		
		int c = sce.rand.Next(5) + 3;
		for(int i = 0; i < c; i++){
			sce.pr.add(new FriendParticle(position + new Vector2d(0d, 0.5d)));
		}
	}
	
	protected override void onDeath(Scene sce, Living s){
		base.onDeath(sce, s);
		if(s == sce.p){
			sce.p.score += 10;
		}
		
		if(s is Necromancer n){
			n.addSkeleton(sce, position, facingLeft);
		}
	}
	
	protected override void onAttack(Scene sce, Living s){
		base.onAttack(sce, s);
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
		}
		
		return 0;
	}
}