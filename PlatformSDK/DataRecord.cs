using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace PlatformSDK
{
    public class DataRecord
    {
        //Constructors
        public DataRecord(string TableName)
        {
            this.TableName = TableName;
            DataSet ds = new DataSet();
            using (DataManager dm = new DataManager(PlatformUser.Current))
            {
                ds = dm.GetDataRecord(TableName, 0);
            }
            if (ds.Tables.Count == 2)
            {
                PopulateRecord(ds);
            }
            else
            {
                throw new Exception("Unable to retrieve schema for '" + TableName + "'.  Please check spelling.");
            }
        }

        public DataRecord(string TableName, int RecordId)
        {
            this.TableName = TableName;
            DataSet ds = new DataSet();
            using (DataManager dm = new DataManager(PlatformUser.Current))
            {
                ds = dm.GetDataRecord(TableName, RecordId);
            }
            if (ds.Tables.Count == 2)
            {
                PopulateRecord(ds);
            }
            else
            {
                throw new Exception("Unable to retrieve schema for '" + TableName + "'.  Please check spelling.");
            }
        }


        //Public properties
        public string TableName { get; set; }

        public string PrimaryKeyName { get; set; }

        public List<DataField> Fields { get; set; }

        public DataField this[string FieldName]
        {
            get
            {
                return this.Fields.Find(f => f.FieldName == FieldName);
            }
        }

        //Public methods

        public void Save()
        {
            using (DataManager dm = new DataManager(PlatformUser.Current))
            {
                this[this.PrimaryKeyName].Value = dm.SaveRecord(this);
            }
        }

        public List<DataError> Validate()
        {
            List<DataError> Errors = new List<DataError>();
            foreach (DataField f in this.Fields)
            {
                if (f.IsRequired == true && f.Value == null && f.FieldName != "Domain_ID" && f.IsComputed == false && f.IsPrimaryKey == false)
                {
                    Errors.Add(new DataError { Field = f, ErrorType = DataError.ValidationErrorType.RequiredField });
                }
                if (f.Value != null && IsString(f.DataType) == true && f.Value.ToString().Length > f.DataSize)
                {
                    Errors.Add(new DataError { Field = f, ErrorType = DataError.ValidationErrorType.InvalidSize });
                }
                if (f.Value != null && IsValid(f.DataType, f.Value) == false)
                {
                    Errors.Add(new DataError { Field = f, ErrorType = DataError.ValidationErrorType.InvalidFormat });
                }
            }


            return Errors;
        }

        //Private helper methods

        private void PopulateRecord(DataSet ds)
        {
            this.Fields = new List<DataField>();
            for (int f = 0; f < ds.Tables[0].Rows.Count; f++)
            {
                string CurrentFieldName = ds.Tables[0].Rows[f]["Field_Name"].ToString();

                this.Fields.Add(new DataField
                {
                    Order = int.Parse(ds.Tables[0].Rows[f]["Field_Order"].ToString()),
                    FieldName = CurrentFieldName,
                    Label = ds.Tables[0].Rows[f]["Field_Label"].ToString(),
                    Description = ds.Tables[0].Rows[f]["Field_Description"].ToString(),
                    IsPrimaryKey = bool.Parse(ds.Tables[0].Rows[f]["Is_Primary_Key"].ToString()),
                    DataType = ParseDataType(ds.Tables[0].Rows[f]["Data_Type"].ToString()),
                    DataSize = int.Parse(ds.Tables[0].Rows[f]["Data_Size"].ToString()),
                    IsRequired = bool.Parse(ds.Tables[0].Rows[f]["Is_Required"].ToString()),
                    IsComputed = bool.Parse(ds.Tables[0].Rows[f]["Is_Computed"].ToString()),
                    ForeignTable = ds.Tables[0].Rows[f]["Foreign_Table_Name"].ToString(),
                    ForeignExpression = ds.Tables[0].Rows[f]["Foreign_Expression"].ToString(),
                    ForeignFieldName = ds.Tables[0].Rows[f]["Foreign_Field_Name"].ToString()
                });
                if (bool.Parse(ds.Tables[0].Rows[f]["Is_Primary_Key"].ToString()) == true)
                {
                    this.PrimaryKeyName = CurrentFieldName;
                }
                if (ds.Tables[1].Rows.Count > 0)
                {
                    this[CurrentFieldName].Value = ds.Tables[1].Rows[0][CurrentFieldName];
                }
            }
            if (this[this.PrimaryKeyName].Value == null)
            {
                this[this.PrimaryKeyName].Value = 0;
            }
        }

        private SqlDbType ParseDataType(string DataType)
        {
            switch (DataType.ToLower())
            {
                case "image":
                    return SqlDbType.Image;
                case "text":
                    return SqlDbType.Text;
                case "uniqueidentifier":
                    return SqlDbType.UniqueIdentifier;
                case "date":
                    return SqlDbType.Date;
                case "time":
                    return SqlDbType.Time;
                case "datetime2":
                    return SqlDbType.DateTime2;
                case "datetimeoffset":
                    return SqlDbType.DateTimeOffset;
                case "tinyint":
                    return SqlDbType.TinyInt;
                case "smallint":
                    return SqlDbType.SmallInt;
                case "int":
                    return SqlDbType.Int;
                case "smalldatetime":
                    return SqlDbType.SmallDateTime;
                case "real":
                    return SqlDbType.Real;
                case "money":
                    return SqlDbType.Money;
                case "datetime":
                    return SqlDbType.DateTime;
                case "float":
                    return SqlDbType.Float;
                case "ntext":
                    return SqlDbType.NText;
                case "bit":
                    return SqlDbType.Bit;
                case "decimal":
                case "numeric":
                    return SqlDbType.Decimal;
                case "smallmoney":
                    return SqlDbType.SmallMoney;
                case "bigint":
                    return SqlDbType.BigInt;
                case "varbinary":
                    return SqlDbType.VarBinary;
                case "varchar":
                    return SqlDbType.VarChar;
                case "binary":
                    return SqlDbType.Binary;
                case "char":
                    return SqlDbType.Char;
                case "timestamp":
                    return SqlDbType.Timestamp;
                case "nvarchar":
                    return SqlDbType.NVarChar;
                case "nchar":
                    return SqlDbType.NChar;
                case "xml":
                    return SqlDbType.Xml;
                default:
                    return SqlDbType.Variant;
            }
        }


        private bool IsString(SqlDbType Type)
        {
            switch (Type)
            {
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NText:
                case SqlDbType.NVarChar:
                    return true;
                default:
                    return false;
            }

        }

        private bool IsValid(SqlDbType Type, object Value)
        {
            try
            {
                switch (Type)
                {
                    case SqlDbType.Date:
                    case SqlDbType.DateTime:
                    case SqlDbType.DateTime2:
                    case SqlDbType.Time:
                    case SqlDbType.SmallDateTime:
                        DateTime.Parse(Value.ToString());
                        break;
                    case SqlDbType.SmallInt:
                        Int16.Parse(Value.ToString());
                        break;
                    case SqlDbType.Int:
                        Int32.Parse(Value.ToString());
                        break;
                    case SqlDbType.BigInt:
                        Int64.Parse(Value.ToString());
                        break;
                    case SqlDbType.Money:
                    case SqlDbType.SmallMoney:
                    case SqlDbType.Decimal:
                    case SqlDbType.Float:
                    case SqlDbType.Real:
                        decimal.Parse(Value.ToString());
                        break;
                }
                return true;
            }
            catch
            {
                return false;
            }

        }
    }
}
