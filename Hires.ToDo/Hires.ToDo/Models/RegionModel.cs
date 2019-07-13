using System.Collections.Generic;

namespace Hires.ToDo.Models
{
    public static class RegionModel
    {
        public static readonly List<KeyValuePair<string, string>> Regions = new List<KeyValuePair<string, string>>
        {
            new KeyValuePair<string, string>("westus", "West US"),
            new KeyValuePair<string, string>("eastus", "East US"),
            new KeyValuePair<string, string>("centralus", "Central US"),
            new KeyValuePair<string, string>("northeurope", "North Europe"),
            new KeyValuePair<string, string>("westeurope", "West Europe"),
            new KeyValuePair<string, string>("uksouth", "UK South"),
            new KeyValuePair<string, string>("francecentral", "France Central")
        };
    }
}
