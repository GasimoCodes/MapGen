using ALOB.Map;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenModule
{
    protected System.Random randomGen;
    protected GeneratorMapPreset gMP;
    protected string moduleName;
    public MapGenModule(System.Random randomGen, GeneratorMapPreset gMP)
    {
        this.randomGen = randomGen;
        this.gMP = gMP;
    }

}
