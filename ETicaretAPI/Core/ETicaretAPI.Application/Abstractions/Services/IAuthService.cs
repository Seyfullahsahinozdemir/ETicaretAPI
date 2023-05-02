using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETicaretAPI.Application.Abstractions.Services.Authentications;

namespace ETicaretAPI.Application.Abstractions.Services
{
    public interface IAuthService: IExternalAuthentication, IInternalAuthentication
    {
    }
}
