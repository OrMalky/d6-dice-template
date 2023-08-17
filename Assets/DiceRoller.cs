using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[System.Serializable]
public class Dice : SerializableDictionary<Die, int> { }

public class DiceRoller : MonoBehaviour
{
    [SerializeField] private Dice dice;

    // Properties
    public int Sum => Values.Sum();
    public int[] Values => dice.Values.ToArray();
    public bool IsRolling => CheckRolling();

    [Header("Default Roll Settings")]
    [SerializeField][Range(0f, 100f)] private float maxRollTorque = 10f;
    [SerializeField][Range(0f, 100f)] private float minRollForce = 5f;
    [SerializeField][Range(0f, 100f)] private float maxRollForce = 10f;

    private void Start()
    {
        ReadValues();
    }

    /// <summary>
    /// Rolls all dice and calls all callback functions with the results.
    /// The results will be passed as an array of integers.
    /// </summary>
    /// <param name="callbacks"> A collection of actions to be called with the results. </param>
    /// <param name="torques"> An array of torques to be applied to each die (null for random). </param> 
    /// <param name="forces"> An array of forces to be applied to each die (null for random). </param> 
    public void RollAll(ICollection<Action<int[]>> callbacks, Vector3?[] torques = null, Vector3?[] forces = null)
    {
        if (IsRolling)
        {
            Debug.LogError("Dice are already rolling");
            return;
        }

        for (int i = 0; i < dice.Count; i++)
        {
            RollOne((Action<int>)null, i, torques?[i], forces?[i]);
        }
        StartCoroutine(HandleRoll(callbacks));
    }

    /// <summary>
    /// Rolls all dice and calls the callback function with the results.
    /// The results will be passed as an array of integers.
    /// </summary>
    /// <param name="callback"> An action to be called with the results. </param>
    /// <param name="torques"> An array of torques to be applied to each die (null for random). </param> 
    /// <param name="forces"> An array of forces to be applied to each die (null for random). </param> 
    public void RollAll(Action<int[]> callback = null, Vector3?[] torques = null, Vector3?[] forces = null)
    {
        RollAll((List<Action<int[]>>)(callback == null ? new() : new() { callback }), torques, forces);
    }

    /// <summary>
    /// Rolls a single die and calls each of the callback functions with the result.
    /// </summary>
    /// <param name="callbacks"> A collection of actions to be called with the result. </param>
    /// <param name="index"> Index of the die to be rolled (0 by default) </param>
    /// <param name="torque"> Torque to be applied to the die (null for random) </param>
    /// <param name="force"> Force to be applied to the die (null for random) </param>
    public void RollOne(ICollection<Action<int>> callbacks, int index = 0, Vector3? torque = null, Vector3? force = null)
    {
        if (index < 0 || index >= dice.Count)
        {
            Debug.LogError("Index out of range");
            return;
        }

        Die die = GetDie(index);
        if (!die.IsRolling)
        {
            callbacks = callbacks?.Count == 0 ? new List<Action<int>>() : callbacks;
            callbacks?.Add((result) => dice[die] = result);
            torque ??= Random.insideUnitSphere * maxRollTorque;
            force ??= Random.Range(minRollForce, maxRollForce) * Vector3.up;
            die.Roll((Vector3)torque, (Vector3)force, callbacks);
        }
        else
        {
            Debug.LogError("Die already rolling");
        }
    }

    /// <summary>
    /// Rolls a single die and calls the callback function with the result.
    /// </summary>
    /// <param name="callback"> An action to be called with the result. </param>
    /// <param name="index"> Index of the die to be rolled (0 by default) </param>
    /// <param name="torque"> Torque to be applied to the die (null for random) </param>
    /// <param name="force"> Force to be applied to the die (null for random) </param>
    public void RollOne(Action<int> callback = null, int index = 0, Vector3? torque = null, Vector3? force = null)
    {
        RollOne((List<Action<int>>)(callback == null ? new() : new() { callback }), index, torque, force);
    }

    /// <summary>
    /// Get the die at the specified index.
    /// </summary>
    /// <param name="index"> The index of the die. </param>
    /// <returns> The <see cref="Die"/> of the relevant die. </returns>
    public Die GetDie(int index = 0)
    {
        if (index < 0 || index >= dice.Count)
        {
            Debug.LogError("Index out of range");
            return null;
        }

        return dice.Keys.ElementAt(index);
    }

    /// <summary>
    /// Sets the values of all dice.
    /// </summary>
    /// <param name="targetValues"> An array of values to be set. </param>
    public void SetValues(int[] targetValues)
    {
        if (targetValues.Length != dice.Count)
        {
            Debug.LogError("Target values array length does not match dice count");
            return;
        }

        for (int i = 0; i < targetValues.Length; i++)
        {
            Die die = dice.Keys.ElementAt(i);
            die.Value = targetValues[i];
            dice[die] = targetValues[i];
        }
    } 

    /// <summary>
    /// Sets the value of a single die.
    /// </summary>
    /// <param name="index"> The index of the die to be set. </param>
    /// <param name="value"> The value to set the die to. </param>
    public void SetDieValue(int index, int value)
    {
        if (index < 0 || index >= dice.Count)
        {
            Debug.LogError("Index out of range");
            return;
        }

        Die die = GetDie(index);
        if (die == null)
        {
            Debug.LogError("Die is null");
            return;
        }

        die.Value = value;
        dice[die] = value;
    }

    /// <summary>
    /// Adds a die to the dice collection.
    /// </summary>
    /// <param name="die"> The die to add. </param>
    public void AddDie(Die die) => dice.Add(die, die.Value);

    /// <summary>
    /// Removes a die from the dice collection.
    /// </summary>
    /// <param name="index"> The index of the die to remove. </param>
    public void RemoveDie(int index) => dice.Remove(GetDie(index));


    /// <summary>
    /// Calls for each of the callback functions when all dice have stopped rolling.
    /// </summary>
    /// <param name="callbacks"> A collection of callback functions. </param>
    /// <returns></returns>
    private IEnumerator HandleRoll(ICollection<Action<int[]>> callbacks)
    {
        while (IsRolling)
        {
            yield return new WaitForFixedUpdate();
        }

        callbacks?.ToList().ForEach(callback => callback?.Invoke(Values));
    }

    /// Checks if any die is rolling.
    /// <returns><see cref="True"/> if any die is rolling, <see cref="False"/> otherwise</returns> 
    private bool CheckRolling()
    {
        foreach (Die die in dice.Keys)
        {
            if (die.IsRolling)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Recreats the dice collection with the current dice values.
    /// </summary>
    private void ReadValues()
    {
        Dice newDice = new Dice();
        foreach (Die die in dice.Keys)
        {
            newDice.Add(die, die.Value);
        }
        dice = newDice;
    }
}
