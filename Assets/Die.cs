using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public enum Side
{
    [Description("Forward")] Forward,
    [Description("Up")] Up,
    [Description("Right")] Right,
    [Description("Back")] Back,
    [Description("Down")] Down,
    [Description("Left")] Left
}

[System.Serializable]
public class SideValueDictionary : SerializableDictionary<Side, int> { }

[RequireComponent(typeof(Rigidbody))]
public class Die : MonoBehaviour
{
    [Header("State")]
    [CustomAttributes.ReadOnly][SerializeField] private int value;
    [CustomAttributes.ReadOnly][SerializeField] private bool isRolling = false;

    [SerializeField]
    private SideValueDictionary values = new() //Maps bwteen side and value. This can be static and readonly if all your dice have the same values.
    {
        { Side.Forward, 1 },
        { Side.Up, 2 },
        { Side.Left, 3 },
        { Side.Right, 4 },
        { Side.Down, 5 },
        { Side.Back, 6 }
    };

    // Members
    private TaskCompletionSource<bool> rollTask;
    private Coroutine rollInstance;
    private Rigidbody rb;

    private static readonly Dictionary<Side, Quaternion> sideToRotation = new() // Used to convert a side to a rotation
    {
        { Side.Forward, Quaternion.LookRotation(Vector3.up, Vector3.forward) },
        { Side.Up, Quaternion.LookRotation(Vector3.forward, Vector3.up) },
        { Side.Left, Quaternion.LookRotation(Vector3.left, Vector3.forward) },
        { Side.Right, Quaternion.LookRotation(Vector3.forward, Vector3.left) },
        { Side.Down, Quaternion.LookRotation(Vector3.forward, Vector3.down) },
        { Side.Back, Quaternion.LookRotation(Vector3.down, Vector3.forward) }
    };

    // Properties
    public int Value { get => value; set => SetValue(value); }
    public bool IsRolling => isRolling;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        value = CalculateValue();
    }

    private void FixedUpdate()
    {
        if (IsRolling && rb.IsSleeping())
        {
            isRolling = false;
            rollTask?.SetResult(true);
        }
    }

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


    /// <summary>
    /// Enumerator that awaits for the die to stop rolling, and handles the result.
    /// </summary>
    /// <param name="callbacks"> A collection of actions to be called with the result.</param>
    private IEnumerator HandleRoll(ICollection<System.Action<int>> callbacks)
    {
        while (isRolling)
        {
            yield return new WaitForFixedUpdate();
            if (rb.IsSleeping())
            {
                //isRolling = false;
                value = CalculateValue();
                foreach (System.Action<int> callback in callbacks ?? new List<System.Action<int>>())
                {
                    callback?.Invoke(value);
                }
            }
        }
        rollInstance = null;
    }

    /// <summary>
    /// Rolls the die and calls the callback function with the result.
    /// </summary>
    /// <param name="callback">Action to be called when the die has stopped rolling.</param>
    /// <param name="torque">Torque to be applied to the die.</param>
    /// <param name="force">Force to be applied to the die.</param>
    public void Roll(Vector3 torque, Vector3 force, System.Action<int> callback = null)
    {
        Roll(torque, force, callback == null ? null : new List<System.Action<int>>() { callback });
    }

    /// <summary>
    /// Rolls the die and calls each of the callback functions with the result.
    /// </summary>
    /// <param name="torque"> Torque to be applied to the die. </param>
    /// <param name="force"> Force to be applied to the die. </param>
    /// <param name="callbacks"> A collection of actions to be called with the result. </param>
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

    /// <summary>
    /// Calculates the value of the die based on its rotation.
    /// </summary>
    /// <returns> <see cref="int"/> The value of the die.</returns>
    private int CalculateValue()
    {
        Vector3[] vectors = new Vector3[3] { transform.forward, transform.up, transform.right };
        float max = -1f;
        Side side = Side.Forward;

        for (int i = 0; i < vectors.Length; i++)
        {
            float dot = Vector3.Dot(vectors[i], Vector3.up);
            if (Mathf.Abs(dot) >= max)
            {
                max = Mathf.Abs(dot);
                side = dot < 0 ? (Side)(i + 3) : (Side)i;
            }
        }
        return values[side];
    }

    /// <summary>
    /// Sets the value of the die. Automatically rotates the die to the correct rotation.
    /// </summary>
    /// <param name="targetValue"> Value to be set. </param>
    private void SetValue(int targetValue)
    {
        foreach (var pair in values)
        {
            if (pair.Value == targetValue)
            {
                transform.rotation = sideToRotation[pair.Key];
                value = targetValue;
                return;
            }
        }
        Debug.LogError($"Die value {targetValue} not found in values.");
    }
}
