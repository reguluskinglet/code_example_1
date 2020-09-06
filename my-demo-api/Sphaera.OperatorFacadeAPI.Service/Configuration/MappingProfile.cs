using AutoMapper;
using demo.CallManagement.HttpContracts.Dto;
using demo.ContactManagement.HttpContracts.Dto;
using demo.GisFacade.Client.Dto;
using demo.InboxDistribution.HttpContracts.Dto;
using demo.IndexService.HttpContracts.Dto;
using demo.DemoApi.Domain.Entities;
using demo.DemoApi.Domain.Entities.SmsMetadata;
using demo.DemoApi.Service.Dtos;
using demo.DemoApi.Service.Dtos.Calls;
using demo.DemoApi.Service.Dtos.Case;
using demo.DemoApi.Service.Dtos.Contact;
using demo.DemoApi.Service.Dtos.Index;
using demo.DemoApi.Service.Dtos.Language;
using demo.DemoApi.Service.Dtos.Line;
using demo.DemoApi.Service.Dtos.Receiver;
using demo.DemoApi.Service.Dtos.Role;
using demo.UserManagement.HttpContracts.Dto;

namespace demo.DemoApi.Service.Configuration
{
    /// <summary>
    /// Настройка маппинга для AutoMapper
    /// Подтягивается автоматически автомаппером.
    /// </summary>
    // ReSharper disable once UnusedType.Global
    public class MappingProfile : Profile
    {
        /// <inheritdoc />
        public MappingProfile()
        {
            CreateMap<MessageContracts.Dtos.Sms.Position, SmsPositionData>();
            CreateMap<MessageContracts.Dtos.Sms.Location, SmsLocationData>();
            CreateMap<MessageContracts.Dtos.Sms.Sms, SmsMessageData>();

            CreateMap<IndexClientDto, IndexDto>();
            CreateMap<IndexTreeClientDto, IndexesDto>();

            CreateMap<ContactPageClientDto, ContactsPageDto>();
            CreateMap<ContactPageInfoClientDto, PageViewModel>();
            CreateMap<ContactClientDto, ContactDto>();
            CreateMap<ContactPhoneClientDto, PhoneDto>();

            CreateMap<GetApplicantLocationClientDto, GetApplicantLocationDto>();
            CreateMap<ApplicantLocationInfoDto, ApplicantLocationInfoClientDto>();
            CreateMap<SmsLocationData, LocationDataClientDto>();
            CreateMap<SmsPositionData, PositionDataClientDto>();
            CreateMap<Language, LanguageExtendedDto>();
            CreateMap<Language, LanguageDto>();

            CreateMap<LineClientDto, LineDto>();
            CreateMap<LineByCaseFolderClientDto, LineByCaseFolderDto>();
            CreateMap<LineByCaseFolderClientDto, LineDto>();
            CreateMap<CallClientDto, CallDto>();

            CreateMap<RoleClientDto, RoleDto>();
            CreateMap<WorkChoiceClientDto, WorkChoiceDto>();
            CreateMap<ReceiverClientDto, ReceiverDto>();

            CreateMap<UserClientDto, UserDto>()
                .ForMember(x => x.CallId, opt => opt.Ignore())
                .ForMember(x => x.CurrentCallStates, opt => opt.Ignore())
                .ForMember(x => x.ConnectionMode, opt => opt.Ignore());
        }
    }
}
