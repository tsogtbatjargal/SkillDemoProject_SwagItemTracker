using Newtonsoft.Json;

namespace caa_mis.Utilities
{
    public static class FilteredDataCaching
    {
        /// <summary>  
        /// set the cookie  
        /// </summary>  
        /// <param name="_context">the HttpContext</param>  
        /// <param name="key">key (unique indentifier)</param>  
        /// <param name="filteredData">value to store in cookie object</param>  
        /// <param name="expireTime">expiration time</param>  
        public static void SaveFilteredData<T>(HttpContext _context, string key, IQueryable<T> filteredData, int? expireTime)
        {
            var filteredDataJson = JsonConvert.SerializeObject(filteredData);
            CookieHelper.CookieSet(_context, key, filteredDataJson, expireTime);
        }
    }
}
