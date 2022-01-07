using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Java.API.Models;

namespace Java.API.Infra
{
    public class AutoMapperWebProfile : AutoMapper.Profile
    {

        public AutoMapperWebProfile()
        {
            CreateMap<EventsDTO, EventsViewModel>();
            CreateMap<EventsViewModel, EventsDTO>();

            //Transfer Entity Model to View Model / DTOs
            CreateMap<Java.API.Models.tblEvent, EventsViewModel>();
            CreateMap<EventsViewModel, Java.API.Models.tblEvent>();
            CreateMap<EventsImageListViewModel, Java.API.Models.tblPastEvent>();
            CreateMap<Java.API.Models.tblPastEvent, EventsImageListViewModel>();

        }

        public static void Run()
        {
            AutoMapper.Mapper.Initialize(a => { a.AddProfile<AutoMapperWebProfile>(); });
        }
    }
}