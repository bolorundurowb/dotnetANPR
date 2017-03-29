namespace dotNETANPR.Gui
{
    public class ReportGenerator
    {
        private string output;
        private bool enabled;

        public ReportGenerator()
        {
            enabled = false;
        }

        public void InsertText(string text)
        {
            if (!enabled)
            {
                return;
            }
            output += text;
            output += "\n";
        }
    }
}