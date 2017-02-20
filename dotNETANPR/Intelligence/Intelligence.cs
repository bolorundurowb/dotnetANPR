using System.IO;

namespace dotNETANPR.Intelligence
{
    public class Intelligence
    {
        public static Configurator.Configurator Configurator { get; set; } = new Configurator.Configurator(
            "." +
            Path.DirectorySeparatorChar +
            "config.xml"
        );
    }
}
