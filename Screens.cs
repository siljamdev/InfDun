using System.Diagnostics;
using OpenTK.Mathematics;
using AshLib;

using Keys = OpenTK.Windowing.GraphicsLibraryFramework.Keys;

partial class Dungeon{
	UiText highscor;
	UiScreen mainMenu;
	
	UiScreen infoMenu;
	
	UiScreen helpMenu;
	
	UiScreen optionsMenu = null!;
	UiCheck vsync;
	UiField maxFpsF;
	UiCheck sound;
	UiCheck particles;
	
	UiScreen controlsMenu = null!;
	
	UiScreen pauseMenu;
	
	public UiScreen deathMenu;
	public UiText score;
	UiText high;
	
	void initializeScreens(){
		highscor = new UiText(Placement.BottomCenter, 0f, 20f, "Highscore: -1", new Color3("FFFF70"));
		
		mainMenu = new UiScreen(
			new UiImage(Placement.TopCenter, 0, 20, 110, 110, "icon", Color3.White),
			highscor,
			new UiButton(Placement.Center, 0, -1f * Renderer.separation, 300f, "Play", Renderer.buttonColor).setAction(setNewScene),
			new UiButton(Placement.Center, 0, 0, 300f, "Options", Renderer.buttonColor).setAction(() => ren.setScreen(optionsMenu)),
			new UiButton(Placement.Center, 0, 2f * Renderer.separation, 300f, "Exit", Renderer.greenButtonColor).setAction(Close),
			new UiImageButton(Placement.BottomRight, 0, 0, 35f, 35f, "info", Renderer.textColor).setDescription("Info").setAction(() => ren.setScreen(infoMenu)),
			new UiImageButton(Placement.BottomLeft, 0, 0, 35f, 35f, "help", Renderer.textColor).setDescription("Help").setAction(() => ren.setScreen(helpMenu))
		);
		
		infoMenu = new UiScreen(
			new UiText(Placement.TopCenter, 0f, 20f, "Info", Renderer.titleTextColor),
			new UiText(Placement.TopCenter, 0f, 3f * Renderer.textSize.Y, "Dungeon Game, created by siljamdev", Renderer.selectedTextColor),
			new UiText(Placement.TopCenter, 0f, 4f * Renderer.textSize.Y, "Version v" + version, Renderer.selectedTextColor),
			new UiText(Placement.TopCenter, 0f, 6f * Renderer.textSize.Y, "Based on Generic game template by siljamdev", Renderer.selectedTextColor),
			new UiButton(Placement.BottomCenter, 0f, 3f * Renderer.separation, 300f, "GitHub", Renderer.buttonColor).setAction(() => openUrl("https://github.com/siljamdev/InfDun")),
			new UiButton(Placement.BottomCenter, 0f, 1f * Renderer.separation, 300f, "Close", Renderer.redButtonColor).setAction(ren.closeScreen)
		).setScrollingLog(new UiLog(20f, 20f, 8f * Renderer.textSize.Y, Renderer.textColor,
					"NO GENERATIVE AI WAS USED IN THE CREATION OF THE ASSETS OF THIS GAME",
					"AI was only used to write small, unimportant and repetitive parts of the game's code, and was always reviewed and tweaked by me (a human) afterwards",
					"All textures were created by hand using the amazing paint.net",
					"All sounds were either recorded by me or downloaded from freesound.org"
		));
		
		helpMenu = new UiScreen(
			new UiText(Placement.TopCenter, 0f, 20f, "Help", Renderer.titleTextColor),
			new UiButton(Placement.BottomCenter, 0f, 1f * Renderer.separation, 300f, "Close", Renderer.redButtonColor).setAction(ren.closeScreen)
		).setScrollingLog(new UiLog(20f, 20f, 3f * Renderer.textSize.Y, Renderer.textColor,
					"This game is a turn based dungeon crawler",
					"You control the player, and your objective is getting the most points before dying",
					"The game works on turns, each turn you can do one action(either moving, or attacking), and then, other creatures will do their turn",
					"You start on level 0, and can go to the next by finding the stairs that go down. Levels get progressively harder",
					"You start with 10 lives and you can increase the maximum with magical orbs or with altars",
					"You get points by collecting coins, killing creatures and passing to the next level",
					"Try to get the best highscore and challenge your friends"
		));
		
		vsync = new UiCheck(Placement.Center, 0f, -2f * Renderer.separation, Renderer.textSize.Y + 10f, Renderer.textSize.Y + 10f, "Vsync:", config.GetValue<bool>("vsync"), Renderer.buttonColor);
		maxFpsF = new UiField(Placement.Center, 0f, -1f * Renderer.separation, 30f, "Max FPS:", maxFps.ToString(), 6, WritingType.FloatPositive);
		sound = new UiCheck(Placement.Center, 0f, 0f * Renderer.separation, Renderer.textSize.Y + 10f, Renderer.textSize.Y + 10f, "Sound:", config.GetValue<bool>("sound"), Renderer.buttonColor);
		particles = new UiCheck(Placement.Center, 0f, 1f * Renderer.separation, Renderer.textSize.Y + 10f, Renderer.textSize.Y + 10f, "Particles:", config.GetValue<bool>("particles"), Renderer.buttonColor);
		
		optionsMenu = new UiScreen(
			new UiText(Placement.TopCenter, 0f, 20f, "Options", Renderer.titleTextColor),
			vsync,
			maxFpsF,
			sound,
			particles,
			new UiButton(Placement.BottomCenter, 0f, 3f * Renderer.separation, 300f, "Controls", Renderer.buttonColor).setAction(() => ren.setScreen(controlsMenu)),
			new UiButton(Placement.BottomCenter, 0f, 2f * Renderer.separation, 300f, "Save", Renderer.greenButtonColor).setAction(() => {
				config.Set("vsync", vsync.on);
				
				if(float.TryParse(maxFpsF.text, out float f)){
					config.Set("maxFps", f);
					optionsMenu.showError("");
				}else{
					optionsMenu.showError("Invalid max fps number");
				}
				
				config.Set("sound", sound.on);
				config.Set("particles", particles.on);
				
				config.Save();
				
				setVsync(config.GetValue<bool>("vsync"));
				maxFps = config.GetValue<float>("maxFps");
				this.UpdateFrequency = maxFps;
				sm.isActive = config.GetValue<bool>("sound");
				ParticleRenderer.isActive = config.GetValue<bool>("particles");
			}),
			new UiButton(Placement.BottomCenter, 0f, 1f * Renderer.separation, 300f, "Close", Renderer.redButtonColor).setAction(ren.closeScreen)
		).setErrorText(new UiText(Placement.BottomCenter, 0f, 200f, "", Renderer.redTextColor));
		
		controlsMenu = new UiScreen(
			new UiText(Placement.TopCenter, 0f, 20f, "Controls", Renderer.titleTextColor),
			new UiKeyField(Placement.Center, 0f, -4f * Renderer.separation, "Fullscreen:", fullscreen),
			new UiKeyField(Placement.Center, 0f, -3f * Renderer.separation, "Screenshot:", screenshot),
			new UiKeyField(Placement.Center, 0f, -2f * Renderer.separation, "Advanced mode:", advancedMode),
			new UiKeyField(Placement.Center, 0f, -1f * Renderer.separation, "Move up:", moveUp),
			new UiKeyField(Placement.Center, 0f, 0f, "Move down:", moveDown),
			new UiKeyField(Placement.Center, 0f, 1f * Renderer.separation, "Move left:", moveLeft),
			new UiKeyField(Placement.Center, 0f, 2f * Renderer.separation, "Move right:", moveRight),
			new UiButton(Placement.BottomCenter, 0f, 2f * Renderer.separation, 300f, "Save", Renderer.greenButtonColor).setAction(saveControls),
			new UiButton(Placement.BottomCenter, 0f, 1f * Renderer.separation, 300f, "Close", Renderer.redButtonColor).setAction(ren.closeScreen)
		).setErrorText(new UiText(Placement.BottomCenter, 0f, 200f, "", Renderer.redTextColor));
		
		pauseMenu = new UiScreen(
			new UiText(Placement.TopCenter, 0f, 20f, "Pause", Renderer.titleTextColor),
			new UiButton(Placement.Center, 0f, -1f * Renderer.separation, 300f, "Close", Renderer.redButtonColor).setAction(ren.closeScreen),
			new UiButton(Placement.Center, 0f, 0f, 300f, "Options", Renderer.buttonColor).setAction(() => ren.setScreen(optionsMenu)),
			new UiButton(Placement.Center, 0f, 2f * Renderer.separation, 300f, "Exit to menu", Renderer.greenButtonColor).setAction(closeScene),
			new UiImageButton(Placement.CenterLeft, 0f, 0f, 35f, 35f, "screenshot", Renderer.textColor).setDescription("Take screenshot").setAction(() => {
				ren.closeScreen();
				takeScreenshotNextTick = true;
			})
		);
		
		score = new UiText(Placement.Center, 0f, 10f, "Score: -1", Renderer.textColor).setCharSize(new Vector2(22f, 28f));
		high = new UiText(Placement.Center, 0f, 50f, "Highscore: -1", new Color3("FFFF70"));
		
		deathMenu = new UiScreen(
			new UiText(Placement.Center, 0f, -60f, "You died", Color3.Red).setCharSize(Renderer.bigTextSize),
			score,
			high,
			new UiButton(Placement.BottomCenter, -120f, 1f * Renderer.separation, 200f, "Return to menu", Renderer.redButtonColor).setAction(closeScene),
			new UiButton(Placement.BottomCenter, 120f, 1f * Renderer.separation, 200f, "Play again", Renderer.redButtonColor).setAction(setNewScene)
		);
	}
	
	void saveControls(){
		int[] k = new List<Keys>{
			fullscreen.key,
			screenshot.key,
			advancedMode.key,
			moveUp.key,
			moveDown.key,
			moveLeft.key,
			moveRight.key
		}.Select(n => (int) n).ToArray();
		
		HashSet<Keys> seen = new HashSet<Keys>();
		
		foreach(Keys key in k){
			if(!seen.Add(key)){
				controlsMenu.showError("Conflict found");
				return;
			}
		}
		
		controlsMenu.showError("");
		
		config.Set("controls", k);
		
		config.Save();
	}
	
	static void openUrl(string url){
		try{
			if(OperatingSystem.IsWindows()){
				Process.Start(new ProcessStartInfo{
					FileName = url,
					UseShellExecute = true
				});
			}
			else if(OperatingSystem.IsLinux()){
				Process.Start("xdg-open", url);
			}
			else if(OperatingSystem.IsMacOS()){
				Process.Start("open", url);
			}
		}
		catch(Exception e){}
	}
}