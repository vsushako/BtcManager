using System;

namespace BtcApi.Repository.Models
{
    public interface IEntity
    {
        Guid Id { get; set; }
    }
}