using System;
using UnityEngine;

namespace GG.BeanBattles.MapEditor
{
    public static class EditorMapValidation
    {
        public static readonly Type[] ValidTypes =
        {
            typeof(Transform),
            typeof(Renderer),
            typeof(MeshFilter),
            typeof(Collider),
            typeof(Light),
            typeof(EditorMapBehaviour)
        };

        public static bool ValidateMap()
        {
            // TODO: Validate EditorMapSettings exists

            // Vadilate Objects / Types
            if (!ValidateTypes()) return false;

            // TODO: Validate player spawns / vehcicals spanws, weaspons spanws



            // TODO: Validate max triangle count

            // TODO: Validate max texture resolution

            // TODO: Validate max material count

            // TODO: Validate max realtime lights

            // TODO: Validate mesh collider restrictions

            // TODO: Validate allowed shader list

            // TODO: Validate bundle size limit

            // TODO: Validate map bounds/max world size

            // TODO: Validate no invalid  postions etc transforms (NaN/Infinity)

            // TODO: Add runtime validation after bundle load

            // TODO: Add map/game version compatibility validation

            return true;
        }

        public static bool ValidateMapSettings()
        {
            return true;
        }

        public static bool ValidateTypes()
        {
            Component[] components = UnityEngine.Object.FindObjectsOfType<Component>();

            foreach (Component component in components)
            {
                if (component == null)
                {
                    Debug.LogError("Missing script detected.");
                    return false;
                }

                Type type = component.GetType();

                if (!IsAllowedComponent(type))
                {
                    Debug.LogError($"Disallowed component: {type.Name} on GameObject: {component.gameObject.name}");
                    return false;
                }
            }

            return true;
        }

        public static bool IsAllowedComponent(Type type)
        {
            // is assignable so we can do inheritance to make it easier
            foreach (Type validType in ValidTypes) if (validType.IsAssignableFrom(type)) return true;
            return false;
        }
    }
}