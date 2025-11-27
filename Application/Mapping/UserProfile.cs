using AutoMapper;
using Domain.DTOS.User.Request;
using Domain.DTOS.User.Response;
using Infrastructure.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Application.Mapping
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            // RegisterRequest to Admin
            CreateMap<RegisterRequest, Admin>()
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.Createtime, opt => opt.MapFrom(src => DateTime.UtcNow));

        

            // Admin to UserProfileResponse
            CreateMap<Admin, UserProfileResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate))
                .ForMember(dest => dest.Createtime, opt => opt.MapFrom(src => src.Createtime));
            // Admin → UserResponse
            CreateMap<Admin, UserResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
                .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
                .ForMember(dest => dest.LastLoginDate, opt => opt.MapFrom(src => src.LastLoginDate))
                .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.Createtime));




// UpdateUserDTO → Admin
            CreateMap<UpdateUserDTO, Admin>()
              // إزالة Ignore للسماح بالتحديث
              .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName))
              .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
              .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
              .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
              .ForAllMembers(opt => opt.Condition((src, dest, srcMember) => srcMember != null));

        }
    }
}
