using Microsoft.Win32;
using System;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows;
using Wpf.Ui.Controls;


namespace Watson
{
    public partial class MainWindow : FluentWindow
    {
        private string softKey_censored;
        private string oemKey_censored;
        private string backupKey_censored;
        private string defaultKey_censored;

        private bool isCensored_oem = true;
        private bool isCensored_soft = true;
        private bool isCensored_backup = true;
        private bool isCensored_default = true;


        public MainWindow()
        {
            InitializeComponent();

            //Wpf.Ui.Appearance.SystemThemeWatcher.Watch(this);

            Wpf.Ui.Appearance.ApplicationThemeManager.Apply(
              Wpf.Ui.Appearance.ApplicationTheme.Dark, // Theme type
              WindowBackdropType.Tabbed,  // Background type
              true                                      // Whether to change accents automatically
            );
            Logo.Source = WindowsHandler.Logo;
            softKey_censored = CensorKey(WindowsHandler.licenseKey);
            oemKey_censored = CensorKey(WindowsHandler.oemKey);
            backupKey_censored = CensorKey(WindowsHandler.backupKey);
            defaultKey_censored = CensorKey(WindowsHandler.defaultKey);

            productName.Text = WindowsHandler.ProductName;
            version.Text = WindowsHandler.DisplayVersion;
            buildVersion.Text = $"{WindowsHandler.Version} {WindowsHandler.Platform}";
            productID.Text = WindowsHandler.ProductID;
            actID.Text = WindowsHandler.ActID;

            ///oemEdition.Text = WindowsHandler.pkChannel;

            softKey.Text = softKey_censored;
            oemKey.Text = oemKey_censored;
            backupKey.Text = backupKey_censored;
            defaultKey.Text = defaultKey_censored;


            if (WindowsHandler.oemKey == "")
            {
                oemKey.Text = "OEM key not present in firmware";
                copyOem_Btn.IsEnabled = false;
                showOem_Btn.IsEnabled = false;
            }

            if (WindowsHandler.backupKey == WindowsHandler.licenseKey)
            {
                backupCard.Visibility = Visibility.Collapsed;
            }
            if (WindowsHandler.backupKey == WindowsHandler.defaultKey ||
                WindowsHandler.licenseKey == WindowsHandler.defaultKey)
            {
                defaultCard.Visibility = Visibility.Collapsed;
            }
        }

        private void CopyToClipboard(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return;

            Clipboard.Clear();
            Clipboard.SetText(value);
        }

        private string CensorKey(string key)
        {
            string[] splitted = key.Split('-');

            for (int i = 0; i < splitted.Length - 1; i++)
            {
                splitted[i] = "XXXXX";
            }
            return string.Join("-", splitted);
        }

        private void copySpecs_Btn_Click(object sender, RoutedEventArgs e)
        {
            var sb = new StringBuilder();

            sb.AppendLine($"Product Name: {productName.Text}");
            sb.AppendLine($"Version: {version.Text}");
            sb.AppendLine($"Build: {buildVersion.Text}");
            sb.AppendLine($"Product ID: {productID.Text}");
            sb.AppendLine($"Act ID: {actID.Text}");
            //sb.AppendLine($"OEM Edition: {oemEdition.Text}");

            CopyToClipboard(sb.ToString());
        }

        private void showSoft_Btn_Click(object sender, RoutedEventArgs e)
        {
            isCensored_soft = !isCensored_soft;

            softKey.Text = isCensored_soft
                ? softKey_censored
                : WindowsHandler.licenseKey;

            showSoft_Btn.Icon = isCensored_soft ? new SymbolIcon(SymbolRegular.Eye20) : new SymbolIcon(SymbolRegular.EyeOff20);
        }
        private void showOem_Btn_Click(object sender, RoutedEventArgs e)
        {
            isCensored_oem = !isCensored_oem;

            oemKey.Text = isCensored_oem
                ? oemKey_censored
                : WindowsHandler.oemKey;

            showOem_Btn.Icon = isCensored_oem ? new SymbolIcon(SymbolRegular.Eye20) : new SymbolIcon(SymbolRegular.EyeOff20);
        }
        private void showBackup_Btn_Click(object sender, RoutedEventArgs e)
        {
            isCensored_backup = !isCensored_backup;

            backupKey.Text = isCensored_backup
                ? backupKey_censored
                : WindowsHandler.backupKey;

            showBackup_Btn.Icon = isCensored_backup ? new SymbolIcon(SymbolRegular.Eye20) : new SymbolIcon(SymbolRegular.EyeOff20);
        }
        private void showDefault_Btn_Click(object sender, RoutedEventArgs e)
        {
            isCensored_default = !isCensored_default;

            defaultKey.Text = isCensored_default
                ? defaultKey_censored
                : WindowsHandler.defaultKey;

            showDefault_Btn.Icon = isCensored_default ? new SymbolIcon(SymbolRegular.Eye20) : new SymbolIcon(SymbolRegular.EyeOff20);
        }

        private void copySoft_Btn_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(WindowsHandler.licenseKey);
        }
        private void copyOem_Btn_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(WindowsHandler.oemKey);
        }
        private void copyBackup_Btn_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(WindowsHandler.backupKey);
        }
        private void copyDefault_Btn_Click(object sender, RoutedEventArgs e)
        {
            CopyToClipboard(WindowsHandler.defaultKey);
        }
    }

    public static class WindowsHandler
    {
        private static RegistryKey WindowsRK = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion", false);
        private static RegistryKey BKkey = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\SoftwareProtectionPlatform", false);
        private static RegistryKey DefaultPK = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\DefaultProductKey2", false);

        // Generic Key
        /*private static RegistryKey testRK = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion\DefaultProductKey", false);
        public static byte[] _test1 = (byte[])testRK.GetValue("DigitalProductId");*/

        // I guess, in this key, there is the previous key used before changing it to the new one, but I'm not sure lol
        private static byte[] rawDefault_pkid = (byte[])DefaultPK.GetValue("DigitalProductId");

        public static string ProductName = WindowsRK.GetValue("ProductName").ToString();
        public static string DisplayVersion { get; set; }
        public static int Build = Convert.ToInt32(WindowsRK.GetValue("CurrentBuildNumber"));
        public static string UBR { get; set; }
        public static string Version { get; set; }
        public static string Platform = Environment.Is64BitOperatingSystem ? "(64-bit)" : "(32-bit)";
        public static string ProductID = WindowsRK.GetValue("ProductId").ToString();
        public static string pkChannel { get; set; }
        public static string oemKey { get; set; }
        public static string backupKey = BKkey.GetValue("BackupProductKeyDefault").ToString();
        public static string ActID { get; set; }
        public static string ProductKey = Encoding.Unicode.GetString((byte[])WindowsRK.GetValue("DigitalProductId4"), 0x3F8, 0x80);
        private static byte[] pkID = (byte[])WindowsHandler.WindowsRK.GetValue("DigitalProductId");
        public static string licenseKey { get; private set; }
        public static string defaultKey { get; private set; }

        private static float CurrentVersion = float.Parse(WindowsRK.GetValue("CurrentVersion").ToString()) / 10f;
        public static Uri Logo { get; set; }

        public static void Initialize()
        {
            switch (CurrentVersion)
            {
                case 6.1f:
                    Logo = new Uri("pack://application:,,,/Assets/w7.svg");
                    licenseKey = KeyFinder.GetKey_NT61(pkID);
                    defaultKey = KeyFinder.GetKey_NT61(rawDefault_pkid);
                    Version = WindowsRK.GetValue("CSDVersion")?.ToString() ?? string.Empty;
                    break;

                case 6.2f:
                    Logo = new Uri("pack://application:,,,/Assets/w81.svg");
                    licenseKey = KeyFinder.GetKey_NT62(pkID);
                    defaultKey = KeyFinder.GetKey_NT62(rawDefault_pkid);
                    Version = Build.ToString();
                    break;

                case 6.3f:
                    licenseKey = KeyFinder.GetKey_NT62(pkID);
                    defaultKey = KeyFinder.GetKey_NT62(rawDefault_pkid);
                    UBR = WindowsRK.GetValue("UBR")?.ToString() ?? string.Empty;

                    if (ProductName.Contains("8.1"))
                    {
                        Logo = new Uri("pack://application:,,,/Assets/w81.svg");
                        Version = $"{Build}.{UBR}";
                    }
                    else
                    {
                        Logo = new Uri("pack://application:,,,/Assets/w10.svg");

                        if (Build >= 22000)
                        {
                            Logo = new Uri("pack://application:,,,/Assets/w11.svg");
                            ProductName = ProductName.Replace("Windows 10", "Windows 11");
                        }
                        DisplayVersion = WindowsRK.GetValue("DisplayVersion").ToString();
                        Version = $"{Build}.{UBR}";
                    }
                    break;
            }
            WindowsRK.Close();
            BKkey.Close();
            DefaultPK.Close();
            LoadWmiData();
        }

        private static void LoadWmiData()
        {
            var licSearcher = new ManagementObjectSearcher(
                "SELECT ID FROM SoftwareLicensingProduct " +
                "WHERE ApplicationID='55c92734-d682-4d71-983e-d6ec3f16059f' " +
                "AND PartialProductKey IS NOT NULL");

            ActID = licSearcher.Get()
                .Cast<ManagementObject>()
                .FirstOrDefault()?["ID"]?.ToString() ?? "";

            var oemSearcher = new ManagementObjectSearcher(
               "SELECT OA3xOriginalProductKey, OA3xOriginalProductKeyDescription FROM SoftwareLicensingService");

            var obj = oemSearcher.Get().Cast<ManagementObject>().FirstOrDefault();

            oemKey = obj?["OA3xOriginalProductKey"]?.ToString().Trim() ?? "";

            var rawDesc = obj?["OA3xOriginalProductKeyDescription"]?.ToString() ?? "";
            if (rawDesc == "")
            {
                return;
            }
            var parts = rawDesc.Split(' ');
            if (parts.Length >= 3)
                pkChannel = $"{parts[1]} {parts[2]}";
        }
    }

    // Based on https://github.com/guilhermelim/Get-Windows-Product-Key
    public static class KeyFinder
    {
        private const string Digits = "BCDFGHJKMPQRTVWXY2346789";
        private const int keyOffset = 52;
        private const int decodeLength = 29;

        // For NT 6.2+ (Windows 8 and above)
        public static string GetKey_NT62(byte[] pkID)
        {
            string key = string.Empty;
            Span<byte> rawKey = new Span<byte>(pkID, keyOffset, 15);
            rawKey[14] &= 0xf7;

            int last = 0;
            for (int i = 24; i >= 0; i--)
            {
                int current = 0;
                for (int j = 14; j >= 0; j--)
                {
                    current = rawKey[j] | (current << 8);
                    rawKey[j] = (byte)(current / 24);
                    current %= 24;
                    last = current;
                }
                key = Digits[current] + key;
            }

            key = key.Substring(1, last) + "N" + key.Substring(last + 1, key.Length - (last + 1));

            for (int i = 5; i < key.Length; i += 6)
            {
                key = key.Insert(i, "-");
            }
            return key;
        }

        // For NT 6.1 (Windows 7 and lower)
        public static string GetKey_NT61(byte[] pkID)
        {
            char[] decodedChars = new char[decodeLength];
            Span<byte> rawKey = new Span<byte>(pkID, keyOffset, 15);

            for (int i = decodeLength - 1; i >= 0; i--)
            {
                if ((i + 1) % 6 != 0)
                {
                    int current = 0;
                    for (int j = 14; j >= 0; j--)
                    {
                        current = (current << 8) | rawKey[j];
                        rawKey[j] = (byte)(current / 24);
                        current %= 24;
                        decodedChars[i] = Digits[current];
                    }
                }
                else
                {
                    decodedChars[i] = '-';
                }
            }
            return new string(decodedChars);
        }
    }
}
