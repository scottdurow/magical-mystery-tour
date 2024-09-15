# Contoso Real Estate Design

This section contains an architectural overview of the Contoso Real Estate System using C4 diagrams.

## System Context [Context - C1]

This diagram show the overall system context of the Contoso Real Estate System
```mermaid
 %% ------------------------------------------------------------------------------------
graph 
subgraph ContosoRealEstateC1["Contoso Real Estate [C1]"]
    
    subgraph ContosoRealEstateC1Inner[" "]
        PortalAdminC1["👤Portal Admin User"]
        ContosoRealEstateManagementSystem["Real Estate Management<br>[System]"]
    end

    AnonymousUserC1["👤Anonymous Portal User"]
    RegisteredUserC1["👤Registered Portal User"]
    StripeExternalSystemC1["Stripe<br>[External System]"]

    AnonymousUserC1 --> | Browse Listings | ContosoRealEstateManagementSystem
    RegisteredUserC1 --> | Reserve Listings | ContosoRealEstateManagementSystem
    PortalAdminC1 --> | Maintain Listings, Refunds | ContosoRealEstateManagementSystem
    ContosoRealEstateManagementSystem <--> | Checkout | StripeExternalSystemC1
end

%% ------------------------------------------------------------------------------------

classDef boundaryClass stroke-dasharray: 5, 5
classDef userClass fill:#87CEEB,stroke:#333,stroke-dasharray: 5, 5,color:black
classDef externalSystem fill:#c0c0c0,stroke:#333,stroke-dasharray: 5, 5,color:black

class ContosoRealEstateC1Inner,ContosoRealEstateManagementSystemC2Inner,PortalC3Inner,PortalC3Inner_CopilotStudio boundaryClass
class AnonymousUserC1,RegisteredUserC1,PortalAdminC1,AnonymousUserC2,RegisteredUserC2,PortalAdminC2,PortalUser_PortalC3 userClass
class StripeExternalSystemC1,StripeExternalSystemC2 externalSystem
```

## Contoso Real Estate Management System [Container - C2]

This diagram shows a drill down container diagram of the Real Estate Management System

```mermaid
 %% ------------------------------------------------------------------------------------
graph 
subgraph ContosoRealEstateManagementSystemC2["Real Estate Management System [C2]"]
    
    StripeExternalSystemC2["Stripe<br>[External System]"]
    RegisteredUserC2["Registered Portal User"]
    PortalAdminC2["Portal Admin User"]

    subgraph ContosoRealEstateManagementSystemC2Inner[" "]
        Portal["Portal<br>[Power Pages]"]
        ListingAdminApp["Listing Admin App<br>[Model Driven App]"]
        RealEsateDatabase["Real Estate Database<br>[Dataverse]"]
        CopilotStudioBot_CRESystemC2["Portal Bot<br>[Copilot Studio]"]
        PaymentsSystemC2["Payments System<br>[Azure Functions]"]
        ListingAdminApp --> RealEsateDatabase
        Portal --> RealEsateDatabase
        Portal --> | Uses | CopilotStudioBot_CRESystemC2
        Portal --> | Checkout | PaymentsSystemC2
        ListingAdminApp --> | Query Payments | PaymentsSystemC2
    end
    
    
    
    RegisteredUserC2 --> | Browse Listings | Portal
    RegisteredUserC2 --> | Reserve | Portal
    PortalAdminC2 --> | Manage Listings & Refunds | ListingAdminApp

    PaymentsSystemC2 --> | Create checkout session | StripeExternalSystemC2
    StripeExternalSystemC2 --> | Webhook | PaymentsSystemC2
    RegisteredUserC2 --> | Check out | StripeExternalSystemC2
    CopilotStudioBot_CRESystemC2 --> | Search Listings | RealEsateDatabase
end

%% ------------------------------------------------------------------------------------

classDef boundaryClass stroke-dasharray: 5, 5
classDef userClass fill:#87CEEB,stroke:#333,stroke-dasharray: 5, 5,color:black
classDef externalSystem fill:#c0c0c0,stroke:#333,stroke-dasharray: 5, 5,color:black

class ContosoRealEstateC1Inner,ContosoRealEstateManagementSystemC2Inner,PortalC3Inner,PortalC3Inner_CopilotStudio boundaryClass
class AnonymousUserC1,RegisteredUserC1,PortalAdminC1,AnonymousUserC2,RegisteredUserC2,PortalAdminC2,PortalUser_PortalC3 userClass
class StripeExternalSystemC1,StripeExternalSystemC2 externalSystem
```

## Portal [Component - C3]

This diagram shows a drill down component diagram of the Power Pages Portal

```mermaid
 %% ------------------------------------------------------------------------------------
graph 
subgraph PortalC3["Portal [C3]"]
    PortalUser_PortalC3["Portal User"]
    AuthenticationProvider["Authentication Provider"]
    subgraph PortalC3Inner["Power Pages"]
        ListingDetails["Listing Details<br>[React Component]"]
        CheckoutFlow["Checkout<br>[Cloud Flow]"]
        PortalBotClient["Portal Bot Client"]
        SearchComponent["Search<br>[React Component]"]
       
        PowerPagesWebApi["Power Pages WebAPI"]
        CompleteReservation["Complete Reservation<br>[React Component]"]
        PortalBotClient -.-> | Links | ListingDetails

        SearchComponent --> | Search Listing | PowerPagesWebApi
        SearchComponent -.-> | Links | ListingDetails
       
        ListingDetails --> CheckoutFlow
        StripeCheckout_PortalC3 -.-> | ReturnUrl from Checkout | CompleteReservation
    end



    StripeCheckout_PortalC3["Stripe Checkout Page"]
    RealEsateDatabase_PortalC3["Real Estate Database<br>[Dataverse]"]
    PaymentSystem_PortalC3["Payment API<br>[Custom Connector]"]
    ListingDetails --> | Query Listing | PowerPagesWebApi
    
    CompleteReservation --> PowerPagesWebApi
    PortalUser_PortalC3 --> StripeCheckout_PortalC3
    PortalUser_PortalC3 --> SearchComponent
    PortalUser_PortalC3 --> PortalBotClient
    PortalUser_PortalC3 --> AuthenticationProvider
    CheckoutFlow --> | Create Checkout Session | PaymentSystem_PortalC3
    PowerPagesWebApi --> RealEsateDatabase_PortalC3
    ListingDetails -.-> | Redirects | StripeCheckout_PortalC3
    
end

%% ------------------------------------------------------------------------------------

classDef boundaryClass stroke-dasharray: 5, 5
classDef userClass fill:#87CEEB,stroke:#333,stroke-dasharray: 5, 5,color:black
classDef externalSystem fill:#c0c0c0,stroke:#333,stroke-dasharray: 5, 5,color:black

class ContosoRealEstateC1Inner,ContosoRealEstateManagementSystemC2Inner,PortalC3Inner,PortalC3Inner_CopilotStudio boundaryClass
class AnonymousUserC1,RegisteredUserC1,PortalAdminC1,AnonymousUserC2,RegisteredUserC2,PortalAdminC2,PortalUser_PortalC3 userClass
class StripeExternalSystemC1,StripeExternalSystemC2 externalSystem
```

## Portal Bot [Component - C3]

This diagram shows a drill down component diagram of the Portal bot.
```mermaid
 %% ------------------------------------------------------------------------------------
graph 
subgraph PortalC3Inner_CopilotStudio["Portal Bot<br>[Copilot Studio]"]
        PortalBot["Portal Bot<br>[Copilot Studio]"]
        PagePagesSearchHTTPEndPoint["Power Pages Search API"]
        PortalBotExtractSearchTermsFlow["Extract Search Terms<br>[Cloud Flow]"]
        PromptAction["Extract Search Terms<br>[Prompt Action]"]
        DataverseConnector_CopilotStudio["Dataverse Connector"]
        PortalBot--> |Extract Search Terms | PortalBotExtractSearchTermsFlow
        PortalBot--> |Search Listings | DataverseConnector_CopilotStudio
        PortalBot--> |Conversation Boosting | PagePagesSearchHTTPEndPoint
        PortalBotExtractSearchTermsFlow --> PromptAction

end

%% ------------------------------------------------------------------------------------

classDef boundaryClass stroke-dasharray: 5, 5
classDef userClass fill:#87CEEB,stroke:#333,stroke-dasharray: 5, 5,color:black
classDef externalSystem fill:#c0c0c0,stroke:#333,stroke-dasharray: 5, 5,color:black

class ContosoRealEstateC1Inner,ContosoRealEstateManagementSystemC2Inner,PortalC3Inner,PortalC3Inner_CopilotStudio boundaryClass
class AnonymousUserC1,RegisteredUserC1,PortalAdminC1,AnonymousUserC2,RegisteredUserC2,PortalAdminC2,PortalUser_PortalC3 userClass
class StripeExternalSystemC1,StripeExternalSystemC2 externalSystem
```


# Dataverse Tables [Class - C4]

This section shows a drill down class diagram of the Dataverse tables, and associate state diagrams

```mermaid
classDiagram
  class Listing {
    + string Name
    + string Address
    + string Description
    + string DisplayName
    + choices Features
    + auto ListingID
    + int MaximumGuests
    + currency PricePerMonth
    + int NumberOfBedrooms
    + int NumberOfBathrooms
    +Reserve(ListingId) ReservationId
    +IsListingAvailable(ListingId, From, To) bool
  }
  class Payment {
    +string CurrencyCode
    +decimal Amount
    +date CreatedOn
    +choice Status
  }
  class ListingReservation {
    +currency Amount
    +date From
    +date To
    +int Guests
    +int Nights
    +date ReservationDate
    +choice ReservationStatus

  }
  class ListingFee {
    +currency FeeAmount
    +bool PerGuest
  }
    Listing "1" --> "1..*" ListingFee : fees
    ListingFee "0..*" --> ListingFeeType  
    Listing "1" --> "1..*" ListingImage : images
    Listing "1" --> "1" ListingImage : primary image
    ListingReservation "0..*" --> "1" Listing : listing
    Payment "1..*" --> "1" ListingReservation : reservation
    ListingReservation "0..*"--> "1" Contact : customer
    Payment "0..*" --> "1" Contact : portal user

```

## State Charts

```mermaid
---
title: Reservation Status 
---
stateDiagram-v2
    [*] --> Checkout : checkout
    Checkout --> Pending : payment complete
    Checkout --> Abandoned : checkout expired/cancelled
    Pending --> Active : payment complete
    Active -->  Cancelled : reservation cancelled
    Active --> [*] : archived
    Cancelled --> [*] : archived
    Pending --> Cancelled : payment failed
    Abandoned --> [*] : archived

```

```mermaid
---
title: Payment Status 
---
stateDiagram-v2
    [*] --> Pending : checkout completed
    Pending --> Active : Webhook (Checkout Session Completed)
    Pending --> Complete : Webhook (Payment Intent Succeeded)
    Active --> Complete : Webhook (Payment Intent Succeeded)
    Active --> Failed : Webhook (Payment Intent Failed)
    Pending --> Failed : Webhook (Payment Intent Failed)
    Complete --> Refunded: refunded

```

</div>
