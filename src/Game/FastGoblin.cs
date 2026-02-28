using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class FastGoblin : Living{	
	const int maxDistSq = 16*16;
	const int wanderDistSq = 48*48;
	
	protected override int atlasColumn => 3;
	
	Living target = null;
	//Vector2i? posTarget = null;
	
	#if DEBUG_TIME
		const int cat = 1;
	#endif
	
	public FastGoblin(Vector2i p, Random rand) : base(p){
		facingLeft = rand.Next(2) == 0;
	}
	
	public override void newLevel(Scene sce){
		//Set target
		setTar(sce);
		if(target == null){
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
		
		//Set target
		if(target == null){
			setTar(sce);
		}
		
		//Attack
		if(attack(1, target, sce)){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return true;
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
		Entity[] possible = sce.entities.Where(n => (n != this && n is Player)).Where(n => n.distanceSquared(position) < maxDistSq).ToArray();
		
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
	
	/* public override bool doTurn(Scene sce){
		if(target != null){
			if(target.isDying || target.distanceSquared(position) > 256f){ //Dist 16, its squared
				target = null;
			}
		}else if(posTarget != null && posTarget == position){
			posTarget = null;
		}
		
		if(target == null){
			setTarget(sce);
		}
		
		if(attack(1, target, sce)){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(1);
			#endif
			return true;
		}
		
		if(target != null){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(1);
			#endif
			Vector2i? x2 = Living.findFirstMove(position, target.position, sce);
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(6);
			#endif
			
			if(x2 != null && tryMove((Vector2i) x2, sce)){
				#if DEBUG_TIME
					Scene.tt.CategoryEnd(1);
				#endif
				return true;
			}
		}
		
		if(posTarget != null){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(1);
			#endif
			Vector2i? x2 = Living.findFirstMove(position, (Vector2i) posTarget, sce);
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(6);
			#endif
			
			if(x2 != null && tryMove((Vector2i) x2, sce)){
				#if DEBUG_TIME
					Scene.tt.CategoryEnd(1);
				#endif
				return true;
			}
		}
		
		setPosTarget(sce);
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(1);
		#endif
		return false;
	} */
	
	protected override void onDeath(Scene sce, Living s){
		base.onDeath(sce, s);
		if(s == sce.p){
			sce.p.score += 20;
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