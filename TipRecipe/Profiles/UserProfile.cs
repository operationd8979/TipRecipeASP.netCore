using AutoMapper;
using TipRecipe.Entities;
using TipRecipe.Models.Dto;

namespace TipRecipe.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRegisterDto, User>();
            CreateMap<User, UserDto>()
                .ForMember(dst => dst.Role, opt => opt.MapFrom(
                    src => string.Join(",", src.UserRoles.Select(r=>r.Role.ToString()))));
        }
    }
}
