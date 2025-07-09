using Chroma.Colorizer;
using Chroma.Lighting;
using SiraUtil.Affinity;
using Technicolor.Managers;
using Technicolor.Settings;
using UnityEngine;

namespace Technicolor.HarmonyPatches
{
    // yes i just harmony patched my own mod, you got a problem?
    internal class TechniLights : IAffinity
    {
        private readonly LightColorizerManager _manager;
        private readonly Config _config;
        private readonly ILightWithId[] _lights = new ILightWithId[1];
        private readonly Color?[] _lightColors = new Color?[4];

        private TechniLights(LightColorizerManager manager, Config config)
        {
            _manager = manager;
            _config = config;
        }

        [AffinityPrefix]
        [AffinityPatch(typeof(ChromaLightSwitchEventEffect), "BasicCallback")]
        private bool Colorize(ChromaLightSwitchEventEffect __instance, BasicBeatmapEventData beatmapEventData)
        {
            if (!_config.TechnicolorEnabled)
            {
                return true;
            }

            LightColorizer lightColorizer = __instance.Colorizer;
            bool warm = BeatmapEventDataLightsExtensions.GetLightColorTypeFromEventDataValue(beatmapEventData.value) == EnvironmentColorType.Color1;
            if (_config.TechnicolorLightsGrouping == TechnicolorLightsGrouping.ISOLATED)
            {
                foreach (ILightWithId light in lightColorizer.Lights)
                {
                    Color color = TechnicolorController.GetTechnicolor(warm, beatmapEventData.time + light.GetHashCode(), _config.TechnicolorLightsStyle);
                    _lights[0] = light;
                    UpdateLightColors(color);
                    lightColorizer.Colorize(false, _lightColors);
                    __instance.Refresh(true, _lights, beatmapEventData);
                }

                return false;
            }

            if (!(TechnicolorController.TechniLightRandom.NextDouble() <
                  _config.TechnicolorLightsFrequency))
            {
                return true;
            }

            {
                Color color = TechnicolorController.GetTechnicolor(warm, beatmapEventData.time, _config.TechnicolorLightsStyle);
                UpdateLightColors(color);
                switch (_config.TechnicolorLightsGrouping)
                {
                    case TechnicolorLightsGrouping.ISOLATED_GROUP:
                        lightColorizer.Colorize(false, _lightColors);
                        break;

                    default:
                        _manager.GlobalColorize(false, _lightColors);
                        break;
                }
            }

            return true;
        }

        private void UpdateLightColors(Color? color)
        {
            _lightColors[0] = color;
            _lightColors[1] = color;
            _lightColors[2] = color;
            _lightColors[3] = color;
        }
    }
}
