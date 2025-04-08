using Microsoft.AspNetCore.Http;
using SimulationProject.DTO;
using SimulationProject.Helper;
using SimulationProject.Models;

namespace SimulationProject.Services
{
    public interface ILinkService<T>
    {
        List<LinkResponseWrapper<UserDto>> AddLinksToList(IEnumerable<UserDto> userdto, string baseUri);
        public LinkResponseWrapper<T> AddLinksForUser(UserDto userdto, string baseUri);
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

    }
}
