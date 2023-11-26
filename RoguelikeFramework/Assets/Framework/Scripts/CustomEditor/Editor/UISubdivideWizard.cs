using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class UISubdivideWizard : ScriptableWizard
{
    public Vector2Int numSplits = Vector2Int.one;
    public Transform parent;

    [MenuItem("Tools/UI Subdivide Wizard")]
    static void CreateWizard()
    {
        ScriptableWizard.DisplayWizard<UISubdivideWizard>("Create Light", "Subdivide", "Shuffle");
        //If you don't want to use the secondary button simply leave it out:
        //ScriptableWizard.DisplayWizard<WizardCreateLight>("Create Light", "Create");
    }
    

    void OnWizardCreate()
    {
        if (Selection.activeTransform != null)
        {
            if (parent == null)
            {
                parent = Selection.activeTransform.parent;
            }
            RectTransform rect = Selection.activeTransform as RectTransform;

            Vector2 offset = (rect.anchorMax - rect.anchorMin) / numSplits;

            for (int i = 0; i < numSplits.x; i++)
            {
                for (int j = 0; j < numSplits.y; j++)
                {
                    RectTransform newRect = Instantiate(rect.gameObject, parent).transform as RectTransform;
                    newRect.anchorMin = rect.anchorMin + offset * new Vector2(i, j);
                    newRect.anchorMax = rect.anchorMin + offset * new Vector2(i + 1, j + 1);
                    newRect.sizeDelta = Vector2.zero;
                }
            }
        }
    }

    void OnWizardUpdate()
    {
        helpString = "Choose an object to subdivide! You can also set an alternate parent";
    }

    // When the user presses the "Apply" button OnWizardOtherButton is called.
    
    void OnWizardOtherButton()
    {
        if (Selection.activeTransform != null)
        {
            for (int i = 0; i < Selection.activeTransform.childCount; i++)
            {
                Selection.activeTransform.GetChild(i).SetSiblingIndex(Random.Range(0, Selection.activeTransform.childCount));
            }
        }
    }
}