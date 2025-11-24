using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BepInEx.Configuration;
using SpinCore.Translation;
using SpinCore.UI;
using SpinSpout.Patches;
using SpinSpout.Spout;
using UnityEngine;

namespace SpinSpout;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public partial class Plugin
{
    private const string TRANSLATION_PREFIX = $"{nameof(SpinSpout)}_";
    
    public static ConfigEntry<bool> Enabled;

    public static ConfigEntry<int> Width;
    public static ConfigEntry<int> Height;

    public static ConfigEntry<Vector3> Offset;
    public static ConfigEntry<Vector3> Rotation;

    public static ConfigEntry<bool> FieldOfViewIsStatic;
    public static ConfigEntry<float> FieldOfView;

    public static ConfigEntry<bool> ShowHud;
    public static ConfigEntry<bool> ShowUi;
    public static ConfigEntry<bool> ForceShowAccuracyBar;
    
    public static ConfigEntry<bool> SecondaryCameraEnabled;
    
    public static ConfigEntry<int> SecondaryWidth;
    public static ConfigEntry<int> SecondaryHeight;
    
    public static ConfigEntry<Vector3> SecondaryOffset;
    public static ConfigEntry<Vector3> SecondaryRotation;
    
    public static ConfigEntry<bool> SecondaryFieldOfViewIsStatic;
    public static ConfigEntry<float> SecondaryFieldOfView;
    
    public static ConfigEntry<bool> SecondaryShowHud;
    public static ConfigEntry<bool> SecondaryShowUi;

    public static ConfigEntry<bool> TakeOverVRSpectatorCamera;
    public static ConfigEntry<int> VRSpectatorWidth;
    public static ConfigEntry<int> VRSpectatorHeight;

    private void RegisterConfigEntries()
    {
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}Name", "SpinSpout");
        
        Enabled = Config.Bind("General", nameof(Enabled), true, "Enable the Spout2 camera output");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(Enabled)}", "Enabled");
        SecondaryCameraEnabled = Config.Bind("General", nameof(SecondaryCameraEnabled), false,
            "Enable a secondary Spout2 camera output");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(SecondaryCameraEnabled)}", "Enable secondary camera");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}ResolutionHeader", "Resolution");

        Width = Config.Bind("Resolution", nameof(Width), 2560, "The width of the Spout2 camera");
        Height = Config.Bind("Resolution", nameof(Height), 1440, "The height of the Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}Resolution", "Primary camera resolution");
        SecondaryWidth = Config.Bind("Resolution", nameof(SecondaryWidth), 2560, "The width of the secondary Spout2 camera");
        SecondaryHeight = Config.Bind("Resolution", nameof(SecondaryHeight), 1440, "The height of the secondary Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}SecondaryResolution", "Secondary camera resolution");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}OffsetHeader", "Offsets");
        
        Offset = Config.Bind("Offset", nameof(Offset), Vector3.zero, "The relative positional offset of the Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(Offset)}", "Primary offset");
        Rotation = Config.Bind("Offset", nameof(Rotation), Vector3.zero, "The relative rotation of the Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(Rotation)}", "Primary rotation");
        SecondaryOffset = Config.Bind("Offset", nameof(SecondaryOffset), Vector3.zero,
            "The relative positional offset of the secondary Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(SecondaryOffset)}", "Secondary offset");
        SecondaryRotation = Config.Bind("Offset", nameof(SecondaryRotation), Vector3.zero,
            "The relative rotation of the secondary Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(SecondaryRotation)}", "Secondary rotation");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}FieldOfViewHeader", "Field of View");
        
        FieldOfViewIsStatic = Config.Bind("FOV", nameof(FieldOfViewIsStatic), false, "Forces the Spout2 camera's field of view");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(FieldOfViewIsStatic)}", "Primary field of view is static");
        FieldOfView = Config.Bind("FOV", nameof(FieldOfView), 45.0f, "The field of view of the Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(FieldOfView)}", "Primary field of view");
        SecondaryFieldOfViewIsStatic = Config.Bind("FOV", nameof(SecondaryFieldOfViewIsStatic), false,
            "Forces the secondary Spout2 camera's field of view");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(SecondaryFieldOfViewIsStatic)}", "Secondary field of view is static");
        SecondaryFieldOfView = Config.Bind("FOV", nameof(SecondaryFieldOfView), 45.0f,
            "The field of view of the secondary Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(SecondaryFieldOfView)}", "Secondary field of view");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}CullingHeader", "Culling");
        
        ShowHud = Config.Bind("Culling", nameof(ShowHud), true, "Render the HUD to the Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(ShowHud)}", "Show HUD on primary");
        ShowUi = Config.Bind("Culling", nameof(ShowUi), true, "Render the menu UI to the Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(ShowUi)}", "Show menus on primary");
        SecondaryShowHud = Config.Bind("Culling", nameof(SecondaryShowHud), true, "Render the HUD to the secondary Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(SecondaryShowHud)}", "Show HUD on secondary");
        SecondaryShowUi = Config.Bind("Culling", nameof(SecondaryShowUi), true, "Render the menu UI to the secondary Spout2 camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(SecondaryShowUi)}", "Show menus on secondary");
        ForceShowAccuracyBar = Config.Bind("Culling", nameof(ForceShowAccuracyBar), false,
            "Force the accuracy bar to always show regardless of culling state");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(ForceShowAccuracyBar)}", "Always show the accuracy bar");
        
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}VRHeader", "VR");
        
        TakeOverVRSpectatorCamera = Config.Bind("VR", nameof(TakeOverVRSpectatorCamera), true,
            "Take over the Spectator Camera while in VR mode");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}{nameof(TakeOverVRSpectatorCamera)}", "Take over the VR spectator camera");
        VRSpectatorWidth = Config.Bind("VR", nameof(VRSpectatorWidth), 2560,
            "The width of the Spout2 output for the VR Spectator Camera");
        VRSpectatorHeight = Config.Bind("VR", nameof(VRSpectatorHeight), 1440,
            "The height of the Spout2 output for the VR Spectator Camera");
        TranslationHelper.AddTranslation($"{TRANSLATION_PREFIX}VRSpectatorResolution", "VR spectator camera resolution");
    }

    private static void CreateModPage()
    {
        CustomPage rootModPage = UIHelper.CreateCustomPage("ModSettings");
        rootModPage.OnPageLoad += RootModPageOnOnPageLoad;
        
        UIHelper.RegisterMenuInModSettingsRoot($"{TRANSLATION_PREFIX}Name", rootModPage);
    }

    private static void RootModPageOnOnPageLoad(Transform rootModPageTransform)
    {
        CustomGroup modGroup = UIHelper.CreateGroup(rootModPageTransform, nameof(SpinSpout));
        UIHelper.CreateSectionHeader(modGroup, "ModGroupHeader", $"{TRANSLATION_PREFIX}Name", false);
        
        #region Enabled
        CustomGroup enabledGroup = UIHelper.CreateGroup(modGroup, "EnabledGroup");
        enabledGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(enabledGroup, "Enabled", $"{TRANSLATION_PREFIX}{nameof(Enabled)}", Enabled.Value, value =>
        {
            Enabled.Value = value;

            foreach (TextureSpoutSender textureSpoutSender in FindObjectsByType<TextureSpoutSender>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (textureSpoutSender.gameObject.name == "SecondaryCameraSpoutObject(Clone)" &&
                    !SecondaryCameraEnabled.Value)
                {
                    continue;
                }
                
                textureSpoutSender.enabled = value;
                
                if (textureSpoutSender.gameObject.TryGetComponent(out Camera camera))
                {
                    camera.enabled = value;
                }
            }
        });
        #endregion
        
        #region SecondaryCameraEnabled
        CustomGroup secondaryCameraEnabledGroup = UIHelper.CreateGroup(modGroup, "SecondaryCameraEnabledGroup");
        secondaryCameraEnabledGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(secondaryCameraEnabledGroup, "Secondary Camera Enabled",
            $"{TRANSLATION_PREFIX}{nameof(SecondaryCameraEnabled)}", SecondaryCameraEnabled.Value, value =>
        {
            SecondaryCameraEnabled.Value = value;

            foreach (TextureSpoutSender textureSpoutSender in FindObjectsByType<TextureSpoutSender>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (textureSpoutSender.gameObject.name != "SecondaryCameraSpoutObject(Clone)")
                {
                    continue;
                }
                
                textureSpoutSender.enabled = value;
                
                if (textureSpoutSender.gameObject.TryGetComponent(out Camera camera))
                {
                    camera.enabled = value;
                }
            }
        });
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "ResolutionHeader", $"{TRANSLATION_PREFIX}ResolutionHeader", false);
        
        #region Resolution
        CustomGroup resolutionGroup = UIHelper.CreateGroup(modGroup, "ResolutionGroup");
        resolutionGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(resolutionGroup, "ResolutionLabel", $"{TRANSLATION_PREFIX}Resolution");
        CustomInputField widthInput = UIHelper.CreateInputField(resolutionGroup, "WidthInput", (_, newValue) =>
        {
            if (!int.TryParse(newValue, out int value))
            {
                return;
            }
            
            Width.Value = value;
            UpdateRenderTexture();
        });
        widthInput.InputField.SetText(Width.Value.ToString());
        
        CustomInputField heightInput = UIHelper.CreateInputField(resolutionGroup, "HeightInput", (_, newValue) =>
        {
            if (!int.TryParse(newValue, out int value))
            {
                return;
            }
            
            Height.Value = value;
            UpdateRenderTexture();
        });
        heightInput.InputField.SetText(Height.Value.ToString());
        #endregion
        
        #region SecondaryResolution
        CustomGroup secondaryResolutionGroup = UIHelper.CreateGroup(modGroup, "SecondaryResolutionGroup");
        secondaryResolutionGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(secondaryResolutionGroup, "SecondaryResolutionLabel", $"{TRANSLATION_PREFIX}SecondaryResolution");
        CustomInputField secondaryWidthInput = UIHelper.CreateInputField(secondaryResolutionGroup,
            "SecondaryWidthInput", (_, newValue) =>
        {
            if (!int.TryParse(newValue, out int value))
            {
                return;
            }
            
            SecondaryWidth.Value = value;
            UpdateRenderTexture();
        });
        secondaryWidthInput.InputField.SetText(SecondaryWidth.Value.ToString());
        
        CustomInputField secondaryHeightInput = UIHelper.CreateInputField(secondaryResolutionGroup, "SecondaryHeightInput", (_, newValue) =>
        {
            if (!int.TryParse(newValue, out int value))
            {
                return;
            }
            
            SecondaryHeight.Value = value;
            UpdateRenderTexture();
        });
        secondaryHeightInput.InputField.SetText(SecondaryHeight.Value.ToString());
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "OffsetHeader", $"{TRANSLATION_PREFIX}OffsetHeader", false);
        
        #region PositionOffset
        CustomGroup offsetGroup = UIHelper.CreateGroup(modGroup, "OffsetGroup");
        offsetGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(offsetGroup, "OffsetLabel", $"{TRANSLATION_PREFIX}{nameof(Offset)}");

        CustomInputField xInput = UIHelper.CreateInputField(offsetGroup, "XInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Offset.Value = Offset.Value with { x = value };
            UpdateCameraTransforms();
        });
        xInput.InputField.SetText(Offset.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField yInput = UIHelper.CreateInputField(offsetGroup, "YInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Offset.Value = Offset.Value with { y = value };
            UpdateCameraTransforms();
        });
        yInput.InputField.SetText(Offset.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField zInput = UIHelper.CreateInputField(offsetGroup, "ZInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Offset.Value = Offset.Value with { z = value };
            UpdateCameraTransforms();
        });
        zInput.InputField.SetText(Offset.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region SecondaryPositionOffset
        CustomGroup secondaryOffsetGroup = UIHelper.CreateGroup(modGroup, "SecondaryOffsetGroup");
        secondaryOffsetGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(secondaryOffsetGroup, "SecondaryOffsetLabel", $"{TRANSLATION_PREFIX}{nameof(SecondaryOffset)}");

        CustomInputField secondaryXInput = UIHelper.CreateInputField(secondaryOffsetGroup, "SecondaryXInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            SecondaryOffset.Value = SecondaryOffset.Value with { x = value };
            UpdateCameraTransforms();
        });
        secondaryXInput.InputField.SetText(SecondaryOffset.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField secondaryYInput = UIHelper.CreateInputField(secondaryOffsetGroup, "SecondaryYInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            SecondaryOffset.Value = SecondaryOffset.Value with { y = value };
            UpdateCameraTransforms();
        });
        secondaryYInput.InputField.SetText(SecondaryOffset.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField secondaryZInput = UIHelper.CreateInputField(secondaryOffsetGroup, "SecondaryZInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            SecondaryOffset.Value = SecondaryOffset.Value with { z = value };
            UpdateCameraTransforms();
        });
        secondaryZInput.InputField.SetText(SecondaryOffset.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region RotationOffset
        CustomGroup rotationGroup = UIHelper.CreateGroup(modGroup, "RotationGroup");
        rotationGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(rotationGroup, "RotationLabel", $"{TRANSLATION_PREFIX}{nameof(Rotation)}");

        CustomInputField rotXInput = UIHelper.CreateInputField(rotationGroup, "RotXInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Rotation.Value = Rotation.Value with { x = value };
            UpdateCameraTransforms();
        });
        rotXInput.InputField.SetText(Rotation.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField rotYInput = UIHelper.CreateInputField(rotationGroup, "RotYInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Rotation.Value = Rotation.Value with { y = value };
            UpdateCameraTransforms();
        });
        rotYInput.InputField.SetText(Rotation.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField rotZInput = UIHelper.CreateInputField(rotationGroup, "RotZInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            Rotation.Value = Rotation.Value with { z = value };
            UpdateCameraTransforms();
        });
        rotZInput.InputField.SetText(Rotation.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region SecondaryRotationOffset
        CustomGroup secondaryRotationGroup = UIHelper.CreateGroup(modGroup, "SecondaryRotationGroup");
        secondaryRotationGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(secondaryRotationGroup, "SecondaryRotationLabel", $"{TRANSLATION_PREFIX}{nameof(SecondaryRotation)}");

        CustomInputField secondaryRotXInput = UIHelper.CreateInputField(secondaryRotationGroup, "SecondaryRotXInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            SecondaryRotation.Value = SecondaryRotation.Value with { x = value };
            UpdateCameraTransforms();
        });
        secondaryRotXInput.InputField.SetText(SecondaryRotation.Value.x.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField secondaryRotYInput = UIHelper.CreateInputField(secondaryRotationGroup, "SecondaryRotYInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            SecondaryRotation.Value = SecondaryRotation.Value with { y = value };
            UpdateCameraTransforms();
        });
        secondaryRotYInput.InputField.SetText(SecondaryRotation.Value.y.ToString(CultureInfo.InvariantCulture));
        
        CustomInputField secondaryRotZInput = UIHelper.CreateInputField(secondaryRotationGroup, "SecondaryRotZInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }

            SecondaryRotation.Value = SecondaryRotation.Value with { z = value };
            UpdateCameraTransforms();
        });
        secondaryRotZInput.InputField.SetText(SecondaryRotation.Value.z.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "FOVHeader", $"{TRANSLATION_PREFIX}FieldOfViewHeader", false);
        
        #region FOVIsStatic
        CustomGroup fovIsStaticGroup = UIHelper.CreateGroup(modGroup, "FOVIsStaticGroup");
        fovIsStaticGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(fovIsStaticGroup, "FOVIsStatic", $"{TRANSLATION_PREFIX}{nameof(FieldOfViewIsStatic)}", FieldOfViewIsStatic.Value,
            value =>
            {
                FieldOfViewIsStatic.Value = value;
                UpdateCameraFieldOfViews();
            });
        #endregion
        
        #region FOV
        CustomGroup fovGroup = UIHelper.CreateGroup(modGroup, "FOVGroup");
        fovGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(fovGroup, "FOVLabel", $"{TRANSLATION_PREFIX}{nameof(FieldOfView)}");
        CustomInputField fovInput = UIHelper.CreateInputField(fovGroup, "FOVInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }
            
            FieldOfView.Value = value;
            UpdateCameraFieldOfViews();
        });
        fovInput.InputField.SetText(FieldOfView.Value.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        #region SecondaryFOVIsStatic
        CustomGroup secondaryFovIsStaticGroup = UIHelper.CreateGroup(modGroup, "SecondaryFOVIsStaticGroup");
        secondaryFovIsStaticGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(secondaryFovIsStaticGroup, "SecondaryFOVIsStatic", $"{TRANSLATION_PREFIX}{nameof(SecondaryFieldOfViewIsStatic)}",
            SecondaryFieldOfViewIsStatic.Value, value =>
            {
                SecondaryFieldOfViewIsStatic.Value = value;
                UpdateCameraFieldOfViews();
            });
        #endregion
        
        #region SecondaryFOV
        CustomGroup secondaryFovGroup = UIHelper.CreateGroup(modGroup, "FOVGroup");
        secondaryFovGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(secondaryFovGroup, "FOVLabel", $"{TRANSLATION_PREFIX}{nameof(SecondaryFieldOfView)}");
        CustomInputField secondaryFovInput = UIHelper.CreateInputField(secondaryFovGroup, "SecondaryFOVInput", (_, newValue) =>
        {
            if (!float.TryParse(newValue, out float value))
            {
                return;
            }
            
            SecondaryFieldOfView.Value = value;
            UpdateCameraFieldOfViews();
        });
        secondaryFovInput.InputField.SetText(SecondaryFieldOfView.Value.ToString(CultureInfo.InvariantCulture));
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "CullingHeader", $"{TRANSLATION_PREFIX}CullingHeader", false);
        
        #region ShowHUD
        CustomGroup showHudGroup = UIHelper.CreateGroup(modGroup, "ShowHUDGroup");
        showHudGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(showHudGroup, "ShowHUD", $"{TRANSLATION_PREFIX}{nameof(ShowHud)}", ShowHud.Value, value =>
        {
            ShowHud.Value = value;
            UpdateCameraHudCulling();
        });
        #endregion
        
        #region ShowUi
        CustomGroup showUiGroup = UIHelper.CreateGroup(modGroup, "ShowUIGroup");
        showUiGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(showUiGroup, "ShowUI", $"{TRANSLATION_PREFIX}{nameof(ShowUi)}", ShowUi.Value, value =>
        {
            ShowUi.Value = value;
            UpdateCameraHudCulling();
        });
        #endregion
        
        #region SecondaryShowHUD
        CustomGroup secondaryShowHudGroup = UIHelper.CreateGroup(modGroup, "SecondaryShowHUDGroup");
        secondaryShowHudGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(secondaryShowHudGroup, "SecondaryShowHUD", $"{TRANSLATION_PREFIX}{nameof(SecondaryShowHud)}",
            SecondaryShowHud.Value, value =>
        {
            SecondaryShowHud.Value = value;
            UpdateCameraHudCulling();
        });
        #endregion
        
        #region SecondaryShowUi
        CustomGroup secondaryShowUiGroup = UIHelper.CreateGroup(modGroup, "SecondaryShowUIGroup");
        secondaryShowUiGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(secondaryShowUiGroup, "SecondaryShowUI", $"{TRANSLATION_PREFIX}{nameof(SecondaryShowUi)}",
            SecondaryShowUi.Value, value =>
            {
                SecondaryShowUi.Value = value;
                UpdateCameraHudCulling();
            });
        #endregion
        
        #region ForceShowAccuracyBar
        CustomGroup forceShowAccuracyBarGroup = UIHelper.CreateGroup(modGroup, "ForceShowAccuracyBarGroup");
        forceShowAccuracyBarGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(forceShowAccuracyBarGroup, "ForceShowAccuracyBar", $"{TRANSLATION_PREFIX}{nameof(ForceShowAccuracyBar)}",
            ForceShowAccuracyBar.Value, value =>
        {
            ForceShowAccuracyBar.Value = value;
            TimingBarPatches.UpdateLayerCulling();
        });
        #endregion
        
        UIHelper.CreateSectionHeader(modGroup, "VRHeader", $"{TRANSLATION_PREFIX}VRHeader", false);
        
        #region TakeOverVRSpectatorCamera
        CustomGroup takeOverVRSpectatorCameraGroup = UIHelper.CreateGroup(modGroup, "TakeOverVRSpectatorCameraGroup");
        takeOverVRSpectatorCameraGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateSmallToggle(takeOverVRSpectatorCameraGroup, "TakeOverVRSpectatorCamera",
            $"{TRANSLATION_PREFIX}{nameof(TakeOverVRSpectatorCamera)}", TakeOverVRSpectatorCamera.Value, value =>
        {
            TakeOverVRSpectatorCamera.Value = value;
            UpdateVRSpectatorCamera();
        });
        #endregion
        
        #region VRSpectatorResolution
        CustomGroup vrSpectatorResolutionGroup = UIHelper.CreateGroup(modGroup, "VRSpectatorResolutionGroup");
        vrSpectatorResolutionGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(vrSpectatorResolutionGroup, "VRSpectatorResolutionLabel", $"{TRANSLATION_PREFIX}VRSpectatorResolution");
        CustomInputField vrSpectatorWidthInput = UIHelper.CreateInputField(vrSpectatorResolutionGroup,
            "VRSpectatorWidthInput", (_, newValue) =>
            {
                if (!int.TryParse(newValue, out int value))
                {
                    return;
                }
            
                VRSpectatorWidth.Value = value;
                UpdateRenderTexture();
            });
        vrSpectatorWidthInput.InputField.SetText(VRSpectatorWidth.Value.ToString());
        
        CustomInputField vrSpectatorHeightInput = UIHelper.CreateInputField(vrSpectatorResolutionGroup, "VRSpectatorHeightInput", (_, newValue) =>
        {
            if (!int.TryParse(newValue, out int value))
            {
                return;
            }
            
            VRSpectatorHeight.Value = value;
            UpdateRenderTexture();
        });
        vrSpectatorHeightInput.InputField.SetText(VRSpectatorHeight.Value.ToString());
        #endregion
    }
}