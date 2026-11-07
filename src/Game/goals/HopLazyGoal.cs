using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Mathematics;

class HopLazyGoal : EntityGoal{
	public HopLazyGoal(Living e) : base(e){
		
	}
	
	public override bool isFinished(Scene sce){
		return false;
	}
	
	public override bool act(Scene sce){
		if(sce.randSerious.Next(2) == 0){
			return false;
		}
		
		Vector2i jumpTarget = entity.position + new Vector2i(sce.randSerious.Next(-1, 2), sce.randSerious.Next(-1, 2));
		if(entity.tryMove(jumpTarget, sce)){
			return true;
		}
		return false;
	}
}