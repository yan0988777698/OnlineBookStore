using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repo.IRepo
{
    public interface IOrderDetailRepo : IGenericRepo<OrderDetail>
    {
        void Update(OrderDetail orderDetail);
    }
}
