using MediatR;
using mvmclean.backend.Application.Features.Services.Commands;
using mvmclean.backend.Application.Features.Contractor.Commands;
using mvmclean.backend.Application.Features.Booking.Commands;
using mvmclean.backend.Application.Features.Promotion.Commands;
using mvmclean.backend.Application.Features.SeoPage.Commands;
using mvmclean.backend.Application.Features.Contact.Commands;
using Microsoft.Extensions.Logging;

namespace mvmclean.backend.Infrastructure.Seeding;

public class DatabaseSeeder
{
    private readonly IMediator _mediator;
    private readonly ILogger<DatabaseSeeder> _logger;

    private readonly bool _seed = false;

    public DatabaseSeeder(IMediator mediator, ILogger<DatabaseSeeder> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        if (!_seed)
        {
            _logger.LogInformation("Database has already been seeded. Skipping seeding.");
            return;
        }

        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Seed services and get their IDs
            //var serviceIds = await SeedServicesAsync();

            // Seed postcode pricing
            //await SeedPostcodePricingAsync(serviceIds);

            // Seed contractors and get their IDs
            //var contractorIds = await SeedContractorsAsync();

            // Seed contractor coverage areas
            //await SeedCoverageAreasAsync(contractorIds);

            // Seed working hours
            //await SeedWorkingHoursAsync(contractorIds);

            // Seed unavailability slots
            //await SeedUnavailabilitySlotsAsync(contractorIds);

            // Seed promotions
            //await SeedPromotionsAsync();

            // Seed SEO pages
            await SeedSeoPagesAsync();

            // Seed past bookings
            //await SeedBookingsAsync(serviceIds, contractorIds);

            // Seed sample contacts
            //await SeedContactsAsync();


            _logger.LogInformation("Database seeding completed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding the database.");
            throw;
        }
    }

    private async Task<List<Guid>> SeedServicesAsync()
    {
        try
        {
            _logger.LogInformation("Seeding services...");

            var serviceIds = new List<Guid>();

            // Carpet Cleaning Services
            var AllServices = new List<CreateServiceRequest>
            {
                new CreateServiceRequest
                {
                    Name = "Lounge Carpet Cleaning",
                    Description = "Professional deep cleaning for lounge carpets",
                    Shortcut = "CRP-LNG",
                    BasePrice = 49.99m,
                    EstimatedDurationMinutes = 50,
                    Category = "Carpet Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Bedroom Carpet Cleaning",
                    Description = "Professional deep cleaning for bedroom carpets",
                    Shortcut = "CRP-BDRM",
                    BasePrice = 39.99m,
                    EstimatedDurationMinutes = 45,
                    Category = "Carpet Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Stairs and Landing Carpet Cleaning",
                    Description = "Professional deep cleaning for stairs and landing carpets",
                    Shortcut = "CRP-STR-LND",
                    BasePrice = 49.99m,
                    EstimatedDurationMinutes = 45,
                    Category = "Carpet Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Corner Sofa Cleaning",
                    Description = "Professional deep cleaning for corner sofas",
                    Shortcut = "SFA-CRNR",
                    BasePrice = 89.99m,
                    EstimatedDurationMinutes = 75,
                    Category = "Upholstery Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "L-Shape Sofa Cleaning",
                    Description = "Professional deep cleaning for L-shape sofas",
                    Shortcut = "SFA-LSH",
                    BasePrice = 79.99m,
                    EstimatedDurationMinutes = 75,
                    Category = "Upholstery Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "2-3 Seater Sofa Cleaning",
                    Description = "Professional deep cleaning for 2-3 seater sofas",
                    Shortcut = "SFA-3ST",
                    BasePrice = 49.99m,
                    EstimatedDurationMinutes = 45,
                    Category = "Upholstery Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Armchair Cleaning",
                    Description = "Professional deep cleaning for armchairs",
                    Shortcut = "SFA-ARM",
                    BasePrice = 29.99m,
                    EstimatedDurationMinutes = 30,
                    Category = "Upholstery Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Footstool Cleaning",
                    Description = "Professional deep cleaning for footstools",
                    Shortcut = "SFA-FTST",
                    BasePrice = 19.99m,
                    EstimatedDurationMinutes = 20,
                    Category = "Upholstery Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Rug Cleaning",
                    Description = "Professional deep cleaning for rugs",
                    Shortcut = "CRP-RUG",
                    BasePrice = 39.99m,
                    EstimatedDurationMinutes = 45,
                    Category = "Carpet Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Single Mattress Cleaning",
                    Description = "Professional deep cleaning for single mattresses",
                    Shortcut = "UPH-SMTRS",
                    BasePrice = 39.99m,
                    EstimatedDurationMinutes = 45,
                    Category = "Upholstery Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Double Mattress Cleaning",
                    Description = "Professional deep cleaning for double mattresses",
                    Shortcut = "UPH-DMTRS",
                    BasePrice = 44.99m,
                    EstimatedDurationMinutes = 45,
                    Category = "Upholstery Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "4-6 Dining Chairs Cleaning",
                    Description = "Professional deep cleaning for 4-6 dining chairs",
                    Shortcut = "CHA-6DC",
                    BasePrice = 39.99m,
                    EstimatedDurationMinutes = 45,
                    Category = "Upholstery Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "5 Seater Car Interior Cleaning",
                    Description = "Professional deep cleaning for 5-seater car interiors",
                    Shortcut = "CAR-5SEAT",
                    BasePrice = 49.99m,
                    EstimatedDurationMinutes = 50,
                    Category = "Vehicle Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Hard Floor Cleaning (1 Room)",
                    Description = "Professional cleaning for hard floors",
                    Shortcut = "FLR-HARD",
                    BasePrice = 49.99m,
                    EstimatedDurationMinutes = 50,
                    Category = "Floor Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Special Cleaning Product",
                    Description = "Specialized cleaning product application",
                    Shortcut = "PROD-SPEC",
                    BasePrice = 7.99m,
                    EstimatedDurationMinutes = 1,
                    Category = "Additional Services"
                },
                new CreateServiceRequest
                {
                    Name = "Jet Wash Cleaning (Up to 30m²)",
                    Description =
                        "High-pressure jet wash cleaning for driveways, patios, and outdoor surfaces up to 30 square meters",
                    Shortcut = "JET-30M2",
                    BasePrice = 119.99m,
                    EstimatedDurationMinutes = 60,
                    Category = "Jet Wash Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Jet Wash Cleaning (30-60m²)",
                    Description =
                        "High-pressure jet wash cleaning for medium-sized outdoor areas between 30-60 square meters",
                    Shortcut = "JET-60M2",
                    BasePrice = 149.99m,
                    EstimatedDurationMinutes = 90,
                    Category = "Jet Wash Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Jet Wash Cleaning (60-100m²)",
                    Description =
                        "High-pressure jet wash cleaning for large outdoor areas between 60-100 square meters",
                    Shortcut = "JET-100M2",
                    BasePrice = 179.99m,
                    EstimatedDurationMinutes = 120,
                    Category = "Jet Wash Cleaning"
                },
                new CreateServiceRequest
                {
                    Name = "Any 2 Rooms/Upholstery",
                    Description =
                        "Choose any 2 rooms or upholstery items for deep steam cleaning. Premium service included. +£15 for L-Shape/Corner Sofas.",
                    Shortcut = "BUNDLE-ANY2",
                    BasePrice = 79.99m,
                    EstimatedDurationMinutes = 60,
                    Category = "Cleaning Packages"
                },
                new CreateServiceRequest
                {
                    Name = "Any 3 Rooms/Upholstery",
                    Description =
                        "Choose any 3 rooms or upholstery items for deep steam cleaning. Premium service included. +£15 for L-Shape/Corner Sofas.",
                    Shortcut = "BUNDLE-ANY3",
                    BasePrice = 94.99m,
                    EstimatedDurationMinutes = 90,
                    Category = "Cleaning Packages"
                },
                new CreateServiceRequest
                {
                    Name = "2-Bed House Carpet Cleaning",
                    Description =
                        "Full house carpet cleaning including: 2 bedrooms + stairs + landing + lounge. Deep cleaning included.",
                    Shortcut = "BUNDLE-2BED",
                    BasePrice = 129.99m,
                    EstimatedDurationMinutes = 120,
                    Category = "Cleaning Packages"
                },
                new CreateServiceRequest
                {
                    Name = "3-Bed House Carpet Cleaning",
                    Description =
                        "Full house carpet cleaning including: 3 bedrooms + stairs + landing + lounge. Deep cleaning included.",
                    Shortcut = "BUNDLE-3BED",
                    BasePrice = 149.99m,
                    EstimatedDurationMinutes = 150,
                    Category = "Cleaning Packages"
                },
                new CreateServiceRequest
                {
                    Name = "4-Bed House Carpet Cleaning",
                    Description =
                        "Full house carpet cleaning including: 4 bedrooms + stairs + landing + lounge. Deep cleaning included.",
                    Shortcut = "BUNDLE-4BED",
                    BasePrice = 169.99m,
                    EstimatedDurationMinutes = 180,
                    Category = "Cleaning Packages"
                },
                new CreateServiceRequest
                {
                    Name = "Day Rate Cleaning Service",
                    Description =
                        "Full day cleaning service (9 AM - 5 PM). Includes any rooms, upholstery, or commercial areas. 1-hour break included. Comprehensive cleaning. +£100 for extra staff with machine.",
                    Shortcut = "BUNDLE-DAYRATE",
                    BasePrice = 349.99m,
                    EstimatedDurationMinutes = 400, 
                    Category = "Cleaning Packages"
                }
            };


            // Combine all services
            var allServices = AllServices.ToList();

            foreach (var service in allServices)
            {
                try
                {
                    var response = await _mediator.Send(service);
                    serviceIds.Add(Guid.Parse(response.ServiceId));
                    _logger.LogInformation($"Service created: {service.Name} (ID: {response.ServiceId})");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Service '{service.Name}' may already exist or error occurred: {ex.Message}");
                }
            }

            _logger.LogInformation("Services seeding completed.");
            return serviceIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding services.");
            throw;
        }
    }

    private async Task<List<string>> SeedContractorsAsync()
    {
        try
        {
            _logger.LogInformation("Seeding contractors...");

            var contractorIds = new List<string>();

            var contractors = new List<CreateContractorRequest>
            {
                new CreateContractorRequest
                {
                    FirstName = "Sait",
                    LastName = "Han Uzun",
                    PhoneNumber = "07700123456",
                    Email = "sait.hanuzun@example.com",
                    Username = "saithanuzun",
                    Password = "123",
                    ImageUrl = null,
                    Street = "42 Baker Street",
                    City = "London",
                    PostcodeValue = "W1A 1AA",
                    AdditionalInfo = "Office 5, Ground Floor"
                },
                new CreateContractorRequest
                {
                    FirstName = "Ahmed",
                    LastName = "Hassan",
                    PhoneNumber = "07701234567",
                    Email = "ahmed.hassan@example.com",
                    Username = "ahmedhassan",
                    Password = "123",
                    ImageUrl = null,
                    Street = "123 Belgrave Road",
                    City = "Leicester",
                    PostcodeValue = "LE1 3RA",
                    AdditionalInfo = "Unit 7, Industrial Estate"
                },
                new CreateContractorRequest
                {
                    FirstName = "Emily",
                    LastName = "Thompson",
                    PhoneNumber = "07702345678",
                    Email = "emily.thompson@example.com",
                    Username = "emilythompson",
                    Password = "123",
                    ImageUrl = null,
                    Street = "78 Victoria Square",
                    City = "London",
                    PostcodeValue = "SW1A 2AA",
                    AdditionalInfo = "Flat 2, First Floor"
                }
            };

            foreach (var contractor in contractors)
            {
                try
                {
                    var response = await _mediator.Send(contractor);
                    contractorIds.Add(response.ContractorId.ToString());
                    _logger.LogInformation(
                        $"Contractor created: {contractor.FirstName} {contractor.LastName} (ID: {response.ContractorId}) - {contractor.Street}, {contractor.City} {contractor.PostcodeValue}");

                    // Activate the contractor
                    var activateRequest = new UpdateContractorStatusRequest
                    {
                        ContractorId = response.ContractorId.ToString(),
                        IsActive = true
                    };
                    var activateResponse = await _mediator.Send(activateRequest);
                    if (activateResponse.Success)
                    {
                        _logger.LogInformation($"Contractor activated: {contractor.FirstName} {contractor.LastName}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"Contractor '{contractor.FirstName} {contractor.LastName}' may already exist or error occurred: {ex.Message}");
                }
            }

            _logger.LogInformation("Contractors seeding completed.");
            return contractorIds;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding contractors.");
            throw;
        }
    }

    private async Task SeedCoverageAreasAsync(List<string> contractorIds)
    {
        try
        {
            _logger.LogInformation("Seeding coverage areas...");

            // Coverage areas for each contractor - system automatically matches by first 2 characters
            var coverageAreaMap = new Dictionary<int, List<string>>
            {
                // Sait Han Uzun covers London postcodes
                {
                    0, new List<string>
                    {
                        "SW1A 0AA", "SW1A 1AA", "SW1A 2AA",
                        "N1 1AA", "N1 2AA", "N1 3AA",
                        "E1 6AN", "E1 7AA", "E1 8AA",
                        "W1A 1AA", "W1A 2AA", "W1B 1AA"
                    }
                },
                // Ahmed Hassan covers Leicester postcodes
                {
                    1, new List<string>
                    {
                        "LE1 3RA", "LE1 4TA", "LE1 5AA",
                        "LE2 0AA", "LE2 1AA", "LE2 2AA",
                        "LE3 1AA", "LE3 2AA",
                        "LE4 4AA", "LE4 5AA",
                        "LE5 0AA", "LE5 1AA"
                    }
                },
                // Emily Thompson covers both London and Leicester
                {
                    2, new List<string>
                    {
                        "SW2 1AA", "SW2 2AA", "SW2 3AA",
                        "SW3 1AA", "SW3 2AA", "SW3 3AA",
                        "LE1 6AA", "LE1 7AA",
                        "LE2 3AA", "LE2 4AA"
                    }
                }
            };

            for (int i = 0; i < contractorIds.Count && i < coverageAreaMap.Count; i++)
            {
                foreach (var postcode in coverageAreaMap[i])
                {
                    try
                    {
                        var request = new CreateContractorCoverageRequest
                        {
                            ContractorId = contractorIds[i],
                            Postcode = postcode
                        };

                        await _mediator.Send(request);
                        _logger.LogInformation(
                            $"Coverage area added: Contractor {contractorIds[i]} - Postcode {postcode}");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Coverage area for postcode '{postcode}' may already exist: {ex.Message}");
                    }
                }
            }

            _logger.LogInformation("Coverage areas seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding coverage areas.");
            throw;
        }
    }

    private async Task SeedWorkingHoursAsync(List<string> contractorIds)
    {
        try
        {
            _logger.LogInformation("Seeding working hours...");

            // Working hours for all contractors: Monday to Friday, 8:30 AM to 6:30 PM
            var workingDays = new List<DayOfWeek>
                { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            var startTime = new TimeOnly(8, 30);
            var endTime = new TimeOnly(18, 30);

            foreach (var contractorId in contractorIds)
            {
                foreach (var day in workingDays)
                {
                    try
                    {
                        var request = new SetWorkingDaysRequest
                        {
                            ContractorId = contractorId,
                            DayOfWeek = day,
                            StartTime = startTime,
                            EndTime = endTime,
                            IsWorkingDay = true
                        };

                        await _mediator.Send(request);
                        _logger.LogInformation(
                            $"Working day set: Contractor {contractorId} - {day} ({startTime:HH:mm} - {endTime:HH:mm})");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Working day for {day} may already exist: {ex.Message}");
                    }
                }
            }

            _logger.LogInformation("Working hours seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding working hours.");
            throw;
        }
    }

    private async Task SeedUnavailabilitySlotsAsync(List<string> contractorIds)
    {
        try
        {
            _logger.LogInformation("Seeding unavailability slots...");

            // Sample unavailability slots for contractors
            foreach (var contractorId in contractorIds)
            {
                // Add some unavailability for next week
                var today = DateTime.UtcNow.Date;
                var unavailableStart = today.AddDays(3); // 3 days from now
                var unavailableEnd = unavailableStart.AddHours(4);

                try
                {
                    var request = new CreateUnavailabilityRequest
                    {
                        ContractorId = contractorId,
                        StartTime = unavailableStart,
                        EndTime = unavailableEnd
                    };

                    await _mediator.Send(request);
                    _logger.LogInformation(
                        $"Unavailability slot created: Contractor {contractorId} - {unavailableStart:yyyy-MM-dd HH:mm} to {unavailableEnd:yyyy-MM-dd HH:mm}");
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"Unavailability slot may already exist: {ex.Message}");
                }
            }

            _logger.LogInformation("Unavailability slots seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding unavailability slots.");
            throw;
        }
    }

    private async Task SeedPostcodePricingAsync(List<Guid> serviceIds)
    {
        try
        {
            _logger.LogInformation("Seeding postcode pricing...");

            // Central London postcodes - premium pricing (1.3x multiplier, +£30 fixed)
            var centralLondonPostcodes = new List<string>
            {
                "SW1A 0AA", "SW1A 1AA", "SW1A 2AA",
                "W1A 1AA", "W1A 2AA", "W1B 1AA",
                "EC1A 1AA", "EC1A 2AA", "EC1B 1AA"
            };

            // Greater London postcodes - standard London pricing (1.2x multiplier, +£20 fixed)
            var londonPostcodes = new List<string>
            {
                "N1 1AA", "N1 2AA", "N1 3AA",
                "E1 6AN", "E1 7AA", "E1 8AA",
                "SW2 1AA", "SW2 2AA", "SW2 3AA",
                "SW3 1AA", "SW3 2AA", "SW3 3AA",
                "NW1 1AA", "NW1 2AA"
            };

            // Midlands postcodes - slightly higher (1.1x multiplier, +£10 fixed)
            var midlandsPostcodes = new List<string>
            {
                "LE1 3RA", "LE1 4TA", "LE1 5AA", "LE1 6AA", "LE1 7AA",
                "LE2 0AA", "LE2 1AA", "LE2 2AA", "LE2 3AA", "LE2 4AA",
                "LE3 1AA", "LE3 2AA",
                "LE4 4AA", "LE4 5AA",
                "LE5 0AA", "LE5 1AA"
            };

            // Standard UK postcodes - base pricing (1.0x multiplier, no adjustment)
            var standardPostcodes = new List<string>
            {
                "M1 1AA", "M1 2AA", "M2 1AA",
                "B1 1AA", "B1 2AA", "B2 1AA",
                "S1 1AA", "S1 2AA", "S2 1AA",
                "L1 1AA", "L1 2AA", "L2 1AA",
                "CF10 1AA", "CF10 2AA", "CF11 1AA"
            };

            // Remote/Rural areas - reduced pricing (0.85x multiplier, -£10 fixed)
            var remotePostcodes = new List<string>
            {
                "PA1 1AA", "PA2 1AA", "PA3 1AA",
                "IV1 1AA", "IV2 1AA",
                "ZE1 1AA", "ZE2 1AA"
            };

            // Apply Central London pricing
            foreach (var serviceId in serviceIds)
            {
                foreach (var postcode in centralLondonPostcodes)
                {
                    try
                    {
                        var request = new AddServicePostcodePricingRequest
                        {
                            ServiceId = serviceId.ToString(),
                            Postcode = postcode,
                            Multiplier = 1.3m,
                            FixedAdjustment = 30m
                        };
                        await _mediator.Send(request);
                        _logger.LogInformation(
                            $"Postcode pricing added: Service {serviceId} - {postcode} (1.3x multiplier, +£30)");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Postcode pricing for {postcode} may already exist: {ex.Message}");
                    }
                }
            }

            // Apply Greater London pricing
            foreach (var serviceId in serviceIds)
            {
                foreach (var postcode in londonPostcodes)
                {
                    try
                    {
                        var request = new AddServicePostcodePricingRequest
                        {
                            ServiceId = serviceId.ToString(),
                            Postcode = postcode,
                            Multiplier = 1.2m,
                            FixedAdjustment = 20m
                        };
                        await _mediator.Send(request);
                        _logger.LogInformation(
                            $"Postcode pricing added: Service {serviceId} - {postcode} (1.2x multiplier, +£20)");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Postcode pricing for {postcode} may already exist: {ex.Message}");
                    }
                }
            }

            // Apply Midlands pricing
            foreach (var serviceId in serviceIds)
            {
                foreach (var postcode in midlandsPostcodes)
                {
                    try
                    {
                        var request = new AddServicePostcodePricingRequest
                        {
                            ServiceId = serviceId.ToString(),
                            Postcode = postcode,
                            Multiplier = 1.1m,
                            FixedAdjustment = 10m
                        };
                        await _mediator.Send(request);
                        _logger.LogInformation(
                            $"Postcode pricing added: Service {serviceId} - {postcode} (1.1x multiplier, +£10)");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Postcode pricing for {postcode} may already exist: {ex.Message}");
                    }
                }
            }

            // Apply standard UK pricing (no adjustment)
            foreach (var serviceId in serviceIds)
            {
                foreach (var postcode in standardPostcodes)
                {
                    try
                    {
                        var request = new AddServicePostcodePricingRequest
                        {
                            ServiceId = serviceId.ToString(),
                            Postcode = postcode,
                            Multiplier = 1.0m,
                            FixedAdjustment = 0m
                        };
                        await _mediator.Send(request);
                        _logger.LogInformation(
                            $"Postcode pricing added: Service {serviceId} - {postcode} (1.0x multiplier, no adjustment)");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Postcode pricing for {postcode} may already exist: {ex.Message}");
                    }
                }
            }

            // Apply remote area pricing (discounted)
            foreach (var serviceId in serviceIds)
            {
                foreach (var postcode in remotePostcodes)
                {
                    try
                    {
                        var request = new AddServicePostcodePricingRequest
                        {
                            ServiceId = serviceId.ToString(),
                            Postcode = postcode,
                            Multiplier = 0.85m,
                            FixedAdjustment = -10m
                        };
                        await _mediator.Send(request);
                        _logger.LogInformation(
                            $"Postcode pricing added: Service {serviceId} - {postcode} (0.85x multiplier, -£10)");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning($"Postcode pricing for {postcode} may already exist: {ex.Message}");
                    }
                }
            }

            _logger.LogInformation("Postcode pricing seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding postcode pricing.");
            throw;
        }
    }

    private async Task SeedBookingsAsync(List<Guid> serviceIds, List<string> contractorIds)
    {
        try
        {
            _logger.LogInformation("Seeding past bookings...");

            // Sample customer data for past bookings
            var bookingRequests = new List<CreateBookingCompleteRequest>
            {
                // London bookings with contractors
                new CreateBookingCompleteRequest
                {
                    CustomerName = "John Smith",
                    CustomerEmail = "john.smith@example.com",
                    CustomerPhone = "07900111111",
                    Address = "10 Downing Street, London",
                    Postcode = "SW1A 2AA",
                    ContractorId = contractorIds[0],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-30).AddHours(10),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-30).AddHours(11),
                    TotalAmount = 89.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[0], ServiceName = "1 Bed Carpet Cleaning", Quantity = 1,
                            Price = 89.99m
                        }
                    }
                },
                new CreateBookingCompleteRequest
                {
                    CustomerName = "Sarah Johnson",
                    CustomerEmail = "sarah.johnson@example.com",
                    CustomerPhone = "07900222222",
                    Address = "25 Oxford Street, London",
                    Postcode = "W1A 1AA",
                    ContractorId = contractorIds[0],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-25).AddHours(14),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-25).AddHours(16),
                    TotalAmount = 149.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[1], ServiceName = "2 Bed Carpet Cleaning", Quantity = 1,
                            Price = 149.99m
                        }
                    }
                },
                new CreateBookingCompleteRequest
                {
                    CustomerName = "Michael Brown",
                    CustomerEmail = "michael.brown@example.com",
                    CustomerPhone = "07900333333",
                    Address = "15 Park Lane, London",
                    Postcode = "W1A 2AA",
                    ContractorId = contractorIds[2],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-20).AddHours(9),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-20).AddHours(10.5),
                    TotalAmount = 79.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[4], ServiceName = "2 Seater Sofa Cleaning", Quantity = 1,
                            Price = 79.99m
                        }
                    }
                },
                // Leicester bookings
                new CreateBookingCompleteRequest
                {
                    CustomerName = "Emma Wilson",
                    CustomerEmail = "emma.wilson@example.com",
                    CustomerPhone = "07900444444",
                    Address = "42 High Street, Leicester",
                    Postcode = "LE1 3RA",
                    ContractorId = contractorIds[1],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-18).AddHours(11),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-18).AddHours(12.5),
                    TotalAmount = 69.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[4], ServiceName = "2 Seater Sofa Cleaning", Quantity = 1,
                            Price = 69.99m
                        }
                    }
                },
                new CreateBookingCompleteRequest
                {
                    CustomerName = "David Taylor",
                    CustomerEmail = "david.taylor@example.com",
                    CustomerPhone = "07900555555",
                    Address = "89 Market Street, Leicester",
                    Postcode = "LE1 4TA",
                    ContractorId = contractorIds[1],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-15).AddHours(13),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-15).AddHours(15),
                    TotalAmount = 149.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[8], ServiceName = "Residential Window Cleaning", Quantity = 1,
                            Price = 149.99m
                        }
                    }
                },
                new CreateBookingCompleteRequest
                {
                    CustomerName = "Lisa Anderson",
                    CustomerEmail = "lisa.anderson@example.com",
                    CustomerPhone = "07900666666",
                    Address = "72 Victoria Road, Leicester",
                    Postcode = "LE2 0AA",
                    ContractorId = contractorIds[1],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-12).AddHours(10),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-12).AddHours(11),
                    TotalAmount = 59.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[10], ServiceName = "Single Oven Cleaning", Quantity = 1,
                            Price = 59.99m
                        }
                    }
                },
                // Mixed location bookings
                new CreateBookingCompleteRequest
                {
                    CustomerName = "Robert Davis",
                    CustomerEmail = "robert.davis@example.com",
                    CustomerPhone = "07900777777",
                    Address = "33 Bell Street, London",
                    Postcode = "N1 1AA",
                    ContractorId = contractorIds[0],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-10).AddHours(15),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-10).AddHours(16.5),
                    TotalAmount = 89.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new() { ServiceId = serviceIds[6], ServiceName = "50m² Jet Wash", Quantity = 1, Price = 89.99m }
                    }
                },
                new CreateBookingCompleteRequest
                {
                    CustomerName = "Jennifer Martinez",
                    CustomerEmail = "jennifer.martinez@example.com",
                    CustomerPhone = "07900888888",
                    Address = "56 Elm Avenue, London",
                    Postcode = "SW3 1AA",
                    ContractorId = contractorIds[2],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-8).AddHours(11),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-8).AddHours(13),
                    TotalAmount = 99.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[11], ServiceName = "Double Oven Cleaning", Quantity = 1,
                            Price = 99.99m
                        }
                    }
                },
                new CreateBookingCompleteRequest
                {
                    CustomerName = "Christopher Lee",
                    CustomerEmail = "christopher.lee@example.com",
                    CustomerPhone = "07900999999",
                    Address = "99 Oak Gardens, London",
                    Postcode = "E1 6AN",
                    ContractorId = contractorIds[0],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-5).AddHours(9),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-5).AddHours(11),
                    TotalAmount = 159.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[2], ServiceName = "3 Bed Carpet Cleaning", Quantity = 1,
                            Price = 159.99m
                        }
                    }
                },
                new CreateBookingCompleteRequest
                {
                    CustomerName = "Rachel White",
                    CustomerEmail = "rachel.white@example.com",
                    CustomerPhone = "07901000000",
                    Address = "12 Cedar Lane, Leicester",
                    Postcode = "LE3 1AA",
                    ContractorId = contractorIds[1],
                    ScheduledStartTime = DateTime.UtcNow.AddDays(-3).AddHours(14),
                    ScheduledEndTime = DateTime.UtcNow.AddDays(-3).AddHours(15.5),
                    TotalAmount = 79.99m,
                    ServiceItems = new List<CreateBookingCompleteRequest.ServiceItemDto>
                    {
                        new()
                        {
                            ServiceId = serviceIds[15], ServiceName = "Single Story Gutter Cleaning", Quantity = 1,
                            Price = 79.99m
                        }
                    }
                }
            };

            foreach (var bookingRequest in bookingRequests)
            {
                try
                {
                    var response = await _mediator.Send(bookingRequest);
                    if (response != null && response.Success)
                    {
                        _logger.LogInformation(
                            $"Past booking created: {bookingRequest.CustomerName} - {bookingRequest.Postcode} (ID: {response.BookingId})");
                    }
                    else if (response != null)
                    {
                        _logger.LogWarning(
                            $"Failed to create booking for '{bookingRequest.CustomerName}': {response.Message}");
                    }
                    else
                    {
                        _logger.LogWarning($"Booking handler returned null for '{bookingRequest.CustomerName}'");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"Booking for '{bookingRequest.CustomerName}' may already exist or error occurred: {ex.Message}");
                }
            }

            _logger.LogInformation("Past bookings seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding bookings.");
            throw;
        }
    }

    private async Task SeedPromotionsAsync()
    {
        try
        {
            _logger.LogInformation("Seeding promotions...");

            var promotions = new List<CreatePromotionRequest>
            {
                new CreatePromotionRequest
                {
                    Code = "SAVE10",
                    Description = "Save 10% on your booking",
                    DiscountType = 0, // 0 = Percentage, 1 = Fixed Amount
                    DiscountValue = 10,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(900),
                    UsageLimit = 100,
                    MinimumOrderAmount = 0
                },
                new CreatePromotionRequest
                {
                    Code = "SAVE20NEW",
                    Description = "Save 20% on your booking",
                    DiscountType = 0, // 0 = Percentage
                    DiscountValue = 20,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(900),
                    UsageLimit = 50,
                    MinimumOrderAmount = 0
                },
                new CreatePromotionRequest
                {
                    Code = "WELCOME10NEW",
                    Description = "Welcome discount - Save 10$ discount on your first booking",
                    DiscountType = 1, // 0 = Percentage
                    DiscountValue = 5,
                    ValidFrom = DateTime.UtcNow.AddDays(-30),
                    ValidTo = DateTime.UtcNow.AddDays(900),
                    UsageLimit = 200,
                    MinimumOrderAmount = 0
                }
            };

            foreach (var promotionRequest in promotions)
            {
                try
                {
                    var response = await _mediator.Send(promotionRequest);
                    if (response != null)
                    {
                        _logger.LogInformation(
                            $"Promotion created: {promotionRequest.Code} - {promotionRequest.Description} (ID: {response.PromotionId})");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"Promotion '{promotionRequest.Code}' may already exist or error occurred: {ex.Message}");
                }
            }

            _logger.LogInformation("Promotions seeding completed.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding promotions.");
            throw;
        }
    }

    private async Task SeedSeoPagesAsync()
    {
        try
        {
            _logger.LogInformation("Seeding SEO pages...");

            var seoPages = SeoPageSeedData.GenerateSeoPagesForCity("Nottingham");
            _logger.LogInformation($"Generated {seoPages.Count} SEO pages for Nottingham to seed");

            if (seoPages.Count == 0)
            {
                _logger.LogWarning("No SEO pages generated!");
                return;
            }

            int createdCount = 0;
            foreach (var page in seoPages)
            {
                try
                {
                    var request = new BulkCreateSeoPageRequest
                    {
                        Pages = new List<Domain.Aggregates.SeoPage.SeoPage> { page }
                    };
                    
                    var response = await _mediator.Send(request);
                    if (response != null && response.CreatedIds.Any())
                    {
                        _logger.LogInformation(
                            $"✅ SEO Page created: {page.Slug} ({page.Level}) - ID: {response.CreatedIds.First()}");
                        createdCount++;
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ Response was null or no IDs returned for {page.Slug}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(
                        $"❌ SEO Page '{page.Slug}' error: {ex.Message}");
                }
            }

            _logger.LogInformation($"✅ SEO pages seeding completed. Created: {createdCount}/{seoPages.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error seeding SEO pages.");
            throw;
        }
    }

    private async Task SeedContactsAsync()
    {
        try
        {
            _logger.LogInformation("Seeding sample contacts...");

            var contacts = new List<CreateContactCommand>
            {
                new CreateContactCommand
                {
                    FullName = "John Smith",
                    Email = "john.smith@example.com",
                    PhoneNumber = "07700 900123",
                    Subject = "Carpet Cleaning Service Inquiry",
                    Message = "I'm interested in your carpet cleaning services for my lounge. Could you provide a quote for a 4m x 5m area?"
                },
                new CreateContactCommand
                {
                    FullName = "Sarah Johnson",
                    Email = "sarah.johnson@example.com",
                    PhoneNumber = "07700 900456",
                    Subject = "Sofa Cleaning Quote",
                    Message = "I have a large 3-seater sofa that needs professional cleaning. It has some stains and odours. What would be your typical pricing?"
                },
                new CreateContactCommand
                {
                    FullName = "Michael Brown",
                    Email = "michael.brown@example.com",
                    PhoneNumber = null,
                    Subject = "Regular Cleaning Service",
                    Message = "We run a small office and are looking for a reliable cleaning service on a weekly basis. Can you handle commercial properties?"
                },
                new CreateContactCommand
                {
                    FullName = "Emma Davis",
                    Email = "emma.davis@example.com",
                    PhoneNumber = "07700 900789",
                    Subject = "Pet Stain Removal",
                    Message = "Our dog has had an accident on the carpet. We need urgent professional cleaning. Are you available this weekend?"
                },
                new CreateContactCommand
                {
                    FullName = "Robert Wilson",
                    Email = "robert.wilson@example.com",
                    PhoneNumber = "07700 900012",
                    Subject = "Rug Cleaning Service",
                    Message = "I have a Persian rug that needs gentle professional cleaning. Do you have experience with delicate fabrics?"
                }
            };

            int createdCount = 0;
            foreach (var contact in contacts)
            {
                try
                {
                    var response = await _mediator.Send(contact);
                    if (response.Success)
                    {
                        _logger.LogInformation($"✅ Contact created: {contact.FullName} ({contact.Email}) - ID: {response.ContactId}");
                        createdCount++;
                    }
                    else
                    {
                        _logger.LogWarning($"⚠️ Contact '{contact.FullName}' error: {response.Message}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning($"❌ Contact '{contact.FullName}' exception: {ex.Message}");
                }
            }

            _logger.LogInformation($"✅ Contacts seeding completed. Created: {createdCount}/{contacts.Count}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error seeding contacts.");
            throw;
        }
    }
}