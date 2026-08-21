using UnityEngine;

namespace DoofusDiaries.Core
{
    /// <summary>
    /// Shared physics materials for the project. "Frictionless" is applied
    /// to both Doofus's collider and every tile's collider so nothing snags
    /// on the hairline seam where two adjacent tiles' colliders meet --
    /// without it, a moving box-shaped collider can catch its corner on
    /// that seam like tripping over an invisible curb.
    /// </summary>
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
