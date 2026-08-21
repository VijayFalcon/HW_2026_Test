using UnityEngine;

namespace DoofusDiaries.Core
{
    /// <summary>
    /// Cycles a handful of colored point lights through hue rotations at
    /// different speeds/phases for a simple disco-club feel, replacing the
    /// single outdoor directional sun light the original prototype used.
    /// The room itself is otherwise unlit (see GameBootstrap.ConfigureAmbience),
    /// so these lights are the only source of illumination.
    /// </summary>
    public class DiscoLightController : MonoBehaviour
    {
        private Light[] _lights;
        private float[] _speeds;
        private float[] _phases;

        public void BuildLights(Transform parent, float height)
        {
            const int count = 4;
            const float radius = 20f;

            _lights = new Light[count];
            _speeds = new float[count];
            _phases = new float[count];

            for (int i = 0; i < count; i++)
            {
                var go = new GameObject($"DiscoLight_{i}");
                go.transform.SetParent(parent, false);

                float angle = i * Mathf.PI * 2f / count;
                go.transform.localPosition = new Vector3(Mathf.Cos(angle) * radius, height, Mathf.Sin(angle) * radius);

                var light = go.AddComponent<Light>();
                light.type = LightType.Point;
                light.range = 60f;
                light.intensity = 3f;

                _lights[i] = light;
                _speeds[i] = 0.15f + i * 0.05f;
                _phases[i] = i / (float)count;
            }
        }

        private void Update()
        {
            if (_lights == null) return;

            for (int i = 0; i < _lights.Length; i++)
            {
                if (_lights[i] == null) continue;
                float hue = Mathf.Repeat(Time.time * _speeds[i] + _phases[i], 1f);
                _lights[i].color = Color.HSVToRGB(hue, 0.85f, 1f);
            }
        }
    }
}
