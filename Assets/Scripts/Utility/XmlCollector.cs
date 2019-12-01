using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace Utility
{
    public class XmlCollector<T>
    {
        public XmlCollector()
        {
        }

        public List<T> Collect(
            XElement from,
            string elementName,
            Func<XElement, T> selector)
        {
            return from.Elements()
                .Where(element => element.Name.ToString() == elementName)
                .ToList()
                .Select(selector)
                .Where(element => element != null)
                .ToList();
        }
    }
}