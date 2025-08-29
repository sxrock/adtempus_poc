namespace BlazorApp1.Model
{
    public class ParameterConfiguration
    {
        public string Name { get; set; }
        public string Region { get; set; }
        public bool IsActive { get; set; }
        public Dictionary<string, string> AdfParameters {get; set;}

        public ParameterConfiguration()
        {
                AdfParameters = new Dictionary<string, string>();
        }

        public void AddAdfParameter(string key, string value)
        {
            AdfParameters.TryAdd(key, value);
        }

        public string GetAdfParameter(string key) 
        {  
            return AdfParameters.TryGetValue(key, out string value) ? value : null;
        }
    }
}
