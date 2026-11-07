using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Skeleton : Living{
	static new Sound hurtSound;
	static new Sound deathSound;
	
	public static new void initialize(){
		hurtSound = Sound.monoFromAssembly("res.sounds.skellyhurt.ogg");
		deathSound = Sound.monoFromAssembly("res.sounds.skellydeath.ogg");
	}
	
	const int maxDistSq = 32*32;
	const int wanderDistSq = 48*48;
	const int followDistSq = 3*3;
	
	protected override int atlasRow => 4;
	
	EntityGoal goal;
	
	public Necromancer master {get; private set;}
	
	#if DEBUG_TIME
		public override int cat => 2;
	#endif
	
	public Skeleton(Vector2i p, Necromancer o, bool? lef, Scene sce) : base(p){
		master = o;
		if(master != null){
			sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 1f, 0.5f + (float)sce.randCasual.NextDouble() * 0.5f);
		}
		
		facingLeft = lef ?? sce.randCasual.Next(2) == 0;
		
		health = 2;
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
		if(master == null){
			if(!(goal is AttackGoal)){
				EntityGoal attack = AttackLazyGoal.closestTarget(sce, this, maxDistSq, 1, typeof(Necromancer));
				if(attack != null){
					goal = attack;
				}
			}
			
			goal ??= WanderLazyGoal.aroundEntity(sce, this, wanderDistSq);
		}else{
			goal ??= new FollowLazyGoal(this, master, followDistSq, sce);
		}
	}
	
	public void masterAttacked(Living s){
		goal = new AttackLazyGoal(this, s, maxDistSq, 1); //not lazy b magic powered n shi
	}
	
	protected override void onDeath(Scene sce, Living s){
		sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
		if(s == sce.p){
			sce.p.score += 10;
		}
	}
	
	protected override void onHurt(Scene sce, Living s){
		sce.sm.play(hurtSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
	}
	
	protected override void onAttack(Scene sce, Living s){
		if(s != master){
			goal = new AttackLazyGoal(this, s, maxDistSq, 1);
		}
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
	
	#if DEBUG_ENTITIES
		protected override void drawDebug(Scene sce){
			if(goal != null){
				Engine.instance.fd.draw(sce, goal.ToString(), position.X - Renderer.sceneTextSize.X / 2f, position.Y + 0.5f, Renderer.sceneTextSize, Renderer.textColor, 0.65f);
			}
			goal?.drawDebug(sce);
		}
	#endif
}