using System.Diagnostics.CodeAnalysis;
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
    
    public static ConfigEntry<bool> SecondaryCameraEnabled;
    
    public static ConfigEntry<int> SecondaryWidth;
    public static ConfigEntry<int> SecondaryHeight;

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
        UIHelper.CreateSectionHeader(modGroup, "ModGroupSection", "SpinSpout_Name", false);
        
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
    }
}