namespace tiny11_ui.Models
{
    /// <summary>
    /// 指定 Tiny11 构建时要移除/禁用哪些组件的选项
    /// </summary>
    public class ComponentRemovalOptions
    {
        // 应用程序移除选项
        public bool RemoveEdge { get; set; } = true;
        public bool RemoveOneDrive { get; set; } = true;
        public bool RemoveCortana { get; set; } = true;
        public bool RemoveTeams { get; set; } = true;
        public bool RemoveXbox { get; set; } = false;

        // 系统优化选项
        public bool DisableTelemetry { get; set; } = true;
        public bool DisableWindowsUpdate { get; set; } = false;
        public bool DisableSponsoredApps { get; set; } = true;
        public bool DisableReservedStorage { get; set; } = true;
        public bool DisableBitLocker { get; set; } = true;

        // 系统要求绕过选项
        public bool BypassTPM { get; set; } = true;
        public bool BypassCPU { get; set; } = true;
        public bool BypassRAM { get; set; } = true;
        public bool BypassSecureBoot { get; set; } = true;

        // OOBE 设置
        public bool BypassMSAccount { get; set; } = true;
        public bool SkipNetworkConnection { get; set; } = true;
        public bool SkipPrivacyQuestions { get; set; } = true;

        /// <summary>
        /// 用户提供的自定义 autounattend.xml 文件路径（可选）。
        /// 如果提供，将复制到生成的 ISO 根目录。
        /// </summary>
        public string? CustomAutounattendPath { get; set; }

        // 深度清理/大小缩减
        public bool CleanupComponentStore { get; set; } = true;
        public bool CompressFinalImage { get; set; } = true;
        public bool RemoveHyperV { get; set; } = false;
        public bool RemoveRecall { get; set; } = true;
        public bool RemoveWidgets { get; set; } = true;
        public bool RemoveCopilot { get; set; } = true;
        public bool RemoveInputComponents { get; set; } = false;
        public bool CleanupDriverStore { get; set; } = false;

        /// <summary>
        /// 返回要移除的 AppX 包列表
        /// </summary>
        public string[] GetPackagesToRemove()
        {
            var packages = new System.Collections.Generic.List<string>
            {
                // 始终移除（臃肿软件）
                "Microsoft.BingNews",
                "Microsoft.BingWeather",
                "Microsoft.GetHelp",
                "Microsoft.Getstarted",
                "Microsoft.MicrosoftOfficeHub",
                "Microsoft.MicrosoftSolitaireCollection",
                "Microsoft.People",
                "Microsoft.PowerAutomateDesktop",
                "Microsoft.Todos",
                "Microsoft.WindowsAlarms",
                "Microsoft.WindowsCommunicationsApps",
                "Microsoft.WindowsFeedbackHub",
                "Microsoft.WindowsMaps",
                "Microsoft.WindowsSoundRecorder",
                "Microsoft.YourPhone",
                "Microsoft.ZuneMusic",
                "Microsoft.ZuneVideo",
                "MicrosoftCorporationII.MicrosoftFamily",
                "MicrosoftCorporationII.QuickAssist",
                "Clipchamp.Clipchamp",
                "Microsoft.Windows.DevHome"
            };

            // Edge 及相关组件
            if (RemoveEdge)
            {
                packages.Add("Microsoft.MicrosoftEdge");
                packages.Add("Microsoft.MicrosoftEdge.Stable");
                packages.Add("Microsoft.MicrosoftEdgeDevToolsClient");
            }

            // OneDrive
            if (RemoveOneDrive)
            {
                packages.Add("Microsoft.OneDrive");
                packages.Add("Microsoft.OneDriveSync");
            }

            // Cortana
            if (RemoveCortana)
            {
                if (!packages.Contains("Microsoft.549981C3F5F10"))
                    packages.Add("Microsoft.549981C3F5F10");
            }

            // Teams/Chat 属于同一包系列；作为单一选项管理
            if (RemoveTeams)
            {
                packages.Add("MicrosoftTeams");
                packages.Add("Microsoft.MicrosoftTeams");
                packages.Add("MSTeams");
                packages.Add("Microsoft.Windows.Teams");
            }

            // Xbox 应用程序
            if (RemoveXbox)
            {
                packages.Add("Microsoft.GamingApp");
                packages.Add("Microsoft.Xbox.TCUI");
                packages.Add("Microsoft.XboxGamingOverlay");
                packages.Add("Microsoft.XboxGameOverlay");
                packages.Add("Microsoft.XboxSpeechToTextOverlay");
                packages.Add("Microsoft.XboxIdentityProvider");
                packages.Add("Microsoft.XboxApp");
            }

            // Widgets（短子字符串 - 包名可能随版本变化）
            if (RemoveWidgets)
            {
                packages.Add("WebExperience");
            }

            // Copilot（短子字符串 - 包名可能随版本变化）
            if (RemoveCopilot)
            {
                packages.Add("Copilot");
            }

            return packages.ToArray();
        }

    }
}
