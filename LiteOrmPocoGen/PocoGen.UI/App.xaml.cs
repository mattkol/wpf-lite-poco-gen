// -----------------------------------------------------------------------
// <copyright file="App.xaml.cs" company="Poco Generator for Lite ORMs">
// Copyright (c) Lite Poco Generator. All rights reserved. 
// </copyright>
// -----------------------------------------------------------------------

namespace PocoGen.UI
{
    using System;
    using System.Windows;
    using System.Windows.Threading;
    using MahApps.Metro.Controls;
    using MahApps.Metro.Controls.Dialogs;

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            string message = "An error as occured!";
            Exception exception = new Exception(message);

            if (e.Exception != null)
            {
                exception = e.Exception.GetBaseException();
                if (!string.IsNullOrEmpty(exception.Message))
                {
                    message = exception.Message;
                }
          
            }

            var shelView = (MetroWindow)MainWindow;
            shelView.ShowMessageAsync("Application Error", message);
            e.Handled = true;

            var logger = new Log4NetLogger(typeof(App));
            logger.Error(message, exception);
        }

    }
}
