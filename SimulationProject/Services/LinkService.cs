using System.Data;
using Microsoft.AspNetCore.Routing;
using SimulationProject.DTO.SimulationDTOs;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Helper.HateoasHelper;

namespace SimulationProject.Services
{
    public interface ILinkService<T>
    {
        // Links
        List<LinkResponseWrapper<T>> AddLinksToList(IEnumerable<T> dto, string baseUri);
        public LinkResponseWrapper<T> AddLinks(T dto, string baseUri);
    }
    public class LinkService: ILinkService<UserDto>
    {
        private readonly LinkGenerator _linkGenerator;

        public LinkService( LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public LinkResponseWrapper<UserDto> AddLinks(UserDto userdto, string baseUri)
        {
            var response = new LinkResponseWrapper<UserDto>(userdto);

            string self = _linkGenerator.GetPathByAction(action: "GetUser", controller: "Users", values: new { userId = userdto.Userid });
            string update = _linkGenerator.GetPathByAction(action: "UpdateUser", controller: "Users", values: new { userId = userdto.Userid });
            string delete = _linkGenerator.GetPathByAction(action: "DeleteUser", controller: "Users", values: new { userId = userdto.Userid });

            if (self is not null) 
            { 
            response._links.Add(new Link(baseUri + self, "self", "GET"));
            }
            if (delete is not null)
            {
                response._links.Add(new Link(baseUri + delete, "delete_user", "DELETE"));
            }
            if (update is not null)
            {
                response._links.Add(new Link(baseUri + update, "update_user", "PUT"));
            }
            return response;
        }
        public List<LinkResponseWrapper<UserDto>> AddLinksToList(IEnumerable<UserDto> userdto, string baseUri)
        {
            return userdto.Select(user => AddLinks(user, baseUri)).ToList();
        }
    }

    public class SimLinkService : ILinkService<SimulationDTO>
    {
        private readonly LinkGenerator _linkGenerator;

        public SimLinkService(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }
        public LinkResponseWrapper<SimulationDTO> AddLinks(SimulationDTO simulationdto, string baseUri)
        {
            var response = new LinkResponseWrapper<SimulationDTO>(simulationdto);
            string self = _linkGenerator.GetPathByAction(action: "GetSimulation", controller: "Simulation", values: new { simId = simulationdto.Simid });
            string update = _linkGenerator.GetPathByAction(action: "UpdateSimulation", controller: "Simulation", values: new { simId = simulationdto.Simid });
            string delete = _linkGenerator.GetPathByAction(action: "DeleteSimulation", controller: "Simulation", values: new { simId = simulationdto.Simid });

            if (self is not null)
            {
                response._links.Add(new Link(baseUri + self, "self", "GET"));
            }
            if (delete is not null)
            {
                response._links.Add(new Link(baseUri + delete, "delete_sim", "DELETE"));
            }
            if (update is not null)
            {
                response._links.Add(new Link(baseUri + update, "update_sim", "PUT"));
            }

            return response;
        }

        public List<LinkResponseWrapper<SimulationDTO>> AddLinksToList(IEnumerable<SimulationDTO> simulationdto, string baseUri)
        {
            return simulationdto.Select(simulation => AddLinks(simulation, baseUri)).ToList();
        }
    }

    // Navigation Bar Links
    public class NavLinkService
    {
        private readonly LinkGenerator _linkGenerator;

        public NavLinkService(LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }
        public LinkResponse AddAuthorizedLinks(string baseUri, bool isAuth, string role)
        {
            var response = new LinkResponse();

            string register = _linkGenerator.GetPathByAction(action: "RegisterUser", controller: "Ath");
            string login = _linkGenerator.GetPathByAction(action: "LoginUser", controller: "Ath");
            string home = _linkGenerator.GetPathByAction(action: "HomePage", controller: "Home");
            string profile = _linkGenerator.GetPathByAction(action: "ViewUserProfile", controller: "Users");
            string logout = _linkGenerator.GetPathByAction(action: "Logout", controller: "Ath");
            string users = _linkGenerator.GetPathByAction(action: "GetAllUsers", controller: "Users");
            string simulations = _linkGenerator.GetPathByAction(action: "GetAllSimulation", controller: "Simulation");

            if (!isAuth)
            {
                if (home is not null)
                    response._links.Add(new Link(baseUri + home, "home", "GET"));
                if (login is not null)
                    response._links.Add(new Link(baseUri + login, "login", "POST"));
                if (register is not null)
                    response._links.Add(new Link(baseUri + register, "register", "POST"));
            }
            else
            {
                if (home is not null)
                    response._links.Add(new Link(baseUri + home, "home", "GET"));
                if (profile is not null)
                    response._links.Add(new Link(baseUri + profile, "profile", "GET"));
                if (logout is not null)
                    response._links.Add(new Link(baseUri + logout, "logout", "POST"));
                if (simulations is not null)
                    response._links.Add(new Link(baseUri + simulations, "simulations", "GET"));

                if (role == "Admin")
                {
                    if (users is not null)
                        response._links.Add(new Link(baseUri + users, "users", "GET"));
                }
            }
            return response;
        }
    }
}
