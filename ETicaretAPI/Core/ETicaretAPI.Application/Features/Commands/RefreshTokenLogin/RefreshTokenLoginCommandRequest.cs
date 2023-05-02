using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MediatR;

namespace ETicaretAPI.Application.Features.Commands.RefreshTokenLogin
{
    public class RefreshTokenLoginCommandRequest: IRequest<RefreshTokenLoginCommandResponse>
    {
        public string RefreshToken { get; set; }
    }
}
