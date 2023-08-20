using System;
using System.Linq;
using System.Threading.Tasks;
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

    private void Start() => dice.Keys.ToList()
        .ForEach(die => die.OnValueChanged.AddListener(() => dice[die] = die.Value));

    /// <summary>
    /// Roll all dice.
    /// </summary>
    /// <param name="torques"> An array of torques to apply to each die (null for default). </param>
    /// <param name="forces"> An array of forces to apply to each die (null for default). </param>
    /// <returns> An array of ints representing each die's result. </returns>
    public async Task<int[]> RollAll(Vector3?[] torques = null, Vector3?[] forces = null)
    {
        Task[] tasks = new Task[dice.Count];
        for (int i = 0; i < dice.Count; i++)
        {
            tasks[i] = RollOne(i, torques?[i], forces?[i]);
        }

        await Task.WhenAll(tasks);
        return Values;
    }

    /// <summary>
    /// Roll one die.
    /// </summary>
    /// <param name="index"> Index of a die to roll (default is 0). </param>
    /// <param name="torque"> A torque to apply to the die. </param>
    /// <param name="force"> A force to apply to the die.</param>
    /// <returns></returns>
    public async Task<int> RollOne(int index = 0, Vector3? torque = null, Vector3? force = null)
    {
        Die die = dice.Keys.ElementAt(index);
        torque ??= Random.insideUnitSphere * maxRollTorque;
        force ??= Random.Range(minRollForce, maxRollForce) * Vector3.up;
        int result = await die.Roll((Vector3)torque, (Vector3)force);
        return result;
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
}