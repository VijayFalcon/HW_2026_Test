// Shared physics materials for the project. Frictionless is applied to
// both Doofus's collider and every tile's collider so nothing snags on the
// hairline seam where two adjacent tiles meet.

using UnityEngine;

namespace DoofusDiaries.Core
{
    public static class PhysicsMaterials
    {
        public static readonly PhysicsMaterial Frictionless = CreateFrictionless();

        private static PhysicsMaterial CreateFrictionless()
        {
            return new PhysicsMaterial("DoofusFrictionless")
            {
                dynamicFriction = 0f,
                staticFriction = 0f,
                bounciness = 0f,
                frictionCombine = PhysicsMaterialCombine.Minimum,
                bounceCombine = PhysicsMaterialCombine.Minimum
            };
        }
    }
}
