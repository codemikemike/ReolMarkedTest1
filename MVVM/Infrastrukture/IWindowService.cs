namespace ReolMarked.MVVM.Infrastructure
{
    public interface IWindowService
    {
        void ShowBarcodeWindow();
        void ShowScannerWindow();
        void ShowInvoiceWindow();
        void ShowTerminationWindow();
    }
}