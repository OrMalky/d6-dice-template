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

    private async void TestAsyncDie()
    {
        int result = await die.Roll(Random.insideUnitSphere * maxRollTorque, Random.Range(minRollForce, maxRollForce) * Vector3.up);
        Debug.Log($"Die rolled {result}");
    }

    private async void TestAsyncRollOne()
    {
        int result = await roller.RollOne(1);
        Debug.Log(result);
    }

    private async void TestAsyncRollAll()
    {
        int[] results = await roller.RollAll();
        Debug.Log(string.Join(", ", results));
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TestAsyncRollAll();
        }
        if (Input.GetKeyDown(KeyCode.Return))
        {
            //TestAsyncDie();
            TestAsyncRollOne();
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
