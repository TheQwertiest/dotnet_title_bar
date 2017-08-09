using System.Xml;
using System.Drawing;
using System.Windows.Forms;

namespace fooTitle.Layers
{
	/// <summary>
	/// A layer for organizing other layers, has no content itself.
	/// </summary>
    [LayerTypeAttribute("no-content")]
    public class NoContentLayer : Layer
	{
	    public override bool HasToolTip => false;

	    public NoContentLayer(Rectangle parentRect, XmlNode node) : base(parentRect, node)
		{
        }
	}
}
