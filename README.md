# Lab Summary
<br>The A* algorithm is developed on the base of [this](https://www.youtube.com/watch?v=-L-WgKMFuhE&list=PLFt_AvWsXl0cq5Umv3pMC9SPnKjfp9eGW&index=1)
tutorial (Note that the heuristic method isn’t used in the code of this script.
<br>To better understand the heuristic and A* see [here](http://theory.stanford.edu/~amitp/GameProgramming/).
## Steps for using the project
1.	Add object and target in the manager script, check manager script is attached to the game object manager, and A* script is attached to game object A*.
<br>2.	Add object in unity, in this scene, there are 2 types of game object
	* 3D object for obstacle, walls, and floor (Create->3D object->Cube)
	* Empty game object for A*, Manager, Target (Create->Create Empty)
<br>3.	Attach the Unit script to the object, drag the target to the holder of “Target” in unit script
<br>4.	Set all the collideable object’s layer to a special layer in the inspector window, including floor, objects, target (in my project the layer is called Unwalkable Layer
<br>5.	Drag different object to the holder of “object” and “target” in game object manager’s manager script, select the layer for the unwalkable layer
<br>6.	Note that the target should be carefully selected to avoid collision in the end
