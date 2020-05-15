using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

namespace XploriaAR.UI
{
    public class UiInvertedMaskImage : Image
    {
        public override Material materialForRendering
        {
            get
            {
                var result = base.materialForRendering;
                result.SetInt("_StencilComp", (int)CompareFunction.NotEqual);
                return result;
            }
        }
    }
}