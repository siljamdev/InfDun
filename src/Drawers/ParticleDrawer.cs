using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class ParticleDrawer{
	public static int maxParticles = 1000;
	
	static Shader particleShader;
	
	public bool enabled {get; private set;} = true;
	List<Particle> pars = new();
	Random rand = new();
	Mesh mesh;
	
	public ParticleDrawer(){
		mesh = new Mesh(Particle.dataFormat, maxParticles, PrimitiveType.Points, "particles");
		
		particleShader ??= Shader.fromAssembly("shaders.particle");
	}
	
	public void setEnabled(bool b){
		enabled = b;
		if(!b){
			clear();
		}
	}
	
	public void add(Particle p){
		if(!enabled || pars.Count >= maxParticles){
			return;
		}
		
		pars.Add(p);
	}
	
	public void addMultiple(Vector2 center, float range, int count, Func<Particle> factory){
		if(!enabled || pars.Count >= maxParticles){
			return;
		}
		
		for(int i = 0; i < count && pars.Count < maxParticles; i++){
			Particle p = factory?.Invoke();
			if(p == null){
				continue;
			}
			
			p.position = center + new Vector2(((float)rand.NextDouble() - 0.5f) * range, ((float)rand.NextDouble() - 0.5f) * range);
			
			pars.Add(p);
		}
	}
	
	public void clear(){
		pars.Clear();
	}
	
	public void draw(Renderer ren){
		if(!enabled){
			return;
		}
		
		//Update
		for(int i = 0; i < pars.Count; i++){
			if(!pars[i].update()){
				pars.RemoveAt(i);
				i--;
			}
		}
		
		//Draw
		float[] data = new float[pars.Count * Particle.dataSize];
		
		for(int i = 0; i < pars.Count; i++){
			pars[i].writeData(data, Particle.dataSize * i);
		}
		
		mesh.addDynamicData(data);
		
		particleShader.use();
		particleShader.setMatrix4("view",  ren.view);
		particleShader.setFloat("zoom", ren.zoom);
		particleShader.setMatrix4("projection", ren.projection);
		
		mesh.draw();
	}
}