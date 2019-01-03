using System;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace DEnc.Serialization
{
#pragma warning disable CS1591

    [XmlRoot(ElementName = "ProgramInformation", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class ProgramInformation
    {
        [XmlElement(ElementName = "Title", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public string Title { get; set; }
        [XmlAttribute(AttributeName = "moreInformationURL")]
        public string MoreInformationURL { get; set; }
    }

    [XmlRoot(ElementName = "Representation", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class Representation
    {
        [XmlElement(ElementName = "BaseURL", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public List<string> BaseURL { get; set; }
        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "bandwidth")]
        public int Bandwidth { get; set; }
        [XmlElement(ElementName = "SegmentBase", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public SegmentBase SegmentBase { get; set; }
        [XmlAttribute(AttributeName = "mimeType")]
        public string MimeType { get; set; }
        [XmlAttribute(AttributeName = "codecs")]
        public string Codecs { get; set; }
        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
        [XmlAttribute(AttributeName = "height")]
        public string Height { get; set; }
        [XmlAttribute(AttributeName = "frameRate")]
        public string FrameRate { get; set; }
        [XmlAttribute(AttributeName = "sar")]
        public string Sar { get; set; }
        [XmlAttribute(AttributeName = "startWithSAP")]
        public string StartWithSAP { get; set; }
        [XmlElement(ElementName = "AudioChannelConfiguration", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public AudioChannelConfiguration AudioChannelConfiguration { get; set; }
        [XmlAttribute(AttributeName = "audioSamplingRate")]
        public string AudioSamplingRate { get; set; }
    }

    [XmlRoot(ElementName = "Initialization", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class Initialization
    {
        [XmlAttribute(AttributeName = "range")]
        public string Range { get; set; }
    }

    [XmlRoot(ElementName = "SegmentBase", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class SegmentBase
    {
        [XmlElement(ElementName = "Initialization", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public Initialization Initialization { get; set; }
        [XmlAttribute(AttributeName = "indexRangeExact")]
        public string IndexRangeExact { get; set; }
        [XmlAttribute(AttributeName = "indexRange")]
        public string IndexRange { get; set; }
    }

    [XmlRoot(ElementName = "AdaptationSet", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class AdaptationSet
    {
        [XmlElement(ElementName = "Representation", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public List<Representation> Representation { get; set; }
        [XmlAttribute(AttributeName = "segmentAlignment")]
        public string SegmentAlignment { get; set; }
        [XmlAttribute(AttributeName = "maxWidth")]
        public string MaxWidth { get; set; }
        [XmlAttribute(AttributeName = "maxHeight")]
        public string MaxHeight { get; set; }
        [XmlAttribute(AttributeName = "maxFrameRate")]
        public string MaxFrameRate { get; set; }
        [XmlAttribute(AttributeName = "par")]
        public string Par { get; set; }
        [XmlAttribute(AttributeName = "lang")]
        public string Lang { get; set; }
        [XmlAttribute(AttributeName = "mimeType")]
        public string MimeType { get; set; }
        [XmlAttribute(AttributeName = "contentType")]
        public string ContentType { get; set; }
        [XmlAttribute(AttributeName = "subsegmentAlignment")]
        public string SubsegmentAlignment { get; set; }
        [XmlAttribute(AttributeName = "subsegmentStartsWithSAP")]
        public string SubsegmentStartsWithSAP { get; set; }
        [XmlAttribute(AttributeName = "group")]
        public string Group { get; set; }
    }

    [XmlRoot(ElementName = "AudioChannelConfiguration", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class AudioChannelConfiguration
    {
        [XmlAttribute(AttributeName = "schemeIdUri")]
        public string SchemeIdUri { get; set; }
        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "Period", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class Period
    {
        [XmlElement(ElementName = "AdaptationSet", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public List<AdaptationSet> AdaptationSet { get; set; }
        [XmlAttribute(AttributeName = "duration")]
        public string Duration { get; set; }
    }

    [XmlRoot(ElementName = "MPD", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class MPD
    {
        [XmlElement(ElementName = "ProgramInformation", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public ProgramInformation ProgramInformation { get; set; }
        [XmlElement(ElementName = "Period", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public List<Period> Period { get; set; }
        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
        [XmlAttribute(AttributeName = "minBufferTime")]
        public string MinBufferTime { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }
        [XmlAttribute(AttributeName = "mediaPresentationDuration")]
        public string MediaPresentationDuration { get; set; }
        [XmlAttribute(AttributeName = "maxSegmentDuration")]
        public string MaxSegmentDuration { get; set; }
        [XmlAttribute(AttributeName = "profiles")]
        public string Profiles { get; set; }

        private static XmlSerializer serializer;
        private static XmlSerializer Serializer
        {
            get
            {
                if ((serializer == null))
                {
                    serializer = new XmlSerializerFactory().CreateSerializer(typeof(MPD));
                }
                return serializer;
            }
        }

        #region Serialize/Deserialize
        /// <summary>
        /// Serializes current MPDtype object into an XML string
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            System.IO.StreamReader streamReader = null;
            System.IO.MemoryStream memoryStream = null;
            try
            {
                memoryStream = new System.IO.MemoryStream();
                System.Xml.XmlWriterSettings xmlWriterSettings = new System.Xml.XmlWriterSettings();
                System.Xml.XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
                Serializer.Serialize(xmlWriter, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new System.IO.StreamReader(memoryStream);
                return streamReader.ReadToEnd();
            }
            finally
            {
                if ((streamReader != null))
                {
                    streamReader.Dispose();
                }
                if ((memoryStream != null))
                {
                    memoryStream.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes workflow markup into an MPDtype object
        /// </summary>
        /// <param name="input">string workflow markup to deserialize</param>
        /// <param name="obj">Output MPDtype object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool Deserialize(string input, out MPD obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MPD);
            try
            {
                obj = Deserialize(input);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool Deserialize(string input, out MPD obj)
        {
            System.Exception exception = null;
            return Deserialize(input, out obj, out exception);
        }

        public static MPD Deserialize(string input)
        {
            System.IO.StringReader stringReader = null;
            try
            {
                stringReader = new System.IO.StringReader(input);
                return ((MPD)(Serializer.Deserialize(XmlReader.Create(stringReader))));
            }
            finally
            {
                if ((stringReader != null))
                {
                    stringReader.Dispose();
                }
            }
        }

        public static MPD Deserialize(System.IO.Stream s)
        {
            return ((MPD)(Serializer.Deserialize(s)));
        }
        #endregion

        /// <summary>
        /// Serializes current MPDtype object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        public virtual bool SaveToFile(string fileName, out System.Exception exception)
        {
            exception = null;
            try
            {
                SaveToFile(fileName);
                return true;
            }
            catch (System.Exception e)
            {
                exception = e;
                return false;
            }
        }

        public virtual void SaveToFile(string fileName)
        {
            System.IO.StreamWriter streamWriter = null;
            try
            {
                string xmlString = Serialize();
                System.IO.FileInfo xmlFile = new System.IO.FileInfo(fileName);
                streamWriter = xmlFile.CreateText();
                streamWriter.WriteLine(xmlString);
                streamWriter.Close();
            }
            finally
            {
                if ((streamWriter != null))
                {
                    streamWriter.Dispose();
                }
            }
        }

        /// <summary>
        /// Deserializes xml markup from file into an MPDtype object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output MPDtype object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        public static bool LoadFromFile(string fileName, out MPD obj, out System.Exception exception)
        {
            exception = null;
            obj = default(MPD);
            try
            {
                obj = LoadFromFile(fileName);
                return true;
            }
            catch (System.Exception ex)
            {
                exception = ex;
                return false;
            }
        }

        public static bool LoadFromFile(string fileName, out MPD obj)
        {
            System.Exception exception = null;
            return LoadFromFile(fileName, out obj, out exception);
        }

        public static MPD LoadFromFile(string fileName)
        {
            System.IO.FileStream file = null;
            System.IO.StreamReader sr = null;
            try
            {
                file = new System.IO.FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new System.IO.StreamReader(file);
                string xmlString = sr.ReadToEnd();
                sr.Close();
                file.Close();
                return Deserialize(xmlString);
            }
            finally
            {
                if ((file != null))
                {
                    file.Dispose();
                }
                if ((sr != null))
                {
                    sr.Dispose();
                }
            }
        }
    }

#pragma warning restore CS1591
}
