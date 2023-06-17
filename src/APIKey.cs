using System;
using System.Collections;
using System.Collections.Generic;

using OpenDatabase;

namespace KeyMan
{
    /// <summary>
    /// Stores an API key with related methods.
    /// </summary>
    public class APIKey : IRecord
    {
        public string Key;

        public string UserID;

        public KeyValidityTime ValidityTime;

        public Dictionary<string, bool> Permissions;

        public bool IsLimitless;

        public bool IsExpired { get { return this.GetIsExpired(); } set { } }

        public Record ToRecord()
        {
            Record record = new Record();

            try
            {
                record = new Record(new string[] {
                        "Key",
                        "UserID",
                        "Permissions",
                        "CreationTime",
                        "ExpiryTime",
                        "IsLimitless"
                    }, new object[] {
                        this.Key,
                        this.UserID,
                        KeyTools.GetPermissionsString(this.Permissions),
                        this.ValidityTime.CreationTime.ToString(),
                        (!this.IsLimitless && this.ValidityTime.ExpiryTime != null) ? this.ValidityTime.ExpiryTime.ToString() : null,
                        this.IsLimitless
                    });
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            return record;
        }

        protected bool GetIsExpired()
        {   
            if (this.IsLimitless)
                return false;

            return this.ValidityTime.IsExpired;
        }
            
        public bool IsValid()
        {
            return (
                this.Key != null &&
                this.UserID != null &&
                this.ValidityTime.IsExpired != true
            );
        }

        public bool HasPermission(string permission)
        {
            if (this.Permissions.ContainsKey(permission))
                return this.Permissions[permission];

            return false;
        }

        public APIKey()
        {
            this.Key = null;
            this.UserID = null;
            this.IsLimitless = false;
            
            this.Permissions = new Dictionary<string, bool>();
        }

        public APIKey(string key, string userID, bool isLimitless = true)
        {
            this.Key = key;
            this.UserID = userID;
            this.IsLimitless = isLimitless;
        }
       
        public APIKey(string key, string userID, Dictionary<string, bool> permissions, bool isLimitless = true)
        {
            this.Key = key;
            this.UserID = userID;
            this.IsLimitless = isLimitless;
       
            this.Permissions = permissions;

            this.ValidityTime = new KeyValidityTime(DateTime.Now, new DateTime());
        }
        
        public APIKey(string key, string userID, KeyValidityTime validityTime)
        {
            this.Key = key;
            this.UserID = userID;
            this.ValidityTime = validityTime;
            this.IsLimitless = false;
        }
       
        public APIKey(string key, string userID, Dictionary<string, bool> permissions, KeyValidityTime validityTime)
        {
            this.Key = key;
            this.UserID = userID;
            this.Permissions = permissions;
            this.ValidityTime = validityTime;
        }

        public APIKey(string key, string userID, DateTime creationTime, TimeDifference validityTime)
        {
            this.Key = key;
            this.UserID = userID;
            this.ValidityTime = new KeyValidityTime(creationTime.ToUniversalTime(), validityTime);
            this.IsLimitless = false;
        }

        public APIKey(string key, string userID, Dictionary<string, bool> permissions, DateTime creationTime, TimeDifference validityTime)
        {
            this.Key = key;
            this.UserID = userID;
            this.ValidityTime = new KeyValidityTime(creationTime.ToUniversalTime(), validityTime);
            this.IsLimitless = false;
            this.Permissions = permissions;
        }

        public APIKey(Record record)
        {
            this.Key = (string)record.Values[0];
            this.UserID = (string)record.Values[1];
            this.Permissions = KeyTools.GetPermissionsMap((string)record.Values[2]);
            this.ValidityTime = new KeyValidityTime(DateTime.Parse((string)record.Values[3]), (record.Values[4] != null) ? DateTime.Parse((string)record.Values[4]) : new DateTime());
            this.IsLimitless = (bool)record.Values[5];
        }
    }
}