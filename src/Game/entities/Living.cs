using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

abstract class Living : Entity{
	#region static
	protected static Sound deathSound;
	protected static Sound hurtSound;
	protected static SoundPool stepSound;
	
	public static new void initialize(){
		deathSound = Sound.monoFromAssembly("res.sounds.death.ogg");
		hurtSound = Sound.monoFromAssembly("res.sounds.hurt.ogg");
		
		stepSound = new SoundPool(Sound.monoFromAssembly("res.sounds.step0.ogg"), Sound.monoFromAssembly("res.sounds.step1.ogg"), Sound.monoFromAssembly("res.sounds.step2.ogg"));
		
		Player.initialize();
		Skeleton.initialize();
		Necromancer.initialize();
		Frog.initialize();
	}
	#endregion static
	
	#if DEBUG_TIME
		public abstract int cat {get;}
	#endif
	
	protected bool isHurt;
	
	public int health{get; set;}
	public int maxHealth{get; set;}
	public bool isDying{get; protected set;}
	protected Animation dyingAnimation;
	
	public Living(){
		maxHealth = 3;
		health = maxHealth;
	}
	
	public Living(Vector2i p) : base(p){
		maxHealth = 3;
		health = maxHealth;
	}
	
	public bool tryMove(Vector2i? p, Scene sce){
		if(!sce.isAvailableToMoveFast(p)){ //Null returns false
			return false;
		}
		
		oldPosition = position;
		position = (Vector2i) p;
		state = EntityState.moving;
		
		if(position.X < oldPosition.X){
			facingLeft = true;
		}else if(position.X > oldPosition.X){
			facingLeft = false;
		}
		
		onMove(sce);
		
		return true;
	}
	
	public bool canCure(int c){
		if(health >= maxHealth){
			health = maxHealth;
			return false;
		}else{
			return true;
		}
	}
	
	public bool cure(int c){
		if(health >= maxHealth){
			health = maxHealth;
			return false;
		}else{
			health = Math.Min(maxHealth, health + c);
			return true;
		}
	}
	
	public bool damage(int d, Living s, Scene sce){
		if(isDying){
			return false;
		}
		health -= d;
		isHurt = true;
		onAttack(sce, s);
		if(health <= 0){
			isDying = true;
			onDeath(sce, s);
		}else{
			onHurt(sce, s);
		}
		
		return true;
	}
	
	public bool canBeDamagedBy(Living s, Scene sce){
		if(isDying){
			return false;
		}
		if(distance(s) > 1.5f){
			return false;
		}
		return true;
	}
	
	public bool tryDamageBy(int d, Living s, Scene sce){
		if(!canBeDamagedBy(s, sce)){
			return false;
		}
		health -= d;
		isHurt = true;
		onAttack(sce, s);
		if(health <= 0){
			isDying = true;
			onDeath(sce, s);
		}else{
			onHurt(sce, s);
		}
		
		return true;
	}
	
	public bool attack(int d, Living s, Scene sce){
		if(s != null && s.tryDamageBy(d, this, sce)){
			state = EntityState.attacking;
			if(s.position.X > position.X){
				facingLeft = false;
			}else if(s.position.X < position.X){
				facingLeft = true;
			}
			
			return true;
		}
		
		return false;
	}
	
	protected virtual void onMove(Scene sce){
		sce.sm.play(stepSound.get(sce.randCasual), new Vector3(position.X, position.Y, 0f), 0.5f, 0.8f + (float)sce.randCasual.NextDouble() * 0.4f);
	}
	
	protected virtual void onHurt(Scene sce, Living s){
		sce.sm.play(hurtSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
	}
	
	protected virtual void onDeath(Scene sce, Living s){
		sce.sm.play(deathSound, new Vector3(position.X, position.Y, 0f), 1f, 0.6f + (float)sce.randCasual.NextDouble() * 0.7f);
	}
	
	protected virtual void onAttack(Scene sce, Living s){
		int c = sce.randCasual.Next(5) + 2;
		for(int i = 0; i < c; i++){
			sce.pd.add(new BloodParticle(sce, this));
		}
	}
	
	public virtual bool doTurn(Scene sce){
		#if DEBUG_TIME
			Scene.tt.CategoryEnd(5);
		#endif
		return false;
	}
	
	public override void endSmooth(Scene sce){	
		if(!isDying){
			isHurt = false;
		}
		
		if(state == EntityState.attacking){
			state = EntityState.idle;
		}
		
		base.endSmooth(sce);
	}
	
	public override bool canPlayerInteract(Scene sce){
		return sce.p != null && canBeDamagedBy(sce.p, sce);
	}
	
	protected override bool doPlayerInteract(Scene sce){
		return sce.p.attack(1, this, sce);
	}
	
	public override Vector2 getPos(Scene sce){
		Vector2 pos;
		if(state == EntityState.moving){
			pos = (Vector2) ((Vector2d) oldPosition + (Vector2d) (position - oldPosition) * (sce.smoothTime/sce.smoothTimeMax));
		}else{
			pos = (Vector2) position;
		}
		
		if(isHurt && !isDying){ //Little bounce up
			float x = (float) (sce.smoothTime * (1d/sce.smoothTimeMax));
			pos.Y += -x*x + x;
		}
		
		return pos;
	}
	
	protected override Matrix4 getModel(Scene sce){
		Vector2 pos = getPos(sce);
		
		Matrix4 r = Matrix4.CreateTranslation(new Vector3(pos.X, pos.Y, 0f));
		
		if(facingLeft){
			r = leftRotation * r;
		}
		
		//Little rottion on attack
		if(state == EntityState.attacking){
			float x = (float) (sce.smoothTime * (1d/sce.smoothTimeMax));
			float y = -x*x + x;
			y *= -20f;
			r = Matrix4.CreateRotationZ(MathHelper.DegreesToRadians(y)) * r;
		}
		
		return r;
	}
	
	public override float getAlpha() => (dyingAnimation != null && dyingAnimation.frame > 4 && dyingAnimation.frame < 6) ? (float) (1d - dyingAnimation.time/dyingAnimation.maxTime) : 1f;
	
	public override bool getHurt() => isHurt;
	
	public override void draw(Scene sce){
		base.draw(sce);
		
		if(isDying && dyingAnimation == null){
			dyingAnimation = new Animation(16, 4, sce.smoothTimeMax); //4 and 5, 16 is as a safeguard
		}
		
		if(isDying){
			doCollision = false;
			dyingAnimation.tick(Engine.instance.dh.deltaTime);
			if(dyingAnimation.frame > 5){
				state = EntityState.idle;
				sce.entitiesToRemove.Add(this);
			}
		}
		
		#if DEBUG_ENTITIES
			AABB2D b = getBox();
			
			if(b.collide(sce.cam.mouseWorldPosition)){
				b.draw(sce);
				drawDebug(sce);
			}
		#endif
	}
	
	#if DEBUG_ENTITIES
		protected virtual void drawDebug(Scene sce){
			
		}
	#endif
}