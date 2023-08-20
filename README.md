# Unity d6 Dice Template
Welcome to the d6 Dice Unity Template repository! This project provides a basic Unity template for physical six-sided dice (d6).  
You can use this template as a starting point to create your own dice-related games, simulations, or applications.

## Table of Content
* [Challanges](#challanges)
* [Setup](#setup)
* [Usage](#usage)
* [API](#api)
* [Acknowledgments](#acknowledgments)

## Challanges
I set to make this little project since I came across some challanges of simulating a die:

### Finding The Die's Value
Finding the value of a die is not simple. The most common solution is to use 6 different colliders, one for each side of the die. This is obviously inefficient and inelegant. Instead, I use linear algebra.  
Using Unity's built-in `transform.up`, `transform.forward` and `transform.right` we can get the vectors that represent the cube's axes in world space. Since two of theese three vectors should be relativly perpendicular to `Vector3.up`, and the third should be relativly parallel to it, we can search for the parallel one by calculating each vector's dot product with `Vector3.up`. By comparing the dot products we can find the one closet to either 1 or -1 (while the others will give values closer to 0), this one is the parallel one. After finding the correct axe, we just need to know if the side pointing up is the one facing the positive side of this vector on the negative, which we can find simply by checking if our dot product was positiove or negative.
<details>

<summary>
  Code
</summary>
  
  ```C#
private int CalculateValue()
{
    Vector3[] vectors = new Vector3[3] { transform.forward, transform.up, transform.right };
    float max = -1f;
    Side side = Side.Forward;

    for (int i = 0; i < vectors.Length; i++)
    {
        float dot = Vector3.Dot(vectors[i], Vector3.up);  // Calculate each vector dot product with Vector3.up
        if (Mathf.Abs(dot) >= max)    // Find the one closet to 1 or -1
        {
            max = Mathf.Abs(dot);
            side = dot < 0 ? (Side)(i + 3) : (Side)i;    // Save the correct side based of the positivty of the dot product
        }
    }
    return values[side];
}
  ```

</details>

### Returning Results
Since the die is physcally simulated, the results are not immidiate, rather, we only know the value of a die after its done rolling.  
On the other hand, functions in C# are usually resolved immidiately. Therefore, we cannot just use a normal function. To solve this we must opt for asynchronized programming.  
The most common solutions are using an `IEnumarator` with an accessible variable, a callback function or have the user subscribe to an event. those are less elegant and pretty inefficient.  
Instead, I used C# async functions. We define a new Task each time the die Roll is called, and use `await` to wait for it to finish.
When the task is finished (i.e. the die has stopped rolling) we simply calculate the value and return it.
<details>

<summary>
  Code
</summary>
  
  ```C#
public async Task<int> Roll(Vector3 torque, Vector3 force)
{
    // Physically roll the die
    rb.AddForce(force, ForceMode.Impulse);
    rb.AddTorque(torque, ForceMode.Impulse);

    if (!IsRolling)
    {
        rollTask = new TaskCompletionSource<bool>();
        isRolling = true;
        await rollTask.Task;
        value = CalculateValue();
        return value;
    }
    return 0;
}
  ```

</details>

The task itself is very simple and is updated via `FixedUpdate`. All it does is to check if the die's rigidbody is sleeping and IsRolling falg is true, if so we know that there was a roll which is now done.
<details>

<summary>
  Code
</summary>
  
  ```C#
private void FixedUpdate()
{
    if (IsRolling && rb.IsSleeping())
    {
        isRolling = false;
        rollTask?.SetResult(true);
    }
}
  ```

</details>


Furthermore, for multiple dice, we have the [`Dice Roller`](Assets/DiceRoller.cs), which makes rolling multiple dice as easy as rolling one. Instead of having to manage waiting for different roll tasks, or using an `IEnumerator` to wait for all the dice to finish rolling, this is all done for you inside the Roller.  
<details>

<summary>
  Code
</summary>
  
  ```C#
// Callable RollAll function to roll all dice
public async Task<int[]> RollAll(Vector3?[] torques = null, Vector3?[] forces = null)
{
    Task[] tasks = new Task[dice.Count];
    for (int i = 0; i < dice.Count; i++)
    {
        tasks[i] = RollOne(i, torques?[i], forces?[i]);  // Create a new task of rolling for each die
    }

    await Task.WhenAll(tasks);    // Waiting for all tasks to finish (i.e. all dice to finish rolling)
    return Values;
}
  ```

</details>

## Setup
1. Make sure you have these in your Unity project:
   * azixMcAze's [SerializableDictionary](https://github.com/azixMcAze/Unity-SerializableDictionary) (can also be taken from [here](Assets/SerializableDictionary)).
   * [ReadOnlyPropertyDrawer](Assets/Editor/ReadOnlyPropertyDrawer) inside an Editor folder and [ReadOnlyAttribute](Assets/ReadOnlyAttributte.cs) anywhere.
   * [Die.cs](Assets/Die.cs) and [DiceRoller.cs](DiceRoller.cs)
2. Add a [Die](Assets/Die.cs) component to your die object. Make sure it also has a rigidbody and a box collider.
3. To use a Dice Roller, just add a [DiceRoller](Assets/DiceRoller.cs) component to any object, and asign the relevant dice in the inspector.

## Usage Example
Notice that since all Roll functions are a sync (as explained [here](#returning-results)), in order to call them and wait for them properly, the calling function must be an async function as well.

### Simple Usage
```C#
public Die die;
public DiceRoller roller;
public Vector3 torque;
public Vector3 force;

// Rolling a die directly
private async void RollDieDirectly()
{
  int result = await die.Roll(torque, force);
  // Use result
}

// Rolling a die using DiceRoller
private async void RollDieUsingRoller()
{
  int result = await roller.RollOne();
  // Use result
}

// Rolling multiple dice using DiceRoller
private async void RollDice()
{
  int[] results = await roller.RollAll();
  // Use results
}
```

### Full Usage
```C#
public Die die;
public DiceRoller roller;
public Vector3 maxRollTorque;
public Vector3 minRollForce;
public Vector3 maxRollForce;

// Roll a die with random torque and force, and print the result
private async void RollRandomly()
{
    Vector3 torque = Random.insideUnitSphere * maxRollTorque;
    Vector3 force = Random.Range(minRollForce, maxRollForce) * Vector3.up
    int result = await die.Roll(torque, force);
    Debug.Log($"Die rolled {result}");
}

// Roll a specific die with a DiceRoller, and print the result
private async void RollSecondDie()
{
    int result = await roller.RollOne(1);
    Debug.Log(result);
}

// Roll dice and print the sum
private async void RollForSum(Vector3[] torques, Vector3[] forces)
{
  int results = await roller.RollAll(torques, forces);
  Debug.Log(results.Sum());
}
```

## API
The code is pretty well documented and explained, but this is a quick overview of this project's functionality

### Die
#### Properties 
* int **Value** - The value of the die. Setting this will rotate the die to the correct rotation.
* bool **IsRolling** - True if the die is rolling, False otherwise.

#### Methods
* _async_ Task<int> **Roll**(Vector3 torque, Vector3 force)  
  Roll the die with a given torque and force, and returns the result as an int.

### DiceRoller
#### Properties
* int[] **Values** - An array of integers represnting the value of each die.
* int **Sum** - The sum of all dice values.
* bool **IsRolling** - True if any die is rolling, False otherwise (no die is rolling).

#### Methods
* _async_ Task<int> **RollOne**(int index = 0, Vector3? torque = null, Vector3? force = null)  
  Roll a single die and returns the result.

* _async_ Task<int[]> **RollAll**(Vector3?[] torques = null, Vector3?[] forces = null)  
  Roll all dice and return the results as an array of ints.
  
* Die **GetDie**(int index = 0)  
  Get the die at the specified index.
  
* void **AddDie**(Die die)  
  Adds a die to the dice collection.
  
* void **RemoveDie**(int index)  
  Removes a die from the dice collection.
  
* void **SetDieValue**(int index, int value)  
  Sets the value of the die in the specified index.

* void **SetValues**(int[] targetValues)  
  Sets the values of all dice from an integer array.

## Acknowledgments
* **Dice Prefabs** - Armor and Rum's [Dice d6 game ready PBR](https://assetstore.unity.com/packages/3d/props/tools/dice-d6-game-ready-pbr-200151)
* **Serializable Dictionary** - azixMcAze's [SerializableDictionary](https://github.com/azixMcAze/Unity-SerializableDictionary)
* **Unity**
