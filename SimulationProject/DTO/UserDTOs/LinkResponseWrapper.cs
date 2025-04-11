using SimulationProject.Helper;

namespace SimulationProject.DTO.UserDTOs
{
    public class LinkResponseWrapper<T>
    {
        public T UsersCollection { get; set; }
        public List<Link> _links { get; set; }

        public LinkResponseWrapper(T users)
        {
            UsersCollection = users;
            _links = new List<Link>();
        }
    }
}
