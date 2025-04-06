using SimulationProject.Helper;

namespace SimulationProject.DTO
{
    public class LinkResponseWrapper<T> //: LinkResource
    {
        //public List<T> Value { get; set; } = new List<T>();

        //public LinkResponseWrapper(List<T> value)
        //{
        //    Value = value;
        //}

        public T UsersCollection { get; set; }
        public List<Link> _links { get; set; }

        public LinkResponseWrapper(T users)
        {
            UsersCollection = users;
            _links = new List<Link>();
        }

    }
}
