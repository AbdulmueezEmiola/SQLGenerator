using System;
using System.Collections.Generic;
using System.Text;

namespace SQLGenerator.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property | System.AttributeTargets.Method)]
    public class Column : System.Attribute
    {
        public bool NotNull { get; set; }
        public bool primaryKey { get; set; }
        public bool unique { get; set; }
        public string name { get; set; }
        public Column(string name)
        {
            this.name = name;
        }
    }
}
