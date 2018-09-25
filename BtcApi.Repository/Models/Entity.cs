using System;

namespace BtcApi.Repository.Models
{
    public class Entity : IEntity
    {
        public Guid Id { get; set; } = new Guid();
    }
}