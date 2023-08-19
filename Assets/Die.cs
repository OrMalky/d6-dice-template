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
    private Rigidbody rb;

    private static readonly Dictionary<Side, Quaternion> sideToRotation = new() 
    {
        { Side.Forward, Quaternion.LookRotation(Vector3.up, Vector3.forward) },
        { Side.Up, Quaternion.LookRotation(Vector3.forward, Vector3.up) },
        { Side.Left, Quaternion.LookRotation(Vector3.left, Vector3.forward) },
        { Side.Right, Quaternion.LookRotation(Vector3.forward, Vector3.left) },
        { Side.Down, Quaternion.LookRotation(Vector3.forward, Vector3.down) },
        { Side.Back, Quaternion.LookRotation(Vector3.down, Vector3.forward) }
    };  // Used to convert a side to a rotation

    // Properties
    public int Value { get => value; set => SetValue(value); }
    public bool IsRolling { get => isRolling; private set => isRolling = value; }

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        value = CalculateValue();
    }

    private void FixedUpdate()
    {
        if (IsRolling && rb.IsSleeping())
        {
            IsRolling = false;
            rollTask?.SetResult(true);
        }
    }

    /// <summary>
    /// Rolls the die and returns the result, using async function.
    /// </summary>
    /// <param name="torque"> Torque to be applied to the die. </param>
    /// <param name="force"> Force to be applied to the die. </param>
    /// <returns> <see cref="int"/> die value. </returns>
    public async Task<int> Roll(Vector3 torque, Vector3 force)
    {
        // Physically roll the die
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);

        if (!IsRolling)
        {
            rollTask = new TaskCompletionSource<bool>();
            IsRolling = true;
            await rollTask.Task;
            value = CalculateValue();
            return value;
        }
        return 0;
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
