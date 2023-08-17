using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using UnityEngine;

[System.Serializable]
public enum Side
{
    [Description("Forward")] Forward,
    [Description("Up")] Up,
    [Description("Left")] Left,
    [Description("Right")] Right,
    [Description("Down")] Down,
    [Description("Back")] Back
}

[System.Serializable]
public class SideValueDictionary : SerializableDictionary<Side, int> { }

[RequireComponent(typeof(Rigidbody))]
public class Die : MonoBehaviour
{
    [Header("State")]
    [CustomAttributes.ReadOnly][SerializeField] private int value;
    [CustomAttributes.ReadOnly][SerializeField] private bool isRolling = false;

    [Header("Values")]
    [SerializeField] private SideValueDictionary values;

    [Header("Settings")]
    [SerializeField] private float stopThreshold = 0.001f;  // Maximum difference between current and last position and rotation to be considered as a stop

    private Vector3 lastPosition;
    private Quaternion lastRotation;
    private Coroutine rollInstance;

    private Rigidbody rb;
    private static readonly Dictionary<Side, Quaternion> sideToRotation = new()
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

    public void Start()
    {
        rb = GetComponent<Rigidbody>();
        value = CalculateValue();
    }

    /// <summary>
    /// Checks if the die is rolling.
    /// </summary>
    /// <returns> <see cref="True"/> if the die is rolling, <see cref="False"/> otherwise.</returns>
    private bool CheckStopped() => Vector3.Distance(transform.position, lastPosition) <= stopThreshold
        && Quaternion.Dot(transform.rotation, lastRotation) >= 1f;


    /// <summary>
    /// Enumerator that awaits for the die to stop rolling, and handles the result.
    /// </summary>
    /// <param name="callbacks"> A collection of actions to be called with the result.</param>
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
        rb.AddForce(force, ForceMode.Impulse);
        rb.AddTorque(torque, ForceMode.Impulse);
        if (rollInstance == null)
        {
            isRolling = true;
            rollInstance = StartCoroutine(HandleRoll(callbacks));
        }
    }

    /// <summary>
    /// Calculates the value of the die based on its rotation.
    /// </summary>
    /// <returns> <see cref="int"/> value of the die.</returns>
    private int CalculateValue()
    {
        Dictionary<Vector3, int> sides = new()
        {
            { transform.forward, values[Side.Forward] },
            { transform.up, values[Side.Up] },
            { -transform.right, values[Side.Left] },
            { transform.right, values[Side.Right] },
            { -transform.up, values[Side.Down] },
            { -transform.forward, values[Side.Back] }
        };

        int value = 0;
        float max = -1f;
        foreach (Vector3 side in sides.Keys)
        {
            float dot = Vector3.Dot(side, Vector3.up);
            if (dot > max)
            {
                max = dot;
                value = sides[side];
            }
        }
        return value;
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
