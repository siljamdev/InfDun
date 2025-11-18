using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;
using AshLib;
using AshLib.Time;

class Scene{	
	static Shader tileShader;
	static Texture2D tileTexture;
	static Texture2D tileAltTexture;
	
	static SoundPool bellSound;
	
	static readonly Vector2i tileSpriteNum = new Vector2i(16, 2);
	
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
	public const double deathTimeMax = 2f;
	
	//General animations
	public Animation idleAnimation{get; private set;}
	public Animation runningAnimation{get; private set;}
	
	//Tiles
	public int[] tiles{get; private set;}
	public int mapXsize{get; private set;}
	public int mapYsize{get; private set;}
	
	public bool[] transitable{get; private set;}
	
	public bool[] globalTransitable{get; private set;}
	
	Mesh tilesMesh;
	
	public bool canMovePlayer => (!doingSmoothing) && (!doingDescent);
	
	public bool playerDead;
	
	public Player p{get; private set;}
	
	public List<Entity> entities{get; private set;}
	public List<Entity> entitiesToRemove{get; private set;}
	public List<Entity> entitiesToAdd{get; private set;}
	
	public Random rand{get; private set;}
	
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
		};
		
		public static readonly TimeTool tt = new TimeTool(categories);
	#endif
	
	public Scene(Renderer ren, SoundManager s){
		Console.WriteLine("Initlializing new scene");
		
		cam = ren.cam;
		sm = s;
		pr = new ParticleRenderer();
		
		cam.onViewChange += setView;
		
		//Initial update needed to set it
		setProjection(ren.projection);
		setView(null, EventArgs.Empty);
		
		rand = new Random();
		
		smoothTimeMax = 0.3d;
		doingSmoothing = false;
		
		doingDescent = false;
		
		idleAnimation = new Animation(2, 0, 0.3d);
		runningAnimation = new Animation(2, 2, 0.1d);
		
		entitiesToRemove = new List<Entity>();
		entitiesToAdd = new List<Entity>();
		
		p = new Player(Vector2i.Zero);
		
		newLevel();
		
		doingDescent = true;
		descentTime = descentTimeMax / 2d;
		temp = true;
	}
	
	void newLevel(){
		//Play bell
		sm.playAbs(bellSound.get(rand), 1f);
		
		//Increment level
		levelNum++;
		
		//Reset smoothing
		smoothTime = 0d;
		smoothTimeMax = 0.3d;
		
		//Generate
		LevelGenerator gen = new LevelGenerator(levelNum, rand);
		
		(int[,] m, entities, Vector2i startPos) = gen.generate();
		
		mapYsize = m.GetLength(0);
		mapXsize = m.GetLength(1);
		
		//Set limit
		Necromancer.minionLimit = (levelNum - 3) / 4;
		
		//Entities
		entities = sortEntities(entities);
		
		//Player
		entities.Add(p);
		p.newLevel(this, startPos);
		
		//Set camera
		cam.moveFast(p.position);
		
		//Tiles
		tiles = m.Cast<int>().ToArray();
		transitable = tiles.Select(n => Tile.transitables.Contains(n)).ToArray();
		
		//Tile mesh
		tilesMesh = createTilesMesh(tiles, mapXsize, mapYsize, rand);
		
		//Setup global map
		globalTransitable = (bool[]) transitable.Clone();
		
		foreach(Entity e in entities){
			if(!e.doCollision){
				continue;
			}
			
			globalTransitable[flatIndexOf(e.position)] = false;
		}
		
		//Init entities
		foreach(Entity e in entities){
			e.newLevel(this);
		}
		
		//Clear global map
		globalTransitable = null;
		
		//Detail
		altTiles = rand.Next(33) == 0;
	}
	
	void turnEntities(){
		#if DEBUG_TIME
			tt.LoopStart();
		#endif
		
		bool entityMoved = false;
		
		//Setup global map
		globalTransitable = (bool[]) transitable.Clone();
		
		foreach(Entity e in entities){
			if(!e.doCollision){
				continue;
			}
			
			globalTransitable[flatIndexOf(e.position)] = false;
		}
		
		#if DEBUG_TIME
			tt.CategoryEnd(5);
		#endif
		
		//Run turns
		foreach(Entity e in entities){
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
	
	public int tileAt(Vector2i v){
		int x = v.Y * mapXsize + v.X;
		if(x < 0 || x >= tiles.Length){
			return 0;
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
		
		if(!transitable[x]){
			return false;
		}
		
		foreach(Entity e in entities){
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
		
		Entity r = null;
		
		foreach(Entity e in entities){
			if(e != p && e.position == new Vector2i(x, y) && e.onClick(this)){
				playerDidAction = true;
				break;
			}
		}
	}
	
	public void draw(Renderer ren){
		//Add entities
		entities.AddRange(entitiesToAdd);
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
				
				foreach(Entity e in entities){
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
		
		//Draw tiles
		tileShader.use();
		(altTiles ? tileAltTexture : tileTexture).bind();
		tilesMesh.draw();
		
		//Draw entities
		foreach(Entity e in entities){
			e.draw(this);
		}
		
		//Remove dead entities
		entities.RemoveAll(n => entitiesToRemove.Contains(n));
		
		//Check for dead player
		if(!playerDead && entitiesToRemove.Contains(p)){
			ren.screenAlpha = 0f;
			playerDead = true;
			ren.dun.score.setText("Score: " + p.score);
			ren.setScreen(ren.dun.deathMenu);
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
	
	static List<Entity> sortEntities(List<Entity> l){
		List<Entity> living = new();
		List<Entity> items = new();
		
		foreach(Entity e in l){
			if(e is Living){
				living.Add(e);
			}else{
				items.Add(e);
			}
		}
		
		items.AddRange(living);
		return items;
	}
	
	static Mesh createTilesMesh(int[] tiles, int x, int y, Random rand){
		List<float> ver = new();
		
		//Prevent weird issues
		float epsilon = 1e-3f;
		
		//vertex positions
		Vector2[] p = new Vector2[]{
			new Vector2(0.5f + epsilon, 0.5f + epsilon),
			new Vector2(-0.5f - epsilon, 0.5f + epsilon),
			new Vector2(-0.5f - epsilon, -0.5f - epsilon),
			new Vector2(0.5f + epsilon, 0.5f + epsilon),
			new Vector2(-0.5f - epsilon, -0.5f - epsilon),
			new Vector2(0.5f + epsilon, -0.5f - epsilon)
		};
		
		Vector2 invSpNum = new Vector2(1f/tileSpriteNum.X, 1f/tileSpriteNum.Y);
		
		Vector2[] r = new Vector2[]{
			new Vector2(invSpNum.X - epsilon, epsilon), //0
			new Vector2(epsilon, epsilon), //1
			new Vector2(epsilon, invSpNum.Y - epsilon), //2
			new Vector2(invSpNum.X - epsilon, invSpNum.Y - epsilon) //3
		};
		
		for(int i = 0; i < y; i++){
			for(int j = 0; j < x; j++){
				int m = tiles[i * x + j];
				
				//Air doesnt need anything
				if(m == 0){
					continue;
				}
				
				//Handle random rotations, these are indices for r
				int[] texCorners = Tile.withRandomRotation.Contains(m) ? rand.Next(4) switch{
					0 => new int[]{0, 1, 2, 0, 2, 3},
					1 => new int[]{1, 2, 3, 1, 3, 0},
					2 => new int[]{2, 3, 0, 2, 0, 1},
					3 => new int[]{3, 0, 1, 3, 1, 2},
					_ => new int[]{0, 1, 2, 0, 2, 3}
				} : new int[]{0, 1, 2, 0, 2, 3};
				
				m -= 1; //Adjust for texture index, as 0 is air
				
				Vector2 textureIndex = new Vector2i(m % tileSpriteNum.X, m / tileSpriteNum.X);
				textureIndex /= (Vector2) tileSpriteNum;
				
				for(int k = 0; k < 6; k++){
					ver.Add((float) j + p[k].X);
					ver.Add((float) i + p[k].Y);
					ver.Add(textureIndex.X + r[texCorners[k]].X);
					ver.Add(textureIndex.Y + r[texCorners[k]].Y);
				}
			}
		}
		
		return new Mesh("22", ver.ToArray(), PrimitiveType.Triangles, "tiles");
	}
	
	public void endLife(){
		cam.onViewChange -= setView;
	}
}