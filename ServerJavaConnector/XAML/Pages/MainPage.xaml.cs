﻿using ServerJavaConnector.Core.Commander;
using ServerJavaConnector.Core.Connection;
using ServerJavaConnector.XAML.Dialogs;
using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace ServerJavaConnector.XAML.Pages
{
    public partial class MainPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private static MainPage instance;

        public MainPage()
        {
            instance = this;
            InitializeComponent();
            this.ConsoleOutput = ConsoleBoxV;
            this.ConsoleInput = ConsoleInputV;
            this.Conn = MainWindow.instance.Conn;
        }

        private void Send_Button_Click(object sender, RoutedEventArgs e)
        {
            if (!Conn.Connected) { return; }
            var btn = sender as Button;
            btn.IsEnabled = false;
            Dispatcher.Invoke(DispatcherPriority.Background, new Action(() => ExecuteMsg()));
        }

        private void Close_Button_Click(object sender, RoutedEventArgs e)
        {
            CDialogManager.ShowClosingDialog();
        }

        private void Connect_Button_Click(object sender, RoutedEventArgs e)
        {
            PageManager.instance.changePage(FrameType.MainFrame, PageType.LoginPage);
        }

        private void Disconnect_Button_Click(object sender, RoutedEventArgs e)
        {
            Conn.Disconnect();
        }

        private void Button_Press(object sender, KeyEventArgs e)
        {
            Core.Commander.CommandManager commandManager = MainWindow.instance.CommandManager;
            if (e.Key == Key.Up)
            {
                String input = ConsoleInput.Text;
                if (input == null || input == ""
                        || !commandManager.WasExecuted(input))
                {
                    ConsoleInput.Text = commandManager.GetLastCommand();
                }
                else
                {
                    String previousCommand = commandManager.GetPreviousCommand(input);
                    if (previousCommand != "")
                    {
                        ConsoleInput.Text = previousCommand;
                    }
                }
            }
            else if (e.Key == Key.Down)
            {
                String input = ConsoleInput.Text;
                String nextCommand = commandManager.GetNextCommand(input);
                if (nextCommand != ""
                        || input.Equals(commandManager.GetLastCommand()))
                {
                    ConsoleInput.Text = nextCommand;
                }
            }
        }

        public void WriteLine(String msg)
        {
            WriteLine(msg, Brushes.LightGreen);
        }
        public void WriteLine(String msg, Brush color)
        {
            if (color == null)
            {
                color = new SolidColorBrush(Colors.LightGreen);
            }
            TextRange tr = new TextRange(ConsoleBoxV.Document.ContentEnd, ConsoleBoxV.Document.ContentEnd);
            tr.Text = msg;
            tr.ApplyPropertyValue(TextElement.ForegroundProperty, color);
            ConsoleBoxV.AppendText("\n");
        }

        private void ExecuteMsg()
        {
            String input = "";
            try
            {
                input = ConsoleInput.Text;
                if (!input.StartsWith("!"))
                {
                    input = "!say " + input;
                }
                var CM = MainWindow.instance.CommandManager;
                WriteLine(input);
                if (CM.ExecuteCommand(input, Conn))
                {
                    ConsoleInput.Text = "";
                    Command cmd = Command.GetCommand(input);
                    if (cmd != Command.GetCommand(Commands.NONE) && cmd != Command.GetCommand(Commands.ERROR))
                    {
                        int len;
                        if (cmd != Command.GetCommand(Commands.SAY))
                        {
                            len = 0;
                        }
                        else
                        {
                            len = cmd.CommandString.Length;
                        }
                        CM.AddCommandToList(input.Substring(len, input.Length - len));
                        WriteLine("Command executed");
                    }
                    else
                    {
                        WriteLine("Command executed but there was something wrong", Brushes.Yellow);
                    }
                }
                else
                {
                    WriteLine("Command not executed!", Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                CDialogManager.ShowExceptionDialog(ex, "You are disconnected!");
                Conn.Disconnect();
            }
            Send_Button.SetResourceReference(Control.IsEnabledProperty, "Connected");
        }


        protected void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
            Resources["Connected"] = Conn.Connected;
            Resources["NegConnected"] = !Conn.Connected;
        }

        public static void OnPropertySChanged(string p)
        {
            if (instance != null)
            {
                instance.OnPropertyChanged(p);
            }
        }

        public Connection Conn { get; private set; }
        public TextBox ConsoleInput { get; private set; }
        public RichTextBox ConsoleOutput { get; private set; }
    }
}