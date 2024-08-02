using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class Logger : MonoBehaviour
{
    public void PrintLog(string message)
    {
        print(message);
    }

    public void PrintLog()
    {
        print("Log");
    }
}
