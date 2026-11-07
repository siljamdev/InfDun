using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class GoblinFriend : Living{
	const int maxDistSq = 16*16;
	const int wanderDistSq = 48*48;
	const int followDistSq = 2*2;
	
	int _c;
	protected override int atlasRow => _c;
	
	EntityGoal goal;
	
	Player master;
	
	#if DEBUG_TIME
		public override int cat => 4;
	#endif
	
	public GoblinFriend(Vector2i p, int tex, bool l, int h, Scene sce) : base(p){	
		_c = tex + 5;
		facingLeft = l;
		health = h;
		
		master = sce.p;
		
		master.addFriend(this);
	}
	
	public override bool doTurn(Scene sce){
		if(master != null && (master.isDying || !sce.entitiesLiving.Contains(master))){
			master = null;
		}
		
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
		if(goal != null){
			return;
		}
		
		if(master != null){
			goal = new FollowLazyGoal(this, master, followDistSq, sce);
		}else{
			goal = WanderLazyGoal.aroundEntity(sce, this, wanderDistSq); 
		}
	}
	
	public void masterAttacked(Living s){
		goal = new AttackGoal(this, s, maxDistSq, 1); 
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
			goal = new AttackLazyGoal(this, s, maxDistSq, 1);
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
	
	#if DEBUG_ENTITIES
		protected override void drawDebug(Scene sce){
			if(goal != null){
				Engine.instance.fd.draw(sce, goal.ToString(), position.X - Renderer.sceneTextSize.X / 2f, position.Y + 0.5f, Renderer.sceneTextSize, Renderer.textColor, 0.65f);
			}
			goal?.drawDebug(sce);
		}
	#endif
}