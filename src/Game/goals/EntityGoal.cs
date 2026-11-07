using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

abstract class EntityGoal{
	protected Living entity;
	
	public EntityGoal(Living e){
		entity = e;
	}
	
	public abstract bool isFinished(Scene sce);
	public abstract bool act(Scene sce);
	
	#if DEBUG_ENTITIES
		public virtual void drawDebug(Scene sce){
			
		}
	#endif
}