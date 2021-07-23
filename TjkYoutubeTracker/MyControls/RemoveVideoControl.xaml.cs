using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using TjkYoutubeTracker.LinkUtils;

namespace TjkYoutubeTracker.MyControls
{
    /// <summary>
    /// Логика взаимодействия для RemoveVideoControl.xaml
    /// </summary>
    public partial class RemoveVideoControl : UserControl
    {
        private LinkInfo currentInfo;

        public Action<RemoveVideoControl> OnRemoveClick;

        public RemoveVideoControl()
        {
            InitializeComponent();
        }


        public void SetInfo(LinkInfo info)
        {
            currentInfo = info.Copy();

            tbVideoName.Text = currentInfo.Title;
        }

        private void BtnLink_Click(object sender, RoutedEventArgs e)
        {
           Process.Start(currentInfo.Url);
        }

        private void BtnRemove_Click(object sender, RoutedEventArgs e)
        {
            var window = new Windows.YesNoDialog("remove video?");

            var res = window.ShowDialog();

            if (res == true)
            {
                OnRemoveClick?.Invoke(this);
            }
        }

        public LinkInfo GetCurrentInfo()
        {
            return currentInfo.Copy();
        }

    }
}
