using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace DEnc.Serialization
{
#pragma warning disable CS1591

    [XmlRoot(ElementName = "AdaptationSet", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class AdaptationSet
    {
        [XmlAttribute(AttributeName = "contentType")]
        public string ContentType { get; set; }

        [XmlAttribute(AttributeName = "group")]
        public string Group { get; set; }

        [XmlAttribute(AttributeName = "lang")]
        public string Lang { get; set; }

        [XmlAttribute(AttributeName = "maxFrameRate")]
        public string MaxFrameRate { get; set; }

        [XmlAttribute(AttributeName = "maxHeight")]
        public string MaxHeight { get; set; }

        [XmlAttribute(AttributeName = "maxWidth")]
        public string MaxWidth { get; set; }

        [XmlAttribute(AttributeName = "mimeType")]
        public string MimeType { get; set; }

        [XmlAttribute(AttributeName = "par")]
        public string Par { get; set; }

        [XmlElement(ElementName = "Representation", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public List<Representation> Representation { get; set; }

        [XmlElement(ElementName = "Role", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public DescriptorType Role { get; set; }

        [XmlAttribute(AttributeName = "segmentAlignment")]
        public string SegmentAlignment { get; set; }
        [XmlElement(ElementName = "SegmentTemplate", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public SegmentTemplate SegmentTemplate { get; set; }

        [XmlAttribute(AttributeName = "startWithSAP")]
        public string StartWithSAP { get; set; }

        [XmlAttribute(AttributeName = "subsegmentAlignment")]
        public string SubsegmentAlignment { get; set; }

        [XmlAttribute(AttributeName = "subsegmentStartsWithSAP")]
        public string SubsegmentStartsWithSAP { get; set; }
    }

    [XmlRoot(ElementName = "AudioChannelConfiguration", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class DescriptorType
    {
        [XmlAttribute(AttributeName = "schemeIdUri")]
        public string SchemeIdUri { get; set; }

        [XmlAttribute(AttributeName = "value")]
        public string Value { get; set; }
    }

    [XmlRoot(ElementName = "Initialization", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class Initialization
    {
        [XmlAttribute(AttributeName = "range")]
        public string Range { get; set; }
    }

    [XmlRoot(ElementName = "MPD", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class MPD
    {
        private static XmlSerializer serializer;

        [XmlAttribute(AttributeName = "maxSegmentDuration")]
        public string MaxSegmentDuration { get; set; }

        [XmlAttribute(AttributeName = "mediaPresentationDuration")]
        public string MediaPresentationDuration { get; set; }

        [XmlAttribute(AttributeName = "minBufferTime")]
        public string MinBufferTime { get; set; }

        [XmlElement(ElementName = "Period", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public List<Period> Period { get; set; }

        [XmlAttribute(AttributeName = "profiles")]
        public string Profiles { get; set; }

        [XmlElement(ElementName = "ProgramInformation", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public ProgramInformation ProgramInformation { get; set; }
        [XmlAttribute(AttributeName = "type")]
        public string Type { get; set; }

        [XmlAttribute(AttributeName = "xmlns")]
        public string Xmlns { get; set; }
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
        /// Deserializes workflow markup into an MPDtype object
        /// </summary>
        /// <param name="input">string workflow markup to deserialize</param>
        /// <param name="obj">Output MPDtype object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TryX pattern, exception returned to caller")]
        public static bool TryDeserialize(string input, out MPD obj, out System.Exception exception)
        {
            exception = null;
            obj = default;
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

        public static bool TryDeserialize(string input, out MPD obj)
        {
            return TryDeserialize(input, out obj, out _);
        }

        public static MPD Deserialize(string input)
        {
            StringReader stringReader = null;
            try
            {
                stringReader = new StringReader(input);
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

        public static MPD Deserialize(Stream s)
        {
            return ((MPD)(Serializer.Deserialize(s)));
        }

        /// <summary>
        /// Serializes current MPDtype object into an XML string
        /// </summary>
        /// <returns>string XML value</returns>
        public virtual string Serialize()
        {
            StreamReader streamReader = null;
            MemoryStream memoryStream = null;
            try
            {
                memoryStream = new MemoryStream();
                XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
                XmlWriter xmlWriter = XmlWriter.Create(memoryStream, xmlWriterSettings);
                Serializer.Serialize(xmlWriter, this);
                memoryStream.Seek(0, SeekOrigin.Begin);
                streamReader = new StreamReader(memoryStream);
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
        #endregion Serialize/Deserialize

        /// <summary>
        /// Deserializes xml markup from file into an MPDtype object
        /// </summary>
        /// <param name="fileName">string xml file to load and deserialize</param>
        /// <param name="obj">Output MPDtype object</param>
        /// <param name="exception">output Exception value if deserialize failed</param>
        /// <returns>true if this Serializer can deserialize the object; otherwise, false</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TryX pattern, exception returned to caller")]
        public static bool TryLoadFromFile(string fileName, out MPD obj, out System.Exception exception)
        {
            exception = null;
            obj = default;
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

        public static bool TryLoadFromFile(string fileName, out MPD obj)
        {
            return TryLoadFromFile(fileName, out obj, out _);
        }

        public static MPD LoadFromFile(string fileName)
        {
            FileStream file = null;
            StreamReader sr = null;
            try
            {
                file = new FileStream(fileName, FileMode.Open, FileAccess.Read);
                sr = new StreamReader(file);
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


        /// <summary>
        /// Serializes current MPDtype object into file
        /// </summary>
        /// <param name="fileName">full path of outupt xml file</param>
        /// <param name="exception">output Exception value if failed</param>
        /// <returns>true if can serialize and save into file; otherwise, false</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "TryX pattern, exception returned to caller")]
        public virtual bool TrySaveToFile(string fileName, out System.Exception exception)
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
            StreamWriter streamWriter = null;
            try
            {
                string xmlString = Serialize();
                FileInfo xmlFile = new FileInfo(fileName);
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
    }

    [XmlRoot(ElementName = "Period", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class Period
    {
        [XmlElement(ElementName = "AdaptationSet", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public List<AdaptationSet> AdaptationSet { get; set; }

        [XmlAttribute(AttributeName = "duration")]
        public string Duration { get; set; }
    }

    [XmlRoot(ElementName = "ProgramInformation", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class ProgramInformation
    {
        [XmlAttribute(AttributeName = "moreInformationURL")]
        public string MoreInformationURL { get; set; }

        [XmlElement(ElementName = "Title", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public string Title { get; set; }
    }

    [XmlRoot(ElementName = "Representation", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class Representation
    {
        [XmlElement(ElementName = "AudioChannelConfiguration", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public DescriptorType AudioChannelConfiguration { get; set; }

        [XmlAttribute(AttributeName = "audioSamplingRate")]
        public string AudioSamplingRate { get; set; }

        [XmlAttribute(AttributeName = "bandwidth")]
        public int Bandwidth { get; set; }

        [XmlElement(ElementName = "BaseURL", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public List<string> BaseURL { get; set; }

        [XmlAttribute(AttributeName = "codecs")]
        public string Codecs { get; set; }

        [XmlAttribute(AttributeName = "frameRate")]
        public string FrameRate { get; set; }

        [XmlAttribute(AttributeName = "height")]
        public string Height { get; set; }

        [XmlAttribute(AttributeName = "id")]
        public string Id { get; set; }
        [XmlAttribute(AttributeName = "mimeType")]
        public string MimeType { get; set; }

        [XmlAttribute(AttributeName = "sar")]
        public string Sar { get; set; }

        [XmlElement(ElementName = "SegmentBase", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public SegmentBase SegmentBase { get; set; }
        [XmlElement(ElementName = "SegmentTemplate", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public SegmentTemplate SegmentTemplate { get; set; }

        [XmlAttribute(AttributeName = "startWithSAP")]
        public string StartWithSAP { get; set; }

        [XmlAttribute(AttributeName = "width")]
        public string Width { get; set; }
    }
    [XmlRoot(ElementName = "SegmentBase", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class SegmentBase
    {
        [XmlAttribute(AttributeName = "indexRange")]
        public string IndexRange { get; set; }

        [XmlAttribute(AttributeName = "indexRangeExact")]
        public string IndexRangeExact { get; set; }

        [XmlElement(ElementName = "Initialization", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
        public Initialization Initialization { get; set; }
    }

    [XmlRoot(ElementName = "AdaptationSet", Namespace = "urn:mpeg:dash:schema:mpd:2011")]
    public class SegmentTemplate
    {
        [XmlAttribute(AttributeName = "duration")]
        public string Duration { get; set; }

        [XmlAttribute(AttributeName = "initialization")]
        public string Initialization { get; set; }

        [XmlAttribute(AttributeName = "media")]
        public string Media { get; set; }
        [XmlAttribute(AttributeName = "startNumber")]
        public string StartNumber { get; set; }

        [XmlAttribute(AttributeName = "timescale")]
        public string Timescale { get; set; }
    }
#pragma warning restore CS1591
}