using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PlatformSDK
{
    public class DataField
    {
        public int Order { get; set; }
        public string FieldName { get; set; }
        public string Label { get; set; }
        public string Description { get; set; }
        public bool IsPrimaryKey { get; set; }
        public SqlDbType DataType { get; set; }
        public int DataSize { get; set; }
        public bool IsRequired { get; set; }
        public bool IsComputed { get; set; }
        public string ForeignTable { get; set; }
        public string ForeignFieldName { get; set; }
        public string ForeignExpression { get; set; }

        public object Value { get; set; }
    }
}
