using System.Data;
using SimulationProject.DTO.UserDTOs;
using SimulationProject.Helper;

namespace SimulationProject.Services
{
    public interface ILinkService<T>
    {
        List<LinkResponseWrapper<UserDto>> AddLinksToList(IEnumerable<UserDto> userdto, string baseUri);
        public LinkResponseWrapper<T> AddLinksForUser(UserDto userdto, string baseUri);
        public LinkResponse AddAuthorizedLinks(string baseUri, bool isAuth, string role);
    }
    public class LinkService: ILinkService<UserDto>
    {
        private readonly LinkGenerator _linkGenerator;

        public LinkService( LinkGenerator linkGenerator)
        {
            _linkGenerator = linkGenerator;
        }

        public LinkResponseWrapper<UserDto> AddLinksForUser(UserDto userdto, string baseUri)
        {
            var response = new LinkResponseWrapper<UserDto>(userdto);

            string self = _linkGenerator.GetPathByAction(action: "GetUser", controller: "Users", values: new { userId = userdto.Userid });
            string update = _linkGenerator.GetPathByAction(action: "UpdateUser", controller: "Users", values: new { userId = userdto.Userid });
            string delete = _linkGenerator.GetPathByAction(action: "DeleteUser", controller: "Users", values: new { userId = userdto.Userid });

            if ((self is not null) && (update is not null) && (delete is not null))
            {
                response._links.Add(new Link(baseUri + self, "self", "GET"));
                response._links.Add(new Link(baseUri + delete, "delete_user", "DELETE"));
                response._links.Add(new Link(baseUri + update, "update_user", "PUT"));
            };

            return response;
        }
        public List<LinkResponseWrapper<UserDto>> AddLinksToList(IEnumerable<UserDto> userdto, string baseUri)
        {
            return userdto.Select(user => AddLinksForUser(user, baseUri)).ToList();
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

                if (role == "admin")
                {
                    if (users is not null)
                        response._links.Add(new Link(baseUri + users, "users", "GET"));
                }
            }          
            return response;
        }
    }
}
