using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Function : MonoBehaviour
{
    public static bool ElementIsChildOf(Transform element, Transform checkIfParent)
    {
        // Find player in hierarchy
        Transform parentTransform = element.parent;

        while (parentTransform != null)
        {
            if (parentTransform == checkIfParent)
            {
                return true;
            }
            parentTransform = parentTransform.parent;
        }
        return false;
    }

    public static string[] AddElementToArray(string[] originalArray, string newElement)
    {
        // Create a new array with one more space, copy elements over, add the new element to the array
        string[] newArray = new string[originalArray.Length + 1];

        for (int i = 0; i < originalArray.Length; i++)
        {
            newArray[i] = originalArray[i];
        }

        newArray[newArray.Length - 1] = newElement;

        return newArray;
    }

    public static string[] DeleteElementToArray(string[] originalArray, string elementToDelete)
    {
        int count = 0;

        // count how many elements to delete
        foreach (string element in originalArray)
        {
            if (element == elementToDelete)
            {
                count++;
            }
        }

        // if not found, return unchanged
        if (count == 0)
        {
            return originalArray;
        }

        // create a new array
        string[] newArray = new string[originalArray.Length - count];

        // copy elements to the new array, excluding element to delete
        int newIndex = 0;
        foreach (string element in originalArray)
        {
            if (element != elementToDelete)
            {
                newArray[newIndex] = element;
                newIndex++;
            }
        }

        return newArray;
    }

    public static bool ArrayContainsElement(string[] array, string element)
    {
        foreach (string e in array)
        {
            if (e == element)
            {
                return true;
            }
        }
        return false;
    }

    public static bool ArrayContainsElementsNoOtherThan(string[] array, string element)
    {
        foreach (string e in array)
        {
            if (e != element)
            {
                return false;
            }
        }
        return true;
    }
}
