using System;
using System.Collections.Generic;

namespace Quiz_Web.Models.Entities;

public partial class Payment
{
    public int PaymentId { get; set; }

    public int PurchaseId { get; set; }

    public string Provider { get; set; } = null!;

    public string? ProviderRef { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = null!;

    public string Status { get; set; } = null!;

    public DateTime? PaidAt { get; set; }

    public string? RawPayload { get; set; }

    public virtual CoursePurchase Purchase { get; set; } = null!;
}
