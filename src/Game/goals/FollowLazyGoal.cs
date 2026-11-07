using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class FollowLazyGoal : FollowGoal{
	public FollowLazyGoal(Living e, Living f, float o, Scene sce) : base(e, f, o, sce){
		
	}
	
	public override bool isFinished(Scene sce){
		if(following.isDying || !sce.entitiesLiving.Contains(following)){
			return true;
		}
		return false;
	}
	
	public override bool act(Scene sce){
		if(sce.randSerious.Next(2) == 0){
			return false;
		}
		
		return base.act(sce);
	}
}