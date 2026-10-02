using MediatR;
using mvmclean.backend.Domain.Aggregates.Contact;

namespace mvmclean.backend.Application.Features.Contact.Commands;

public class CreateContactCommand : IRequest<CreateContactResponse>
{
    public string FullName { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string Subject { get; set; }
    public string Message { get; set; }
}

public class CreateContactResponse
{
    public bool Success { get; set; }
    public required string Message { get; set; }
    public Guid? ContactId { get; set; }
}

public class CreateContactHandler : IRequestHandler<CreateContactCommand, CreateContactResponse>
{
    private readonly IContactRepository _contactRepository;

    public CreateContactHandler(IContactRepository contactRepository)
    {
        _contactRepository = contactRepository;
    }

    public async Task<CreateContactResponse> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var contact = Domain.Aggregates.Contact.Contact.Create(
                fullName: request.FullName,
                email: request.Email,
                phoneNumber: request.PhoneNumber,
                subject: request.Subject,
                message: request.Message
            );

            //todo: spam contact
            var found = await _contactRepository.FirstOrDefaultAsync(i =>
                i.Email == contact.Email ||
                (!string.IsNullOrWhiteSpace(contact.PhoneNumber) &&
                 i.PhoneNumber == contact.PhoneNumber) ||
                i.Message == contact.Message);
            
            if (found is not null)
            {
                return new CreateContactResponse
                {
                    Success = false,
                    Message = $"Error creating contact: SPAM",
                    ContactId = null
                };
            }
                
            await _contactRepository.AddAsync(contact);

            return new CreateContactResponse
            {
                Success = true,
                Message = "Contact created successfully",
                ContactId = contact.Id
            };
        }
        catch (Exception ex)
        {
            return new CreateContactResponse
            {
                Success = false,
                Message = $"Error creating contact: {ex.Message}",
                ContactId = null
            };
        }
    }
}
