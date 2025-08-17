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

    private void RegisterConfigEntries()
    {
        TranslationHelper.AddTranslation("SpinSpout_Name", "SpinSpout");
        
        Enabled = Config.Bind("General", nameof(Enabled), true, "Enable the Spout2 camera output");
        TranslationHelper.AddTranslation("SpinSpout_Enabled", "Enabled");

        Width = Config.Bind("Resolution", nameof(Width), 2560, "The width of the Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_Width", "Width");
        Height = Config.Bind("Resolution", nameof(Height), 1440, "The height of the Spout2 camera");
        TranslationHelper.AddTranslation("SpinSpout_Height", "Height");
        
        TranslationHelper.AddTranslation("SpinSpout_Resolution", "Resolution");
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
    }
}