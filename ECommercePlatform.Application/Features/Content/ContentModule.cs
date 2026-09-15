using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using System.Text.Json.Serialization;
using ECommercePlatform.Application.Common.Abstractions;
using ECommercePlatform.Application.Common.Messaging;
using ECommercePlatform.Application.Features.Admin.Common;
using ECommercePlatform.Domain.Errors;

namespace ECommercePlatform.Application.Features.Content;

// IDs #149, #151-156 — site content (testimonials, FAQs, trust badges,
// hero copy, help topics, ...) served to the storefront and managed by admin.
// In-memory seeded store like the other admin CMS modules (#69-147):
// no DbContext changes, no migrations.

public sealed record ContentItemResponse : IAdminEntity
{
    public Guid Id { get; init; }
    public string Section { get; init; } = string.Empty;
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string? ImageUrl { get; init; }
    public string? LinkUrl { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset UpdatedAt { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record GetContentAdminQuery : IQuery<Result<PagedResult<ContentItemResponse>>>
{
    public string? Section { get; init; }
    public string? Search { get; init; }
    public bool? IsActive { get; init; }
    [Range(1, int.MaxValue)] public int Page { get; init; } = 1;
    [Range(1, 200)] public int PageSize { get; init; } = 25;
}

public sealed record GetPublicContentQuery : IQuery<Result<IReadOnlyList<ContentItemResponse>>>
{
    public string? Section { get; init; }
    public string? Search { get; init; }
}

public sealed record GetContentByIdQuery(Guid Id) : IQuery<Result<ContentItemResponse>>;

public sealed record CreateContentCommand : ICommand<Result<ContentItemResponse>>
{
    [Required, MaxLength(100)] public string Section { get; init; } = string.Empty;
    [Required, MaxLength(200)] public string Title { get; init; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; init; }
    [MaxLength(1000)] public string? ImageUrl { get; init; }
    [MaxLength(1000)] public string? LinkUrl { get; init; }
    public int SortOrder { get; init; }
    public bool IsActive { get; init; } = true;

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record UpdateContentCommand : ICommand<Result<ContentItemResponse>>
{
    public Guid Id { get; init; }
    [MaxLength(100)] public string? Section { get; init; }
    [MaxLength(200)] public string? Title { get; init; }
    [MaxLength(2000)] public string? Description { get; init; }
    [MaxLength(1000)] public string? ImageUrl { get; init; }
    [MaxLength(1000)] public string? LinkUrl { get; init; }
    public int? SortOrder { get; init; }
    public bool? IsActive { get; init; }

    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Extra { get; init; }
}

public sealed record DeleteContentCommand(Guid Id) : ICommand<Result>;

public sealed record SetContentStatusCommand(Guid Id, bool IsActive) : ICommand<Result<ContentItemResponse>>;

internal static class ContentSeeds
{
    internal static void Ensure()
    {
        AdminCrudStore<ContentItemResponse>.EnsureSeeded(() =>
        {
            var now = DateTimeOffset.UtcNow;
            var items = new List<ContentItemResponse>();
            var order = 0;

            void Add(string section, string title, string? description = null,
                string? imageUrl = null, string? linkUrl = null,
                Dictionary<string, JsonElement>? extra = null)
            {
                items.Add(new ContentItemResponse
                {
                    Id = Guid.NewGuid(),
                    Section = section,
                    Title = title,
                    Description = description,
                    ImageUrl = imageUrl,
                    LinkUrl = linkUrl,
                    SortOrder = order++,
                    IsActive = true,
                    CreatedAt = now,
                    UpdatedAt = now,
                    Extra = extra,
                });
            }

            JsonElement Str(string value) =>
                JsonSerializer.SerializeToElement(value);

            // ---- Home ----
            Add("testimonials", "Rajesh Kumar",
                "Aanzara's wholesale rates helped us improve our margins while keeping quality consistent across every order.",
                extra: new() { ["role"] = Str("Kirana Store Owner") });
            Add("testimonials", "Priya Sharma",
                "Ordering in bulk is simple and the GST invoices arrive with every order. Delivery has always been on time.",
                extra: new() { ["role"] = Str("Supermarket Manager") });
            Add("testimonials", "Amit Patel",
                "The product range for our restaurant chain sourcing is excellent. Support resolves queries quickly.",
                extra: new() { ["role"] = Str("Restaurant Owner") });
            Add("hero_features", "Wholesale Prices", "Factory-rate pricing on every bulk order.");
            Add("hero_features", "GST Invoicing", "Compliant tax invoices with every purchase.");
            Add("hero_features", "Pan-India Delivery", "Reliable dispatch across the country.");
            Add("hero_features", "Verified Sellers", "Sourced directly from manufacturers.");
            Add("industry_solutions", "Kirana Stores", "Daily essentials at wholesale rates for neighbourhood stores.");
            Add("industry_solutions", "Restaurants & Cafes", "Bulk ingredients with consistent quality for food businesses.");
            Add("industry_solutions", "Supermarkets", "Full-shelf supply with scheduled replenishment.");
            Add("industry_solutions", "Institutions", "Canteen and hostel provisioning at contracted rates.");

            // ---- Why choose / why shop / why buy ----
            Add("why_choose", "Direct Factory Sourcing", "Stock sourced directly from mills, cutting intermediate delays.");
            Add("why_choose", "Automated GST Invoicing", "Itemized GST invoices with every wholesale order.");
            Add("why_choose", "Dedicated Fleet Delivery", "Consistent dispatch and delivery windows.");
            Add("why_choose", "Business Credit Options", "Eligible buyers access credit cycles on approval.");
            Add("why_shop", "Best Wholesale Prices", "Save more on every bulk purchase.");
            Add("why_shop", "Wide Selection", "Thousands of products across FMCG categories.");
            Add("why_shop", "Reliable Delivery", "Timely delivery across India.");
            Add("why_buy", "100% Original", "Genuine sourced products.");
            Add("why_buy", "Verified Sellers", "Checked business partners.");
            Add("why_buy", "GST Invoice", "Instant billing on every order.");
            Add("why_buy", "Fast Delivery", "Reliable dispatch timelines.");
            Add("why_buy", "Bulk Discounts", "Bigger orders, bigger savings.");
            Add("why_buy", "Secure Payments", "Encrypted transactions always.");

            // ---- FAQs ----
            Add("faqs", "How do I claim GST input credit?",
                "Every order placed through a registered business account generates a GST-compliant tax invoice, downloadable from your order history.");
            Add("faqs", "What is the wholesale shipping timeline?",
                "Standard wholesale orders dispatch within 48-72 hours and arrive within 2-5 business days depending on region and volume.");
            Add("faqs", "How can I register my business?",
                "Register as a corporate buyer from Business Solutions — our team verifies your GST details and assigns a manager.");
            Add("faqs", "What is the return policy for bulk orders?",
                "Bulk orders can be returned within 7 days of delivery if unopened and in original packaging.");
            Add("faqs", "Do you offer credit facilities?",
                "Verified enterprise accounts with consistent history can apply for 15 to 45 day credit cycles.");

            // ---- Enterprise / product detail ----
            Add("enterprise_info", "Distributor Pricing Structure",
                "Authorized dealers qualify for tiered pricing above standard bulk rate slabs.");
            Add("enterprise_info", "Dealer Profit Margins",
                "Estimated retail margin ranges between 12-18% depending on region and volume.");
            Add("enterprise_docs", "Official Product Brochure", "PDF • 1.2 MB");
            Add("enterprise_docs", "Safety Data Sheet", "PDF • 640 KB");
            Add("enterprise_docs", "GST Registration", "PDF • 310 KB");

            // ---- Trust ----
            Add("trust_guarantees", "Verified Stores", "Every listed store is manually checked.");
            Add("trust_guarantees", "Real Locations", "Accurate pins with live map directions.");
            Add("trust_guarantees", "No Hidden Fees", "Transparent pricing, zero surprises.");
            Add("trust_guarantees", "Community Ratings", "Genuine reviews from real shoppers.");
            Add("cart_trust", "100% Genuine Products", "Sourced directly from manufacturers.");
            Add("cart_trust", "GST Invoice Available", "Tax invoice with every order.");
            Add("cart_trust", "Pan-India Delivery", "Reliable dispatch timelines.");
            Add("cart_trust", "Easy Returns", "Simple return process for eligible orders.");
            Add("cart_trust", "Secure Payments", "Encrypted transactions always.");
            Add("checkout_trust", "Secure Checkout", "Your payment details stay encrypted.");
            Add("checkout_trust", "GST Invoice Included", "Tax invoice generated automatically.");
            Add("checkout_trust", "Buyer Protection", "Refunds for failed or cancelled orders.");

            // ---- How it works / steps ----
            Add("how_it_works", "Create an Account", "Register your business in minutes.",
                extra: new() { ["step"] = Str("1") });
            Add("how_it_works", "Browse & Order", "Add wholesale products to your cart.",
                extra: new() { ["step"] = Str("2") });
            Add("how_it_works", "Receive & Grow", "Get scheduled delivery and reorder easily.",
                extra: new() { ["step"] = Str("3") });
            Add("customer_steps", "Discover Deals", "Browse real-time discounts from verified stores near you.",
                extra: new() { ["step"] = Str("1") });
            Add("customer_steps", "Claim Coupon", "Tap to unlock the offer and get a digital coupon code.",
                extra: new() { ["step"] = Str("2") });
            Add("customer_steps", "Visit & Save", "Show your coupon in-store and enjoy the discount.",
                extra: new() { ["step"] = Str("3") });
            Add("owner_steps", "Register Store", "Sign up your store and get verified within 48 hours.",
                extra: new() { ["step"] = Str("1") });
            Add("owner_steps", "Publish Deals", "List offers and update discounts anytime, for free.",
                extra: new() { ["step"] = Str("2") });
            Add("owner_steps", "Boost Footfall", "Nearby shoppers discover your store and walk in.",
                extra: new() { ["step"] = Str("3") });

            // ---- Business solutions ----
            Add("bs_hero_stats", "10K+", "Business Clients");
            Add("bs_hero_stats", "50K+", "Products Available");
            Add("bs_hero_stats", "15+", "Years of Trust");
            Add("bs_hero_stats", "100%", "GST Compliant");
            Add("bs_hero_perks", "Bulk Pricing", "Save more on every order.");
            Add("bs_hero_perks", "Quality Assured", "100% authentic products.");
            Add("bs_hero_perks", "Fast Delivery", "Quick dispatch & shipping.");
            Add("bs_hero_perks", "Dedicated Support", "Personal account manager.");
            Add("bs_solutions", "Wholesale Supply", "Bulk products at competitive wholesale prices.");
            Add("bs_solutions", "GST Invoicing", "Compliant tax invoices generated automatically.");
            Add("bs_solutions", "Flexible Credit", "Extended payment terms for verified businesses.");
            Add("bs_solutions", "Fast Logistics", "Reliable delivery with real-time tracking.");
            Add("bs_solutions", "Easy Returns", "Simple returns for bulk orders.");
            Add("bs_solutions", "Priority Support", "Dedicated business support team.");
            Add("bs_industries", "Kirana & Retail", "Daily essentials for neighbourhood stores.", linkUrl: "/contact");
            Add("bs_industries", "HoReCa", "Bulk food supplies for hotels and caterers.", linkUrl: "/contact");
            Add("bs_industries", "Institutions", "Canteen provisioning at contracted rates.", linkUrl: "/contact");
            Add("bs_why_choose", "Lowest Wholesale Rates", "Factory-gate pricing passed on to you.");
            Add("bs_why_choose", "Assured Authenticity", "Every batch traceable to the manufacturer.");
            Add("bs_why_choose", "Credit & Terms", "Structured credit for growing businesses.");
            Add("bs_how_it_works", "Tell Us Your Needs", "Share your product list and volumes.",
                extra: new() { ["step"] = Str("1") });
            Add("bs_how_it_works", "Get a Quote", "Receive competitive pricing within 24 hours.",
                extra: new() { ["step"] = Str("2") });
            Add("bs_how_it_works", "Schedule Supply", "Set up recurring deliveries.",
                extra: new() { ["step"] = Str("3") });
            Add("bs_cta_perks", "Free business registration");
            Add("bs_cta_perks", "Dedicated account manager");
            Add("bs_cta_perks", "Priority dispatch");
            Add("bs_cta_contact", "+91 1800-309-8080", "Mon–Sat, 9 AM – 7 PM");

            // ---- Wholesale ----
            Add("ws_hero_perks", "Best Wholesale Prices", "Save more on every bulk order.");
            Add("ws_hero_perks", "Wide Product Range", "Thousands of products to choose from.");
            Add("ws_hero_perks", "Reliable Delivery", "On-time dispatch across India.");
            Add("ws_hero_bar", "Minimum Order Value", "Starting at ₹2,000 per order.");
            Add("ws_hero_bar", "Bulk Discounts", "The more you buy, the more you save.");
            Add("ws_hero_bar", "Secure Payments", "100% safe and encrypted.");
            Add("ws_bulk_checklist", "Upload your requirement list");
            Add("ws_bulk_checklist", "Get the best wholesale quote");
            Add("ws_bulk_checklist", "Fast response within 24 hours");
            Add("ws_why_buy", "Better Prices", "Lowest prices on bulk purchases.");
            Add("ws_why_buy", "Bulk Discounts", "Higher quantity, higher savings.");
            Add("ws_why_buy", "Wide Selection", "Huge range across FMCG categories.");
            Add("ws_why_buy", "Reliable Delivery", "Timely delivery across India.");
            Add("ws_segments", "Kirana Stores");
            Add("ws_segments", "Retailers");
            Add("ws_segments", "Institutions");
            Add("ws_segments", "Caterers");

            // ---- Contact / help ----
            Add("contact_quick_info", "Call Us", "+91 1800-309-8080",
                extra: new() { ["sub"] = Str("Mon–Sat, 9:00 AM – 7:00 PM") });
            Add("contact_quick_info", "Email Us", "support@aanzara.com",
                extra: new() { ["sub"] = Str("We reply within 24 hours") });
            Add("contact_quick_info", "Visit Us", "Mumbai, Maharashtra",
                extra: new() { ["sub"] = Str("Head office & warehouse") });
            Add("contact_office", "Aanzara Market", "Andheri East, Mumbai, Maharashtra 400069");
            Add("help_topics", "Order & Delivery", "Track orders, delivery timelines, and shipping queries.");
            Add("help_topics", "Billing & Invoices", "GST invoices, payment issues, and refund status.");
            Add("help_topics", "Returns & Refunds", "Return requests, replacement, and refund policy.");
            Add("help_topics", "Wholesale Enquiry", "Bulk pricing, dealer registration, and partnerships.");
            Add("help_options", "Track Your Order", "See live status and delivery timelines.",
                extra: new() { ["detail"] = Str("Visit the orders page for real-time updates.") });
            Add("help_options", "Talk to Support", "Reach our support team for order help.",
                extra: new() { ["detail"] = Str("Available Mon–Sat, 9 AM – 7 PM.") });
            Add("grow_business", "Become a Supplier", "List your products for thousands of buyers.",
                extra: new() { ["cta"] = Str("Apply now") });
            Add("grow_business", "Open a Store", "Get a branded storefront on Aanzara.",
                extra: new() { ["cta"] = Str("Get started") });
            Add("business_documents", "GST Certificate", "PDF • Verified");
            Add("business_documents", "Trade License", "PDF • Verified");

            // ---- New arrivals strips ----
            Add("na_features", "Fresh Stock Weekly", "New products added every week.");
            Add("na_features", "Introductory Prices", "Launch discounts on new arrivals.");
            Add("na_perks", "Early Access", "Shop new launches before everyone else.");
            Add("na_perks", "Launch Offers", "Extra savings on freshly added products.");
            Add("na_trust", "Quality Checked");
            Add("na_trust", "GST Invoiced");
            Add("na_trust", "Easy Returns");

            return items;
        });
    }
}

public sealed class GetContentAdminQueryHandler
    : IQueryHandler<GetContentAdminQuery, Result<PagedResult<ContentItemResponse>>>
{
    public Task<Result<PagedResult<ContentItemResponse>>> Handle(
        GetContentAdminQuery request, CancellationToken cancellationToken)
    {
        ContentSeeds.Ensure();
        var filtered = AdminCrudStore<ContentItemResponse>.All()
            .Where(c => string.IsNullOrWhiteSpace(request.Section) ||
                string.Equals(c.Section.Trim(), request.Section!.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(c => !request.IsActive.HasValue || c.IsActive == request.IsActive.Value)
            .Where(c => AdminPaging.Matches(request.Search, c.Section, c.Title, c.Description))
            .OrderBy(c => c.Section).ThenBy(c => c.SortOrder);

        return Task.FromResult(Result.Success(AdminPaging.ToPaged(filtered, request.Page, request.PageSize)));
    }
}

public sealed class GetPublicContentQueryHandler
    : IQueryHandler<GetPublicContentQuery, Result<IReadOnlyList<ContentItemResponse>>>
{
    public Task<Result<IReadOnlyList<ContentItemResponse>>> Handle(
        GetPublicContentQuery request, CancellationToken cancellationToken)
    {
        ContentSeeds.Ensure();
        var items = AdminCrudStore<ContentItemResponse>.All()
            .Where(c => c.IsActive)
            .Where(c => string.IsNullOrWhiteSpace(request.Section) ||
                string.Equals(c.Section.Trim(), request.Section!.Trim(), StringComparison.OrdinalIgnoreCase))
            .Where(c => AdminPaging.Matches(request.Search, c.Title, c.Description))
            .OrderBy(c => c.Section).ThenBy(c => c.SortOrder)
            .ToList();

        return Task.FromResult(Result.Success<IReadOnlyList<ContentItemResponse>>(items));
    }
}

public sealed class GetContentByIdQueryHandler
    : IQueryHandler<GetContentByIdQuery, Result<ContentItemResponse>>
{
    public Task<Result<ContentItemResponse>> Handle(
        GetContentByIdQuery request, CancellationToken cancellationToken)
    {
        ContentSeeds.Ensure();
        return Task.FromResult(
            AdminCrudStore<ContentItemResponse>.TryGet(request.Id, out var v) && v is not null
                ? Result.Success(v)
                : Result.Failure<ContentItemResponse>(AdminErrors.NotFound("Content", request.Id)));
    }
}

public sealed class CreateContentCommandHandler
    : IRequestHandler<CreateContentCommand, Result<ContentItemResponse>>
{
    public Task<Result<ContentItemResponse>> Handle(
        CreateContentCommand request, CancellationToken cancellationToken)
    {
        ContentSeeds.Ensure();
        if (string.IsNullOrWhiteSpace(request.Section))
        {
            return Task.FromResult(Result.Failure<ContentItemResponse>(
                Error.Validation("admin.content_section_required", "Section is required.")));
        }

        if (string.IsNullOrWhiteSpace(request.Title))
        {
            return Task.FromResult(Result.Failure<ContentItemResponse>(
                Error.Validation("admin.content_title_required", "Title is required.")));
        }

        var now = DateTimeOffset.UtcNow;
        var response = new ContentItemResponse
        {
            Id = Guid.NewGuid(),
            Section = request.Section.Trim().ToLowerInvariant(),
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            ImageUrl = request.ImageUrl?.Trim(),
            LinkUrl = request.LinkUrl?.Trim(),
            SortOrder = request.SortOrder,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now,
            Extra = request.Extra,
        };
        AdminCrudStore<ContentItemResponse>.Put(response);
        return Task.FromResult(Result.Success(response));
    }
}

public sealed class UpdateContentCommandHandler
    : IRequestHandler<UpdateContentCommand, Result<ContentItemResponse>>
{
    public Task<Result<ContentItemResponse>> Handle(
        UpdateContentCommand request, CancellationToken cancellationToken)
    {
        ContentSeeds.Ensure();
        if (!AdminCrudStore<ContentItemResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<ContentItemResponse>(AdminErrors.NotFound("Content", request.Id)));
        }

        var mergedExtra = existing.Extra;
        if (request.Extra is not null)
        {
            mergedExtra = new Dictionary<string, JsonElement>(mergedExtra ?? new());
            foreach (var kv in request.Extra)
            {
                mergedExtra[kv.Key] = kv.Value;
            }
        }

        var updated = existing with
        {
            Section = string.IsNullOrWhiteSpace(request.Section)
                ? existing.Section : request.Section.Trim().ToLowerInvariant(),
            Title = string.IsNullOrWhiteSpace(request.Title) ? existing.Title : request.Title.Trim(),
            Description = request.Description ?? existing.Description,
            ImageUrl = request.ImageUrl ?? existing.ImageUrl,
            LinkUrl = request.LinkUrl ?? existing.LinkUrl,
            SortOrder = request.SortOrder ?? existing.SortOrder,
            IsActive = request.IsActive ?? existing.IsActive,
            UpdatedAt = DateTimeOffset.UtcNow,
            Extra = mergedExtra,
        };
        AdminCrudStore<ContentItemResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}

public sealed class DeleteContentCommandHandler : IRequestHandler<DeleteContentCommand, Result>
{
    public Task<Result> Handle(DeleteContentCommand request, CancellationToken cancellationToken)
    {
        ContentSeeds.Ensure();
        if (!AdminCrudStore<ContentItemResponse>.Remove(request.Id))
        {
            return Task.FromResult(Result.Failure(AdminErrors.NotFound("Content", request.Id)));
        }

        return Task.FromResult(Result.Success());
    }
}

public sealed class SetContentStatusCommandHandler
    : IRequestHandler<SetContentStatusCommand, Result<ContentItemResponse>>
{
    public Task<Result<ContentItemResponse>> Handle(
        SetContentStatusCommand request, CancellationToken cancellationToken)
    {
        ContentSeeds.Ensure();
        if (!AdminCrudStore<ContentItemResponse>.TryGet(request.Id, out var existing) || existing is null)
        {
            return Task.FromResult(Result.Failure<ContentItemResponse>(AdminErrors.NotFound("Content", request.Id)));
        }

        var updated = existing with { IsActive = request.IsActive, UpdatedAt = DateTimeOffset.UtcNow };
        AdminCrudStore<ContentItemResponse>.Put(updated);
        return Task.FromResult(Result.Success(updated));
    }
}
