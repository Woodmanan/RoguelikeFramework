using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//A place for holding the custom delegate types!
//Mostly used for making the effect system work with references

public delegate void ActionRef();

public delegate void ActionRef<T>(ref T item);

public delegate void ActionRef<T1, T2>(ref T1 item1, ref T2 item2);

public delegate void ActionRef<T1, T2, T3>(ref T1 item1, ref T2 item2, ref T3 item3);

public delegate void BoolDelegate(bool val);
