﻿using Microsoft.AspNetCore.Routing;

namespace Cache.Common;

public interface IEndpoint
{
    static abstract void Map(IEndpointRouteBuilder app);
}