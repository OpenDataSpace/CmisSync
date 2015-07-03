using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CmisSync {
    /// <summary>
    /// Interaction logic for BandwidthLimitWidget.xaml
    /// </summary>
    public partial class BandwidthLimitWidget : UserControl {
        public BandwidthLimitWidget() {
            InitializeComponent();
        }

        public long Limit {
            get {
                if (this.isLimitedCheckbox.IsChecked == false) {
                    return 0;
                }

                long limit;
                if (long.TryParse(this.limitTextBox.Text, out limit)) {
                    return limit > 0 ? limit * 1024 : 0;
                } else {
                    return 0;
                }
            }

            set {
                this.isLimitedCheckbox.IsChecked = value > 0;
                this.limitTextBox.IsEnabled = this.isLimitedCheckbox.IsChecked == true;
                this.limitTextBox.Text = value > 0 ? (value/1024).ToString() : "100";
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e) {
            e.Handled = !this.IsPositivNumeric(e.Text);
        }

        private bool IsPositivNumeric(string text) {
            long result;
            if (long.TryParse(text, out result)) {
                return result > -1;
            } else {
                return false;
            }
        }

        private void PastingHandler(object sender, DataObjectPastingEventArgs e) {
            if (e.DataObject.GetDataPresent(typeof(String))) {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!this.IsPositivNumeric(text)) {
                    e.CancelCommand();
                }
            } else {
                e.CancelCommand();
            }
        }

        private void isLimitedCheckbox_Click(object sender, RoutedEventArgs e) {
            this.limitTextBox.IsEnabled = this.isLimitedCheckbox.IsChecked == true;
        }
    }
}