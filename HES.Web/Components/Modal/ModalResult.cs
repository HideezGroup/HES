namespace HES.Web.Components
{
    public class ModalResult
    {
        public bool Succeeded { get; protected set; }
        public bool Cancelled { get; private set; }

        public static ModalResult Success => new ModalResult { Succeeded = true };
        public static ModalResult Cancel => new ModalResult { Cancelled = true };
    }
}