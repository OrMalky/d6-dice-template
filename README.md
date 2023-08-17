# Unity d6 Dice Template
Welcome to the d6 Dice Unity Template repository! This project provides a basic Unity template for physical six-sided dice (d6).  
You can use this template as a starting point to create your own dice-related games, simulations, or applications.

## Challanges
I set to make this little project since I came across some challanges of simulating a die:

### Returning Results
Since the die is physcally simulated, the results are not immidiate, rather, we only know the value of a die after its done rolling.  
On the other hand, functions in C# are resolved immidiately. Therefore, we cannot just call a function and have an integer return value with the result:  
```C#
int result = die.Roll(); // This cannot work
```

To solve this we must opt for asynchronized programming, in this case the use of Unity's `IEnumarator`.  
One solution is using an accessible variable to store the result, or make the user subscribe to an event, but these approaches are inelegant and are not intuitive to use for the user.

Instead, we wrap the `IEnumarator` with a normal function, and pass a callback function directly. This gives us a much more elegent and intuitive way to work with the die:
```C#
private void UseResult(int result)
{
  //...
}

private void RollDie() => die.Roll(UseResult);
```

Furthermore, for multiple dice, we have the `Dice Roller`, which makes rolling multiple dice as easy as rolling one. Instead of having to use another `IEnumarator` to wait for all the dice to finish rolling, and managing a collection of dice, this is all done inside the Roller. You can just pass a callback function:
```C#
private void UseResults(int[] results)
{
  //...
}

private void RollDice() => dice.RollAll(UseResults);
```

### Finding The Die's Value
Finding the value of a die is not simple. The most common solution is to use 6 different colliders, one for each side of the die. This is obviously inefficient and inelegant. Instead, I use linear algebra.  
Using Unity's built-in `transform.up`, `transform.forward` and `transform.right` we can get the vectors that represent the cube's side's normals in world space. Then we can calculate the dot product between these vectors and the `Vector3.up` (which is just `(0, 1, 0)`) to find the one with the smallest angle. This vector is the one pointing up, and since each of the vectors corospond to one side, this side is the one pointing up as well.

## Features

## Usage
### Setup
1. Make sure you have these in your Unity project:
   * azixMcAze's [SerializableDictionary](https://github.com/azixMcAze/Unity-SerializableDictionary) (can also be taken from [here](Assets/SerializableDictionary)).
   * [ReadOnlyPropertyDrawer](Assets/Editor/ReadOnlyPropertyDrawer) inside the Editor folder and [ReadOnlyAttribute](Assets/ReadOnlyAttributte.cs) anywhere.
2. Add a [Die](Assets/Die.cs) component to your die object. Make sure it also has a rigidbody (should be added if missing) and a box collider.
3. To use a Dice Roller, just add a [DiceRoller](Assets/DiceRoller.cs) component to any object, and asign the relevant dice.

## Acknowledgments
* **Dice Prefabs** - Armor and Rum's [Dice d6 game ready PBR](https://assetstore.unity.com/packages/3d/props/tools/dice-d6-game-ready-pbr-200151)
* **Serializable Dictionary** - azixMcAze's [SerializableDictionary](https://github.com/azixMcAze/Unity-SerializableDictionary)
* **Unity**
