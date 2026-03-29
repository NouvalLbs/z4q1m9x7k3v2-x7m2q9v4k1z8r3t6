namespace ProjectSMP.Plugins.AndroidDialogBridge
{
    public class AndroidDialogData
    {
        public int PlayerId { get; set; }
        public int DialogId { get; set; }
        public int Style { get; set; }
        public string Caption { get; set; } = "";
        public string Info { get; set; } = "";
        public string Button1 { get; set; } = "";
        public string Button2 { get; set; } = "";
    }
}