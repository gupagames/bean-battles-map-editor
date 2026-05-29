using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public class ShowOnlyAttribute : PropertyAttribute
    {
        public string Tooltip { get; }

        public ShowOnlyAttribute(string tooltip = null)
        {
            Tooltip = tooltip;
        }
    }
}