using System;

class Animation{
	public int frame => frameSum + fram;
	public int frameNum{get; private set;}
	
	public int fram;
	int frameSum;
	
	public double time;
	public double maxTime;
	
	public Animation(int fn, int startFrame, double max){
		frameSum = startFrame;
		frameNum = fn;
		
		maxTime = max;
		
		reset();
	}
	
	public void tick(double dt){
		if(frameNum == 0 || frameNum == 1){
			return;
		}
		
		time += dt;
		
		if(time > maxTime){
			time = 0d;
			fram++;
			fram %= frameNum;
		}
	}
	
	public void reset(){
		fram = 0;
		
		time = 0d;
	}
}