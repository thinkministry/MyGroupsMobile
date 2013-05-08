using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.Text;
using System.Configuration;
using System.Globalization;

namespace PlatformSDK
{
    public class DataManager : IDisposable
    {
        private Platform.apiSoapClient _service;
        private PlatformUser _user;
        private string _apiPassword;

        public DataManager(PlatformUser CurrentUser)
        {
            _user = CurrentUser;
            _apiPassword = ConfigurationManager.AppSettings["APIPassword"];
            _service = new Platform.apiSoapClient();
            _service.Open();
        }

        /// <summary>
        ///     Releases internal resources.
        /// </summary>
        public void Dispose()
        {
            if (_service != null)
            {
                _service.Close();
                _service = null;
            }
        }

        public DataSet ExecuteProcedure(string ProcedureName, string Parameters)
        {

            if (_service == null)
            {
                throw new ObjectDisposedException(this.GetType().FullName);
            }
            if (String.IsNullOrEmpty(ProcedureName))
            {
                throw new ArgumentNullException("ProcedureName");
            }

            DataSet data = _service.ExecuteStoredProcedure(_user.DomainGuid, _apiPassword,
                ProcedureName, Parameters ?? String.Empty);

            if (data.Tables.Count == 1 &&
                data.Tables[0].Rows.Count == 1 &&
                data.Tables[0].Columns.Count == 1 &&
                data.Tables[0].Columns[0].Caption == "ErrorMessage")
            {
                string error = data.Tables[0].Rows[0].Field<string>(0);

                if (!String.IsNullOrEmpty(error))
                {
                    string[] parts = error.Split('|');

                    if (parts.Length > 2)
                    {
                        throw new InvalidOperationException(parts[2]);
                    }
                }

                throw new InvalidOperationException("Cannot execute stored procedure.");
            }

            return data;
        }

        public DataSet GetDataRecord(string TableName, int RecordId)
        {
            return ExecuteProcedure("api_SDK_GetDataRecord", String.Format(CultureInfo.InvariantCulture, "TableName={0}&RecordID={1}", TableName, RecordId));
        }


        public int SaveRecord(DataRecord Record)
        {
            int id = int.Parse("0" + Record[Record.PrimaryKeyName].Value.ToString());
            StringBuilder req = new StringBuilder();
            foreach (DataField f in Record.Fields)
            {
                if (f.IsComputed == false && f.FieldName != "Domain_ID" && f.DataType != SqlDbType.UniqueIdentifier)
                {
                    if ((f.IsPrimaryKey == false && f.Value != null) || id > 0)
                    {
                        req.Append(f.FieldName + "=" + Encode(f.Value) + "&");
                    }
                }
            }
            req.Remove(req.Length - 1, 1);

            if (id > 0)
            {
                _service.UpdateRecord(_user.DomainGuid, _apiPassword, _user.UserId, Record.TableName, Record.PrimaryKeyName, req.ToString());
            }
            else
            {
                string result = _service.AddRecord(_user.DomainGuid, _apiPassword, _user.UserId, Record.TableName, Record.PrimaryKeyName, req.ToString());
                string[] results = result.Split('|');

                id = Int32.Parse(results[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
                if (id == 0)
                {
                    throw new InvalidOperationException(results[2]);
                }
            }

            return id;
        }






        /// <summary>
        ///     Replaces forbidden symbols in the provided string.
        /// </summary>
        /// <param name="value">
        ///     String that needs to be encoded.
        /// </param>
        /// <returns>
        ///     String with forbidded symbols been replaced.
        /// </returns>
        private string Encode(object ToEncode)
        {
            if (ToEncode == null)
            {
                return "";
            }
            else
            {
                string toReturn = ToEncode.ToString();
                toReturn = toReturn.Replace("#", "dp_Pound");
                toReturn = toReturn.Replace("&", "dp_Amp");
                toReturn = toReturn.Replace("=", "dp_Equal");
                toReturn = toReturn.Replace("?", "dp_Qmark");
                return toReturn.Trim();
            }
        }

    }
}
