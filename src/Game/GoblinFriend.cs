using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class GoblinFriend : Living{
	static readonly Vector2i[] offsets = new Vector2i[]{
		new Vector2i(-2, -2), new Vector2i(-1, -2), new Vector2i(0, -2), new Vector2i(1, -2), new Vector2i(2, -2),
		new Vector2i(-2, -1),                                                                 new Vector2i(2, -1),
		new Vector2i(-2,  0),                                                                 new Vector2i(2,  0),
		new Vector2i(-2,  1),                                                                 new Vector2i(2,  1),
		new Vector2i(-2,  2), new Vector2i(-1,  2), new Vector2i(0,  2), new Vector2i(1,  2), new Vector2i(2,  2)
	};
	
	const int maxDistSq = 16*16;
	const int wanderDistSq = 48*48;
	
	Vector2i offset;
	
	int _c;
	protected override int atlasColumn => _c;
	
	public Living target = null;
	//Vector2i? posTarget = null;
	
	Player master;
	
	#if DEBUG_TIME
		const int cat = 4;
	#endif
	
	public GoblinFriend(Vector2i p, int tex, bool l, int h, Scene sce) : base(p){	
		_c = tex + 5;
		facingLeft = l;
		health = h;
		
		master = sce.p;
		
		master.addFriend(this);
		
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
			if(target != null && target.isDying){
				target = null;
			}
			
			if(target == null){
				setPosTarget(sce, master.position + offset);
			}
		}else{
			if(target != null){
				if(target.isDying || target.distanceSquared(position) > maxDistSq){
					target = null;
				}
			}else if(posTarget == position){
				setPosTarget(sce, null);
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
		if(sce.rand.Next(3) == 0){
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
	
	void setPosTar(Scene sce){
		int[] posibleTiles = sce.transitable.Select((v, i) => (v, i)).Where(n => n.Item1).Select(n => n.Item2).
		Where(n => this.distanceSquared(new Vector2i(n % sce.mapXsize, n / sce.mapXsize)) < wanderDistSq).ToArray();
		
		int x2 = posibleTiles[sce.rand.Next(posibleTiles.Length)];
		
		setPosTarget(sce, new Vector2i(x2 % sce.mapXsize, x2 / sce.mapXsize));
		target = null;
	}
	
	protected override void onDeath(Scene sce, Living s){
		base.onDeath(sce, s);
		
		if(s is Necromancer n){
			n.addSkeleton(sce, position, facingLeft);
		}
	}
	
	protected override void onAttack(Scene sce, Living s){
		base.onAttack(sce, s);
		
		if(s != master){
			target = s;
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
		}
		
		return 0;
	}
}