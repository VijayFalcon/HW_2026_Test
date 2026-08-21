// Shared physics materials for the project. Frictionless is applied to
// both Doofus's collider and every tile's collider so nothing snags on the
// hairline seam where two adjacent tiles meet.

using UnityEngine;

namespace DoofusDiaries.Core
{
    public static class PhysicsMaterials
    {
        public static readonly PhysicMaterial Frictionless = CreateFrictionless();

        private static PhysicMaterial CreateFrictionless()
        {
            return new PhysicMaterial("DoofusFrictionless")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicMaterialCombine.Minimum,
                bounceCombine = PhysicMaterialCombine.Minimum
            };
        }
    }
}
