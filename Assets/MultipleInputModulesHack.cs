using UnityEngine;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Reflection;

public class MultipleInputModulesHack : BaseInputModule {
    public override bool ShouldActivateModule() {
        return true;
    }

    public override void Process() {

        BaseInputModule[] inputModules = GetComponents<BaseInputModule>();
        foreach (BaseInputModule module in inputModules) {
            if (module == this || ! module.IsActive())
                continue;

            module.Process();
        }
    }
}
