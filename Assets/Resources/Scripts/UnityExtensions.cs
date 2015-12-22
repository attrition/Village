using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

public static class Util
{
	public static void Swap<T>(ref T left, ref T right)
	{
		T tmp = left;
		left = right;
		right = tmp;
	}
}

public static class UnityExtensions
{
    public static T GetOrAddComponent<T>(this GameObject child) where T : Component
    {
        T result = child.GetComponent<T>();
        if (result == null)
        {
            result = child.gameObject.AddComponent<T>();
        }
        return result;
    }

}
