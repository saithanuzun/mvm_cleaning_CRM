using mvmclean.backend.Domain.Aggregates.SeoPage;

namespace mvmclean.backend.Infrastructure.Seeding;

/// <summary>
/// Helper to generate SEO page configurations for seeding
/// Creates 4 levels: CityOnly, CityService, CityArea, CityAreaService
/// </summary>
public static class SeoPageSeedData
{
    public static Dictionary<string, CityConfig> GetCitiesWithAreas()
    {
        return new Dictionary<string, CityConfig>
        {
            {
                "Leicester", new CityConfig
                {
                    City = "Leicester",
                    Areas = new[]
                    {
                        "City Centre", "Wigston", "Oadby", "Glen Parva", "Knighton", "Belgrave",
                        "Syston", "Birstall", "Groby", "Kirby Muxloe", "Anstey", "Countesthorpe",
                        "Braunstone", "Evington", "Humberstone", "Thurmaston", "Beaumont Leys",
                        "Hamilton", "Leicester Forest East", "Leicester Forest West", "New Parks",
                        "Stoneygate", "Westcotes", "Aylestone", "South Knighton", "Rowley Fields"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Loughborough", new CityConfig
                {
                    City = "Loughborough",
                    Areas = new[]
                    {
                        "Town Centre", "Quorn", "Shepshed", "Sileby", "Barrow upon Soar",
                        "Mountsorrel", "Woodhouse", "Woodthorpe", "Hathern", "Thorpe Acre",
                        "Dishley", "Garendon", "Nanpantan", "Outwoods"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Hinckley", new CityConfig
                {
                    City = "Hinckley",
                    Areas = new[]
                    {
                        "Town Centre", "Earl Shilton", "Barwell", "Desford", "Kirby Muxloe",
                        "Burbage", "Stoke Golding", "Market Bosworth", "Sutton Cheney",
                        "Dadlington", "Newbold Verdon", "Markfield", "Ratby", "Stapleton"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Melton Mowbray", new CityConfig
                {
                    City = "Melton Mowbray",
                    Areas = new[]
                    {
                        "Town Centre", "Asfordby", "Bottesford", "Wymondham", "Long Clawson",
                        "Stathern", "Redmile", "Gaddesby", "Somerby", "Oakham",
                        "Uppingham", "Langham", "Brooksby", "Kirby Bellars"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Market Harborough", new CityConfig
                {
                    City = "Market Harborough",
                    Areas = new[]
                    {
                        "Town Centre", "Lutterworth", "Broughton Astley", "Kibworth Harcourt",
                        "Kibworth Beauchamp", "Great Bowden", "East Langton", "Foxton",
                        "Smeeton Westerby", "Theddingworth", "Mowsley", "Arnesby", "Fleckney"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Coalville", new CityConfig
                {
                    City = "Coalville",
                    Areas = new[]
                    {
                        "Town Centre", "Ibstock", "Heather", "Whitwick", "Thringstone",
                        "Hugglescote", "Donington le Heath", "Swannington", "Coleorton",
                        "Ashby de la Zouch", "Measham", "Moira", "Appleby Magna", "Snarestone"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Lutterworth", new CityConfig
                {
                    City = "Lutterworth",
                    Areas = new[]
                    {
                        "Town Centre", "Bitteswell", "Bittesby", "Swinnerton", "Ullesthorpe",
                        "Leire", "Dunton Bassett", "Catthorpe", "Claybrooke", "Walcote"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Syston", new CityConfig
                {
                    City = "Syston",
                    Areas = new[]
                    {
                        "Town Centre", "Barkby", "Barkby Thorpe", "Thurmaston", "Rearsby",
                        "Queniborough", "Ratcliffe on the Wreake", "East Goscote"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Oakham", new CityConfig
                {
                    City = "Oakham",
                    Areas = new[]
                    {
                        "Town Centre", "Rutland Water", "Uppingham", "Langham", "Ashwell",
                        "Braunston", "Brooke", "Burley", "Cottesmore", "Egleton", "Empingham",
                        "Exton", "Glaston", "Hambleton", "Ketton", "Manton", "Normanton",
                        "North Luffenham", "South Luffenham", "Tickencote", "Whissendine"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Shepshed", new CityConfig
                {
                    City = "Shepshed",
                    Areas = new[]
                    {
                        "Town Centre", "Hathern", "Long Whatton", "Diseworth", "Kegworth",
                        "Belton", "Garendon Park", "Charnwood Edge"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Ashby de la Zouch", new CityConfig
                {
                    City = "Ashby de la Zouch",
                    Areas = new[]
                    {
                        "Town Centre", "Blackfordby", "Boundary", "Shellbrook", "Stanton under Bardon",
                        "Top Brand", "Willesley", "Smisby", "Staunton Harold", "Calke"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Wigston", new CityConfig
                {
                    City = "Wigston",
                    Areas = new[]
                    {
                        "Town Centre", "South Wigston", "Little Wigston", "Wigston Magna",
                        "Glen Parva", "Blaby", "Narborough", "Enderby"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Oadby", new CityConfig
                {
                    City = "Oadby",
                    Areas = new[]
                    {
                        "Town Centre", "Oadby Grange", "Glen Rise", "Brooke Heath", "Uplands",
                        "Woodland Grange", "Stoughton", "Stoughton Drive"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Birstall", new CityConfig
                {
                    City = "Birstall",
                    Areas = new[]
                    {
                        "Town Centre", "Wanlip", "Thurmaston", "Syston", "Rothley",
                        "Cossington", "Barkby", "Barkby Thorpe"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Broughton Astley", new CityConfig
                {
                    City = "Broughton Astley",
                    Areas = new[]
                    {
                        "Town Centre", "Sutton in the Elms", "Leire", "Frolesworth",
                        "Potters Marston", "Shawell", "Swepstone"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Rothley", new CityConfig
                {
                    City = "Rothley",
                    Areas = new[]
                    {
                        "Village Centre", "Mountsorrel", "Swithland", "Woodhouse",
                        "Cropston", "Newtown Linford", "Bradgate Park", "Swithland Wood"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            // Keep your existing non-Leicestershire entries below (Coventry, Rugby, London etc.)
            {
                "Coventry", new CityConfig
                {
                    City = "Coventry",
                    Areas = new[] { "City Centre", "Canley", "Earlsdon", "Keresley", "Holbrooks", "Whitley" },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Rugby", new CityConfig
                {
                    City = "Rugby",
                    Areas = new[] { "Town Centre", "Hillmorton", "Lawford", "Brownsover" },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Castle Donington", new CityConfig
                {
                    City = "Castle Donington",
                    Areas = new[] { "Town Centre", "Long Eaton", "Sawley" },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Swadlincote", new CityConfig
                {
                    City = "Swadlincote",
                    Areas = new[] { "Town Centre", "Gresley", "Church Gresley" },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "London", new CityConfig
                {
                    City = "London",
                    Areas = new[]
                    {
                        "Westminster", "Camden", "Islington", "Hackney", "Tower Hamlets", "Bethnal Green", "Shoreditch",
                        "Whitechapel", "Stepney", "Clerkenwell", "King's Cross", "Bloomsbury", "Soho", "Mayfair",
                        "Kensington", "Chelsea", "Belgravia", "Knightsbridge"
                    },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "Nuneaton", new CityConfig
                {
                    City = "Nuneaton",
                    Areas = new[] { "Town Centre", "Bedworth", "Arley", "Stockingford" },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            },
            {
                "West Bridgford", new CityConfig
                {
                    City = "West Bridgford",
                    Areas = new[] { "Town Centre", "Wilford", "Edwalton" },
                    Services = new[]
                    {
                        "Carpet Cleaning", "Sofa Cleaning", "Mattress Cleaning", "Stain Removal", "Upholstery Cleaning",
                        "Rug Cleaning"
                    }
                }
            }
        };
    }

    /// <summary>
    /// Generate all SEO page objects for seeding
    /// Creates pages using single factory method and Add methods
    /// </summary>
    public static List<SeoPage> GenerateAllSeoPages()
    {
        var pages = new List<SeoPage>();
        var citiesConfig = GetCitiesWithAreas();

        foreach (var cityEntry in citiesConfig)
        {
            var cityConfig = cityEntry.Value;
            var servicesList = cityConfig.Services.ToList();
            var areasList = cityConfig.Areas.ToList();

            // 1. Create CityOnly page (e.g., /leicester)
            var cityPage = SeoPage.CreateSeoPage(cityConfig.City);
            pages.Add(cityPage);

            // 2. Create CityService pages (e.g., /leicester/carpet-cleaning)
            foreach (var service in servicesList)
            {
                var cityServicePage = SeoPage.CreateSeoPage(cityConfig.City);
                cityServicePage.AddService(service);
                pages.Add(cityServicePage);
            }

            // 3. Create CityArea pages (e.g., /leicester/wigston)
            foreach (var area in areasList)
            {
                var cityAreaPage = SeoPage.CreateSeoPage(cityConfig.City);
                cityAreaPage.AddArea(area);
                pages.Add(cityAreaPage);
            }

            // 4. Create CityAreaService pages (e.g., /leicester/wigston/carpet-cleaning)
            foreach (var area in areasList)
            {
                foreach (var service in servicesList)
                {
                    var fullPage = SeoPage.CreateSeoPage(cityConfig.City);
                    fullPage.AddArea(area);
                    fullPage.AddService(service);
                    pages.Add(fullPage);
                }
            }
        }

        return pages;
    }
}

public class CityConfig
{
    public string City { get; set; }
    public string[] Areas { get; set; } = Array.Empty<string>();
    public string[] Services { get; set; } = Array.Empty<string>();
}