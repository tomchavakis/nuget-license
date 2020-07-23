   /* 
    Licensed under the Apache License, Version 2.0
    
    http://www.apache.org/licenses/LICENSE-2.0
    */
using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Xml;

   namespace NugetUtility
{
	[XmlRoot(ElementName="license")]
	public class License {
		[XmlAttribute(AttributeName="type")]
		public string Type { get; set; }
		[XmlText]
		public string Text { get; set; }
	}

	[XmlRoot(ElementName="repository")]
	public class Repository {
		[XmlAttribute(AttributeName="type")]
		public string Type { get; set; }
		[XmlAttribute(AttributeName="url")]
		public string Url { get; set; }
		[XmlAttribute(AttributeName="commit")]
		public string Commit { get; set; }
	}

	[XmlRoot(ElementName="group")]
	public class Group {
		[XmlAttribute(AttributeName="targetFramework")]
		public string TargetFramework { get; set; }
		[XmlElement(ElementName="dependency")]
		public List<Dependency> Dependency { get; set; }
	}

	[XmlRoot(ElementName="dependency")]
	public class Dependency {
		[XmlAttribute(AttributeName="id")]
		public string Id { get; set; }
		[XmlAttribute(AttributeName="version")]
		public string Version { get; set; }
		[XmlAttribute(AttributeName="exclude")]
		public string Exclude { get; set; }
	}

	[XmlRoot(ElementName="dependencies")]
	public class Dependencies {
		[XmlElement(ElementName="group")]
		public List<Group> Group { get; set; }
	}

	[XmlRoot(ElementName="metadata")]
	public class Metadata {
		[XmlElement(ElementName="id")]
		public string Id { get; set; }
		[XmlElement(ElementName="version")]
		public string Version { get; set; }
		[XmlElement(ElementName="title")]
		public string Title { get; set; }
		[XmlElement(ElementName="authors")]
		public string Authors { get; set; }
		[XmlElement(ElementName="owners")]
		public string Owners { get; set; }
		[XmlElement(ElementName="requireLicenseAcceptance")]
		public string RequireLicenseAcceptance { get; set; }
		[XmlElement(ElementName="license")]
		public License License { get; set; }
		[XmlElement(ElementName="licenseUrl")]
		public string LicenseUrl { get; set; }
		[XmlElement(ElementName="projectUrl")]
		public string ProjectUrl { get; set; }
		[XmlElement(ElementName="iconUrl")]
		public string IconUrl { get; set; }
		[XmlElement(ElementName="description")]
		public string Description { get; set; }
		[XmlElement(ElementName="copyright")]
		public string Copyright { get; set; }
		[XmlElement(ElementName="tags")]
		public string Tags { get; set; }
		[XmlElement(ElementName="repository")]
		public Repository Repository { get; set; }
		[XmlElement(ElementName="dependencies")]
		public Dependencies Dependencies { get; set; }
		[XmlAttribute(AttributeName="minClientVersion")]
		public string MinClientVersion { get; set; }
	}

	[XmlRoot(ElementName="package", Namespace = "")]
	public class Package {
		[XmlElement(ElementName="metadata")]
		public Metadata Metadata { get; set; }

		public override string ToString()
		{
			return $"{Metadata.Id} {Metadata.Version}";
		}
	}
	
	
	// helper class to ignore namespaces when de-serializing
	public class NamespaceIgnorantXmlTextReader : XmlTextReader
	{
		public NamespaceIgnorantXmlTextReader(System.IO.TextReader reader): base(reader) { }

		public override string NamespaceURI
		{
			get { return ""; }
		}
	}

	// helper class to omit XML decl at start of document when serializing
	public class XTWFND  : XmlTextWriter {
		public XTWFND (System.IO.TextWriter w) : base(w) { Formatting= System.Xml.Formatting.Indented;}
		public override void WriteStartDocument () { }
	}
	
	

}
