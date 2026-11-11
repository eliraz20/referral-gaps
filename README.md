# Referral Gaps

Compare local service capacity with a constrained sharing plan and show which transfers cover additional demand.

## Try it

From this folder with the .NET 10 SDK installed:

```sh
dotnet run -- share study.tsv
```

Rows: area / area name / service / requests / capacity; link / origin area / receiving area / service / maximum shared slots. One row per area and service. Local coverage is reserved first. Remote capacity is allocated by maximum flow with direct eligible links. All results are potential coverage.

The included study.tsv uses tab-separated fields. Comment lines start with `#`.

The original exercise remains available with `dotnet run -- example.tsv`.

## When to use it

A placement student needs to compare which unmet requests could be covered if neighboring service areas share slots.

Example result:

```text
Tutoring: 4 local potential coverage, 6 additional with sharing.
  Hill: 2 requests still uncovered
  Hill -> Lake: 6 slots
  Lake: 0 requests still uncovered
```

Related course or field workflow: [PolyU social work fieldwork manual](https://www.polyu.edu.hk/apss/spis/images/content/Fieldwork_Manuals/MSW__Fieldwork_Manual_2021_Final.pdf).
