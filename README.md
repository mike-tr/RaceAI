# RaceAI

Idk what to show for seminar so i want to make a racing game where the "enemies" would be car that learned how to drive via Genetic algorithm.
And i wanted to do it anyway, so 2 birds with 1 stone.

![Alt Text](https://github.com/mike-tr/RaceAI/blob/main/GitImages/CarGame.gif)

## Done
- Added Matrix Class that basically allows all matrix operations needed for neural networks, i.e Multiply Matrices, multiply be scalar, add scalar, devide,
  get random matrix etc...
- Added support for applying any (doube) -> (double) function on every entry of the matrix. (i.e apply activation function).
- Built a car model, track, trees, envirment in blender and added to game.
- Added Car Scripts times etc...
- Added CarAI neuralnetwork, as well as a save/load system.
- Add Sensors for AI
  - In this case simply raycast that tell the distance from "track borders" ( they are invisisble ).
- Add Genetic algorithm, and training processs.


## Need to do:
- Possible allow AI to hit other cars in training so they can avoid collitions ( In the inital stage each CarAI has ignored all other cars ).
- Add Respawns and detect off course position.
- Possibly add Directions into the AI-Brain, try to use the Neural networks as input, and add on top of it directions.

#### Made by Michael Trushlin.

Big thanks to the Imphenzia Youtube channel for the tutorials on Making Low Poly models, and tutorial to make a racing game.
( Reference https://www.youtube.com/watch?v=ODVV3eUE5zM&t=0s&ab_channel=Imphenzia )
