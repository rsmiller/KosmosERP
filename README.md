

![](https://raw.githubusercontent.com/rsmiller/KosmosERP/refs/heads/main/docs/kosmos_erp_logo.png)

# Kosmos ERP

[![.NET Run Unit Tests](https://github.com/rsmiller/KosmosERP/actions/workflows/dotnet_unit_tests.yml/badge.svg?branch=main)](https://github.com/rsmiller/KosmosERP/actions/workflows/dotnet_unit_tests.yml)

Kosmos ERP is a basic enterprise resource system written in C# and uses a React/NextJs as the programming frameworks for the front-end. 

## Inceptions
I had been in the manufacturing space for many years and built a simular ERP from scratch. In that setting, nothing was centralized, and everything was based on on-prem solutions. When creating the original, I wanted to integrate as many ad-hoc systems as possible; this is why Kosmos offloads to many vendors such as Azure, Stripe, and Keycloak.

My design philosophy for this project was to allow a developer or development team to use the vendors they already have while making integrating outside vendors accessible via provider classes for logs, payments, storage, and ect.

## Problems It Solves
So many companies still utilize paper as a means for record keeping and conducting manufacturing operations. For example, an order may be entered in Excel and printed. That sales order will then travel with the manufacturing goods on it's journey of assembly. Then that rigid piece of paper will accompany the final goods to Will-Call, where it will then be shipped with the sales order or a packing slip from another spreadsheet. Kosmos attempts to digitize that process, allowing the executing staff and staff to gain metrics and KPIs.

ERP systems are notoriously expensive, especially the implementation. I have talked to many vendors in the past, and on average for a small to mid-sized company with prior data that needs import will run around $200,000 and that includes licensing, which varies depending on the vendor. Kosmos ERP is a free solution that can be implemented with or without help. Costs associated with file and database storage can be localized via on-prem servers, which is a one-time cost, or can be cloud-based depending on current infrastructure.

The last point is development. Many manufacturers will pay salaries to people who build BOMs in Excel or will pay for developers in-house to help manage current software solutions. Kosmos gives stakeholders the ability to run a basic ERP system and customize the hell out of it to match processes native to their company. C# and React are used because it is taught in schools, the documentation is strong, they have been time-tested, and in the age of AI, modifications can be readily implemented.

## Use Cases
The use cases for Kosmos are typical of a manufacturer of some kind. The company may or may not have cloud infrastructure, which is why these so many provider categories that allow the use of local or database connections. 

## Installation
Check the docs section of this project for helpful guides on how to implement and run Kosmos. You can compile the API project and run it in IIS, or run it through Rancher or Kubernetes. The front-end compiles as static pages and likewise can be run in IIS, or run it through Rancher or Kubernetes.

## Reports
For compatability I went with [Fast Reports](https://github.com/FastReports/FastReport), and open-source reporting engine that comes with a [visual reporting tool](https://github.com/FastReports/FastReport/releases/latest) to editing and creating reports. As a single developer I couldn't be locked into Crystal Reports or PowerBI, and I didn't want to over complicate it with Superset or Metabase installations. If you want to edit or create reports the Reports project houses the templates and the configuration file there will serve as a means of telling the front-end UI how to render it and what API end-point to call.

## AI Statement
Since I am a single developer Claude was used in the making of this project. However, the bulk of its use was in automating common tasks, identiying issues, finding areas of improvement. The largest contributions were in the database seed tool and initial report generation. This project has over 180,000 lines of code, but AI only contributes to around 10%. This is not a vibe coded project as I have nearly 30 years of daily software development experience and have been utilizing .NET since 1.1. Since porting this to GitHub from GitLab, this project took me around a year and a half to get to that point.

## Implementation Help and Donations
If you find yourself having trouble implementing this product or have implemented it and it helped your company, feel free to reach out to me on [LinkedIn](https://www.linkedin.com/in/more-guids/). I'd love to hear your story or assist you. In addition, since this project is free and probably saved you a ton of money in one way or another, if you'd like to make a donation to help pay off my student loans, I'd apperciate that lol.
