using System;
using System.Data;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using NHibernate;
using NHibernate.Engine;
using NHibernate.SqlTypes;
using NHibernate.Type;

namespace Microservice.Workflow
{
    public class XmlType<T> : MutableType
    {
        private readonly Type serializableClass;
        private readonly XmlDocType xmlDocType;

        public override Type ReturnedClass
        {
            get
            {
                return serializableClass;
            }
        }

        public override string Name
        {
            get
            {
                return serializableClass == typeof(ISerializable) ? "xmlserializable" : serializableClass.FullName;
            }
        }

        public XmlType()
            : this(typeof(T))
        {
        }

        public XmlType(Type serializableClass)
            : base(new XmlSqlType())
        {
            this.serializableClass = serializableClass;
            xmlDocType = (XmlDocType)NHibernateUtil.XmlDoc;
        }

        public XmlType(Type serializableClass, XmlSqlType sqlType)
            : base(sqlType)
        {
            this.serializableClass = serializableClass;
            xmlDocType = (XmlDocType)TypeFactory.GetSerializableType(sqlType.Length);
        }

        public override void Set(IDbCommand st, object value, int index)
        {
            xmlDocType.Set(st, ToXml(value), index);
        }


        public override object Get(IDataReader rs, string name)
        {
            return Get(rs, rs.GetOrdinal(name));
        }

        public override object Get(IDataReader rs, int index)
        {
            var xmlDoc = xmlDocType.Get(rs, index) as XmlDocument;

            return xmlDoc != null
                ? FromXml(xmlDoc)
                : null;
        }

        public override bool IsEqual(object x, object y)
        {
            if (x == y)
                return true;
            if (x == null || y == null)
                return false;

            return x.Equals(y) || xmlDocType.IsEqual(ToXml(x), ToXml(y));
        }

        public override int GetHashCode(object x, EntityMode entityMode)
        {
            return xmlDocType.GetHashCode(ToXml(x), entityMode);
        }

        public override string ToString(object value)
        {
            return xmlDocType.ToString(ToXml(value));
        }

        public override object FromStringValue(string xml)
        {
            return xmlDocType.FromStringValue(xml);
        }

        public override object DeepCopyNotNull(object value)
        {
            return FromXml(ToXml(value));
        }

        private static XmlDocument ToXml(object value)
        {
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            var ser = new XmlSerializer(typeof(T));

            using (var memoryStream = new MemoryStream())
            {
                ser.Serialize(memoryStream, value, namespaces);

                memoryStream.Position = 0;
                var xmlDoc = new XmlDocument();
                xmlDoc.Load(memoryStream);

                return xmlDoc;
            }
        }

        public object FromXml(XmlDocument xmlDoc)
        {
            var ser = new XmlSerializer(typeof(T));

            using (var textReader = new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(xmlDoc.OuterXml))))
            {
                return ser.Deserialize(textReader);
            }
        }

        public override object Assemble(object cached, ISessionImplementor session, object owner)
        {
            if (cached == null) return null;

            var xmlDoc = new XmlDocument();
            xmlDoc.LoadXml(cached.ToString());

            return FromXml(xmlDoc);
        }

        public override object Disassemble(object value, ISessionImplementor session, object owner)
        {
            return value == null ? null : ToXml(value);
        }
    }
}