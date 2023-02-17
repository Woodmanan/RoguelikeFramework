using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IDescribable
{
    string GetName(bool shorten = false);
    string GetDescription();

    Sprite GetImage();
}
