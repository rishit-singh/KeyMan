namespace KeyMan
{

    /// <summary>
    /// Chained builder for an APIKey object
    /// </summary>
    public class APIKeyBuilder
    {
        protected APIKey KeyInstance;

        public APIKeyBuilder SetUserID(string userID)
        {
            this.KeyInstance.UserID = userID;
            
            return this;
        }

        public APIKeyBuilder AddPermission(string permission, bool allowance)
        {
            this.KeyInstance.Permissions.Add(permission, allowance);
            
            return this;
        }

        public APIKeyBuilder SetKeyValidityTime(KeyValidityTime validityTime)
        {
            this.KeyInstance.ValidityTime = validityTime;

            return this;
        }

        public APIKeyBuilder SetIsLimitless(bool isLimitless)
        {
            this.KeyInstance.IsLimitless = isLimitless;

            return this;
        }

        public APIKey GenerateKey()
        {
            this.KeyInstance.Key = KeyMan.GeneralTools.GetRandomBase64(64);
            
            return this.KeyInstance;
        }
        
        public APIKeyBuilder()
        {
            this.KeyInstance = new APIKey();
        }
    } 
}