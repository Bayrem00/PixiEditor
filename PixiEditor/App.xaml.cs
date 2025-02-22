﻿using PixiEditor.Models.DataHolders;
using PixiEditor.Models.Dialogs;
using PixiEditor.Models.Enums;
using PixiEditor.ViewModels;
using PixiEditor.Views.Dialogs;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using PixiEditor.Models.Controllers;
using PixiEditor.Models.UserPreferences;

namespace PixiEditor
{
    /// <summary>
    ///     Interaction logic for App.xaml.
    /// </summary>
    public partial class App : Application
    {
        /// <summary>The event mutex name.</summary>
        private const string UniqueEventName = "33f1410b-2ad7-412a-a468-34fe0a85747c";

        /// <summary>The unique mutex name.</summary>
        private const string UniqueMutexName = "ab2afe27-b9ee-4f03-a1e4-c18da16a349c";

        /// <summary>The event wait handle.</summary>
        private EventWaitHandle _eventWaitHandle;

        private string passedArgsFile = Path.Join(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PixiEditor", ".passedArgs");

        /// <summary>The mutex.</summary>
        private Mutex _mutex;

        protected override void OnStartup(StartupEventArgs e)
        {
            if (HandleNewInstance())
            {
                StartupArgs.Args = e.Args.ToList();
                string arguments = string.Join(' ', e.Args);

                if (ParseArgument("--crash (\"?)([A-z0-9:\\/\\ -_.]+)\\1", arguments, out Group[] groups))
                {
                    CrashReport report = CrashReport.Parse(groups[2].Value);
                    MainWindow = new CrashReportDialog(report);
                }
                else
                {
                    MainWindow = new MainWindow();
                }

                MainWindow.Show();
            }
        }

        private bool HandleNewInstance()
        {
            bool isOwned;
            _mutex = new Mutex(true, UniqueMutexName, out isOwned);
            _eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset, UniqueEventName);

            GC.KeepAlive(_mutex);

            if (isOwned)
            {
                var thread = new Thread(
                    () =>
                    {
                        while (_eventWaitHandle.WaitOne())
                        {
                            Current.Dispatcher.BeginInvoke(
                                (Action)(() =>
                                {
                                    MainWindow mainWindow = ((MainWindow)Current.MainWindow);
                                    if (mainWindow != null)
                                    {
                                        mainWindow.BringToForeground();
                                        StartupArgs.Args = File.ReadAllText(passedArgsFile).Split(' ').ToList();
                                        File.Delete(passedArgsFile);
                                        StartupArgs.Args.Add("--openedInExisting");
                                        mainWindow.DataContext.OnStartupCommand.Execute(null);
                                    }
                                }));
                        }
                    })
                {
                    // It is important mark it as background otherwise it will prevent app from exiting.
                    IsBackground = true
                };

                thread.Start();
                return true;
            }

            // Notify other instance so it could bring itself to foreground.
            File.WriteAllText(passedArgsFile, string.Join(' ', Environment.GetCommandLineArgs()));
            _eventWaitHandle.Set();

            // Terminate this instance.
            Shutdown();
            return false;
        }

        protected override void OnSessionEnding(SessionEndingCancelEventArgs e)
        {
            base.OnSessionEnding(e);

            if (ViewModelMain.Current.BitmapManager.Documents.Any(x => !x.ChangesSaved))
            {
                ConfirmationType confirmation = ConfirmationDialog.Show($"{e.ReasonSessionEnding} with unsaved data. Are you sure?", $"{e.ReasonSessionEnding}");
                e.Cancel = confirmation != ConfirmationType.Yes;
            }
        }

        private bool ParseArgument(string pattern, string args, out Group[] groups)
        {
            Match match = Regex.Match(args, pattern, RegexOptions.IgnoreCase);
            groups = null;

            if (match.Success)
            {
                groups = match.Groups.Values.ToArray();
            }

            return match.Success;
        }
    }
}
