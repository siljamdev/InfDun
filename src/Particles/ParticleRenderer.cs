using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class ParticleRenderer{
	static protected Random rand = new Random();
	
	public static bool particlesEnabled = true;
	
	List<Particle> pars;
	
	public ParticleRenderer(){
		pars = new();
	}
	
	public void add(Particle p){
		if(!particlesEnabled){
			return;
		}
		
		pars.Add(p);
	}
	
	public void addMultiple(Vector2 center, float range, int count, Func<Particle> factory){
		if(!particlesEnabled){
			return;
		}
		
		for(int i = 0; i < count; i++){
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
		if(!particlesEnabled){
			return;
		}
		
		List<Particle> del = new();
		foreach(Particle p in pars){
			if(!p.draw(ren)){
				del.Add(p);
			}
		}
		
		pars.RemoveAll(n => del.Contains(n));
	}
}