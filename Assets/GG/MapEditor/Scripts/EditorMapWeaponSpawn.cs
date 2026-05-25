using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public class EditorMapWeaponSpawn : EditorMapBehaviour
    {
        private Vector3 _gizmoOffset = new Vector3(0, 0, 0);
        private Vector3 _gizmoSize = new Vector3(2, 2, 2);
        private Vector3 _labelOffset = new Vector3(0, 2, 0);

        private Color _color = Color.yellow;

        private void OnDrawGizmosSelected()
        {
            EditorMapGizmos.DrawSpawn(transform.position, transform.rotation, _gizmoSize, _gizmoOffset, _color, "Weapon Spawn", _labelOffset);
        }
    }
}