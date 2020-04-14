using System;
using System.Collections.Generic;
using System.Text;

namespace SQLGenerator.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Class)]
    public class Table : System.Attribute
    {
        public string name { get; set; }
        public Table(string name)
        {
            this.name = name;
        }
    }
}
