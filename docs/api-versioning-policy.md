# TMS API Versioning Policy

## 1. What counts as a breaking change
A change is breaking if an existing, correctly-written client could start
failing or receiving different data because of it. This includes:
- Removing a field from a response.
- Renaming a field (clients read by name, not position).
- Changing a response's status code for an existing scenario.
- Tightening validation on a request field (a previously-accepted payload
  now gets rejected).
- Changing a default sort order or default page size.

## 2. What counts as additive (non-breaking)
These changes are safe to ship on the current version without bumping it:
- Adding a new optional field to a response.
- Adding a brand-new endpoint.
- Adding a new optional query parameter with a sensible default.

## 3. Sunset window
When a new major version ships, the previous version keeps running for a
**minimum of 6 months** before it is shut down. This gives rural training
centres on quarterly maintenance schedules at least two maintenance
windows to migrate. The exact sunset date is published in the `Sunset`
response header the day the new version ships — it is never a surprise.

## 4. Communication
From day one of a new version shipping, the previous version's responses
carry three headers: `Deprecation: true`, `Sunset: <date>`, and
`Link: <new-version-url>; rel="successor-version"`. In addition:
- A CHANGELOG entry is added describing what changed and why.
- Every team holding an API key receives an email with the sunset date
  and a link to this policy.
- A calendar invite is sent for the actual shutdown date, so it is on
  record and not just in an inbox.

## 5. Skipping versions
Clients are never forced to migrate through every intermediate version.
A client on V1 is allowed to jump straight to V3 once V3 ships; it does
not need to pass through V2 first. Each version's contract stands on its
own and is documented independently in Scalar.
