using ReolMarked.MVVM.Views;

namespace ReolMarked.MVVM.Infrastructure
{
    public class WindowService : IWindowService
    {
        public void ShowBarcodeWindow()
        {
            var window = new BarcodeWindow();
            window.ShowDialog();
        }

        public void ShowScannerWindow()
        {
            var window = new ScannerWindow();
            window.ShowDialog();
        }

        public void ShowInvoiceWindow()
        {
            var window = new InvoiceWindow();
            window.ShowDialog();
        }

        public void ShowTerminationWindow()
        {
            var window = new TerminationWindow();
            window.ShowDialog();
        }
    }
}