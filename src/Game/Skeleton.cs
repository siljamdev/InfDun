using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Skeleton : Living{
	static Sound hurtSound;
	static Sound deathSound;
	
	static readonly Vector2i[] offsets = new Vector2i[]{
		new Vector2i(-2, -2), new Vector2i(-1, -2), new Vector2i(0, -2), new Vector2i(1, -2), new Vector2i(2, -2),
		new Vector2i(-2, -1),                                                                 new Vector2i(2, -1),
		new Vector2i(-2,  0),                                                                 new Vector2i(2,  0),
		new Vector2i(-2,  1),                                                                 new Vector2i(2,  1),
		new Vector2i(-2,  2), new Vector2i(-1,  2), new Vector2i(0,  2), new Vector2i(1,  2), new Vector2i(2,  2)
	};
	
	public static void initialize(){
		hurtSound = Sound.monoFromAssembly("res.sounds.skellyhurt.ogg");
		deathSound = Sound.monoFromAssembly("res.sounds.skellydeath.ogg");
	}
	
	const int maxDistSq = 32*32;
	const int wanderDistSq = 48*48;
	
	Vector2i offset;
	
	protected override int atlasColumn => 4;
	
	Living target = null;
	//Vector2i? posTarget = null;
	
	Necromancer master;
	int posCounter;
	
	#if DEBUG_TIME
		const int cat = 2;
	#endif
	
	public Skeleton(Vector2i p, Necromancer o, bool? lef, Scene sce) : base(p){
		master = o;
		if(master != null){
			sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 1f, 0.5f + (float)sce.rand.NextDouble() * 0.5f);
		}
		
		facingLeft = lef ?? sce.rand.Next(2) == 0;
		
		health = 2;
		
		posCounter = -1;
		
		offset = offsets[sce.rand.Next(offsets.Length)];
	}
	
	public override bool doTurn(Scene sce){
		//Reset master
		if(master != null && master.isDying){
			master = null;
			target = null;
			setPosTarget(sce, null);
		}
		
		//Resets
		if(master != null){
			target = master.target;
			if(target == null){
				if(posCounter > 4 || posCounter < 0){
					posCounter++;
					
					setPosTarget(sce, master.position + offset);
				}else{
					posCounter++;
				}
			}
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(8);
			#endif
		}else{
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
		}
		
		//Attack
		if(attack(1, target, sce)){
			#if DEBUG_TIME
				Scene.tt.CategoryEnd(cat);
			#endif
			return true;
		}
		
		//Do nothing
		if(sce.rand.Next(4) == 0){
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
			if(master != null && posTarget == position){
				#if DEBUG_TIME
					Scene.tt.CategoryEnd(cat);
				#endif
				return false;
			}
			
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
		if(master == null){
			setPosTar(sce);
		}
		
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(cat);
		#endif
		return false;
	}
	
	void setTar(Scene sce){
		Entity[] possible = sce.entities.Where(n => (n != this && n is Necromancer)).Where(n => n.distanceSquared(position) < maxDistSq).ToArray();
		
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
	
	protected override void onDeath(Scene sce, Living s){
		sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.rand.NextDouble() * 0.7f);
		if(s == sce.p){
			sce.p.score += 10;
		}
	}
	
	protected override void onHurt(Scene sce, Living s){
		sce.sm.play(hurtSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.rand.NextDouble() * 0.7f);
	}
	
	protected override void onAttack(Scene sce, Living s){
		target = s;
	}
	
	public override int getAnimationFrame(Scene sce){
		if(isDying){
			return dyingAnimation?.frame ?? 4;
		}
		
		if(state == EntityState.idle){
			return sce.idleAnimation.frame + (master == null ? 6 : 0);
		}else if(state == EntityState.moving){
			return sce.runningAnimation.frame + (master == null ? 6 : 0);
		}
		
		return 0;
	}
}