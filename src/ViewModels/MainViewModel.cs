using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using WinForms = System.Windows.Forms;
using tiny11_ui.Services;
using tiny11_ui.Models;

namespace tiny11_ui.ViewModels
{
    public class MainViewModel : BaseViewModel
    {
        private string _isoPath = string.Empty;
        public string IsoPath
        {
            get => _isoPath;
            set
            {
                _isoPath = value;
                OnPropertyChanged();
                StartBuildCommand.RaiseCanExecuteChanged();
            }
        }

        private string _scratchPath = string.Empty;
        public string ScratchPath
        {
            get => _scratchPath;
            set
            {
                _scratchPath = value;
                OnPropertyChanged();
                StartBuildCommand.RaiseCanExecuteChanged();
            }
        }

        private string _outputPath = string.Empty;
        public string OutputPath
        {
            get => _outputPath;
            set
            {
                _outputPath = value;
                OnPropertyChanged();
                StartBuildCommand.RaiseCanExecuteChanged();
            }
        }

        private string _customAutounattendPath = string.Empty;
        public string CustomAutounattendPath
        {
            get => _customAutounattendPath;
            set
            {
                _customAutounattendPath = value;
                OnPropertyChanged();
            }
        }

        private bool _isStandardBuild = true;
        public bool IsStandardBuild
        {
            get => _isStandardBuild;
            set
            {
                _isStandardBuild = value;
                OnPropertyChanged();
            }
        }

        private bool _isCoreBuild = false;
        public bool IsCoreBuild
        {
            get => _isCoreBuild;
            set
            {
                _isCoreBuild = value;
                OnPropertyChanged();
            }
        }

        #region 高级选项属性

        // 预设选项
        private bool _isMinimalPreset = false;
        public bool IsMinimalPreset
        {
            get => _isMinimalPreset;
            set { _isMinimalPreset = value; OnPropertyChanged(); if (value) ApplyMinimalPreset(); }
        }

        private bool _isBalancedPreset = true;
        public bool IsBalancedPreset
        {
            get => _isBalancedPreset;
            set { _isBalancedPreset = value; OnPropertyChanged(); if (value) ApplyBalancedPreset(); }
        }

        private bool _isGamingPreset = false;
        public bool IsGamingPreset
        {
            get => _isGamingPreset;
            set { _isGamingPreset = value; OnPropertyChanged(); if (value) ApplyGamingPreset(); }
        }

        private bool _isEnterprisePreset = false;
        public bool IsEnterprisePreset
        {
            get => _isEnterprisePreset;
            set { _isEnterprisePreset = value; OnPropertyChanged(); if (value) ApplyEnterprisePreset(); }
        }

        // 应用程序移除
        private bool _removeEdge = true;
        public bool RemoveEdge { get => _removeEdge; set { _removeEdge = value; OnPropertyChanged(); } }

        private bool _removeOneDrive = true;
        public bool RemoveOneDrive { get => _removeOneDrive; set { _removeOneDrive = value; OnPropertyChanged(); } }

        private bool _removeCortana = true;
        public bool RemoveCortana { get => _removeCortana; set { _removeCortana = value; OnPropertyChanged(); } }

        private bool _removeTeams = true;
        public bool RemoveTeams { get => _removeTeams; set { _removeTeams = value; OnPropertyChanged(); } }

        private bool _removeXbox = false;
        public bool RemoveXbox { get => _removeXbox; set { _removeXbox = value; OnPropertyChanged(); } }

        // 系统优化
        private bool _disableTelemetry = true;
        public bool DisableTelemetry { get => _disableTelemetry; set { _disableTelemetry = value; OnPropertyChanged(); } }

        private bool _disableWindowsUpdate = false;
        public bool DisableWindowsUpdate { get => _disableWindowsUpdate; set { _disableWindowsUpdate = value; OnPropertyChanged(); } }

        private bool _disableSponsoredApps = true;
        public bool DisableSponsoredApps { get => _disableSponsoredApps; set { _disableSponsoredApps = value; OnPropertyChanged(); } }

        private bool _disableReservedStorage = true;
        public bool DisableReservedStorage { get => _disableReservedStorage; set { _disableReservedStorage = value; OnPropertyChanged(); } }

        private bool _disableBitLocker = true;
        public bool DisableBitLocker { get => _disableBitLocker; set { _disableBitLocker = value; OnPropertyChanged(); } }

        // 系统要求绕过
        private bool _bypassTPM = true;
        public bool BypassTPM { get => _bypassTPM; set { _bypassTPM = value; OnPropertyChanged(); } }

        private bool _bypassCPU = true;
        public bool BypassCPU { get => _bypassCPU; set { _bypassCPU = value; OnPropertyChanged(); } }

        private bool _bypassRAM = true;
        public bool BypassRAM { get => _bypassRAM; set { _bypassRAM = value; OnPropertyChanged(); } }

        private bool _bypassSecureBoot = true;
        public bool BypassSecureBoot { get => _bypassSecureBoot; set { _bypassSecureBoot = value; OnPropertyChanged(); } }

        // OOBE 设置
        private bool _bypassMSAccount = true;
        public bool BypassMSAccount { get => _bypassMSAccount; set { _bypassMSAccount = value; OnPropertyChanged(); } }

        private bool _skipNetworkConnection = true;
        public bool SkipNetworkConnection { get => _skipNetworkConnection; set { _skipNetworkConnection = value; OnPropertyChanged(); } }

        private bool _skipPrivacyQuestions = true;
        public bool SkipPrivacyQuestions { get => _skipPrivacyQuestions; set { _skipPrivacyQuestions = value; OnPropertyChanged(); } }

        // 深度清理/大小缩减
        private bool _cleanupComponentStore = true;
        public bool CleanupComponentStore { get => _cleanupComponentStore; set { _cleanupComponentStore = value; OnPropertyChanged(); } }

        private bool _compressFinalImage = true;
        public bool CompressFinalImage { get => _compressFinalImage; set { _compressFinalImage = value; OnPropertyChanged(); } }

        private bool _removeHyperV = false;
        public bool RemoveHyperV { get => _removeHyperV; set { _removeHyperV = value; OnPropertyChanged(); } }

        private bool _removeRecall = true;
        public bool RemoveRecall { get => _removeRecall; set { _removeRecall = value; OnPropertyChanged(); } }

        private bool _removeWidgets = true;
        public bool RemoveWidgets { get => _removeWidgets; set { _removeWidgets = value; OnPropertyChanged(); } }

        private bool _removeCopilot = true;
        public bool RemoveCopilot { get => _removeCopilot; set { _removeCopilot = value; OnPropertyChanged(); } }

        private bool _removeInputComponents = false;
        public bool RemoveInputComponents { get => _removeInputComponents; set { _removeInputComponents = value; OnPropertyChanged(); } }

        private bool _cleanupDriverStore = false;
        public bool CleanupDriverStore { get => _cleanupDriverStore; set { _cleanupDriverStore = value; OnPropertyChanged(); } }

        #endregion

        private ObservableCollection<string> _windowsEditions = new ObservableCollection<string>();
        public ObservableCollection<string> WindowsEditions
        {
            get => _windowsEditions;
            set
            {
                _windowsEditions = value;
                OnPropertyChanged();
            }
        }

        private string? _selectedEdition;
        public string? SelectedEdition
        {
            get => _selectedEdition;
            set
            {
                _selectedEdition = value;
                OnPropertyChanged();
                StartBuildCommand.RaiseCanExecuteChanged();
            }
        }

        private bool _isEditionSelectionEnabled = false;
        public bool IsEditionSelectionEnabled
        {
            get => _isEditionSelectionEnabled;
            set
            {
                _isEditionSelectionEnabled = value;
                OnPropertyChanged();
            }
        }

        private string _logOutput = string.Empty;
        public string LogOutput
        {
            get => _logOutput;
            set
            {
                _logOutput = value;
                OnPropertyChanged();
            }
        }

        private string _statusText = "Ready";
        public string StatusText
        {
            get => _statusText;
            set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        private double _progressValue = 0;
        public double ProgressValue
        {
            get => _progressValue;
            set
            {
                _progressValue = value;
                OnPropertyChanged();
            }
        }

        private bool _isIndeterminate = false;
        public bool IsIndeterminate
        {
            get => _isIndeterminate;
            set
            {
                _isIndeterminate = value;
                OnPropertyChanged();
            }
        }

        private bool _isBuildRunning = false;
        public bool IsBuildRunning
        {
            get => _isBuildRunning;
            set
            {
                _isBuildRunning = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanClose));
                OnPropertyChanged(nameof(CanStartNewBuild));
                CancelBuildCommand?.RaiseCanExecuteChanged();
                StartBuildCommand?.RaiseCanExecuteChanged();
            }
        }

        /// <summary>
        /// 应用程序是否可以关闭？
        /// </summary>
        public bool CanClose => !IsBuildRunning;

        /// <summary>
        /// 应用程序版本（从程序集读取）
        /// </summary>
        public string AppVersion
        {
            get
            {
                var build = System.Reflection.CustomAttributeExtensions.GetCustomAttribute<System.Reflection.AssemblyInformationalVersionAttribute>(
                    System.Reflection.Assembly.GetExecutingAssembly())?.InformationalVersion;
                if (!string.IsNullOrWhiteSpace(build)) return "v" + build.Split('+')[0];
                var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
                return version == null ? "v?" : $"v{version.Major}.{version.Minor}.{version.Build}";
            }
        }

        /// <summary>
        /// 是否可以启动新构建
        /// </summary>
        public bool CanStartNewBuild => !IsBuildRunning;

        private readonly PowerShellService _powerShellService;
        private readonly LocalizationService _localizationService;

        public ICommand BrowseIsoCommand { get; }
        public ICommand BrowseScratchCommand { get; }
        public ICommand BrowseOutputCommand { get; }
        public ICommand BrowseAutounattendCommand { get; }
        public ICommand ClearAutounattendCommand { get; }
        public ICommand ChangeLanguageCommand { get; }
        public RelayCommand StartBuildCommand { get; }
        public RelayCommand CancelBuildCommand { get; }

        // 本地化属性
        private ObservableCollection<LanguageInfo> _availableLanguages = new ObservableCollection<LanguageInfo>();
        public ObservableCollection<LanguageInfo> AvailableLanguages
        {
            get => _availableLanguages;
            set { _availableLanguages = value; OnPropertyChanged(); }
        }

        private LanguageInfo? _selectedLanguage;
        public LanguageInfo? SelectedLanguage
        {
            get => _selectedLanguage;
            set
            {
                _selectedLanguage = value;
                OnPropertyChanged();
                if (value != null)
                {
                    _localizationService.LoadLanguage(value.Code);
                }
            }
        }

        public MainViewModel()
        {
            _localizationService = LocalizationService.Instance;
            _powerShellService = new PowerShellService(_localizationService);

            // 绑定 PowerShell service 事件
            _powerShellService.OutputReceived += OnOutputReceived;
            _powerShellService.ErrorReceived += OnErrorReceived;

            // 绑定本地化事件
            _localizationService.LanguageChanged += OnLanguageChanged;

            BrowseIsoCommand = new RelayCommand(BrowseIso);
            BrowseScratchCommand = new RelayCommand(BrowseScratch);
            BrowseOutputCommand = new RelayCommand(BrowseOutput);
            BrowseAutounattendCommand = new RelayCommand(BrowseAutounattend);
            ClearAutounattendCommand = new RelayCommand(ClearAutounattend);
            ChangeLanguageCommand = new RelayCommand(ChangeLanguage);
            StartBuildCommand = new RelayCommand(StartBuild, CanStartBuild);
            CancelBuildCommand = new RelayCommand(CancelBuild, () => IsBuildRunning);

            // 加载语言选项
            LoadAvailableLanguages();

            // 默认工作目录
            ScratchPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Tiny11_Temp");

            // 启动消息（将本地化）
            UpdateStartupMessage();
        }

        private void BrowseIso()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = GetLocalizedString("IsoDialogTitle"),
                Filter = GetLocalizedString("IsoDialogFilter"),
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                IsoPath = openFileDialog.FileName;
                LogOutput += string.Format(GetLocalizedString("IsoSelected"), IsoPath) + "\n";

                // 加载 Windows 版本
                _ = LoadWindowsEditionsAsync();
            }
        }

        private void BrowseScratch()
        {
            using var folderDialog = new WinForms.FolderBrowserDialog
            {
                Description = GetLocalizedString("WorkingDirDialogTitle"),
                UseDescriptionForTitle = true,
                SelectedPath = ScratchPath
            };

            if (folderDialog.ShowDialog() == WinForms.DialogResult.OK)
            {
                ScratchPath = folderDialog.SelectedPath;
                LogOutput += string.Format(GetLocalizedString("WorkingDirSelected"), ScratchPath) + "\n";
            }
        }

        private void BrowseOutput()
        {
            var saveFileDialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = GetLocalizedString("OutputDialogTitle"),
                Filter = GetLocalizedString("IsoDialogFilter"),
                DefaultExt = "iso",
                FileName = "tiny11.iso"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                OutputPath = saveFileDialog.FileName;
                LogOutput += string.Format(GetLocalizedString("OutputPathSelected"), OutputPath) + "\n";
            }
        }

        private void BrowseAutounattend()
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = GetLocalizedString("AutounattendDialogTitle"),
                Filter = GetLocalizedString("AutounattendDialogFilter"),
                FilterIndex = 1
            };

            if (openFileDialog.ShowDialog() == true)
            {
                CustomAutounattendPath = openFileDialog.FileName;
                LogOutput += string.Format(GetLocalizedString("AutounattendSelected"), CustomAutounattendPath) + "\n";
            }
        }

        private void ClearAutounattend()
        {
            if (string.IsNullOrEmpty(CustomAutounattendPath))
                return;

            CustomAutounattendPath = string.Empty;
            LogOutput += GetLocalizedString("AutounattendCleared") + "\n";
        }

        /// <summary>
        /// 取消操作
        /// </summary>
        private async void CancelBuild()
        {
            if (!IsBuildRunning) return;

            var result = System.Windows.MessageBox.Show(
                GetLocalizedString("CancelConfirmMessage"),
                GetLocalizedString("CancelConfirmTitle"),
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                StatusText = GetLocalizedString("StatusCancelling");
                await _powerShellService.CancelAsync();
                IsBuildRunning = false;
                IsIndeterminate = false;
                StatusText = GetLocalizedString("StatusCancelled");
            }
        }

        /// <summary>
        /// 窗口关闭时调用 - 如果有运行中的操作则阻止关闭
        /// </summary>
        public bool HandleWindowClosing()
        {
            if (!IsBuildRunning) return true; // 可以关闭

            var result = System.Windows.MessageBox.Show(
                GetLocalizedString("CloseConfirmMessage"),
                GetLocalizedString("CloseConfirmTitle"),
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning);

            if (result == System.Windows.MessageBoxResult.Yes)
            {
                // 取消操作然后关闭
                Task.Run(async () =>
                {
                    await _powerShellService.CancelAsync();

                    // 在 UI 线程中关闭应用程序
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        IsBuildRunning = false;
                        System.Windows.Application.Current.Shutdown();
                    });
                });

                return false; // 暂时阻止关闭，async 完成后将关闭
            }

            return false; // 阻止关闭
        }

        private async void StartBuild()
        {
            if (!CanStartBuild())
                return;

            IsBuildRunning = true;

            try
            {
                StatusText = GetLocalizedString("StatusStarting");
                IsIndeterminate = true;

                // 创建工作目录
                if (!Directory.Exists(ScratchPath))
                {
                    Directory.CreateDirectory(ScratchPath);
                }

                // 将用户选项转换为 ComponentRemovalOptions
                var options = new ComponentRemovalOptions
                {
                    // 应用程序移除
                    RemoveEdge = RemoveEdge,
                    RemoveOneDrive = RemoveOneDrive,
                    RemoveCortana = RemoveCortana,
                    RemoveTeams = RemoveTeams,
                    RemoveXbox = RemoveXbox,

                    // 系统优化
                    DisableTelemetry = DisableTelemetry,
                    DisableWindowsUpdate = DisableWindowsUpdate,
                    DisableSponsoredApps = DisableSponsoredApps,
                    DisableReservedStorage = DisableReservedStorage,
                    DisableBitLocker = DisableBitLocker,

                    // 系统要求绕过
                    BypassTPM = BypassTPM,
                    BypassCPU = BypassCPU,
                    BypassRAM = BypassRAM,
                    BypassSecureBoot = BypassSecureBoot,

                    // OOBE 设置
                    BypassMSAccount = BypassMSAccount,
                    SkipNetworkConnection = SkipNetworkConnection,
                    SkipPrivacyQuestions = SkipPrivacyQuestions,
                    CustomAutounattendPath = string.IsNullOrWhiteSpace(CustomAutounattendPath) ? null : CustomAutounattendPath,

                    // 深度清理/大小缩减
                    CleanupComponentStore = CleanupComponentStore,
                    CompressFinalImage = CompressFinalImage,
                    RemoveHyperV = RemoveHyperV,
                    RemoveRecall = RemoveRecall,
                    RemoveWidgets = RemoveWidgets,
                    RemoveCopilot = RemoveCopilot,
                    RemoveInputComponents = RemoveInputComponents,
                    CleanupDriverStore = CleanupDriverStore
                };

                LogOutput += GetLocalizedString("LogBuildStarted") + "\n";
                LogOutput += string.Format(GetLocalizedString("LogIsoPath"), IsoPath) + "\n";
                LogOutput += string.Format(GetLocalizedString("LogWorkingDir"), ScratchPath) + "\n";
                LogOutput += string.Format(GetLocalizedString("LogOutputPath"), OutputPath) + "\n";
                LogOutput += string.Format(GetLocalizedString("LogEdition"), SelectedEdition) + "\n";
                LogOutput += string.Format(GetLocalizedString("LogBuildType"), IsCoreBuild ? GetLocalizedString("LogBuildTypeCore") : GetLocalizedString("LogBuildTypeStandard")) + "\n\n";

                // 记录所选设置
                var yes = GetLocalizedString("LogYes");
                var no = GetLocalizedString("LogNo");
                LogOutput += GetLocalizedString("LogSelectedOptions") + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogRemoveEdge"), RemoveEdge ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogRemoveOneDrive"), RemoveOneDrive ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogRemoveCortana"), RemoveCortana ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogRemoveTeams"), RemoveTeams ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogRemoveXbox"), RemoveXbox ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogDisableTelemetry"), DisableTelemetry ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogDisableUpdate"), DisableWindowsUpdate ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogBypassTPM"), BypassTPM ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogBypassMSAccount"), BypassMSAccount ? yes : no) + "\n";
                LogOutput += "   " + string.Format(GetLocalizedString("LogCustomAutounattend"), string.IsNullOrWhiteSpace(CustomAutounattendPath) ? no : CustomAutounattendPath) + "\n";
                LogOutput += "\n";

                // 从 Windows 版本字符串中提取版本索引
                var editionIndex = 1; // 默认值
                if (SelectedEdition!.Contains(" - "))
                {
                    var parts = SelectedEdition.Split(new[] { " - " }, StringSplitOptions.None);
                    if (int.TryParse(parts[0], out int parsedIndex))
                    {
                        editionIndex = parsedIndex;
                    }
                }

                LogOutput += string.Format(GetLocalizedString("LogEditionIndex"), editionIndex) + "\n\n";

                StatusText = GetLocalizedString("LogProcessing");

                // 使用新方法 - 带用户选项
                var success = await _powerShellService.RunTiny11WithOptionsAsync(
                    IsoPath!,
                    ScratchPath,
                    OutputPath,
                    editionIndex,
                    options,
                    IsCoreBuild
                );

                // 检查输出 ISO
                if (success && File.Exists(OutputPath))
                {
                    StatusText = GetLocalizedString("BuildCompleted");
                    LogOutput += "\n" + GetLocalizedString("LogBuildSuccess") + "\n";
                    LogOutput += string.Format(GetLocalizedString("LogOutputLocation"), OutputPath) + "\n";

                    // 显示文件大小
                    var fileInfo = new FileInfo(OutputPath);
                    var sizeInGB = fileInfo.Length / (1024.0 * 1024.0 * 1024.0);
                    LogOutput += string.Format(GetLocalizedString("LogFileSize"), sizeInGB.ToString("F2"), fileInfo.Length.ToString("N0")) + "\n";
                }
                else if (success)
                {
                    StatusText = GetLocalizedString("LogBuildFailedNoIso");
                    LogOutput += "\n" + GetLocalizedString("LogBuildFailedNoIso") + "\n";
                    LogOutput += string.Format(GetLocalizedString("LogExpectedLocation"), OutputPath) + "\n";
                    LogOutput += GetLocalizedString("LogCheckWorkingDir") + "\n";
                }
                else
                {
                    StatusText = GetLocalizedString("BuildFailed");
                    LogOutput += "\n" + GetLocalizedString("LogBuildFailed") + "\n";
                }
            }
            catch (Exception ex)
            {
                LogOutput += string.Format(GetLocalizedString("LogError"), ex.Message) + "\n";
                StatusText = GetLocalizedString("Error");
            }
            finally
            {
                IsBuildRunning = false;
                IsIndeterminate = false;
            }
        }

        private bool CanStartBuild()
        {
            // 如果正在运行则无法启动
            if (IsBuildRunning) return false;

            var canStart = !string.IsNullOrEmpty(IsoPath) &&
                           !string.IsNullOrEmpty(ScratchPath) &&
                           !string.IsNullOrEmpty(OutputPath) &&
                           !string.IsNullOrEmpty(SelectedEdition);

            // 调试信息
            if (!canStart)
            {
                var missing = new System.Collections.Generic.List<string>();
                if (string.IsNullOrEmpty(IsoPath)) missing.Add("ISO");
                if (string.IsNullOrEmpty(ScratchPath)) missing.Add(GetLocalizedString("WorkingDirectory"));
                if (string.IsNullOrEmpty(SelectedEdition)) missing.Add(GetLocalizedString("WindowsEdition"));

                StatusText = string.Format(GetLocalizedString("StatusMissing"), string.Join(", ", missing));
            }
            else
            {
                StatusText = GetLocalizedString("StatusReady");
            }

            return canStart;
        }

        private bool IsRunningAsAdministrator()
        {
            var identity = WindowsIdentity.GetCurrent();
            var principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        private async Task LoadWindowsEditionsAsync()
        {
            try
            {
                if (!IsRunningAsAdministrator())
                {
                    LogOutput += GetLocalizedString("LogAdminRightsRecommended") + "\n";
                    LogOutput += GetLocalizedString("LogAdminRightsNeededForEditions") + "\n";
                    LogOutput += GetLocalizedString("LoadingDefaultEditions") + "\n\n";

                    // 如果没有管理员权限则添加所有默认版本
                    WindowsEditions.Clear();
                    WindowsEditions.Add("1 - Windows 11 Home");
                    WindowsEditions.Add("2 - Windows 11 Home Single Language");
                    WindowsEditions.Add("3 - Windows 11 Education");
                    WindowsEditions.Add("4 - Windows 11 Pro");
                    WindowsEditions.Add("5 - Windows 11 Pro Education");
                    WindowsEditions.Add("6 - Windows 11 Pro for Workstations");

                    SelectedEdition = WindowsEditions[3]; // 默认选择 Windows 11 Pro
                    IsEditionSelectionEnabled = true;
                    StatusText = "就绪 (默认版本)";
                    return;
                }

                StatusText = "Windows 版本加载中...";
                IsIndeterminate = true;

                var editions = await _powerShellService.GetWindowsEditionsAsync(IsoPath!);

                WindowsEditions.Clear();
                foreach (var edition in editions)
                {
                    WindowsEditions.Add(edition);
                }

                if (WindowsEditions.Count > 0)
                {
                    SelectedEdition = WindowsEditions[0];
                    IsEditionSelectionEnabled = true;
                    LogOutput += $"{WindowsEditions.Count} " + GetLocalizedString("EditionsLoaded") + "\n";
                }
                else
                {
                    LogOutput += GetLocalizedString("NoEditionsFound") + "\n";
                }

                StatusText = "就绪";
            }
            catch (Exception ex)
            {
                LogOutput += string.Format(GetLocalizedString("LogError"), GetLocalizedString("EditionsLoadFailed") + ": " + ex.Message) + "\n";
                LogOutput += GetLocalizedString("LoadingDefaultEditions") + "\n";

                // 错误时添加所有默认版本
                WindowsEditions.Clear();
                WindowsEditions.Add("1 - Windows 11 Home");
                WindowsEditions.Add("2 - Windows 11 Home Single Language");
                WindowsEditions.Add("3 - Windows 11 Education");
                WindowsEditions.Add("4 - Windows 11 Pro");
                WindowsEditions.Add("5 - Windows 11 Pro Education");
                WindowsEditions.Add("6 - Windows 11 Pro for Workstations");
                SelectedEdition = WindowsEditions[3]; // Windows 11 Pro
                IsEditionSelectionEnabled = true;

                StatusText = "就绪 (默认版本)";
            }
            finally
            {
                IsIndeterminate = false;
            }
        }

        private void OnOutputReceived(string output)
        {
            // 在 UI 线程中运行
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LogOutput += output + "\n";

                // 根据输出消息进行进度跟踪
                if (output.Contains("Exporting image"))
                    StatusText = "Exporting image...";
                else if (output.Contains("Unmounting image"))
                    StatusText = GetLocalizedString("StatusUnmounting");
                else if (output.Contains("Cleanup complete"))
                    StatusText = GetLocalizedString("StatusCleanup");
                else if (output.Contains("Creating ISO"))
                    StatusText = GetLocalizedString("StatusCreatingIso");
                else if (output.Contains("Mounting"))
                    StatusText = GetLocalizedString("StatusMounting");
                else if (output.Contains("Copying"))
                    StatusText = GetLocalizedString("StatusCopying");
            });
        }

        private void OnErrorReceived(string error)
        {
            // 在 UI 线程中运行
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                LogOutput += string.Format(GetLocalizedString("LogError"), error) + "\n";
            });
        }

        #region 预设方法

        private void ApplyMinimalPreset()
        {
            // Minimal：最大清理，最少功能
            RemoveEdge = true;
            RemoveOneDrive = true;
            RemoveCortana = true;
            RemoveTeams = true;
            RemoveXbox = true;

            DisableTelemetry = true;
            DisableWindowsUpdate = true;
            DisableSponsoredApps = true;
            DisableReservedStorage = true;
            DisableBitLocker = true;

            BypassTPM = true;
            BypassCPU = true;
            BypassRAM = true;
            BypassSecureBoot = true;

            BypassMSAccount = true;
            SkipNetworkConnection = true;
            SkipPrivacyQuestions = true;

            CleanupComponentStore = true;
            CompressFinalImage = true;
            RemoveHyperV = true;
            RemoveRecall = true;
            RemoveWidgets = true;
            RemoveCopilot = true;
            RemoveInputComponents = true;
            CleanupDriverStore = true;

            LogOutput += GetLocalizedString("LogPresetMinimalApplied") + "\n";
        }

        private void ApplyBalancedPreset()
        {
            // Balanced：平衡方法（默认）
            RemoveEdge = true;
            RemoveOneDrive = true;
            RemoveCortana = true;
            RemoveTeams = true;
            RemoveXbox = false;

            DisableTelemetry = true;
            DisableWindowsUpdate = false;
            DisableSponsoredApps = true;
            DisableReservedStorage = true;
            DisableBitLocker = true;

            BypassTPM = true;
            BypassCPU = true;
            BypassRAM = true;
            BypassSecureBoot = true;

            BypassMSAccount = true;
            SkipNetworkConnection = true;
            SkipPrivacyQuestions = true;

            CleanupComponentStore = true;
            CompressFinalImage = true;
            RemoveHyperV = false;
            RemoveRecall = true;
            RemoveWidgets = true;
            RemoveCopilot = true;
            RemoveInputComponents = false;
            CleanupDriverStore = false;

            LogOutput += GetLocalizedString("LogPresetBalancedApplied") + "\n";
        }

        private void ApplyGamingPreset()
        {
            // Gaming：性能导向，Xbox 应用保留
            RemoveEdge = false; // 一些游戏使用 Edge WebView
            RemoveOneDrive = true;
            RemoveCortana = true;
            RemoveTeams = true;
            RemoveXbox = false; // 保留 Xbox 应用用于游戏

            DisableTelemetry = true;
            DisableWindowsUpdate = false; // 用于游戏更新
            DisableSponsoredApps = true;
            DisableReservedStorage = false; // 用于性能
            DisableBitLocker = true;

            BypassTPM = true;
            BypassCPU = true;
            BypassRAM = true;
            BypassSecureBoot = true;

            BypassMSAccount = false; // 用于 Xbox 集成
            SkipNetworkConnection = false;
            SkipPrivacyQuestions = true;

            CleanupComponentStore = true;
            CompressFinalImage = true;
            RemoveHyperV = false;
            RemoveRecall = true;
            RemoveWidgets = true;
            RemoveCopilot = true;
            RemoveInputComponents = false;
            CleanupDriverStore = false;

            LogOutput += GetLocalizedString("LogPresetGamingApplied") + "\n";
        }

        private void ApplyEnterprisePreset()
        {
            // Enterprise：企业环境的安全性和稳定性导向
            RemoveEdge = false; // 企业应用
            RemoveOneDrive = false; // 办公文件
            RemoveCortana = true;
            RemoveTeams = false; // 企业通信
            RemoveXbox = true;

            DisableTelemetry = false; // 企业遥测可保留
            DisableWindowsUpdate = false; // 安全更新
            DisableSponsoredApps = true;
            DisableReservedStorage = false;
            DisableBitLocker = false; // 企业安全

            BypassTPM = false; // 企业安全需要保留 TPM
            BypassCPU = true;
            BypassRAM = true;
            BypassSecureBoot = false; // 企业安全

            BypassMSAccount = true;
            SkipNetworkConnection = false;
            SkipPrivacyQuestions = false; // 企业合规

            CleanupComponentStore = true;
            CompressFinalImage = false; // 保留标准 WIM 以便服务
            RemoveHyperV = false; // 可能需要虚拟化
            RemoveRecall = true; // 隐私/合规
            RemoveWidgets = true;
            RemoveCopilot = true; // 企业合规
            RemoveInputComponents = false; // 可访问性需求
            CleanupDriverStore = false; // 不同硬件的广泛驱动支持

            LogOutput += GetLocalizedString("LogPresetEnterpriseApplied") + "\n";
        }

        #endregion

        #region 本地化方法

        private void LoadAvailableLanguages()
        {
            var languages = _localizationService.GetAvailableLanguages();
            AvailableLanguages.Clear();
            foreach (var language in languages)
            {
                AvailableLanguages.Add(language);
            }

            // 选择当前语言 - Ensure DefaultLanguage sync with _currentLanguage
            SelectedLanguage = AvailableLanguages.FirstOrDefault(l => l.Code == _localizationService.CurrentLanguage);
        }

        private void ChangeLanguage()
        {
            if (SelectedLanguage != null)
            {
                _localizationService.LoadLanguage(SelectedLanguage.Code);
            }
        }

        private void OnLanguageChanged(object? sender, string languageCode)
        {
            // 更新 UI 中的所有字符串
            OnPropertyChanged(nameof(LocalizedLabels));
            OnPropertyChanged(nameof(LocalizedButtons));
            OnPropertyChanged(nameof(LocalizedHeaders));
            OnPropertyChanged(nameof(LocalizedPresets));
            OnPropertyChanged(nameof(LocalizedApps));
            OnPropertyChanged(nameof(LocalizedSystemFeatures));
            OnPropertyChanged(nameof(LocalizedSystemRequirements));
            OnPropertyChanged(nameof(LocalizedInstallationProcess));
            OnPropertyChanged(nameof(LocalizedDeepCleanup));
            OnPropertyChanged(nameof(LocalizedTooltips));

            // 更新 StatusText
            if (!CanStartBuild())
            {
                CanStartBuild(); // 此方法更新 StatusText
            }
            else
            {
                StatusText = GetLocalizedString("Ready");
            }

            // 在日志中添加语言变更消息
            var languageName = AvailableLanguages.FirstOrDefault(language => language.Code == languageCode)?.DisplayName
                               ?? languageCode;
            var message = string.Format(GetLocalizedString("LanguageChanged"), languageName);
            LogOutput += $"{message}\n";

            // 也更新对话框
            UpdateDialogTexts();
        }

        private void UpdateStartupMessage()
        {
            LogOutput = GetLocalizedString("AppStarted") + "\n";
            if (IsRunningAsAdministrator())
            {
                LogOutput += GetLocalizedString("AdminMode") + "\n";
            }
            else
            {
                LogOutput += GetLocalizedString("UserMode") + "\n";
                LogOutput += GetLocalizedString("AdminRecommendation") + "\n";
            }
            LogOutput += GetLocalizedString("DefaultWorkingDir") + ": " + ScratchPath + "\n\n";
        }

        private void UpdateDialogTexts()
        {
            // 对话框标题更新放在这里
            // 暂时留空
        }

        public string GetLocalizedString(string key) => _localizationService.GetString(key);
        public string GetLocalizedString(string key, params object[] args) => _localizationService.GetString(key, args);

        // 本地化 UI 属性
        public LocalizedLabels LocalizedLabels => new LocalizedLabels(_localizationService);
        public LocalizedButtons LocalizedButtons => new LocalizedButtons(_localizationService);
        public LocalizedHeaders LocalizedHeaders => new LocalizedHeaders(_localizationService);
        public LocalizedPresets LocalizedPresets => new LocalizedPresets(_localizationService);
        public LocalizedApps LocalizedApps => new LocalizedApps(_localizationService);
        public LocalizedSystemFeatures LocalizedSystemFeatures => new LocalizedSystemFeatures(_localizationService);
        public LocalizedSystemRequirements LocalizedSystemRequirements => new LocalizedSystemRequirements(_localizationService);
        public LocalizedInstallationProcess LocalizedInstallationProcess => new LocalizedInstallationProcess(_localizationService);
        public LocalizedDeepCleanup LocalizedDeepCleanup => new LocalizedDeepCleanup(_localizationService);
        public LocalizedTooltips LocalizedTooltips => new LocalizedTooltips(_localizationService);

        #endregion
    }

    public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool>? _canExecute;

    public RelayCommand(Action execute, Func<bool>? canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler? CanExecuteChanged;

    public bool CanExecute(object? parameter) => _canExecute == null || _canExecute();

    public void Execute(object? parameter) => _execute();

    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}


}
