using p5rpc.events.saepov.Template.Configuration;
using System.ComponentModel;

namespace p5rpc.events.saepov.Configuration
{
    public class Config : Configurable<Config>
    {
        /*
            User Properties:
                - Please put all of your configurable properties here.

            By default, configuration saves as "Config.json" in mod user config folder.    
            Need more config files/classes? See Configuration.cs

            Available Attributes:
            - Category
            - DisplayName
            - Description
            - DefaultValue

            // Technically Supported but not Useful
            - Browsable
            - Localizable

            The `DefaultValue` attribute is used as part of the `Reset` button in Reloaded-Launcher.
        */

        [DisplayName("Event Fixes")]
        [Description("Manually fixes some events to deactivate POV in instances where it lingers way after Sae leaves the scene.")]
        [DefaultValue(true)]
        public bool EventFixes { get; set; } = true;

        [DisplayName("Shadow Sae Hat Removal")]
        [Description("Removes Sae's hat from her model to reduce clipping.")]
        [DefaultValue(true)]
        public bool HatRemoval { get; set; } = true;

        [DisplayName("Distance")]
        [Description("How far in front of Sae the camera will be.")]
        [DefaultValue(6)]
        public float CamDistance { get; set; } = 6;

        [DisplayName("FOV")]
        [Description("POV Camera Field of view")]
        [DefaultValue(50)]
        public float CamFOV { get; set; } = 50;

        [DisplayName("POV Model Major Id")]
        [Description("Allows the user to change which model POV will trigger for. THIS MAY NOT WORK CONSISTENLY FOR ALL MODELS.")]
        [DefaultValue(1005)]
        public short PovModelMajorId { get; set; } = 1005;
    }

    /// <summary>
    /// Allows you to override certain aspects of the configuration creation process (e.g. create multiple configurations).
    /// Override elements in <see cref="ConfiguratorMixinBase"/> for finer control.
    /// </summary>
    public class ConfiguratorMixin : ConfiguratorMixinBase
    {
        // 
    }
}
