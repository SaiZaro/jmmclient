﻿using System;
using System.Globalization;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using JMMClient.Forms;
using JMMClient.ViewModel;

namespace JMMClient.UserControls
{
    /// <summary>
    /// Interaction logic for MultipleFilesDetailControl.xaml
    /// </summary>
    public partial class MultipleFilesDetailControl : UserControl
    {
        public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register("IsExpanded",
            typeof(bool), typeof(MultipleFilesDetailControl), new UIPropertyMetadata(false, isExpandedCallback));

        public static readonly DependencyProperty IsCollapsedProperty = DependencyProperty.Register("IsCollapsed",
            typeof(bool), typeof(MultipleFilesDetailControl), new UIPropertyMetadata(true, isCollapsedCallback));

        public bool IsExpanded
        {
            get { return (bool)GetValue(IsExpandedProperty); }
            set { SetValue(IsExpandedProperty, value); }
        }

        public bool IsCollapsed
        {
            get { return (bool)GetValue(IsCollapsedProperty); }
            set { SetValue(IsCollapsedProperty, value); }
        }

        private static void isExpandedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //EpisodeDetail input = (EpisodeDetail)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        private static void isCollapsedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //EpisodeDetail input = (EpisodeDetail)d;
            //input.tbTest.Text = e.NewValue as string;
        }

        public MultipleFilesDetailControl()
        {
            InitializeComponent();

            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo(AppSettings.Culture);

            btnToggleExpander.Click += new RoutedEventHandler(btnToggleExpander_Click);
        }

        private void CommandBinding_DeleteFile(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VideoDetailedVM))
                {
                    VideoDetailedVM vid = obj as VideoDetailedVM;


                    AskDeleteFile dlg = new AskDeleteFile(string.Format(Properties.Resources.DeleteFile_Title, vid.FileName), Properties.Resources.MultipleFiles_ConfirmDelete + "\r\n\r\n" + Properties.Resources.DeleteFile_Confirm, vid.Places);
                    dlg.Owner = Window.GetWindow(this);
                    bool? res = dlg.ShowDialog();
                    if (res.HasValue && res.Value)
                    {
                        string tresult = string.Empty;
                        this.Cursor = Cursors.Wait;
                        foreach (VideoLocal_PlaceVM lv in dlg.Selected)
                        {
                            string result =
                                JMMServerVM.Instance.clientBinaryHTTP.DeleteVideoLocalPlaceAndFile(
                                    lv.VideoLocal_Place_ID);
                            if (result.Length > 0)
                                tresult += result + "\r\n";
                        }
                        if (!string.IsNullOrEmpty(tresult))
                            MessageBox.Show(tresult, Properties.Resources.Error, MessageBoxButton.OK,
                                MessageBoxImage.Error);
                        AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
                        if (ep != null)
                        {
                            MainListHelperVM.Instance.UpdateHeirarchy(ep);
                            ep.LocalFileCount--;
                        }
                        DisplayFiles();
                    }
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        private void CommandBinding_ToggleVariation(object sender, ExecutedRoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);

            object obj = e.Parameter;
            if (obj == null) return;

            try
            {
                if (obj.GetType() == typeof(VideoDetailedVM))
                {
                    VideoDetailedVM vid = obj as VideoDetailedVM;

                    vid.VideoLocal_IsVariation = vid.Variation ? 0 : 1;

                    string result = JMMServerVM.Instance.clientBinaryHTTP.SetVariationStatusOnFile(vid.VideoLocalID, vid.Variation);
                    if (result.Length > 0)
                        MessageBox.Show(result, Properties.Resources.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
            finally
            {
                this.Cursor = Cursors.Arrow;
            }
        }

        public void DisplayFiles()
        {
            try
            {
                AnimeEpisodeVM ep = this.DataContext as AnimeEpisodeVM;
                if (ep != null)
                {
                    ep.RefreshFilesForEpisode();
                    ep.RefreshAnime();
                    fileSummary.DataContext = null;
                    fileSummary.DataContext = ep.AniDB_Anime;

                    lbFiles.ItemsSource = ep.FilesForEpisode;

                }
            }
            catch (Exception ex)
            {
                Utils.ShowErrorMessage(ex);
            }
        }

        void btnToggleExpander_Click(object sender, RoutedEventArgs e)
        {
            IsExpanded = !IsExpanded;
            IsCollapsed = !IsCollapsed;

            if (IsExpanded)
            {
                DisplayFiles();
            }
        }
    }
}
