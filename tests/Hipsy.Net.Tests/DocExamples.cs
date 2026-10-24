namespace Hipsy.Net.Tests;

/// <summary>Example responses copied from https://docs.hipsy.nl (the "..." placeholders removed).</summary>
internal static class DocExamples
{
    public const string Me = """
        { "id": 1234, "name": "Jane Doe", "email": "jane@example.com" }
        """;

    public const string Organisations = """
        {
            "data": [
                {
                    "id": 1000,
                    "name": "My organisation",
                    "slug": "my-organisation",
                    "logo": "https:\/\/cdn.hipsy.nl\/images\/organisations\/1000ItXCGss6.png"
                }
            ]
        }
        """;

    public const string Events = """
        {
            "data": [
                {
                    "id": 1032,
                    "title": "Ecstatic Dance Universe - Kareem",
                    "description": "Description of the event\n\nAll the info of the event",
                    "location": "Amersfoort, Nederland",
                    "latitude": 52.1561113,
                    "longitude": 5.3878266,
                    "picture": "https:\/\/cdn.hipsy.nl\/images\/events\/pic1.jpg",
                    "picture_small": "https:\/\/cdn.hipsy.nl\/images\/events\/pic1.jpg",
                    "date": "2023-03-01T19:00:00.000000Z",
                    "date_until": "2023-03-01T22:00:00.000000Z",
                    "sold_out": false,
                    "private": false,
                    "disable_tickets": false,
                    "url_hipsy": "https:\/\/hipsy.nl\/event\/1032-ecstatic-dance-universe-kareem",
                    "url_ticketshop": "https:\/\/hipsy.nl\/shop\/1032-ecstatic-dance-universe-kareem",
                    "qr_ticketshop": "https:\/\/hipsy.nl\/qr\/ticketshop-1032.png",
                    "tickets": [
                        {
                            "id": 83,
                            "type": "ticket",
                            "name": "Early Bird",
                            "description": "",
                            "price": 17.5,
                            "is_entry_ticket": true,
                            "status": "sale",
                            "available": 7,
                            "sale_start": "2023-01-22T10:00:00.000000Z",
                            "sale_end": null
                        },
                        {
                            "id": 84,
                            "type": "donation",
                            "name": "Support us",
                            "description": "",
                            "price": 5,
                            "is_entry_ticket": false,
                            "status": "start_future",
                            "available": 99999,
                            "sale_start": null,
                            "sale_end": null
                        },
                        {
                            "id": 85,
                            "type": "something_new",
                            "name": "Hidden",
                            "description": "",
                            "price": 0,
                            "is_entry_ticket": true,
                            "status": "brand_new_status",
                            "available": 1,
                            "sale_start": null,
                            "sale_end": null,
                            "url": "https:\/\/hipsy.nl\/shop\/secret"
                        }
                    ],
                    "categories": [ "Ecstatic Dance" ]
                }
            ],
            "links": {
                "first": "https:\/\/api.hipsy.nl\/v1\/organisation\/ecstatic-dance-universe\/events?page=1",
                "last": "https:\/\/api.hipsy.nl\/v1\/organisation\/ecstatic-dance-universe\/events?page=1",
                "prev": null,
                "next": null
            },
            "meta": {
                "current_page": 1,
                "from": 1,
                "last_page": 1,
                "links": [],
                "path": "https:\/\/api.hipsy.nl\/v1\/organisation\/ecstatic-dance-universe\/events",
                "per_page": 25,
                "to": 1,
                "total": 1
            }
        }
        """;

    public const string Orders = """
        {
            "data": [
                {
                    "id": 1148,
                    "event_id": 1030,
                    "event_name": "Ecstatic Dance Universe - Kareem",
                    "revenue": 17.5,
                    "first_name": "Jane",
                    "last_name": "Doe",
                    "email": "jane@example.com",
                    "locale": "nl",
                    "newsletter": false,
                    "date": "2022-12-16T15:04:51.000000Z",
                    "personal_data": {
                        "city": "Utrecht", "phone": null, "gender": null, "address": null, "comment": null,
                        "company": null, "dietary": null, "birthday": null, "postcode": null,
                        "question1": "Yes", "question2": null, "question3": null
                    },
                    "utm": {
                        "utm_source": "newsletter", "utm_medium": "email",
                        "utm_campaign": null, "utm_term": null, "utm_content": null
                    },
                    "tickets": [
                        {
                            "number": 114857291,
                            "event_ticket_id": 83,
                            "event_ticket_name": "Early Bird",
                            "checkin": false,
                            "price_ticket": "17.50",
                            "price_final": "18.45"
                        }
                    ]
                }
            ],
            "links": {
                "first": "https:\/\/api.hipsy.nl\/v1\/organisation\/ecstatic-dance-universe\/orders?page=1",
                "last": "https:\/\/api.hipsy.nl\/v1\/organisation\/ecstatic-dance-universe\/orders?page=4",
                "prev": null,
                "next": "https:\/\/api.hipsy.nl\/v1\/organisation\/ecstatic-dance-universe\/orders?page=2"
            },
            "meta": {
                "current_page": 1, "from": 1, "last_page": 4, "links": [],
                "path": "https:\/\/api.hipsy.nl\/v1\/organisation\/ecstatic-dance-universe\/orders",
                "per_page": 25, "to": 25, "total": 81
            }
        }
        """;

    public const string Reviews = """
        {
            "data": [
                {
                    "id": 512,
                    "reviewer": { "id": 4821, "name": "Jane", "avatar_url": null },
                    "event": "Ecstatic Dance Universe - Kareem",
                    "text": "What a beautiful evening, great music and a lovely atmosphere!",
                    "rating": 5,
                    "reply": "Thank you Jane, see you next time!",
                    "replied_at": "2023-03-03T09:12:44.000000Z",
                    "created_at": "2023-03-02T18:30:10.000000Z"
                }
            ],
            "links": { "first": "?page=1", "last": "?page=3", "prev": null, "next": "?page=2" },
            "meta": {
                "current_page": 1, "from": 1, "last_page": 3, "links": [], "path": "",
                "per_page": 15, "to": 15, "total": 38
            }
        }
        """;

    /// <summary>A minimal page with the given paging numbers and one item per page.</summary>
    public static string ReviewPage(int page, int lastPage) => $$"""
        {
            "data": [ { "id": {{page}}, "reviewer": { "id": 1, "name": "A" }, "event": "E", "text": "t", "rating": 4, "created_at": "2023-03-02T18:30:10.000000Z" } ],
            "links": { "first": "?page=1", "last": "?page={{lastPage}}", "prev": null, "next": null },
            "meta": { "current_page": {{page}}, "from": {{page}}, "last_page": {{lastPage}}, "path": "", "per_page": 15, "to": {{page}}, "total": {{lastPage}} }
        }
        """;
}
