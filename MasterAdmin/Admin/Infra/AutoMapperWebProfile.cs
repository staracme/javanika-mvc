using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Admin.Models;

namespace Admin.Infra
{
    public class AutoMapperWebProfile : AutoMapper.Profile
    {

        public AutoMapperWebProfile()
        {
            CreateMap<EventsDTO, EventsViewModel>();
            CreateMap<EventsViewModel, EventsDTO>();

            //Transfer Entity Model to View Model / DTOs
            CreateMap<Admin.Models.tblEvent, EventsViewModel>();
            CreateMap<EventsViewModel, Admin.Models.tblEvent>();
            CreateMap<EventsImageListViewModel, Admin.Models.tblPastEvent>();
            CreateMap<Admin.Models.tblPastEvent, EventsImageListViewModel>();

        }

        public static void Run()
        {
            AutoMapper.Mapper.Initialize(a => { a.AddProfile<AutoMapperWebProfile>(); });
        }
    }
}