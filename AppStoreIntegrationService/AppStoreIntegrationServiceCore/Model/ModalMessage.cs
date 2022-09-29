namespace AppStoreIntegrationServiceCore.Model
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
		public string Title { get; set; }
		public ModalType ModalType { get; set; }	
		public string RequestPage { get; set; }
	}
}
