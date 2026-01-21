using UnityEngine;

[ExecuteAlways]
public class FlashPostProcessParameter : MonoBehaviour
{
    public static class ShaderProperty
    {
        public static readonly int DistortionCenter = Shader.PropertyToID("_DistortionCenter");
        public static readonly int DistortionStrength = Shader.PropertyToID("_DistortionStrength");

        public static readonly int PosterizationThreshold = Shader.PropertyToID("_PosterizationThreshold");
        public static readonly int PosterizationLightColor = Shader.PropertyToID("_PosterizationLightColor");
        public static readonly int PosterizationDarkColor = Shader.PropertyToID("_PosterizationDarkColor");

        public static readonly int FlashAlpha = Shader.PropertyToID("_FlashAlpha");
    }

    [SerializeField] private Vector2 _distortionCenter = new Vector2(0.5f, 0.5f);
    [SerializeField] private float _distortionStrength = 0.1f;

    [SerializeField] private float _posterizationThreshold = 0.1f;
    [SerializeField, ColorUsage(false, true)] private Color _posterizationLightColor = Color.white;
    [SerializeField, ColorUsage(false, true)] private Color _posterizationDarkColor = Color.black;

    [SerializeField] private float _flashAlpha = 0.0f;

    private void LateUpdate()
    {
        Shader.SetGlobalVector(ShaderProperty.DistortionCenter, _distortionCenter);
        Shader.SetGlobalFloat(ShaderProperty.DistortionStrength, _distortionStrength);

        Shader.SetGlobalFloat(ShaderProperty.PosterizationThreshold, _posterizationThreshold);
        Shader.SetGlobalColor(ShaderProperty.PosterizationLightColor, _posterizationLightColor);
        Shader.SetGlobalColor(ShaderProperty.PosterizationDarkColor, _posterizationDarkColor);

        Shader.SetGlobalFloat(ShaderProperty.FlashAlpha, _flashAlpha);
    }
}