using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using BepInEx.Configuration;
using SpinCore.Translation;
using SpinCore.UI;
using SpinSpout.Spout;
using UnityEngine;

namespace SpinSpout;

[SuppressMessage("ReSharper", "UnusedMember.Local")]
public partial class Plugin
{
    public static ConfigEntry<bool> Enabled;

    public static ConfigEntry<int> Width;
    public static ConfigEntry<int> Height;

    public static ConfigEntry<Vector3> Offset;
    public static ConfigEntry<Vector3> Rotation;
    
    public static ConfigEntry<bool> SecondaryCameraEnabled;
    
    public static ConfigEntry<int> SecondaryWidth;
    public static ConfigEntry<int> SecondaryHeight;
    
    public static ConfigEntry<Vector3> SecondaryOffset;
    public static ConfigEntry<Vector3> SecondaryRotation;

    private void RegisterConfigEntries()
    {
        TranslationHelper.AddTranslation("SpinSpout_Name", "SpinSpout");
        
        Enabled = Config.Bind("General", nameof(Enabled), true, "Enable the Spout2 camera output");
        TranslationHelper.AddTranslation("SpinSpout_Enabled", "Enabled");
        SecondaryCameraEnabled = Config.Bind("General", nameof(SecondaryCameraEnabled), false,
            "Enable a secondary Spout2 camera output");
        TranslationHelper.AddTranslation("SpinSpout_SecondaryCameraEnabled", "Secondary Camera Enabled");

        Width = Config.Bind("Resolution", nameof(Width), 2560, "The width of the Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_Width", "Width");
        Height = Config.Bind("Resolution", nameof(Height), 1440, "The height of the Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_Height", "Height");
        SecondaryWidth = Config.Bind("Resolution", nameof(SecondaryWidth), 2560, "The width of the secondary Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_SecondaryWidth", "Secondary Width");
        SecondaryHeight = Config.Bind("Resolution", nameof(SecondaryHeight), 1440, "The height of the secondary Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_SecondaryHeight", "Secondary Height");
        
        Offset = Config.Bind("Offset", nameof(Offset), Vector3.zero, "The relative positional offset of the Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_Offset", "Offset");
        Rotation = Config.Bind("Offset", nameof(Rotation), Vector3.zero, "The relative rotation of the Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_Rotation", "Rotation");
        SecondaryOffset = Config.Bind("Offset", nameof(SecondaryOffset), Vector3.zero,
            "The relative positional offset of the secondary Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_SecondaryOffset", "Secondary Offset");
        SecondaryRotation = Config.Bind("Offset", nameof(SecondaryRotation), Vector3.zero,
            "The relative rotation of the secondary Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_SecondaryRotation", "Secondary Rotation");
        
        TranslationHelper.AddTranslation("SpinSpout_Resolution", "Resolution");
        TranslationHelper.AddTranslation("SpinSpout_SecondaryResolution", "Secondary Camera Resolution");
    }

    private static void CreateModPage()
    {
        CustomPage rootModPage = UIHelper.CreateCustomPage("ModSettings");
        rootModPage.OnPageLoad += RootModPageOnOnPageLoad;
        
        UIHelper.RegisterMenuInModSettingsRoot("SpinSpout_Name", rootModPage);
    }

    private static void RootModPageOnOnPageLoad(Transform rootModPageTransform)
    {
        CustomGroup modGroup = UIHelper.CreateGroup(rootModPageTransform, nameof(SpinSpout));
        UIHelper.CreateSectionHeader(modGroup, "ModGroupHeader", "SpinSpout_Name", false);
        
        #region Enabled
        CustomGroup enabledGroup = UIHelper.CreateGroup(modGroup, "EnabledGroup");
        enabledGroup.LayoutDirection = Axis.Horizontal;
        //UIHelper.CreateLabel(enabledGroup, "EnabledLabel", "SpinSpout_Enabled");
        UIHelper.CreateSmallToggle(enabledGroup, "Enabled", "SpinSpout_Enabled", Enabled.Value, value =>
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
            "SpinSpout_SecondaryCameraEnabled", SecondaryCameraEnabled.Value, value =>
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
        
        UIHelper.CreateSectionHeader(modGroup, "ResolutionHeader", "SpinSpout_Resolution", false);
        
        #region Resolution
        CustomGroup resolutionGroup = UIHelper.CreateGroup(modGroup, "ResolutionGroup");
        resolutionGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(resolutionGroup, "ResolutionLabel", "SpinSpout_Resolution");
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
        UIHelper.CreateLabel(secondaryResolutionGroup, "SecondaryResolutionLabel", "SpinSpout_SecondaryResolution");
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
        
        UIHelper.CreateSectionHeader(modGroup, "OffsetHeader", "SpinSpout_Offset", false);
        
        #region PositionOffset
        CustomGroup offsetGroup = UIHelper.CreateGroup(modGroup, "OffsetGroup");
        offsetGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(offsetGroup, "OffsetLabel", "SpinSpout_Offset");

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
        UIHelper.CreateLabel(secondaryOffsetGroup, "SecondaryOffsetLabel", "SpinSpout_SecondaryOffset");

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
        
        UIHelper.CreateSectionHeader(modGroup, "RotationHeader", "SpinSpout_Rotation", false);
        
        #region RotationOffset
        CustomGroup rotationGroup = UIHelper.CreateGroup(modGroup, "RotationGroup");
        rotationGroup.LayoutDirection = Axis.Horizontal;
        UIHelper.CreateLabel(rotationGroup, "RotationLabel", "SpinSpout_Rotation");

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
        UIHelper.CreateLabel(secondaryRotationGroup, "SecondaryRotationLabel", "SpinSpout_SecondaryRotation");

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
    }
}