using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tester : MonoBehaviour
{
    public DiceRoller roller;
    public Die die;

    [Header("Roll Settings")]
    [SerializeField] private float maxRollTorque = 10f;
    [SerializeField] private float minRollForce = 5f;
    [SerializeField] private float maxRollForce = 10f;

    private void Test(int value)
    {
        Debug.Log(value);
    }

    private void Test(int[] values)
    {
        Debug.Log(string.Join(", ", values));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            roller.RollAll(Test);
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            roller.RollOne(Test);
        }
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            die.Value = 1;
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {
            die.Value = 2;
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {
            die.Value = 3;
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {
            die.Value = 4;
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {
            die.Value = 5;
        }
        if (Input.GetKeyDown(KeyCode.Keypad6))
        {
            die.Value = 6;
        }
    }
}
