using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : UI_Base
{
    enum Images
    {
        Image
    }
    public override void Init()
    {
        BindImage(typeof(Images));

    }

    
}
