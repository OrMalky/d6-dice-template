# Unity d6 Dice Template
Welcome to the d6 Dice Unity Template repository! This project provides a basic Unity template for physical six-sided dice (d6).  
You can use this template as a starting point to create your own dice-related games, simulations, or applications.

## Challanges
I set to make this little project since I came across some challanges of simulating a die:

### Finding The Die's Value
Finding the value of a die is not simple. The most common solution is to use 6 different colliders, one for each side of the die. This is obviously inefficient and inelegant. Instead, I use linear algebra.
Using Unity's built-in `transform.up`, `transform.forward` and `transform.right` we can get the vectors that represent the cube's side's normals in world space.
<details>

<summary>
  Code
</summary>
  
  ```C#
  // Map world space vector to side on the die
Dictionary<Vector3, Side> sides = new()
{
    { transform.forward, Side.Forward },
    { transform.up, Side.Up },
    { -transform.right, Side.Left },
    { transform.right, Side.Right },
    { -transform.up, Side.Down },
    { -transform.forward, Side.Back }
};
  ```

</details>

Then we can calculate the dot product between each of these vectors and the `Vector3.up` (which is just `(0, 1, 0)`), to find the one with the smallest angle. This vector is the one pointing up, and since each of the vectors corospond to one side, this side is the one pointing up as well.
<details>

<summary>
  Code
</summary>
  
  ```C#
// Find the side with the highest dot product with Vector3.up
int value = 0;
float max = -1f;
foreach (var side in sides)
{
    float dot = Vector3.Dot(side.Key, Vector3.up);
    if (dot > max)
    {
        max = dot;
        value = values[side.Value];
    }
}
return value;
  ```

</details>

### Returning Results
Since the die is physcally simulated, the results are not immidiate, rather, we only know the value of a die after its done rolling.  
On the other hand, functions in C# are resolved immidiately. Therefore, we cannot just call a function and have an integer return value with the result:  
```C#
int result = die.Roll(); // This cannot work
```

To solve this we must opt for asynchronized programming, in this case the use of Unity's `IEnumerator`.  
One solution is using an accessible variable to store the result, or make the user subscribe to an event, but these approaches are inelegant and are not intuitive to use for the user.  
Instead, I decided to wrap the `IEnumerator` with a normal function, and pass a callback function directly.  
<details>

<summary>
  Code
</summary>
  
  ```C#
// Callable Roll function of the die
public void Roll(Vector3 torque, Vector3 force, ICollection<System.Action<int>> callbacks)
{
    // Physically roll the die
    rb.AddForce(force, ForceMode.Impulse);
    rb.AddTorque(torque, ForceMode.Impulse);

    if (rollInstance == null)
    {
        isRolling = true;
        rollInstance = StartCoroutine(HandleRoll(callbacks));   // Start coroutine to handle the result
    }
}

// Enumerator to wait for result and call callbacks
private IEnumerator HandleRoll(ICollection<System.Action<int>> callbacks)
{
   while (isRolling)
   {
       yield return new WaitForFixedUpdate();
       if (CheckStopped())
       {
           isRolling = false;
           value = CalculateValue();
           foreach (System.Action<int> callback in callbacks ?? new List<System.Action<int>>())
           {
               callback?.Invoke(value);
           }
       }
       else
       {
           lastPosition = transform.position;
           lastRotation = transform.rotation;
       }
   }
   rollInstance = null;
}
  ```

</details>

This gives us a much more elegent and intuitive way to work with the die:
```C#
public Die die;

private void UseResult(int result)
{
  //...
}

private void RollDie() => die.Roll(UseResult);
```


Furthermore, for multiple dice, we have the [`Dice Roller`](Assets/DiceRoller.cs), which makes rolling multiple dice as easy as rolling one. Instead of having to use another `IEnumerator` to wait for all the dice to finish rolling, and managing a collection of dice, this is all done inside the Roller.  
<details>

<summary>
  Code
</summary>
  
  ```C#
// Callable RollAll function to roll all dice
public void RollAll(ICollection<Action<int[]>> callbacks, Vector3?[] torques = null, Vector3?[] forces = null)
{
    // Just rolls each die with no callback
    for (int i = 0; i < dice.Count; i++)
    {
        RollOne((Action<int>)null, i, torques?[i], forces?[i]);
    }
    StartCoroutine(HandleRoll(callbacks));
}

// Enumerator that waits for the dice to stop rolling, and then calls the callback functions
private IEnumerator HandleRoll(ICollection<Action<int[]>> callbacks)
{
    while (IsRolling)
    {
        yield return new WaitForFixedUpdate();
    }

    callbacks?.ToList().ForEach(callback => callback?.Invoke(Values));
}
  ```

</details>

This makes so it so rolling multiple dice is done the same as rolling a single die:
```C#
private void UseResults(int[] results)
{
  //...
}

private void RollDice() => dice.RollAll(UseResults);
```

## Setup
1. Make sure you have these in your Unity project:
   * azixMcAze's [SerializableDictionary](https://github.com/azixMcAze/Unity-SerializableDictionary) (can also be taken from [here](Assets/SerializableDictionary)).
   * [ReadOnlyPropertyDrawer](Assets/Editor/ReadOnlyPropertyDrawer) inside an Editor folder and [ReadOnlyAttribute](Assets/ReadOnlyAttributte.cs) anywhere.
2. Add a [Die](Assets/Die.cs) component to your die object. Make sure it also has a rigidbody and a box collider.
3. To use a Dice Roller, just add a [DiceRoller](Assets/DiceRoller.cs) component to any object, and asign the relevant dice in the inspector.

## API
The code is pretty well documented and explained, but this is a quick overview of this project's functionality

### Die
#### Properties 
* int **Value** - The value of the die. Setting this will rotate the die to the correct rotation.
* bool **IsRolling** - True if the die is rolling, False otherwise.

#### Methods
* void **Roll**(Vector3 torque, Vector3 force, System.Action\<int> callback = null)  
  Rolls the die with a given torque and force, and calls the callback function with the result.
  
* void **Roll**(Vector3 torque, Vector3 force, ICollection<System.Action\<int>> callbacks)  
  Rolls the die with a given torque and force, and calls the callback function with the result.

### DiceRoller
#### Properties
* int[] **Values** - An array of integers represnting the value of each die.
* int **Sum** - The sum of all dice values.
* bool **IsRolling** - True if any die is rolling, False otherwise (no die is rolling).

#### Methods
* void **RollOne**(ICollection<Action<int>> callbacks, int index = 0, Vector3? torque = null, Vector3? force = null)  
  Rolls a single die and calls each of the callback functions in the collection with the result.

* void **RollOne**(Action<int> callback = null, int index = 0, Vector3? torque = null, Vector3? force = null)  
  Rolls a single die and calls the callback function with the result.

* void **RollAll**(ICollection<Action<int[]>> callbacks, Vector3?[] torques = null, Vector3?[] forces = null)  
  Rolls all dice and calls all callback functions with the results.

* void **RollAll**(Action<int[]> callback = null, Vector3?[] torques = null, Vector3?[] forces = null)  
  Rolls all dice and calls the callback function with the results.

* Die **GetDie**(int index = 0)  
  Get the die at the specified index
  
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
