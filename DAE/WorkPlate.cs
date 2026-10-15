using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DAERun
{

    [Serializable]
    public class WorkPlate : IXmlSerializable
    {

        private decimal xcale = 1.0m;
        private decimal ycale = 1.0m;
        private decimal zcale = 1.0m;
        private decimal pscale = 1.0m;
        private decimal wscale = 1.0m;
        private decimal qscale = 1.0m;
        private decimal mscale = 1.0m; // added by liu 2019-09-19

        public decimal Xcale
        {
            get
            {
                return xcale;
            }

            set
            {
                xcale = value;
            }
        }

        public decimal Ycale
        {
            get
            {
                return ycale;
            }

            set
            {
                ycale = value;
            }
        }

        public decimal Zcale
        {
            get
            {
                return zcale;
            }

            set
            {
                zcale = value;
            }
        }

        public decimal Pcale
        {
            get
            {
                return pscale;
            }

            set
            {
                pscale = value;
            }
        }

        public decimal Wcale
        {
            get { return wscale; }
            set { wscale = value; }
        }
       
        public decimal Qcale
        {
            get { return qscale; }
            set { qscale = value; }
        }

        public decimal Mcale
        {
            get { return mscale; }
            set { mscale = value; }
        }

        // 吸、喷液轴线性关系修正参数（一般线性关系公式是y=kx+b）
        public decimal Pk { get; set; } = 0;
        public decimal Pb { get; set; } = 0;

        public decimal WorkPlateX0 { get; set; } = 0;

        public decimal WorkPlateY0 { get; set; } = 0;

        public decimal WorkPlateZ0 { get; set; } = 0;

        public decimal WorkPlateW0 { get; set; } = 0;


        public void WriteXml(XmlWriter writer)
        {
            var properties = GetType().GetProperties();

            foreach (var propertyInfo in properties)
            {
                if (propertyInfo.IsDefined(typeof(XmlCommentAttribute), false))
                {
                    writer.WriteComment(
                        propertyInfo.GetCustomAttributes(typeof(XmlCommentAttribute), false)
                            .Cast<XmlCommentAttribute>().Single().Value);
                }

                writer.WriteElementString(propertyInfo.Name, propertyInfo.GetValue(this, null).ToString());
            }
        }
        public XmlSchema GetSchema()
        {
            throw new NotImplementedException();
        }



        public void ReadXml(XmlReader reader)
        {
            try
            {
                var properties = GetType().GetProperties();

                foreach (var propertyInfo in properties)
                {
                    reader.MoveToAttribute(propertyInfo.Name);
                    string o = reader.ReadElementString();
                    object oo = o;
                    if (propertyInfo.PropertyType == typeof(int))
                        oo = int.Parse(o);
                    if (propertyInfo.PropertyType == typeof(decimal))
                        oo = decimal.Parse(o);
                    if (propertyInfo.PropertyType == typeof(string))
                        oo = o;

                    propertyInfo.SetValue(this, oo, null);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("读取Scale值错误，请查看workplate.xml文件是否缺少参数");
            }
            
        }
    }


    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class XmlCommentAttribute : Attribute
    {
        public string Value { get; set; }
    }
}
