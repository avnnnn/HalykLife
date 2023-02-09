using System;
using System.Collections.Generic;

namespace HalykLife.Models;

public partial class RCurrency
{
    public int Id { get; set; }

    public string? Title { get; set; }

    public string Code { get; set; } = null!;

    public decimal Value { get; set; }

    public DateTime ADate { get; set; }
}
