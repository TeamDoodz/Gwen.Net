using System.IO;
using System.Text;
using static Gwen.Net.Platform.GwenPlatform;

namespace Gwen.Net.Xml;

/// <summary>
/// Implement this in a class that can be used as a source for XML parser.
/// </summary>
public interface IXmlSource {
	Stream GetStream();
}

/// <summary>
/// XML source as a string.
/// </summary>
public class XmlStringSource : IXmlSource {
	public XmlStringSource(string xml) : this(xml, Encoding.UTF8) {}

	public XmlStringSource(string xml, Encoding encoding) {
		this.xml = xml;
		this.encoding = encoding;
	}

	public Stream GetStream() {
		Stream stream = new MemoryStream();
		StreamWriter writer = new StreamWriter(stream, encoding);
		writer.Write(xml);
		writer.Flush();
		stream.Position = 0;
		return stream;
	}

	private string xml;
	private Encoding encoding;
}

/// <summary>
/// XML source as a file.
/// </summary>
public class XmlFileSource : IXmlSource {
	public XmlFileSource(string fileName) {
		this.fileName = fileName;
	}

	public Stream GetStream() {
		return GetFileStream(fileName, false);
	}

	private string fileName;
}