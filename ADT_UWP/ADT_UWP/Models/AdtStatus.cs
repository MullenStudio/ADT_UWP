//-----------------------------------------------------------------------
// <copyright file="AdtStatus.cs" company="Mullen Studio">
//     Copyright (c) Mullen Studio. All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace MullenStudio.ADT_UWP.Models
{
    using System;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using Windows.UI.Xaml.Media.Imaging;

    /// <summary>
    /// The ADT status.
    /// </summary>
    public sealed class AdtStatus
    {
        /// <summary>
        /// Gets the summary icon.
        /// </summary>
        public BitmapImage SummaryIcon { get; private set; }
        
        /// <summary>
        /// Gets the current arm status.
        /// </summary>
        public string CurrentArm { get; private set; }
        
        /// <summary>
        /// Gets the current mode.
        /// </summary>
        public string CurrentMode { get; private set; }

        /// <summary>
        /// Gets the available arm options.
        /// </summary>
        public ObservableCollection<ArmOption> ArmOptions { get; } = new ObservableCollection<ArmOption>();

        /// <summary>
        /// Gets the available mode options.
        /// </summary>
        public ObservableCollection<ModeOption> ModeOptions { get; } = new ObservableCollection<ModeOption>();

        /// <summary>
        /// Gets the log.
        /// </summary>
        public ObservableCollection<string> Log { get; } = new ObservableCollection<string>();

        /// <summary>
        /// Initializes the ADT status.
        /// </summary>
        /// <param name="userName">The user name.</param>
        /// <param name="password">The password.</param>
        /// <returns>The task.</returns>
        public async Task<bool> Initialize(string userName, string password)
        {
            if (await AdtApi.Current.SignIn(userName, password))
            {
                return await this.Refresh();
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Refreshes the ADT status.
        /// </summary>
        /// <returns>The task.</returns>
        public async Task<bool> Refresh()
        {
            var summary = await AdtApi.Current.GetSummary();
            if (summary != null)
            {
                this.SummaryIcon = null;
                try
                {
                    this.SummaryIcon = new BitmapImage(new Uri(summary.IconUrl));
                }
                catch (Exception)
                {
                    return false;
                }

                this.CurrentArm = summary.Arm;
                this.CurrentMode = summary.Mode;
            }
            else
            {
                return false;
            }

            var armOptions = (await AdtApi.Current.ListArmOptions())?.ToList();
            if (armOptions != null)
            {
                this.ArmOptions.Clear();
                armOptions.ForEach(armOption => this.ArmOptions.Add(new ArmOption { Value = armOption.Key, DisplayValue = armOption.Value }));
            }
            else
            {
                return false;
            }

            var modeOptions = (await AdtApi.Current.ListModes())?.ToList();
            if (modeOptions != null)
            {
                this.ModeOptions.Clear();
                modeOptions.ForEach(modeOption => this.ModeOptions.Add(new ModeOption { Value = modeOption.Key, DisplayValue = modeOption.Value }));
            }
            else
            {
                return false;
            }

            var log = (await AdtApi.Current.GetLog())?.ToList();
            if (log != null)
            {
                this.Log.Clear();
                log.ForEach(record => this.Log.Add(record));
            }
            else
            {
                return false;
            }

            return true;
        }
    }
}
