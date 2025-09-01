using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace LeTai.Asset.TranslucentImage
{
/// <summary>
/// Dynamic blur-behind UI element
/// </summary>
[HelpURL("https://leloctai.com/asset/translucentimage/docs/articles/customize.html#translucent-image")]
public partial class TranslucentImage : Image, IMeshModifier, IActiveRegionProvider
{
    [SerializeField]
    [FormerlySerializedAs("source")]
    TranslucentImageSource _source;

    /// <summary>
    /// Source of blur for this image
    /// </summary>
    public TranslucentImageSource source
    {
        get => _source;
        set
        {
            _source = value;

            // We need a separated variable, as the backing field is set before the setter is called from EditorProperty
            if (_source == _sourcePrev)
                return;

            DisconnectSource(_sourcePrev);
            ConnectSource(_source);
            _sourcePrev = source;
        }
    }


    /// <summary>
    /// (De)Saturate the image, 1 is normal, 0 is grey scale, below zero make the image negative
    /// </summary>
    [Obsolete("Use Get/SetFloat(ShaderID.VIBRANCY) on material/materialForRendering directly instead")]
    public float vibrancy
    {
        get => materialForRendering.GetFloat(ShaderID.VIBRANCY);
        set => materialForRendering.SetFloat(ShaderID.VIBRANCY, value);
    }

    /// <summary>
    /// Brighten/darken the image
    /// </summary>
    [Obsolete("Use Get/SetFloat(ShaderID.BRIGHTNESS) on material/materialForRendering directly instead")]
    public float brightness
    {
        get => materialForRendering.GetFloat(ShaderID.BRIGHTNESS);
        set => materialForRendering.SetFloat(ShaderID.BRIGHTNESS, value);
    }

    /// <summary>
    /// Flatten the color behind to help keep contrast on varying background
    /// </summary>
    [Obsolete("Use Get/SetFloat(ShaderID.FLATTEN) on material/materialForRendering directly instead")]
    public float flatten
    {
        get => materialForRendering.GetFloat(ShaderID.FLATTEN);
        set => materialForRendering.SetFloat(ShaderID.FLATTEN, value);
    }

    Canvas Canvas;
    bool   shouldRun;
    bool   isBirp;

    TranslucentImageSource _sourcePrev;

    protected override void Start()
    {
        isBirp = !GraphicsSettings.currentRenderPipeline;

        AutoAcquireSource();

        if (material)
        {
            //Have to use string comparison as Addressable break object comparision :(
            if (Application.isPlaying && material.shader.name != "UI/TranslucentImage")
            {
                Debug.LogWarning("Translucent Image requires a material using the \"UI/TranslucentImage\" shader");
            }
            else if (source)
            {
                material.SetTexture(ShaderID.BLUR_TEX, source.BlurredScreen);
            }
        }

        m_OnDirtyMaterialCallback += OnDirtyMaterial;
#if UNITY_5_6_OR_NEWER
        if (Canvas)
            Canvas.additionalShaderChannels |= AdditionalCanvasShaderChannels.TexCoord1;
#endif
    }

    protected override void OnEnable()
    {
        Canvas = canvas;
        base.OnEnable();
        SetVerticesDirty();

        ConnectSource(source);
        _sourcePrev = source;

#if UNITY_EDITOR
        if (!UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Start();
        }

        UnityEditor.Undo.undoRedoPerformed += ApplySerializedData;
#endif
    }

    protected override void OnDisable()
    {
        SetVerticesDirty();
        base.OnDisable();

        DisconnectSource(source);
#if UNITY_EDITOR
        UnityEditor.Undo.undoRedoPerformed -= ApplySerializedData;
#endif
    }

    void Update()
    {
#if DEBUG
        if (Application.isPlaying && !IsInPrefabMode())
        {
            if (!source)
                Debug.LogWarning("TranslucentImageSource is missing. " +
                                 "Please add the TranslucentImageSource component to your main camera, " +
                                 "then assign it to the Source field of the Translucent Image(s)");
        }
#endif
    }

    public bool HaveActiveRegion()
    {
        return IsActive();
    }

    public void GetActiveRegion(VPMatrixCache vpMatrixCache, out ActiveRegion activeRegion)
    {
        VPMatrixCache.Index vpMatrixIndex;
        if (Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            vpMatrixIndex = VPMatrixCache.Index.INVALID;
        }
        else
        {
            var refCamera = Canvas.worldCamera;
            if (!refCamera)
            {
                Debug.LogError("Translucent Image need an Event Camera for World Space Canvas");
                vpMatrixIndex = VPMatrixCache.Index.INVALID;
            }
            else
            {
                vpMatrixIndex = vpMatrixCache.IndexOf(refCamera);
                if (!vpMatrixIndex.IsValid())
                    vpMatrixIndex = vpMatrixCache.Add(refCamera);
            }
        }


        activeRegion = new ActiveRegion(rectTransform.rect,
                                            rectTransform.localToWorldMatrix,
                                            vpMatrixIndex);
    }

    /// <summary>
    /// Copy material keywords state and properties, except stencil properties
    /// </summary>
    public static void CopyMaterialPropertiesTo(Material src, Material dst)
    {
        dst.SetFloat(ShaderID.VIBRANCY,   src.GetFloat(ShaderID.VIBRANCY));
        dst.SetFloat(ShaderID.BRIGHTNESS, src.GetFloat(ShaderID.BRIGHTNESS));
        dst.SetFloat(ShaderID.FLATTEN,    src.GetFloat(ShaderID.FLATTEN));
    }

    void ConnectSource(TranslucentImageSource source)
    {
        if (!source) return;

        source.RegisterActiveRegionProvider(this);
        source.blurredScreenChanged += SetBlurTex;
        source.blurRegionChanged    += SetBlurRegion;
        SetBlurTex();
        SetBlurRegion();
    }

    void DisconnectSource(TranslucentImageSource source)
    {
        if (!source) return;

        source.UnRegisterActiveRegionProvider(this);
        source.blurredScreenChanged -= SetBlurTex;
        source.blurRegionChanged    -= SetBlurRegion;
    }

    void SetBlurTex()
    {
        if (!source)
            return;

        materialForRendering.SetTexture(ShaderID.BLUR_TEX, source.BlurredScreen);
    }

    void SetBlurRegion()
    {
        if (!source)
            return;

        if (isBirp || Canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            var minMaxVector = RectUtils.ToMinMaxVector(source.BlurRegionNormalizedScreenSpace);
            materialForRendering.SetVector(ShaderID.CROP_REGION, minMaxVector);
        }
        else
        {
            materialForRendering.SetVector(ShaderID.CROP_REGION, RectUtils.ToMinMaxVector(source.BlurRegion));
        }
    }

    void OnDirtyMaterial()
    {
        SetBlurTex();
        SetBlurRegion();
    }

    bool IsInPrefabMode()
    {
#if !UNITY_EDITOR
        return false;
#else // UNITY_EDITOR
#if UNITY_2021_2_OR_NEWER
        var stage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#else
        var stage = UnityEditor.Experimental.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
#endif
        return stage != null;
#endif // !UNITY_EDITOR
    }

    bool sourceAcquiredOnStart = false;

    void AutoAcquireSource()
    {
        var found = Shims.FindObjectOfType<TranslucentImageSource>();
        Debug.Log($"[TranslucentImage] AutoAcquireSource found: {found}");
        source = found;
        sourceAcquiredOnStart = true;
    }

    public void ForceAcquireSource()
    {
        sourceAcquiredOnStart = false;
        AutoAcquireSource();
    }

    void ApplySerializedData()
    {
        source = _source;
    }
}
}
