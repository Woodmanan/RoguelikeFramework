using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using TMPro;

public class AnimationDebugUI : MonoBehaviour
{
    [SerializeField]
    TextMeshProUGUI text;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        bool hasAnims = AnimationController.hasAnimations;
        int numAnims = AnimationController.numGroups;
        int numGroupZero = numAnims == 0 ? 0 : AnimationController.GetNumAnimsInGroup(0);
        text.text = string.Format("Has Anims: {0} {1}\nNum Groups {2}:\nNum in group 0: {3}\n{4}", hasAnims, AnimationController.animationSpeed, numAnims, numGroupZero, GetGroupCounts());
    }

    string GetGroupCounts()
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < AnimationController.numGroups; i++)
        {
            builder.Append(AnimationController.GetNumAnimsInGroup(i));
            builder.Append(" : ");
        }
        return builder.ToString();
    }
}
