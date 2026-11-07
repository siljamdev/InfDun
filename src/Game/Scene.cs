using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;
using AshLib.Time;

class Scene : Renderer{
	static Shader tileShader;
	public static Texture2D tileTexture;
	static Texture2D tileAltTexture;
	
	static SoundPool bellSound;
	
	public static void initialize(){
		tileShader = Shader.fromAssembly("shaders.tile");
		
		tileTexture = Texture2D.fromAssembly("res.textures.tiles.png", TextureParams.Default);
		tileAltTexture = Texture2D.fromAssembly("res.textures.tilesAlt.png", TextureParams.Default);
		
		bellSound = new SoundPool(Sound.fromAssembly("res.sounds.bell0.ogg"), Sound.fromAssembly("res.sounds.bell1.ogg"));
		
		Entity.initialize();
	}
	
	public Camera2D cam{get; private set;}
	public SoundManager sm{get; private set;} //Shared with engine
	public ParticleDrawer pd{get; private set;}
	public ParticleDrawer voidpd{get; private set;}
	
	SceneOverlay overlay; //Overlay drawer
	
	public bool showInteractions = false; //If possible interactions will be shown
	
	public int levelNum{get; private set;} = -1; //bc it will increase
	bool altTiles;
	
	//in-between turn smoothing
	public double smoothTime{get; private set;}
	public double smoothTimeMax{get; private set;}
	public bool doingSmoothing{get; private set;}
	
	//Exit descent
	public double descentTime{get; private set;}
	public const double descentTimeMax = 2d;
	public bool doingDescent;
	
	//Has already generated a new level on descent
	bool temp;
	
	//Death gradual alpha animation
	public double deathTime{get; private set;}
	public const double deathTimeMax = 2d;
	
	//General animations
	public Animation idleAnimation{get; private set;}
	public Animation runningAnimation{get; private set;}
	public Animation coinAnimation{get; private set;}
	
	public int frogAnimationFrame => smoothTime > smoothTimeMax/2f ? 3 : 2;
	
	//Tiles
	public int[] tiles{get; private set;}
	public int mapXsize{get; private set;}
	public int mapYsize{get; private set;}
	
	Vector2i exitPos;
	
	public bool[] transitableTiles{get; private set;} //Answers the question. is tile transitable?
	
	public bool[] globalTransitable{get; private set;} //Only built and used during entity turns, has entities positions
	
	Mesh tilesMesh;
	
	//Void particles counter
	double voidTime;
	const double voidTimeMax = 0.25d;
	
	const double voidStart = 300d; //In seconds, time it takes for void particles to start going towards the exit
	const double voidEnd = 400d; //In seconds, time it takes for void particles to finish transition of going towards the exit. Max strength from this moment onwards
	const double voidStrength = 2d; //Max strength, from normalized vector
	
	//Time player has been in the current level. Used for void particles
	double startLevelTime;
	double levelTime => Engine.instance.dh.GetTime() - startLevelTime;
	
	public bool canMovePlayer => (!doingSmoothing) && (!doingDescent) && (!playerDead);
	
	public bool playerDead;
	
	public Player p{get; private set;}
	
	public List<Entity> entitiesInanimate{get; private set;} = new();
	public List<Entity> entitiesItems{get; private set;} = new();
	public List<Entity> entitiesLiving{get; private set;} = new();
	
	IEnumerable<Entity> allEntities => entitiesInanimate.Concat(entitiesItems).Concat(entitiesLiving);
	
	//Only add when it is proper
	public List<Entity> entitiesToRemove{get; private set;} = new();
	public List<Entity> entitiesToAdd{get; private set;} = new();
	
	public Random randSerious{get; private set;}
	public Random randCasual{get; private set;}
	
	bool playerDidAction;
	bool playerTriedMoving;
	
	#if DEBUG_TIME
		static readonly string[] categories = new string[]{
			"Goblin",       //0
			"FastGoblin",   //1
			"Skeleton",     //2
			"Necromancer",  //3
			"GoblinFriend", //4
			"Other",        //5
			"Pathfinding old",  //6
			"Pathfinding new",  //7
			"Skelly mfollow",  //8
			"Frog",  //9
			"Flee pathfinding",  //10
		};
		
		public static readonly TimeTool tt = new TimeTool(categories);
	#endif
	
	public Scene(SoundManager s){
		Console.WriteLine("Initlializing new scene");
		
		cam = new Camera2D(width, height);
		cam.onViewChange += setView;
		cam.updateForce(); //Trigger first view update
		
		sm = s;
		pd = new ParticleDrawer();
		voidpd = new ParticleDrawer();
		
		overlay = new SceneOverlay(this);
		
		//Event callbacks
		Engine.instance.onParticleActivation += particleActivation;
		Engine.instance.onFrameEnd += endFrame;
		
		randSerious = new Random();
		randCasual = new Random(randSerious.Next());
		
		smoothTimeMax = 0.3d;
		doingSmoothing = false;
		
		doingDescent = false;
		
		//Setup animations
		idleAnimation = new Animation(2, 0, 0.3d);
		runningAnimation = new Animation(2, 2, 0.1d);
		coinAnimation = new Animation(8, 0, 0.1d);
		
		p = new Player(Vector2i.Zero);
		
		newLevel();
		
		doingDescent = true;
		descentTime = descentTimeMax / 2d;
		temp = true;
	}
	
	void newLevel(){
		//Play bell sound
		sm.playNonPositional(bellSound.get(randCasual), 1f);
		
		//Increment level
		levelNum++;
		
		//Reset smoothing
		smoothTime = 0d;
		smoothTimeMax = 0.3d;
		
		//set start time
		startLevelTime = Engine.instance.dh.GetTime();
		
		//Generate
		LevelGenerator gen = new LevelGenerator(levelNum, randSerious, randCasual);
		
		(int[,] m, List<Entity> entities, Vector2i startPos, exitPos) = gen.generate();
		
		mapYsize = m.GetLength(0);
		mapXsize = m.GetLength(1);
		
		//Set limit
		Necromancer.minionLimit = (levelNum - 1) / 3;
		
		//Clear particles
		pd.clear();
		voidpd.clear();
		
		//Entities
		clearEntities();
		addEntities(entities);
		
		//Player
		entitiesLiving.Add(p);
		p.newLevel(this, startPos);
		
		//Set camera
		resetCam();
		
		//Tiles
		tiles = m.Cast<int>().ToArray();
		transitableTiles = tiles.Select(n => Tile.transitables.Contains(n)).ToArray();
		
		//Tile mesh
		tilesMesh = Tile.createTilesMesh(tiles, mapXsize, mapYsize, randCasual);
		
		//Setup global map
		globalTransitable = (bool[]) transitableTiles.Clone();
		
		foreach(Entity e in allEntities){
			if(!e.doCollision){
				continue;
			}
			
			globalTransitable[flatIndexOf(e.position)] = false;
		}
		
		//Init entities
		foreach(Entity e in allEntities){
			e.newLevel(this);
		}
		
		//Clear global map
		globalTransitable = null;
		
		//Detail
		altTiles = randCasual.Next(33) == 0;
	}
	
	void turnEntities(){
		#if DEBUG_TIME
			tt.LoopStart();
		#endif
		
		bool entityMoved = false;
		
		//Setup global map
		globalTransitable = (bool[]) transitableTiles.Clone();
		
		foreach(Entity e in allEntities){
			if(!e.doCollision){
				continue;
			}
			
			globalTransitable[flatIndexOf(e.position)] = false;
		}
		
		#if DEBUG_TIME
			tt.CategoryEnd(5);
		#endif
		
		//Run turns
		foreach(Entity e in allEntities){
			if(e is not Living l){
				#if DEBUG_TIME
					tt.CategoryEnd(5);
				#endif
				continue;
			}
			if(l.isDying){
				#if DEBUG_TIME
					tt.CategoryEnd(5);
				#endif
				continue;
			}
			bool b = l.doTurn(this);
			
			entityMoved = entityMoved || b;
			
			//Update global map
			if(e.state == EntityState.moving){
				globalTransitable[flatIndexOf(e.position)] = false;
				globalTransitable[flatIndexOf(e.oldPosition)] = true;
			}
		}
		
		if(playerDidAction || entityMoved){
			doingSmoothing = true;
		}
		
		//Clear global transitable
		globalTransitable = null;
		
		#if DEBUG_TIME
			tt.CategoryEnd(5);
		#endif
		
		#if DEBUG_TIME
			tt.LoopEnd();
		#endif
	}
	
	//Tile vector coords to flat tile index
	public int flatIndexOf(Vector2i v){
		int x = v.Y * mapXsize + v.X;
		if(x < 0 || x >= tiles.Length){
			return -1;
		}
		
		return x;
	}
	
	public Vector2i? coordinatesOfFlatIndex(int findex){
		if(findex < 0 || findex >= mapXsize * mapYsize){
			return null;
		}
		
		return new Vector2i(findex % mapXsize, findex / mapXsize);
	}
	
	public int tileAt(Vector2i v){
		int x = v.Y * mapXsize + v.X;
		if(x < 0 || x >= tiles.Length){
			return Tile.Air;
		}
		
		return tiles[x];
	}
	
	public bool isAvailableToMove(Vector2i? v2){
		if(v2 == null){
			return false;
		}
		
		Vector2i v = (Vector2i) v2;
		
		if(v.X < 0 || v.X >= mapXsize || v.Y < 0 || v.Y >= mapYsize){
			return false;
		}
		
		int x = v.Y * mapXsize + v.X;
		
		if(!transitableTiles[x]){
			return false;
		}
		
		foreach(Entity e in allEntities){
			if(e.doCollision && e.position == v){
				return false;
			}
		}
		return true;
	}
	
	//Only available in enetity turns!!
	public bool isAvailableToMoveFast(Vector2i? v2){
		if(v2 == null){
			return false;
		}
		
		Vector2i v = (Vector2i) v2;
		
		if(v.X < 0 || v.X >= mapXsize || v.Y < 0 || v.Y >= mapYsize){
			return false;
		}
		
		int x = v.Y * mapXsize + v.X;
		
		return globalTransitable[x];
	}
	
	bool movePlayer(int x, int y){
		if(playerDidAction || playerDead){
			return false;
		}
		
		playerTriedMoving = true;
		
		Vector2i v = p.position + new Vector2i(x, y);
		
		if(p.tryMovePlayer(v, this)){
			doingSmoothing = true;
			cam.move(p.position);
			return true;
		}else{
			return false;
		}
	}
	
	public bool movePlayerUp(){
		return movePlayer(0, 1);
	}
	
	public bool movePlayerDown(){
		return movePlayer(0, -1);
	}
	
	public bool movePlayerLeft(){
		return movePlayer(-1, 0);
	}
	
	public bool movePlayerRight(){
		return movePlayer(1, 0);
	}
	
	public void click(){
		if(!canMovePlayer){
			return;
		}
		
		int x = (int) (cam.mouseWorldPosition.X + 0.5f);
		int y = (int) cam.mouseWorldPosition.Y;
		
		foreach(Entity e in allEntities.Reverse()){
			if(e != p && e.position == new Vector2i(x, y) && e.playerInteract(this)){
				playerDidAction = true;
				break;
			}
		}
	}
	
	#if DEBUG_LEVELS
		public void instaKill(){
			if(!canMovePlayer){
				return;
			}
			
			int x = (int) (cam.mouseWorldPosition.X + 0.5f);
			int y = (int) cam.mouseWorldPosition.Y;
			
			foreach(Entity e in allEntities.Reverse()){
				if(e != p && e is Living l && e.position == new Vector2i(x, y)){
					l.damage(10, p, this);
					playerDidAction = true;
					break;
				}
			}
		}
	#endif
	
	public bool interact(int target = 1){
		if(!canMovePlayer){
			return false;
		}
		
		foreach(Entity e in allEntities.Reverse()){
			if(e != p && e.canPlayerInteract(this)){
				target--;
				if(target == 0){
					if(e.playerInteract(this)){
						playerDidAction = true;
						return true;
					}
					
					return false;
				}
			}
		}
		
		return false;
	}
	
	public void skipTime(){
		if(!canMovePlayer){
			return;
		}
		
		playerDidAction = true;
	}
	
	#if DEBUG_LEVELS
		public void skipLevel(){
			if(!doingDescent){
				doingDescent = true;
				descentTime = 0d;	
			}
		}
		
		public void cheatKey(){
			p.hasKey = true;
		}
	#endif
	
	public void addEntities(List<Entity> l){
		foreach(Entity e in l){
			if(e is Inanimate){
				entitiesInanimate.Add(e);
			}else if(e is Living || e is Magic){
				entitiesLiving.Add(e);
			}else{
				entitiesItems.Add(e);
			}
		}
	}
	
	public void removeEntity(Entity l){
		entitiesInanimate.Remove(l);
		entitiesItems.Remove(l);
		entitiesLiving.Remove(l);
	}
	
	public void removeEntities(List<Entity> l){
		entitiesInanimate.RemoveAll(n => l.Contains(n));
		entitiesItems.RemoveAll(n => l.Contains(n));
		entitiesLiving.RemoveAll(n => l.Contains(n));
	}
	
	public void clearEntities(){
		entitiesInanimate.Clear();
		entitiesItems.Clear();
		entitiesLiving.Clear();
		
		entitiesToAdd.Clear();
		entitiesToRemove.Clear();
	}
	
	public override void draw(){
		//Add entities
		addEntities(entitiesToAdd);
		entitiesToAdd.Clear();
		
		//Update highscore
		if(p.score > Engine.instance.highscore){
			Engine.instance.highscore = p.score;
		}
		
		//Update descent
		if(doingDescent){
			descentTime += Engine.instance.dh.deltaTime;
			if(descentTime > descentTimeMax){
				if(!temp){
					newLevel();
				}
				doingDescent = false;
				descentTime = 0d;
				temp = false;
			}else if(!temp && descentTime > descentTimeMax / 2d){
				newLevel();
				temp = true;
			}
		}
		
		//Update smooth
		if(doingSmoothing){
			smoothTime += Engine.instance.dh.deltaTime;
			
			if(smoothTime > smoothTimeMax){
				doingSmoothing = false;
				smoothTime = 0d;
				
				foreach(Entity e in allEntities){
					e.endSmooth(this);
				}
				
				if(!p.isDying && tileAt(p.position) == 17){
					p.score += 100;
					doingDescent = true;
					descentTime = 0d;
				}
			}else if(p.state == EntityState.moving){
				Vector2 pos = (Vector2) ((Vector2d) p.oldPosition + (Vector2d) (p.position - p.oldPosition) * (smoothTime/smoothTimeMax));
				sm.setPos(new Vector3(pos.X, pos.Y, 0f));
			}
		}
		
		//Update animations
		idleAnimation.tick(Engine.instance.dh.deltaTime);
		runningAnimation.tick(Engine.instance.dh.deltaTime);
		coinAnimation.tick(Engine.instance.dh.deltaTime);
		
		//Add void particles
		voidTime += Engine.instance.dh.deltaTime;
		if(voidTime > voidTimeMax){
			voidTime = 0d;
			
			double t = levelTime; //In secs
			Vector2 voidDirection = (t >= voidStart) ? (Vector2) (voidStrength * (Math.Min(t, voidEnd) - voidStart) / (voidEnd - voidStart) * Vector2d.Normalize(exitPos - p.position)) : Vector2.Zero;
			
			voidpd.add(new VoidParticle(p.position + (Vector2) new Vector2d(getRandomCasual(-16f, 16f), getRandomCasual(-16f, 16f)), voidDirection));
			voidpd.add(new VoidParticle(p.position + (Vector2) new Vector2d(getRandomCasual(-16f, 16f), getRandomCasual(-16f, 16f)), voidDirection));
			voidpd.add(new VoidParticle(p.position + (Vector2) new Vector2d(getRandomCasual(-16f, 16f), getRandomCasual(-16f, 16f)), voidDirection));
		}
		
		//Render void particles
		voidpd.draw(this);
		
		//Draw tiles
		tileShader.use();
		(altTiles ? tileAltTexture : tileTexture).bind();
		tilesMesh.draw();
		
		//Draw entities
		foreach(Entity e in allEntities){
			e.draw(this);
		}
		
		//Remove dead entities
		removeEntities(entitiesToRemove);
		
		//Check for dead player
		if(!playerDead && entitiesToRemove.Contains(p)){
			overlay.screenAlpha = 0f;
			playerDead = true;
			Engine.instance.uren.setScreen(Engine.instance.sc.getDeathScreen(p.score, levelNum));
			smoothTimeMax = 0.5d;
		}
		
		entitiesToRemove.Clear();
		
		//Render dead player
		if(playerDead){
			p.draw(this);
			
			deathTime += Engine.instance.dh.deltaTime;
			overlay.screenAlpha = (float) Math.Min(deathTime/deathTimeMax * 0.6f, 0.6f);
		}
		
		//Render particles
		pd.draw(this);
		
		//Draw possible interactions
		if(showInteractions){
			showInteractions = false;
			
			if(canMovePlayer){
				drawInteractions();
			}
		}
		
		//Draw overlay
		overlay.draw();
	}
	
	public void drawInteractions(){
		int i = 1;
		foreach(Entity e in allEntities.Reverse()){
			if(e != p && e.canPlayerInteract(this)){
				fd.draw(this, i.ToString(), e.position.X - Renderer.sceneTextSize.X / 2f, e.position.Y + 0.5f, Renderer.sceneTextSize, Renderer.textColor, 0.65f);
				i++;
				
				if(i >= 10) //Max of 9
					break;
			}
		}
	}
	
	public void endFrame(){
		if(playerDidAction || playerTriedMoving || (!doingSmoothing && playerDead)){
			turnEntities();
			
			playerDidAction = false;
			playerTriedMoving = false;
		}
	}
	
	public void resetCam(){
		cam.moveFast(p.position);
	}
	
	public double getRandomCasual(double min, double max){
		return randCasual.NextDouble() * (max - min) + min;
	}
	
	public double getRandomSerious(double min, double max){
		return randSerious.NextDouble() * (max - min) + min;
	}
	
	public void particleActivation(bool e){
		pd.setEnabled(e);
		voidpd.setEnabled(e);
	}
	
	public void setView(Matrix4 view, float zoom){
		tileShader.setMatrix4("view", view);
		this.view = view;
		this.zoom = zoom;
		
		Entity.setView(view);
	}
	
	protected override void onResize(int width, int height){
		projection = Matrix4.CreateOrthographic((float) width, (float) height, -1.0f, 1.0f);
		tileShader.setMatrix4("projection", projection);
		Entity.setProjection(projection);
	}
	
	public override void Dispose(){
		Engine.instance.onParticleActivation -= particleActivation;
		Engine.instance.onFrameEnd -= endFrame;
		
		cam.Dispose();
		overlay.Dispose();
		base.Dispose();
	}
}