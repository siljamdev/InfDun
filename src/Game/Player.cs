using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class Player : Living{
	static Texture2D t;
	static Sound loseSound;
	
	public static void initialize(){
		loseSound = Sound.monoFromAssembly("res.sounds.lose.ogg");
	}
	
	protected override int atlasColumn => 0;
	
	public int score;
	
	List<GoblinFriend> friends = new();
	
	double time;
	double maxTime;
	
	public Player(Vector2i p) : base(p){
		maxHealth = 10;
		health = maxHealth;
	}
	
	public void newLevel(Scene sce, Vector2i y){
		position = y;
		oldPosition = y;
		
		endSmooth(sce); //To update listener pos
		
		facingLeft = true; //Stairs sprite faces left
		
		friends.Clear();
		
		cure(1);
	}
	
	public bool tryMovePlayer(Vector2i? p, Scene sce){
		if(!sce.isAvailableToMove(p)){ //Null returns false
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
	
	public void addFriend(GoblinFriend b){
		friends.Add(b);
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
	
	public override void endSmooth(Scene sce){
		base.endSmooth(sce);
		sce.sm.setPos(new Vector3(position.X, position.Y, 0f)); //Update listener pos
		
		friends.RemoveAll(n => n.isDying);
	}
	
	protected override void onMove(Scene sce){
		sce.sm.playAbs(stepSound.get(sce.rand), 0.5f, 0.8f + (float)sce.rand.NextDouble() * 0.4f);
	}
	
	protected override void onHurt(Scene sce, Living s){
		sce.sm.playAbs(hurtSound, 1f, 0.6f + (float)sce.rand.NextDouble() * 0.7f);
		
		foreach(GoblinFriend f in friends){
			f.target = s;
		}
	}
	
	protected override void onDeath(Scene sce, Living s){
		sce.sm.playAbs(loseSound, 0.7f);
		
		if(s is Necromancer n){
			n.addSkeleton(sce, position, facingLeft);
		}
	}
	
	public override void draw(Scene sce){		
		Matrix4 model = getModel(sce);
		
		entityShader.use();
		entityShader.setMatrix4("model", model);
		entityShader.setVector2i("sprite", new Vector2i(getAnimationFrame(sce), atlasColumn));
		entityShader.setBool("isHurt", getHurt());
		entityShader.setFloat("alpha", 1f);
		
		entityTexture.bind();
		
		entityMesh.draw();
		
		if(isDying && dyingAnimation == null){
			dyingAnimation = new Animation(16, 0, 0.3d);
		}
		
		if(isDying){			
			dyingAnimation.tick(Dungeon.dh.deltaTime);
			if(dyingAnimation.frame > 0){
				sce.entitiesToRemove.Add(this);
			}
			
			time += Dungeon.dh.deltaTime;
			if(time > maxTime){
				time = 0d;
				
				sce.pr.add(new BloodParticle(sce, this));
				
				maxTime = getRandom(sce, 0.3d, 1d);
			}
		}
	}
	
	public override int getAnimationFrame(Scene sce){
		if(isDying){
			return 4;
		}
		
		if(state == EntityState.idle){
			return sce.idleAnimation.frame;
		}else if(state == EntityState.moving){
			return sce.runningAnimation.frame;
		}
		
		return 0;
	}
}