namespace RayCast.Properties
{
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "17.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase
    {
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));

        public static Settings Default
        {
            get
            {
                return defaultInstance;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public double WindowLeft
        {
            get
            {
                return ((double)(this["WindowLeft"]));
            }
            set
            {
                this["WindowLeft"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public double WindowTop
        {
            get
            {
                return ((double)(this["WindowTop"]));
            }
            set
            {
                this["WindowTop"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public double WindowWidth
        {
            get
            {
                return ((double)(this["WindowWidth"]));
            }
            set
            {
                this["WindowWidth"] = value;
            }
        }

        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public double WindowHeight
        {
            get
            {
                return ((double)(this["WindowHeight"]));
            }
            set
            {
                this["WindowHeight"] = value;
            }
        }
    }
} 