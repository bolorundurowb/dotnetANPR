using System.Drawing;
using DotNetANPR.Configuration;

namespace DotNetANPR.ImageAnalysis;

public class Band : Photo
{
    private static readonly ProbabilityDistributor distributor = new ProbabilityDistributor(0, 0, 25, 25);
    private static readonly int numberOfCandidates =
        Configurator.Instance.Get<int>("intelligence_numberOfPlates");
    private BandGraph graphHandle = null;
    
    public Band(Bitmap image) : base(image) { }
}
