

using System.ComponentModel.DataAnnotations;

namespace ThinkTank.Application.Configuration.Queries
{
    public class IGetTByIdQuery<T>:IQuery<T> where T:class
    {
        [Range(1, int.MaxValue, ErrorMessage = "Only positive number allowed")]
        public int Id { get; }
        public IGetTByIdQuery(int id)
        {
            Id = id;
        }
    }
}
