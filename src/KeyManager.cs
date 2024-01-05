using System.ComponentModel.Design;
using KeyMan.Models;
using Microsoft.EntityFrameworkCore;
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
            Task<bool> KeyExistsAsync(APIKey key, bool dbCheck = false);
            
            APIKey? GetAPIKey(string key);

            Task<APIKey> GetAPIKeyAsync(string key);

            void BackupKeys();

            Task BackupKeysAsync(); 

            void LoadKeys();

            int GetAPIKeyCount();

            APIKey IssueAPIKey(string userID, Dictionary<string, bool> permissionsMap, bool backUp = true);
            
            Task<APIKey> IssueAPIKeyAsync(string userID, Dictionary<string, bool> permissionsMap, bool backUp = true);

            APIKey IssueAPIKey(string userID, Dictionary<string, bool> permissionsMap, KeyValidityTime validityTime,
                bool backUp = true);
            
            Task<APIKey> IssueAPIKeyAsync(string userID, Dictionary<string, bool> permissionsMap, KeyValidityTime validityTime,
                bool backUp = true);
            
            bool RevokeAPIKey(string key);
            
            bool IsValid(string key, string[] permissions = null);
            
            bool AddAPIKey(APIKey key);

            Task<bool> AddAPIKeyAsync(APIKey key);
            
            List<APIKey> List(); 
        }

        /// <summary>
        /// Routines for creating and managing API keys.
        /// </summary>
        public class APIKeyManager : IKeyManager, IDisposable
        {
            private readonly APIKeyDBContext _dbContext;
            
            private Dictionary<string, APIKey> APIKeyMap;

            public string APIKeyTable { get; private set; }  // Default API key table

            public static readonly TimeDifference DefaultKeyTimeDifference = new TimeDifference();
    
            public bool AutoBackup;

            public List<APIKey> List()
            {
                return this._dbContext.ApiKeys.Select(model => APIKey.FromModel(model)).ToList(); 
            }
            
            public bool KeyExists(APIKey key, bool dbCheck = true)
            {
                bool exists;
                
                exists = this.APIKeyMap.ContainsKey(key.Key);

                if (dbCheck) 
                    exists = (this.GetAPIKey(key.Key) != null);

                return exists;
            }
    
            public bool KeyExists(string key, bool dbCheck = true)
            {
                bool exists;
                
                exists = this.APIKeyMap.ContainsKey(key);

                if (dbCheck) 
                    exists = (this.GetAPIKey(key) != null);

                return exists;
            }
            
            public async Task<bool> KeyExistsAsync(APIKey key, bool dbCheck = true)
            {
                if (!dbCheck)
                    return this.APIKeyMap.ContainsKey(key.Key);
                    
                return (await this.GetAPIKeyAsync(key.Key).ConfigureAwait(false) != null);
            }
    
            public async Task<bool> KeyExistsAsync(string key, bool dbCheck = true)
            {
                if (!dbCheck)
                    return this.APIKeyMap.ContainsKey(key);

                return (await this.GetAPIKeyAsync(key).ConfigureAwait(false) != null);
            }

            public APIKey? GetAPIKey(string key)
            {
                return APIKey.FromModel(this._dbContext.ApiKeys.FirstOrDefault(apiKey => apiKey.Key == key)); 
            }

            public async Task<APIKey> GetAPIKeyAsync(string key)
            {
                return APIKey.FromModel(await this._dbContext.ApiKeys.FirstOrDefaultAsync(apiKey => apiKey.Key == key).ConfigureAwait(false)); 
            }

            public void BackupKeys()
            {
                Dictionary<string, APIKey>.ValueCollection keys = this.APIKeyMap.Values;

                APIKey fetchedKey;

                foreach (APIKey key in keys)
                {
                    if ((fetchedKey = this.GetAPIKey(key.Key)) != null)
                    {
                        fetchedKey.Key = key.Key;
                        fetchedKey.Permissions = key.Permissions;
                        fetchedKey.IsExpired = key.IsExpired;
                        fetchedKey.IsLimitless = key.IsExpired;
                        fetchedKey.ValidityTime = key.ValidityTime;
                    }
                    else
                        this._dbContext.ApiKeys.Add(key.ToModel());
                    
                    this._dbContext.SaveChanges();
                }
            }

            public async Task BackupKeysAsync()
            {
                Dictionary<string, APIKey>.ValueCollection keys = this.APIKeyMap.Values;

                APIKey fetchedKey;

                foreach (APIKey key in keys)
                {
                    if ((fetchedKey = await this.GetAPIKeyAsync(key.Key).ConfigureAwait(false)) != null)
                    {
                        fetchedKey.Key = key.Key;
                        fetchedKey.Permissions = key.Permissions;
                        fetchedKey.IsExpired = key.IsExpired;
                        fetchedKey.IsLimitless = key.IsExpired;
                        fetchedKey.ValidityTime = key.ValidityTime;
                    }
                    else
                        await this._dbContext.ApiKeys.AddAsync(key.ToModel());
                    
                    this._dbContext.SaveChangesAsync();
                }
            }
            
            public void LoadKeys()
            {
                List<APIKey> keys = this._dbContext.ApiKeys.Select(model => APIKey.FromModel(model)).ToList();
                
                foreach (APIKey key in keys)
                    this.APIKeyMap.Add(key.Key, key);
            }

            public int GetAPIKeyCount()
            {
                return this._dbContext.ApiKeys.Count();
            }
           
            public APIKey IssueAPIKey(string userID, Dictionary<string, bool> permissionsMap, bool backUp = true)
            {
                APIKey key = new APIKey(GeneralTools.GetRandomBase64(64), userID, permissionsMap, true);
                
                this.APIKeyMap.Add(key.Key, key);

                if (backUp)
                    this._dbContext.ApiKeys.Add(key.ToModel());

                return key;
            }

            public async Task<APIKey> IssueAPIKeyAsync(string userID, Dictionary<string, bool> permissionsMap, bool backUp = true)
            {
                APIKey key = new APIKey(GeneralTools.GetRandomBase64(64), userID, permissionsMap, true);
                
                this.APIKeyMap.Add(key.Key, key);

                if (backUp)
                    await this._dbContext.ApiKeys.AddAsync(key.ToModel()).ConfigureAwait(false);

                return key;
            }

            public APIKey IssueAPIKey(string userID, Dictionary<string, bool> permissionsMap, KeyValidityTime validityTime, bool backUp = true)
            {
                APIKey key = new APIKey(GeneralTools.GetRandomBase64(64), userID, permissionsMap, validityTime);
                
                this.APIKeyMap.Add(key.Key, key);

                if (backUp)
                    this._dbContext.ApiKeys.Add(key.ToModel());

                return key;
            }

            public async Task<APIKey> IssueAPIKeyAsync(string userID, Dictionary<string, bool> permissionsMap, KeyValidityTime validityTime, bool backUp = true)
            {
                APIKey key = new APIKey(GeneralTools.GetRandomBase64(64), userID, permissionsMap, validityTime);
                
                this.APIKeyMap.Add(key.Key, key);

                if (backUp)
                    await this._dbContext.ApiKeys.AddAsync(key.ToModel()).ConfigureAwait(false);

                return key;
            }

            public bool RevokeAPIKey(string key)
            {
                if (this.KeyExists(key, true))
                {
                    this._dbContext.ApiKeys.Remove(this.GetAPIKey(key).ToModel());

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
                this._dbContext.ApiKeys.Add(key.ToModel()); 
                
                this.APIKeyMap.Add(key.Key, key);
                this._dbContext.SaveChanges();
                
                return true;
            }

            public async Task<bool> AddAPIKeyAsync(APIKey key)
            {
                await this._dbContext.ApiKeys.AddAsync(key.ToModel()).ConfigureAwait(false);

                this.APIKeyMap.Add(key.Key, key);
                await this._dbContext.SaveChangesAsync();
                
                return true;
            }

            public APIKeyManager(DbContext dbContext, bool load = true, bool autoBackup = false)
            {
                this._dbContext = dbContext as APIKeyDBContext;
                
                this.AutoBackup = autoBackup;
                
                this.APIKeyMap = new Dictionary<string, APIKey>();

                this._dbContext.Database.EnsureCreated();
                
                if (load) 
                    this.LoadKeys();
            }

            public void Dispose()
            {
                if (this.AutoBackup)
                    this.BackupKeys();
            }
        }
    }
    