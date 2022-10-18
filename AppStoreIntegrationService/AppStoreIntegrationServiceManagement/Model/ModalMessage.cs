namespace AppStoreIntegrationServiceManagement.Model
{
    public enum ModalType
    {
        WarningMessage,
        ConfirmationMessage,
        SuccessMessage
    }

    public class ModalMessage
    {
        public string Message { get; set; }
        public string Title { get; set; } = "Confirmation";
        public ModalType ModalType { get; set; } = ModalType.ConfirmationMessage;
        public string RequestPage { get; set; }
    }
}
