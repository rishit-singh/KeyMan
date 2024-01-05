using Microsoft.Extensions.Logging.Abstractions;
using OpenDatabase;
using OpenDatabaseAPI;
using Newtonsoft.Json;

namespace KeyMan
{
		public interface IKeyManager
		{
			bool KeyExists(APIKey key, bool dbCheck = true);
		
            bool KeyExists(string key, bool dbCheck = true);

            APIKey GetAPIKey(string key);

            void BackupKeys();

            void LoadKeys();

            int GetAPIKeyCount();

            APIKey IssueAPIKey(string userID, Dictionary<string, bool> permissionsMap, bool backUp = true);

            APIKey IssueAPIKey(string userID, Dictionary<string, bool> permissionsMap, KeyValidityTime validityTime,
                bool backUp = true);

            bool RevokeAPIKey(string key);

            bool IsValid(string key, string[] permissions = null);

            bool AddAPIKey(APIKey key);
        }

        /// <summary>
        /// Routines for creating and managing API keys.
        /// </summary>
        public class APIKeyManager : IKeyManager
        {
            public PostGRESDatabase KeyDB; // Database instance where the keys will be stored.

            public Dictionary<string, APIKey> APIKeyMap;

            public string APIKeyTable; // Default API key table

            public static readonly TimeDifference DefaultKeyTimeDifference = new TimeDifference();

            public bool AutoBackup;
           
            public static Table GetTableSchema(string tableName)
            {
                return new Table(tableName,
                    new Field[] {
                         new Field("Key", FieldType.Char, new Flag[] {  Flag.PrimaryKey, Flag.NotNull }, 88),
                         new Field("UserID", FieldType.Char, new Flag[] {Flag.NotNull }, 64),
                         new Field("Permissions", FieldType.VarChar, new Flag[]{}, 1024),
                         new Field("CreationTime", FieldType.VarChar, new Flag[] { Flag.NotNull}, 22),
                         new Field("ExpiryTime", FieldType.VarChar, new Flag[]{}, 22),
                         new Field("IsLimitless", FieldType.Bool, new Flag[] { Flag.NotNull })
                    }); 
            }
            
            public bool KeyExists(APIKey key, bool dbCheck = false)
            {
                if (!dbCheck)
                    return this.APIKeyMap.ContainsKey(key.Key);

                return (this.KeyDB.FetchQueryData($"SELECT * FROM {this.APIKeyTable} WHERE Key=\'{key.Key}\'", this.APIKeyTable).Length != 0);
            }
    
            public bool KeyExists(string key, bool dbCheck = true)
            {
                if (!dbCheck)
                    return this.APIKeyMap.ContainsKey(key);

                return (this.KeyDB.FetchQueryData($"SELECT * FROM {this.APIKeyTable} WHERE Key=\'{key}\'", this.APIKeyTable).Length != 0);
            }

            public APIKey GetAPIKey(string key)
            {
                Record[] records = this.KeyDB.FetchQueryData($"SELECT * FROM {this.APIKeyTable} WHERE Key=\'{key}\'", this.APIKeyTable);

                if (records.Length == 0)
                    return null;
                
                return new APIKey(records[0]);
            }

            public void BackupKeys()
            {
                Dictionary<string, APIKey>.ValueCollection keys = this.APIKeyMap.Values;

                foreach (APIKey key in keys)
                    if (this.KeyExists(key, true))
                        this.KeyDB.UpdateRecord(new Record(new string[] { "Key" }, new object[] { key.Key }),
                            key.ToRecord(), this.APIKeyTable);
                    else
                        this.KeyDB.InsertRecord(key.ToRecord(), this.APIKeyTable);
            }
            
            public void LoadKeys()
            {
                Record[] keyRecords = this.KeyDB.FetchQueryData($"SELECT * FROM {this.APIKeyTable};", this.APIKeyTable);

                APIKey temp;
                
                for (int x = 0; x < keyRecords.Length; x++)
                    this.APIKeyMap.Add((temp = new APIKey(keyRecords[x])).Key, temp);
            }

            public int GetAPIKeyCount()
            {
                return this.APIKeyMap.Count;
            }
           
            public APIKey IssueAPIKey(string userID, Dictionary<string, bool> permissionsMap, bool backUp = true)
            {
                APIKey key = new APIKey(GeneralTools.GetRandomBase64(64), userID, permissionsMap, true);
                
                this.APIKeyMap.Add(key.Key, key);

                if (backUp)
                {
                    Console.WriteLine(JsonConvert.SerializeObject(key.ToRecord()));
                 
                    this.KeyDB.InsertRecord(key.ToRecord(), this.APIKeyTable);
                }

                return key;
            }

            public APIKey IssueAPIKey(string userID, Dictionary<string, bool> permissionsMap, KeyValidityTime validityTime, bool backUp = true)
            {
                APIKey key = new APIKey(GeneralTools.GetRandomBase64(64), userID, permissionsMap, validityTime);
                
                this.APIKeyMap.Add(key.Key, key);

                if (backUp)
                    this.KeyDB.InsertRecord(key.ToRecord(), this.APIKeyTable);

                return key;
            }

            public bool RevokeAPIKey(string key)
            {
                if (this.KeyExists(key, true))
                {
                    this.KeyDB.ExecuteQuery($"DELETE FROM {this.APIKeyTable} WHERE Key=\'{key}\'");
                    
                    return true;
                }

                return false;
            }

            public bool IsValid(string key, string[] permissions = null)
            {
                APIKey apiKey;

                if ((apiKey = this.GetAPIKey(key)) == null)
                    return false;
                
                for (int x = 0; x < permissions.Length; x++)
                    if (!apiKey.HasPermission(permissions[x]))
                        return false;
                
                return true;
            } 

            public bool AddAPIKey(APIKey key)
            {
                if (!this.KeyDB.InsertRecord(key.ToRecord(), this.APIKeyTable))
                    return false;
                
                this.APIKeyMap.Add(key.Key, key);
                
                return true;
            }

            public APIKeyManager(PostGRESDatabase dbInstance, string tableName = "APIKeys", bool load = false, bool autoBackup = false)
            {
                this.KeyDB = dbInstance;
                this.AutoBackup = autoBackup;
                this.APIKeyTable = tableName;
                
                this.APIKeyMap = new Dictionary<string, APIKey>();

                this.KeyDB.Connect();            
                
                if (!this.KeyDB.TableExists(this.APIKeyTable))
                    this.KeyDB.ExecuteQuery(APIKeyManager.GetTableSchema("APIKeys").GetCreateQuery());
                
                if (load) 
                    this.LoadKeys();
            }

            ~APIKeyManager()
            {
                if (this.AutoBackup)
                    this.BackupKeys();
            }
        }
    }
    