using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace NewsExtractor
{
    public static class XLinqExtensions
    {
        public static string ParseAttributeValue(this XElement xElement, string attributeName, string defaultValue)
        {
            if (xElement != null && xElement.Attribute(attributeName) != null)
            {
                return xElement.Attribute(attributeName).Value;
            }
            else
            {
                return defaultValue;
            }
        }

        public static string ParseChildElementValue(this XElement xElement, string childElementName, string defaultValue)
        {
            if (xElement != null && xElement.Element(childElementName) != null)
            {
                return xElement.Element(childElementName).Value;
            }
            else
            {
                return defaultValue;
            }
        }


        public static int ParseAttributeValue(this XElement xElement, string attribute, int defaultValue)
        {
            int retVal = -1;
            if (xElement != null && xElement.Attribute(attribute) != null && Int32.TryParse(xElement.Attribute(attribute).Value, out retVal))
            {
                return retVal;
            }
            else
            {
                return defaultValue;
            }

        }

        public static int ParseChildElementValue(this XElement xElement, string childElementName, int defaultValue)
        {
            int retVal = -1;
            if (xElement != null && xElement.Element(childElementName) != null && Int32.TryParse(xElement.Element(childElementName).Value, out retVal))
            {
                return retVal;
            }
            else
            {
                return defaultValue;
            }

        }

        public static DateTime ParseChildElementValue(this XElement xElement, string childElementName, DateTime defaultValue)
        {
            DateTime retVal = DateTime.MinValue;
            if (xElement != null && xElement.Element(childElementName) != null && DateTime.TryParse(xElement.Element(childElementName).Value, out retVal))
            {
                return retVal;
            }
            else
            {
                return defaultValue;
            }
        }
    }
}
