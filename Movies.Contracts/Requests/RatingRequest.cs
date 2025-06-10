using System;

namespace Movies.Contracts.Requests;

public class RatingRequest
{
    public required int Rating { get; init; }
}
