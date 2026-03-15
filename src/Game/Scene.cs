using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;
using AshLib.Time;

class Scene : IDisposable{
	static Shader tileShader;
	static Texture2D tileTexture;
	static Texture2D tileAltTexture;
	
	static SoundPool bellSound;
	
	public static void initialize(){
		tileShader = Shader.fromAssembly("shaders.tile");
		
		tileTexture = Texture2D.fromAssembly("res.textures.tiles.png", TextureParams.Default);
		tileAltTexture = Texture2D.fromAssembly("res.textures.tilesAlt.png", TextureParams.Default);
		
		bellSound = new SoundPool(Sound.fromAssembly("res.sounds.bell0.ogg"), Sound.fromAssembly("res.sounds.bell1.ogg"));
		
		Entity.initialize();
	}
	
	public Camera cam{get; private set;}
	public SoundManager sm{get; private set;}
	public ParticleRenderer pr{get; private set;}
	public ParticleRenderer voidpr{get; private set;}
	
	public int levelNum{get; private set;} = -1; //bc it will increase
	bool altTiles;
	
	//in-between turn smoothing
	public double smoothTime{get; private set;}
	public double smoothTimeMax{get; private set;}
	public bool doingSmoothing{get; private set;}
	
	//Exit descent
	public double descentTime{get; private set;}
	public const double descentTimeMax = 2d;
	bool doingDescent;
	
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
	
	public bool[] globalTransitable{get; private set;} //Only built and used during turns, has entities positions
	
	Mesh tilesMesh;
	
	//Void particles counter
	double voidTime;
	const double voidTimeMax = 0.3d;
	
	//Time player has been in the current level
	double startLevelTime;
	double levelTime => Dungeon.dh.GetTime() - startLevelTime;
	
	public bool canMovePlayer => (!doingSmoothing) && (!doingDescent);
	
	public bool playerDead;
	
	public Player p{get; private set;}
	
	public List<Entity> entitiesInanimate{get; private set;} = new();
	public List<Entity> entitiesItems{get; private set;} = new();
	public List<Entity> entitiesLiving{get; private set;} = new();
	
	IEnumerable<Entity> allEntities => entitiesInanimate.Concat(entitiesItems).Concat(entitiesLiving);
	
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
		};
		
		public static readonly TimeTool tt = new TimeTool(categories);
	#endif
	
	public Scene(Renderer ren, SoundManager s){
		Console.WriteLine("Initlializing new scene");
		
		cam = ren.cam;
		sm = s;
		pr = new ParticleRenderer();
		voidpr = new ParticleRenderer();
		
		cam.onViewChange += setView;
		
		//Initial update needed to set it
		setProjection(ren.projection);
		setView(null, EventArgs.Empty);
		
		randSerious = new Random();
		randCasual = new Random(randSerious.Next());
		
		smoothTimeMax = 0.3d;
		doingSmoothing = false;
		
		doingDescent = false;
		
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
		//Play bell
		sm.playNonPositional(bellSound.get(randCasual), 1f);
		
		//Increment level
		levelNum++;
		
		//Reset smoothing
		smoothTime = 0d;
		smoothTimeMax = 0.3d;
		
		startLevelTime = Dungeon.dh.GetTime();
		
		//Generate
		LevelGenerator gen = new LevelGenerator(levelNum, randSerious, randCasual);
		
		(int[,] m, List<Entity> entities, Vector2i startPos, exitPos) = gen.generate();
		
		mapYsize = m.GetLength(0);
		mapXsize = m.GetLength(1);
		
		//Set limit
		Necromancer.minionLimit = (levelNum - 1) / 3;
		
		//Clear particles
		pr.clear();
		
		//Entities
		clearEntities();
		addEntities(entities);
		
		//Player
		entitiesLiving.Add(p);
		p.newLevel(this, startPos);
		
		//Set camera
		cam.moveFast(p.position);
		
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
		
		int x = (int) (cam.mouseWorldPos.X + 0.5d);
		int y = (int) cam.mouseWorldPos.Y;
		
		foreach(Entity e in allEntities.Reverse()){
			if(e != p && e.position == new Vector2i(x, y) && e.onClick(this)){
				playerDidAction = true;
				break;
			}
		}
	}
	
	public bool interact(){
		if(!canMovePlayer){
			return false;
		}
		
		foreach(Entity e in allEntities.Reverse()){
			if(e != p && e.onClick(this)){
				playerDidAction = true;
				return true;
			}
		}
		
		return false;
	}
	
	#if DEBUG_LEVELS
		public void skipLevel(){
			if(!doingDescent){
				doingDescent = true;
				descentTime = 0d;	
			}
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
	
	public void draw(Renderer ren){
		//Add entities
		addEntities(entitiesToAdd);
		entitiesToAdd.Clear();
		
		//Update highscore
		if(p.score > ren.dun.highscore){
			ren.dun.highscore = p.score;
		}
		
		//Update descent
		if(doingDescent){
			descentTime += Dungeon.dh.deltaTime;
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
			smoothTime += Dungeon.dh.deltaTime;
			
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
		idleAnimation.tick(Dungeon.dh.deltaTime);
		runningAnimation.tick(Dungeon.dh.deltaTime);
		coinAnimation.tick(Dungeon.dh.deltaTime);
		
		//Add void particles
		voidTime += Dungeon.dh.deltaTime;
		if(voidTime > voidTimeMax){
			voidTime = 0d;
			
			double t = levelTime; //In secs
			Vector2d voidDirection = t >= 400d ? (Math.Min(t, 500d) - 400d) / 50d * Vector2d.Normalize(exitPos - p.position) : Vector2.Zero;
			
			voidpr.add(new VoidParticle(p.position + new Vector2((float) getRandomCasual(-16f, 16f), (float) getRandomCasual(-16f, 16f)), voidDirection));
			voidpr.add(new VoidParticle(p.position + new Vector2((float) getRandomCasual(-16f, 16f), (float) getRandomCasual(-16f, 16f)), voidDirection));
			voidpr.add(new VoidParticle(p.position + new Vector2((float) getRandomCasual(-16f, 16f), (float) getRandomCasual(-16f, 16f)), voidDirection));
		}
		
		//Render void particles
		voidpr.draw(ren);
		
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
			ren.screenAlpha = 0f;
			playerDead = true;
			ren.dun.setDeathScreen(p.score, levelNum);
			smoothTimeMax = 0.5d;
		}
		
		entitiesToRemove.Clear();
		
		//Render dead player
		if(playerDead){
			p.draw(this);
			
			deathTime += Dungeon.dh.deltaTime;
			ren.screenAlpha = (float) Math.Min(deathTime/deathTimeMax * 0.6f, 0.6f);
		}
		
		//Render particles
		pr.draw(ren);
		
		//Draw descent
		if(doingDescent){
			float factor = (float) (descentTime/descentTimeMax);
			float alpha = -4f * factor * factor + 4f * factor;
			ren.drawRect(-ren.width/2f, ren.height/2f, ren.width, ren.height, Color3.Black, alpha);
			
			string temp3 = "Level " + levelNum;
			ren.fr.drawText(temp3, -temp3.Length / 2 * Renderer.bigTextSize.X, 0f, Renderer.bigTextSize, Renderer.textColor, alpha);
		}
		
		//Draw text
		ren.fr.drawText(p.health.ToString() + "/" + p.maxHealth.ToString() , -ren.width/2f + 45f, ren.height/2f, Renderer.bigTextSize, new Color3("AA0000"));
		string temp2 = p.score.ToString();
		ren.fr.drawText(temp2, -temp2.Length / 2 * Renderer.middleTextSize.X, ren.height/2f, Renderer.middleTextSize, Renderer.textColor);
		
		//Draw overlay
		ren.drawTexture("heart", -ren.width/2f, ren.height/2f, 40f, 40f, Color3.White);
		if(p.hasKey){
			ren.drawTexture("key", -ren.width/2f, ren.height/2f - 45f, 40f, 40f, Color3.White);
		}
	}
	
	public void endFrame(){
		playerDidAction = playerDidAction;
		
		if(playerDidAction || playerTriedMoving || (!doingSmoothing && playerDead)){
			turnEntities();
			
			playerDidAction = false;
			playerTriedMoving = false;
		}
	}
	
	public void setProjection(Matrix4 m){
		Entity.setProjection(m);
		tileShader.setMatrix4("projection", m);
	}
	
	public void setView(object s, EventArgs a){
		Entity.setView(cam.view);
		tileShader.setMatrix4("view", cam.view);
	}
	
	public double getRandomCasual(double min, double max){
		return randCasual.NextDouble() * (max - min) + min;
	}
	
	public double getRandomSerious(double min, double max){
		return randSerious.NextDouble() * (max - min) + min;
	}
	
	public void Dispose(){
		cam.onViewChange -= setView;
	}
}